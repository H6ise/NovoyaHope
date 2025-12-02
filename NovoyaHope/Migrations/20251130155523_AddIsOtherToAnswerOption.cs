using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovoyaHope.Migrations
{
    /// <inheritdoc />
    public partial class AddIsOtherToAnswerOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOther",
                schema: "Identity",
                table: "AnswerOptions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOther",
                schema: "Identity",
                table: "AnswerOptions");
        }
    }
}
