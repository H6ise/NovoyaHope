using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovoyaHope.Migrations
{
    /// <inheritdoc />
    public partial class db6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultMaxPoints",
                schema: "Identity",
                table: "Surveys",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GradePublication",
                schema: "Identity",
                table: "Surveys",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsTestMode",
                schema: "Identity",
                table: "Surveys",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowCorrectAnswers",
                schema: "Identity",
                table: "Surveys",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowIncorrectAnswers",
                schema: "Identity",
                table: "Surveys",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowPoints",
                schema: "Identity",
                table: "Surveys",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultMaxPoints",
                schema: "Identity",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "GradePublication",
                schema: "Identity",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "IsTestMode",
                schema: "Identity",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "ShowCorrectAnswers",
                schema: "Identity",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "ShowIncorrectAnswers",
                schema: "Identity",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "ShowPoints",
                schema: "Identity",
                table: "Surveys");
        }
    }
}
