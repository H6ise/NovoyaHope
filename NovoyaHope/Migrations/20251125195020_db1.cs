using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovoyaHope.Migrations
{
    /// <inheritdoc />
    public partial class db1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyResponses_User_UserId",
                schema: "Identity",
                table: "SurveyResponses");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "Identity",
                table: "SurveyResponses",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyResponses_User_UserId",
                schema: "Identity",
                table: "SurveyResponses",
                column: "UserId",
                principalSchema: "Identity",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyResponses_User_UserId",
                schema: "Identity",
                table: "SurveyResponses");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                schema: "Identity",
                table: "SurveyResponses",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyResponses_User_UserId",
                schema: "Identity",
                table: "SurveyResponses",
                column: "UserId",
                principalSchema: "Identity",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
