using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenessansAPI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "AbtCamps");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "AbtCamps",
                newName: "TitleUz");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "AbtCamps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionRu",
                table: "AbtCamps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionUz",
                table: "AbtCamps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEn",
                table: "AbtCamps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleRu",
                table: "AbtCamps",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "AbtCamps");

            migrationBuilder.DropColumn(
                name: "DescriptionRu",
                table: "AbtCamps");

            migrationBuilder.DropColumn(
                name: "DescriptionUz",
                table: "AbtCamps");

            migrationBuilder.DropColumn(
                name: "TitleEn",
                table: "AbtCamps");

            migrationBuilder.DropColumn(
                name: "TitleRu",
                table: "AbtCamps");

            migrationBuilder.RenameColumn(
                name: "TitleUz",
                table: "AbtCamps",
                newName: "Title");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AbtCamps",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
