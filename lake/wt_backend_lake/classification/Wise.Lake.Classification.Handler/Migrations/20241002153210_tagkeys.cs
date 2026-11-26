using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
namespace wisetown.lake.classification.handler.Migrations
{
    /// <inheritdoc />
    public partial class tagkeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentTags",
                schema: "trasimeno",
                table: "DocumentTags");

            migrationBuilder.AlterColumn<string>(
                name: "Tag",
                schema: "trasimeno",
                table: "DocumentTags",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentTags",
                schema: "trasimeno",
                table: "DocumentTags",
                columns: new[] { "Id", "Tag" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentTags",
                schema: "trasimeno",
                table: "DocumentTags");

            migrationBuilder.AlterColumn<string>(
                name: "Tag",
                schema: "trasimeno",
                table: "DocumentTags",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentTags",
                schema: "trasimeno",
                table: "DocumentTags",
                column: "Id");
        }
    }
}
