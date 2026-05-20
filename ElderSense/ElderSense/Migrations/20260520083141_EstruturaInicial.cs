using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElderSense.Migrations
{
    /// <inheritdoc />
    public partial class EstruturaInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DadosMonitorizacao_Alertas_AlertaId",
                table: "DadosMonitorizacao");

            migrationBuilder.DropIndex(
                name: "IX_DadosMonitorizacao_AlertaId",
                table: "DadosMonitorizacao");

            migrationBuilder.DropColumn(
                name: "AlertaId",
                table: "DadosMonitorizacao");

            migrationBuilder.CreateTable(
                name: "AlertaDadosMonitorizacao",
                columns: table => new
                {
                    ListadeAlertasId = table.Column<int>(type: "int", nullable: false),
                    ListadeDadosId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertaDadosMonitorizacao", x => new { x.ListadeAlertasId, x.ListadeDadosId });
                    table.ForeignKey(
                        name: "FK_AlertaDadosMonitorizacao_Alertas_ListadeAlertasId",
                        column: x => x.ListadeAlertasId,
                        principalTable: "Alertas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlertaDadosMonitorizacao_DadosMonitorizacao_ListadeDadosId",
                        column: x => x.ListadeDadosId,
                        principalTable: "DadosMonitorizacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertaDadosMonitorizacao_ListadeDadosId",
                table: "AlertaDadosMonitorizacao",
                column: "ListadeDadosId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertaDadosMonitorizacao");

            migrationBuilder.AddColumn<int>(
                name: "AlertaId",
                table: "DadosMonitorizacao",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DadosMonitorizacao_AlertaId",
                table: "DadosMonitorizacao",
                column: "AlertaId");

            migrationBuilder.AddForeignKey(
                name: "FK_DadosMonitorizacao_Alertas_AlertaId",
                table: "DadosMonitorizacao",
                column: "AlertaId",
                principalTable: "Alertas",
                principalColumn: "Id");
        }
    }
}
