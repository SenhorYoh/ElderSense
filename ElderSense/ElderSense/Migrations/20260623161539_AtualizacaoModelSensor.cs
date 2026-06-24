using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElderSense.Migrations
{
    /// <inheritdoc />
    public partial class AtualizacaoModelSensor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FKIdoso",
                table: "Sensores",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sensores_FKIdoso",
                table: "Sensores",
                column: "FKIdoso");

            migrationBuilder.AddForeignKey(
                name: "FK_Sensores_AspNetUsers_FKIdoso",
                table: "Sensores",
                column: "FKIdoso",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sensores_AspNetUsers_FKIdoso",
                table: "Sensores");

            migrationBuilder.DropIndex(
                name: "IX_Sensores_FKIdoso",
                table: "Sensores");

            migrationBuilder.DropColumn(
                name: "FKIdoso",
                table: "Sensores");
        }
    }
}
