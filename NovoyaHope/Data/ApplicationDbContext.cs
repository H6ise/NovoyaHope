using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NovoyaHope.Models;
using NovoyaHope.Models.DataModels;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NovoyaHope.Data
{
    // ApplicationDbContext наследуется от IdentityDbContext для работы с IdentityUser и IdentityRole
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- DbSet'ы для Основных Таблиц Опросов ---
        public DbSet<Survey> Surveys { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }

        // --- DbSet'ы для Ответов Пользователей ---
        public DbSet<SurveyResponse> SurveyResponses { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        // Добавьте DbSet для связующей таблицы множественного выбора, если она используется
        // public DbSet<UserMultipleChoiceAnswer> UserMultipleChoiceAnswers { get; set; }

        // --- DbSet'ы для Шаблонов ---
        public DbSet<SurveyTemplate> SurveyTemplates { get; set; }
        public DbSet<TemplateQuestion> TemplateQuestions { get; set; }
        public DbSet<TemplateAnswerOption> TemplateAnswerOptions { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Переименование таблиц Identity в нижний регистр для чистоты (опционально)
            builder.HasDefaultSchema("Identity");
            builder.Entity<ApplicationUser>(entity => { entity.ToTable(name: "User"); });
            builder.Entity<IdentityRole>(entity => { entity.ToTable(name: "Role"); });
            builder.Entity<IdentityUserRole<string>>(entity => { entity.ToTable("UserRoles"); });
            // ... и другие таблицы Identity

            // --- Настройка Каскадного Удаления ---

            // При удалении Survey, удаляются все связанные Questions и SurveyResponses
            builder.Entity<Survey>()
                .HasMany(s => s.Questions)
                .WithOne(q => q.Survey)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Survey>()
                .HasMany(s => s.Responses)
                .WithOne(r => r.Survey)
                .OnDelete(DeleteBehavior.Cascade);

            // При удалении Question, удаляются все связанные AnswerOptions
            builder.Entity<Question>()
                .HasMany(q => q.AnswerOptions)
                .WithOne(o => o.Question)
                .OnDelete(DeleteBehavior.Cascade);

            // Установка связи Survey.CreatorId (Один-ко-Многим)
            builder.Entity<Survey>()
                .HasOne(s => s.Creator)
                .WithMany(u => u.CreatedSurveys)
                .HasForeignKey(s => s.CreatorId)
                .OnDelete(DeleteBehavior.Restrict); // Не удалять пользователя, если у него есть опросы

            // Настройка составного ключа для связующей таблицы, если используется UserMultipleChoiceAnswer
            // builder.Entity<UserMultipleChoiceAnswer>()
            //     .HasKey(umca => new { umca.UserAnswerId, umca.AnswerOptionId });
        }
    }
}