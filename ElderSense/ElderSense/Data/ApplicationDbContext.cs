using ElderSense.Data.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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
        }
    }
}
