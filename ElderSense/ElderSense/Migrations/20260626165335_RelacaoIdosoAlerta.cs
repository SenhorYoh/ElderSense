using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElderSense.Migrations
{
    /// <inheritdoc />
    public partial class RelacaoIdosoAlerta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FKIdoso",
                table: "Alertas",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_FKIdoso",
                table: "Alertas",
                column: "FKIdoso");

            migrationBuilder.AddForeignKey(
                name: "FK_Alertas_AspNetUsers_FKIdoso",
                table: "Alertas",
                column: "FKIdoso",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alertas_AspNetUsers_FKIdoso",
                table: "Alertas");

            migrationBuilder.DropIndex(
                name: "IX_Alertas_FKIdoso",
                table: "Alertas");

            migrationBuilder.DropColumn(
                name: "FKIdoso",
                table: "Alertas");
        }
    }
}
