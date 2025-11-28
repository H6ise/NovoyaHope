using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovoyaHope.Migrations
{
    /// <inheritdoc />
    public partial class db4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Исправление колонки TextAnswer - делаем её nullable
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[UserAnswers]') AND type in (N'U'))
                BEGIN
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Identity].[UserAnswers]') AND name = 'TextAnswer' AND is_nullable = 0)
                    BEGIN
                        ALTER TABLE [Identity].[UserAnswers] ALTER COLUMN [TextAnswer] nvarchar(max) NULL;
                    END
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Откат: делаем колонку NOT NULL (не рекомендуется, но для полноты миграции)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[UserAnswers]') AND type in (N'U'))
                BEGIN
                    -- Сначала обновляем все NULL значения на пустую строку
                    UPDATE [Identity].[UserAnswers] SET [TextAnswer] = '' WHERE [TextAnswer] IS NULL;
                    -- Затем делаем колонку NOT NULL
                    ALTER TABLE [Identity].[UserAnswers] ALTER COLUMN [TextAnswer] nvarchar(max) NOT NULL;
                END
            ");
        }
    }
}
