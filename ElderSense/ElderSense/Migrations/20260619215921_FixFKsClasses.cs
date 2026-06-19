using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElderSense.Migrations
{
    /// <inheritdoc />
    public partial class FixFKsClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SensorId",
                table: "DadosMonitorizacao",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UtilizadorId",
                table: "DadosMonitorizacao",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DadosMonitorizacao_SensorId",
                table: "DadosMonitorizacao",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_DadosMonitorizacao_UtilizadorId",
                table: "DadosMonitorizacao",
                column: "UtilizadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DadosMonitorizacao_AspNetUsers_UtilizadorId",
                table: "DadosMonitorizacao",
                column: "UtilizadorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DadosMonitorizacao_Sensores_SensorId",
                table: "DadosMonitorizacao",
                column: "SensorId",
                principalTable: "Sensores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DadosMonitorizacao_AspNetUsers_UtilizadorId",
                table: "DadosMonitorizacao");

            migrationBuilder.DropForeignKey(
                name: "FK_DadosMonitorizacao_Sensores_SensorId",
                table: "DadosMonitorizacao");

            migrationBuilder.DropIndex(
                name: "IX_DadosMonitorizacao_SensorId",
                table: "DadosMonitorizacao");

            migrationBuilder.DropIndex(
                name: "IX_DadosMonitorizacao_UtilizadorId",
                table: "DadosMonitorizacao");

            migrationBuilder.DropColumn(
                name: "SensorId",
                table: "DadosMonitorizacao");

            migrationBuilder.DropColumn(
                name: "UtilizadorId",
                table: "DadosMonitorizacao");
        }
    }
}
