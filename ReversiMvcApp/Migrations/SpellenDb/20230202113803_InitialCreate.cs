using Microsoft.EntityFrameworkCore.Migrations;

namespace ReversiMvcApp.Migrations.SpellenDb
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Spellen",
                columns: table => new
                {
                    omschrijving = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    bord = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    aanDeBeurt = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spellen", x => x.omschrijving);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Spellen");
        }
    }
}
