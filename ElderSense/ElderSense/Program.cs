using ElderSense.Data;
using ElderSense.Data.Model;
using ElderSense.Hubs;
using ElderSense.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// configuração do identity base (Cookies do site)
builder.Services.AddDefaultIdentity<Utilizador>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Regista a lógica que cria os sensores e limpa o lixo
builder.Services.AddScoped<SimuController>();

// 2. Regista o robô invisível que corre em segundo plano a cada 1 minuto
builder.Services.AddHostedService<SimuWorker>();

///<summary>
///configuração unificada de autenticação (Google + JWT)
///Se alguém aceder as páginas normais (Razor Pages, usa o esquema de cookies do Identity.
///Se vier uma requisição para a API, valida o cabeçalho bearer (JWT)
///</summary>
builder.Services.AddAuthentication(options =>
{
    // Por padrão, o site usa cookies. A API vai pedir explicitamente o "Bearer" (JWT)
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
})
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? string.Empty; ;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? string.Empty; ;
    })
    .AddJwtBearer("Bearer", options => // Configuração JWT para a API
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "ChaveSuperSecretaComMaisDe32Caracteres");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddRazorPages();

// Regista o SignalR para permitir notificações em tempo real
builder.Services.AddSignalR();

///<summary>
///O utilizador não pode fazer login até confirmar o seu email 
///+ o tempo limite de inatividade é definido para 5 dias
///</summary>
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);

builder.Services.ConfigureApplicationCookie(o => {
    o.ExpireTimeSpan = TimeSpan.FromDays(5);
    o.SlidingExpiration = true;
    o.LoginPath = "/Identity/Account/Login";
    o.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// *******************************************************************
// Instalar o package
// Microsoft.AspNetCore.Authentication.JwtBearer
//
// using Microsoft.IdentityModel.Tokens;
// *******************************************************************
// JWT Settings


// configuração do JWT
builder.Services.AddScoped<TokenService>();


// Eliminar a proteção de 'ciclos' qd se faz uma pesquisa que envolva um relacionamento 1-N em Linq
// https://code-maze.com/aspnetcore-handling-circular-references-when-working-with-json/
// https://marcionizzola.medium.com/como-resolver-jsonexception-a-possible-object-cycle-was-detected-27e830ea78e5
builder.Services.AddControllers()
                .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapControllers();

// Mapeia o endpoint do Hub - é aqui que o browser se vai ligar para receber notificações em tempo real
app.MapHub<AlertaHub>("/alertaHub");

app.Run();