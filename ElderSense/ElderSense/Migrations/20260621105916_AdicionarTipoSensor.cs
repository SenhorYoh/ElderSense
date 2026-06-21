using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElderSense.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTipoSensor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Sensores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Sensores");
        }
    }
}
