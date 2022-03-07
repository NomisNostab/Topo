using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Topo.Migrations
{
    public partial class version01 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authentications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccessToken = table.Column<string>(type: "TEXT", nullable: true),
                    ExpiresIn = table.Column<int>(type: "INTEGER", nullable: true),
                    IdToken = table.Column<string>(type: "TEXT", nullable: true),
                    RefreshToken = table.Column<string>(type: "TEXT", nullable: true),
                    TokenType = table.Column<string>(type: "TEXT", nullable: true),
                    MemberName = table.Column<string>(type: "TEXT", nullable: true),
                    TokenExpiry = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authentications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OASTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TemplateName = table.Column<string>(type: "TEXT", nullable: false),
                    TemplateTitle = table.Column<string>(type: "TEXT", nullable: false),
                    InputGroup = table.Column<string>(type: "TEXT", nullable: false),
                    InputGroupSort = table.Column<int>(type: "INTEGER", nullable: false),
                    InputId = table.Column<string>(type: "TEXT", nullable: false),
                    InputLabel = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OASTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OASWorksheetAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InputId = table.Column<string>(type: "TEXT", nullable: false),
                    InputTitle = table.Column<string>(type: "TEXT", nullable: false),
                    InputTitleSortIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    InputLabel = table.Column<string>(type: "TEXT", nullable: false),
                    InputSortIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    MemberId = table.Column<string>(type: "TEXT", nullable: false),
                    MemberName = table.Column<string>(type: "TEXT", nullable: false),
                    MemberAnswer = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OASWorksheetAnswers", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Authentications");

            migrationBuilder.DropTable(
                name: "OASTemplates");

            migrationBuilder.DropTable(
                name: "OASWorksheetAnswers");
        }
    }
}
