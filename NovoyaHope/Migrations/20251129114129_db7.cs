using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovoyaHope.Migrations
{
    /// <inheritdoc />
    public partial class db7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackgroundColor",
                schema: "Identity",
                table: "Surveys",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeaderFontFamily",
                schema: "Identity",
                table: "Surveys",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HeaderFontSize",
                schema: "Identity",
                table: "Surveys",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeaderImagePath",
                schema: "Identity",
                table: "Surveys",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionFontFamily",
                schema: "Identity",
                table: "Surveys",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuestionFontSize",
                schema: "Identity",
                table: "Surveys",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextFontFamily",
                schema: "Identity",
                table: "Surveys",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TextFontSize",
                schema: "Identity",
                table: "Surveys",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThemeColor",
                schema: "Identity",
                table: "Surveys",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundColor",
                schema: "Identity",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "HeaderFontFamily",
                schema: "Identity",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "HeaderFontSize",
                schema: "Identity",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "HeaderImagePath",
                schema: "Identity",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "QuestionFontFamily",
                schema: "Identity",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "QuestionFontSize",
                schema: "Identity",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "TextFontFamily",
                schema: "Identity",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "TextFontSize",
                schema: "Identity",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "ThemeColor",
                schema: "Identity",
                table: "Surveys");
        }
    }
}
