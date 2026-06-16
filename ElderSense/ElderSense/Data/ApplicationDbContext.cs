using ElderSense.Data.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ElderSense.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options){

        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<Alerta> Alertas { get; set; }
        public DbSet<DadosMonitorizacao> DadosMonitorizacao { get; set; }
        public DbSet<Sensor> Sensores { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Guarda o Enum como String na Base de Dados
            builder.Entity<Utilizador>()
                .Property(u => u.Tipo)
                .HasConversion<string>();

            // Resolve cascade paths em DadosMonitorizacao -> Sensor
            builder.Entity<DadosMonitorizacao>()
                .HasOne(d => d.Sensor)
                .WithMany()
                .HasForeignKey(d => d.FKSensor)
                .OnDelete(DeleteBehavior.Restrict);

            // Configura o M:N entre Alerta e DadosMonitorizacao sem cascade paths
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
        }
    }
}
