using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
namespace wisetown.lake.classification.handler.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "trasimeno");

            migrationBuilder.CreateTable(
                name: "Documents",
                schema: "trasimeno",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Position = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    IngestionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Stoned = table.Column<bool>(type: "bit", nullable: false),
                    StoneDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTags",
                schema: "trasimeno",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trash",
                schema: "trasimeno",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trash", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documents",
                schema: "trasimeno");

            migrationBuilder.DropTable(
                name: "DocumentTags",
                schema: "trasimeno");

            migrationBuilder.DropTable(
                name: "Trash",
                schema: "trasimeno");
        }
    }
}
