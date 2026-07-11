using ElderSense.Data;
using ElderSense.Data.Model;
using ElderSense.Hubs;
using ElderSense.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


// Configuração da base de dados
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// configuração do identity base (Cookies do site)
builder.Services.AddDefaultIdentity<Utilizador>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Regista a lógica que cria os sensores e limpa o lixo
builder.Services.AddScoped<SimuController>();

// Regista o serviço em segundo plano que corre a cada 1 minuto
builder.Services.AddHostedService<SimuWorker>();

// configuração unificada de autenticação (Google + JWT)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
})
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? string.Empty;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? string.Empty;
    })
    .AddJwtBearer("Bearer", options => // Configuração JWT para a API
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var jwtKey = jwtSettings["Key"] ?? throw new InvalidOperationException("A chave Jwt:Key não foi configurada.");
        var key = Encoding.UTF8.GetBytes(jwtKey);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

builder.Services.AddRazorPages();
builder.Services.AddControllers(); //ativa o suporte para a API

// Regista o SignalR para permitir notificações em tempo real
builder.Services.AddSignalR();

// Regista o gerador de documentação Swagger/OpenAPI para a API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // define o esquema de segurança JWT Bearer no Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduza o token JWT (sem escrever 'Bearer' à frente)."
    });

    // usa um filtro para exigir o token em todas as operações
    options.OperationFilter<ElderSense.Swagger.AuthOperationFilter>();
});

// O utilizador não pode fazer login até confirmar o seu email
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);

builder.Services.ConfigureApplicationCookie(o => {
    o.ExpireTimeSpan = TimeSpan.FromDays(5);
    o.SlidingExpiration = true;
    o.LoginPath = "/Identity/Account/Login";
    o.AccessDeniedPath = "/AcessoNegado";
});

// Configuração do JWT
builder.Services.AddScoped<TokenService>();

// Eliminar a proteção de 'ciclos' qd se faz uma pesquisa que envolva um relacionamento 1-N em Linq
builder.Services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

var app = builder.Build();


//cria as roles automaticamente no arranque
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Cuidador", "Idoso" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Pipeline de requisições HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// Captura códigos de erro HTTP (404, 403, etc.) e redireciona para páginas próprias,
// mas SÓ fora das rotas da API (senão a API devolveria respostas vazias em vez do JSON)
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"), appBuilder =>
{
    appBuilder.UseStatusCodePagesWithReExecute("/Erro/{0}");
});

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapControllers(); //diz ao servidor para escutar as rotas api

// Mapeia o endpoint do Hub
app.MapHub<AlertaHub>("/alertaHub");

app.Run();