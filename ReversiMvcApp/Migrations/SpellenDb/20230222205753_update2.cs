using Microsoft.EntityFrameworkCore.Migrations;

namespace ReversiMvcApp.Migrations.SpellenDb
{
    public partial class update2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "bord",
                table: "Spellen",
                newName: "bordString");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "bordString",
                table: "Spellen",
                newName: "bord");
        }
    }
}
