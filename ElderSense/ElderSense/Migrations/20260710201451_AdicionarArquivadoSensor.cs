using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElderSense.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarArquivadoSensor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Arquivado",
                table: "Sensores",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Arquivado",
                table: "Sensores");
        }
    }
}
