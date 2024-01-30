using Microsoft.EntityFrameworkCore.Migrations;

namespace ReversiMvcApp.Migrations.SpellenDb
{
    public partial class update1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Spellen",
                table: "Spellen");

            migrationBuilder.AlterColumn<string>(
                name: "omschrijving",
                table: "Spellen",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "token",
                table: "Spellen",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Spellen",
                table: "Spellen",
                column: "token");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Spellen",
                table: "Spellen");

            migrationBuilder.DropColumn(
                name: "token",
                table: "Spellen");

            migrationBuilder.AlterColumn<string>(
                name: "omschrijving",
                table: "Spellen",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Spellen",
                table: "Spellen",
                column: "omschrijving");
        }
    }
}
