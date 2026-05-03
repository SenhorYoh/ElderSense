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

            // Configura o Enum para ser guardado como String na Base de Dados
            builder.Entity<Utilizador>()
                .Property(u => u.Tipo)
                .HasConversion<string>();
        }
    }
}
