using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovoyaHope.Migrations
{
    /// <inheritdoc />
    public partial class db3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Identity");

            // Проверка существования таблицы Role перед созданием
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[Role]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Identity].[Role] (
                        [Id] nvarchar(450) NOT NULL,
                        [Name] nvarchar(256) NULL,
                        [NormalizedName] nvarchar(256) NULL,
                        [ConcurrencyStamp] nvarchar(max) NULL,
                        CONSTRAINT [PK_Role] PRIMARY KEY ([Id])
                    );
                END
            ");

            // Создаем индекс только если таблица существует или индекс не существует
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'RoleNameIndex' AND object_id = OBJECT_ID(N'[Identity].[Role]'))
                BEGIN
                    CREATE UNIQUE INDEX [RoleNameIndex] ON [Identity].[Role] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
                END
            ");

            // Проверка существования таблицы SurveyTemplates перед созданием
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[SurveyTemplates]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Identity].[SurveyTemplates] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [Title] nvarchar(max) NOT NULL,
                        [Description] nvarchar(max) NOT NULL,
                        [Type] int NOT NULL,
                        [CreatorId] nvarchar(max) NOT NULL,
                        CONSTRAINT [PK_SurveyTemplates] PRIMARY KEY ([Id])
                    );
                END
            ");

            // Проверка существования таблицы User перед созданием
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[User]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Identity].[User] (
                        [Id] nvarchar(450) NOT NULL,
                        [FirstName] nvarchar(max) NULL,
                        [LastName] nvarchar(max) NULL,
                        [ProfileImagePath] nvarchar(max) NULL,
                        [ShowPhoneToPublic] bit NOT NULL,
                        [UserName] nvarchar(256) NULL,
                        [NormalizedUserName] nvarchar(256) NULL,
                        [Email] nvarchar(256) NULL,
                        [NormalizedEmail] nvarchar(256) NULL,
                        [EmailConfirmed] bit NOT NULL,
                        [PasswordHash] nvarchar(max) NULL,
                        [SecurityStamp] nvarchar(max) NULL,
                        [ConcurrencyStamp] nvarchar(max) NULL,
                        [PhoneNumber] nvarchar(max) NULL,
                        [PhoneNumberConfirmed] bit NOT NULL,
                        [TwoFactorEnabled] bit NOT NULL,
                        [LockoutEnd] datetimeoffset NULL,
                        [LockoutEnabled] bit NOT NULL,
                        [AccessFailedCount] int NOT NULL,
                        CONSTRAINT [PK_User] PRIMARY KEY ([Id])
                    );
                END
            ");

            // Создаем индексы только если таблица существует и индексы не существуют
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[User]') AND type in (N'U'))
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'EmailIndex' AND object_id = OBJECT_ID(N'[Identity].[User]'))
                    BEGIN
                        CREATE INDEX [EmailIndex] ON [Identity].[User] ([NormalizedEmail]);
                    END
                    
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UserNameIndex' AND object_id = OBJECT_ID(N'[Identity].[User]'))
                    BEGIN
                        CREATE UNIQUE INDEX [UserNameIndex] ON [Identity].[User] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
                    END
                END
            ");

            // Проверка существования таблицы AspNetRoleClaims перед созданием
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[AspNetRoleClaims]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Identity].[AspNetRoleClaims] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [RoleId] nvarchar(450) NOT NULL,
                        [ClaimType] nvarchar(max) NULL,
                        [ClaimValue] nvarchar(max) NULL,
                        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_AspNetRoleClaims_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Identity].[Role] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [Identity].[AspNetRoleClaims] ([RoleId]);
                END
            ");

            // Проверка существования таблицы TemplateQuestions перед созданием
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[TemplateQuestions]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Identity].[TemplateQuestions] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [SurveyTemplateId] int NOT NULL,
                        [Order] int NOT NULL,
                        [Text] nvarchar(max) NOT NULL,
                        [Type] int NOT NULL,
                        CONSTRAINT [PK_TemplateQuestions] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_TemplateQuestions_SurveyTemplates_SurveyTemplateId] FOREIGN KEY ([SurveyTemplateId]) REFERENCES [Identity].[SurveyTemplates] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_TemplateQuestions_SurveyTemplateId] ON [Identity].[TemplateQuestions] ([SurveyTemplateId]);
                END
            ");

            // Проверка существования таблицы AspNetUserClaims перед созданием
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[AspNetUserClaims]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Identity].[AspNetUserClaims] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [UserId] nvarchar(450) NOT NULL,
                        [ClaimType] nvarchar(max) NULL,
                        [ClaimValue] nvarchar(max) NULL,
                        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_AspNetUserClaims_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [Identity].[User] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [Identity].[AspNetUserClaims] ([UserId]);
                END
            ");

            // Проверка существования таблицы AspNetUserLogins перед созданием
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[AspNetUserLogins]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Identity].[AspNetUserLogins] (
                        [LoginProvider] nvarchar(450) NOT NULL,
                        [ProviderKey] nvarchar(450) NOT NULL,
                        [ProviderDisplayName] nvarchar(max) NULL,
                        [UserId] nvarchar(450) NOT NULL,
                        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
                        CONSTRAINT [FK_AspNetUserLogins_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [Identity].[User] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [Identity].[AspNetUserLogins] ([UserId]);
                END
            ");

            // Проверка существования таблицы AspNetUserTokens перед созданием
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[AspNetUserTokens]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Identity].[AspNetUserTokens] (
                        [UserId] nvarchar(450) NOT NULL,
                        [LoginProvider] nvarchar(450) NOT NULL,
                        [Name] nvarchar(450) NOT NULL,
                        [Value] nvarchar(max) NULL,
                        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
                        CONSTRAINT [FK_AspNetUserTokens_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [Identity].[User] ([Id]) ON DELETE CASCADE
                    );
                END
            ");

            // Проверка существования таблицы Surveys перед созданием
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[Surveys]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Identity].[Surveys] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [Title] nvarchar(max) NOT NULL,
                        [Description] nvarchar(max) NOT NULL,
                        [Type] int NOT NULL,
                        [IsPublished] bit NOT NULL,
                        [IsAnonymous] bit NOT NULL,
                        [CreatedDate] datetime2 NOT NULL,
                        [EndDate] datetime2 NULL,
                        [CreatorId] nvarchar(450) NOT NULL,
                        CONSTRAINT [PK_Surveys] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_Surveys_User_CreatorId] FOREIGN KEY ([CreatorId]) REFERENCES [Identity].[User] ([Id]) ON DELETE NO ACTION
                    );
                    CREATE INDEX [IX_Surveys_CreatorId] ON [Identity].[Surveys] ([CreatorId]);
                END
            ");

            // Проверка существования таблицы UserRoles перед созданием
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[UserRoles]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Identity].[UserRoles] (
                        [UserId] nvarchar(450) NOT NULL,
                        [RoleId] nvarchar(450) NOT NULL,
                        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
                        CONSTRAINT [FK_UserRoles_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Identity].[Role] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_UserRoles_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [Identity].[User] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_UserRoles_RoleId] ON [Identity].[UserRoles] ([RoleId]);
                END
            ");

            // Проверка существования таблицы TemplateAnswerOptions перед созданием
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[TemplateAnswerOptions]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Identity].[TemplateAnswerOptions] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [QuestionId] int NOT NULL,
                        [Text] nvarchar(max) NOT NULL,
                        [Order] int NOT NULL,
                        CONSTRAINT [PK_TemplateAnswerOptions] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_TemplateAnswerOptions_TemplateQuestions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Identity].[TemplateQuestions] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_TemplateAnswerOptions_QuestionId] ON [Identity].[TemplateAnswerOptions] ([QuestionId]);
                END
            ");

            // Проверка существования таблицы Questions перед созданием
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[Questions]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Identity].[Questions] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [SurveyId] int NOT NULL,
                        [Text] nvarchar(max) NOT NULL,
                        [Type] int NOT NULL,
                        [IsRequired] bit NOT NULL,
                        [Order] int NOT NULL,
                        CONSTRAINT [PK_Questions] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_Questions_Surveys_SurveyId] FOREIGN KEY ([SurveyId]) REFERENCES [Identity].[Surveys] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_Questions_SurveyId] ON [Identity].[Questions] ([SurveyId]);
                END
            ");

            // Проверка существования таблицы Sections перед созданием
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[Sections]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Identity].[Sections] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [SurveyId] int NOT NULL,
                        [Title] nvarchar(200) NOT NULL,
                        [Description] nvarchar(1000) NULL,
                        [Order] int NOT NULL,
                        CONSTRAINT [PK_Sections] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_Sections_Surveys_SurveyId] FOREIGN KEY ([SurveyId]) REFERENCES [Identity].[Surveys] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_Sections_SurveyId] ON [Identity].[Sections] ([SurveyId]);
                END
            ");

            // Проверка существования таблицы SurveyResponses перед созданием
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[SurveyResponses]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Identity].[SurveyResponses] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [SurveyId] int NOT NULL,
                        [SubmissionDate] datetime2 NOT NULL,
                        [UserId] nvarchar(450) NULL,
                        CONSTRAINT [PK_SurveyResponses] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_SurveyResponses_Surveys_SurveyId] FOREIGN KEY ([SurveyId]) REFERENCES [Identity].[Surveys] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_SurveyResponses_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [Identity].[User] ([Id])
                    );
                    CREATE INDEX [IX_SurveyResponses_SurveyId] ON [Identity].[SurveyResponses] ([SurveyId]);
                    CREATE INDEX [IX_SurveyResponses_UserId] ON [Identity].[SurveyResponses] ([UserId]);
                END
            ");

            // Проверка существования таблицы AnswerOptions перед созданием
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[AnswerOptions]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Identity].[AnswerOptions] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [QuestionId] int NOT NULL,
                        [Text] nvarchar(max) NOT NULL,
                        [Order] int NOT NULL,
                        CONSTRAINT [PK_AnswerOptions] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_AnswerOptions_Questions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Identity].[Questions] ([Id]) ON DELETE CASCADE
                    );
                    CREATE INDEX [IX_AnswerOptions_QuestionId] ON [Identity].[AnswerOptions] ([QuestionId]);
                END
            ");

            // Проверка существования таблицы Media перед созданием
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[Media]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Identity].[Media] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [SurveyId] int NOT NULL,
                        [Type] int NOT NULL,
                        [Url] nvarchar(500) NOT NULL,
                        [Title] nvarchar(200) NULL,
                        [Description] nvarchar(1000) NULL,
                        [Order] int NOT NULL,
                        [QuestionId] int NULL,
                        CONSTRAINT [PK_Media] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_Media_Questions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Identity].[Questions] ([Id]) ON DELETE SET NULL,
                        CONSTRAINT [FK_Media_Surveys_SurveyId] FOREIGN KEY ([SurveyId]) REFERENCES [Identity].[Surveys] ([Id]) ON DELETE NO ACTION
                    );
                    CREATE INDEX [IX_Media_QuestionId] ON [Identity].[Media] ([QuestionId]);
                    CREATE INDEX [IX_Media_SurveyId] ON [Identity].[Media] ([SurveyId]);
                END
            ");

            // Проверка существования таблицы Answer перед созданием
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[Answer]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Identity].[Answer] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [ResponseId] int NOT NULL,
                        [QuestionId] int NOT NULL,
                        [Value] nvarchar(max) NOT NULL,
                        CONSTRAINT [PK_Answer] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_Answer_Questions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Identity].[Questions] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_Answer_SurveyResponses_ResponseId] FOREIGN KEY ([ResponseId]) REFERENCES [Identity].[SurveyResponses] ([Id]) ON DELETE NO ACTION
                    );
                    CREATE INDEX [IX_Answer_QuestionId] ON [Identity].[Answer] ([QuestionId]);
                    CREATE INDEX [IX_Answer_ResponseId] ON [Identity].[Answer] ([ResponseId]);
                END
            ");

            // Проверка существования таблицы UserAnswers перед созданием
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[UserAnswers]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [Identity].[UserAnswers] (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [ResponseId] int NOT NULL,
                        [QuestionId] int NOT NULL,
                        [TextAnswer] nvarchar(max) NULL,
                        [SelectedOptionId] int NULL,
                        CONSTRAINT [PK_UserAnswers] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_UserAnswers_AnswerOptions_SelectedOptionId] FOREIGN KEY ([SelectedOptionId]) REFERENCES [Identity].[AnswerOptions] ([Id]),
                        CONSTRAINT [FK_UserAnswers_Questions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Identity].[Questions] ([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_UserAnswers_SurveyResponses_ResponseId] FOREIGN KEY ([ResponseId]) REFERENCES [Identity].[SurveyResponses] ([Id]) ON DELETE NO ACTION
                    );
                    CREATE INDEX [IX_UserAnswers_QuestionId] ON [Identity].[UserAnswers] ([QuestionId]);
                    CREATE INDEX [IX_UserAnswers_ResponseId] ON [Identity].[UserAnswers] ([ResponseId]);
                    CREATE INDEX [IX_UserAnswers_SelectedOptionId] ON [Identity].[UserAnswers] ([SelectedOptionId]);
                END
            ");

            // Исправление колонки TextAnswer, если таблица уже существует и колонка не допускает NULL
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Identity].[UserAnswers]') AND type in (N'U'))
                BEGIN
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Identity].[UserAnswers]') AND name = 'TextAnswer' AND is_nullable = 0)
                    BEGIN
                        ALTER TABLE [Identity].[UserAnswers] ALTER COLUMN [TextAnswer] nvarchar(max) NULL;
                    END
                END
            ");

            // Все индексы уже созданы в SQL-коде выше при создании таблиц с проверкой существования
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answer",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "Media",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "Sections",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "TemplateAnswerOptions",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserAnswers",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "TemplateQuestions",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "AnswerOptions",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "SurveyResponses",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "Role",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "SurveyTemplates",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "Questions",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "Surveys",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "User",
                schema: "Identity");
        }
    }
}
