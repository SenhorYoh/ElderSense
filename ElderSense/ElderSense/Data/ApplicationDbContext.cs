using ElderSense.Data.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ElderSense.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {

        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<Alerta> Alertas { get; set; }
        public DbSet<DadosMonitorizacao> DadosMonitorizacao { get; set; }
        public DbSet<Sensor> Sensores { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configura o Enum para ser guardado como String na Base de Dados
            builder.Entity<Utilizador>()
                .Property(u => u.Tipo)
                .HasConversion<string>();

            // Configura o Enum do Sensor (Beacon/Pulseira) para ser guardado como String na Base de Dados
            builder.Entity<Sensor>()
                .Property(s => s.Tipo)
                .HasConversion<string>();

            ///<summary>
            ///Aqui tem-se a lógica de apagamento em cascata. Se um utilizador for eliminado do sistema,
            ///todos os dados associados também devem ser apagados 
            ///</summary>

            // 1. UTILIZADOR -> SENSORES (Cascade)
            // Se o Utilizador for apagado, os seus sensores são apagados automaticamente
            builder.Entity<Sensor>()
                .HasOne<Utilizador>(s => s.Utilizador)
                .WithMany()
                .HasForeignKey(s => s.FKUtilizador)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. UTILIZADOR -> DADOS MONITORIZAÇÃO (Cascade)
            // Se o Utilizador for apagado, os seus dados de saúde são apagados automaticamente
            builder.Entity<DadosMonitorizacao>()
                .HasOne<Utilizador>(d => d.Utilizador)
                .WithMany()
                .HasForeignKey(d => d.FKUtilizador)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. SENSOR -> DADOS MONITORIZAÇÃO (NoAction - Crucial para quebrar o ciclo!)
            // Isto impede o SQL Server de entrar em loop, mas garante que se o USER for apagado, TUDO desaparece.
            builder.Entity<DadosMonitorizacao>()
                .HasOne<Sensor>(d => d.Sensor)
                .WithMany()
                .HasForeignKey(d => d.FKSensor)
                .OnDelete(DeleteBehavior.NoAction);

            // 4. Configura o M:N entre Alerta e DadosMonitorizacao sem cascade paths
            // ListadeAlertasId é Cascade (apagar o Alerta limpa a junção); ListadeDadosId é Restrict
            // porque já existe um caminho cascade direto Utilizador -> Alerta, e dois caminhos cascade
            // até Alerta dão erro "multiple cascade paths" no SQL Server.
            builder.Entity<Alerta>()
                .HasMany(a => a.ListadeDados)
                .WithMany(d => d.ListadeAlertas)
                .UsingEntity<Dictionary<string, object>>(
                    "AlertaDadosMonitorizacao",
                    j => j.HasOne<DadosMonitorizacao>()
                          .WithMany()
                          .HasForeignKey("ListadeDadosId")
                          .OnDelete(DeleteBehavior.Restrict),
                    j => j.HasOne<Alerta>()
                          .WithMany()
                          .HasForeignKey("ListadeAlertasId")
                          .OnDelete(DeleteBehavior.Cascade)
                );

            // Diz ao EF Core para não apagar o Sensor automaticamente se o perfil do Idoso for apagado,
            // evitando o erro "multiple cascade paths". O apagamento em cascata fica só no FKUtilizador (Cuidador).
            builder.Entity<Sensor>()
            .HasOne(s => s.IdosoAssociado)
            .WithMany()
            .HasForeignKey(s => s.FKIdoso)
            .OnDelete(DeleteBehavior.Restrict);
        }
    }
}