// Data/DbInitializer.cs

using Microsoft.AspNetCore.Identity;
using NovoyaHope.Models;
using System.Threading.Tasks;
using System.Linq;
using NovoyaHope.Models.DataModels;

namespace NovoyaHope.Data
{
    public static class DbInitializer
    {
        // Названия ролей, которые мы будем использовать в приложении
        private static readonly string[] Roles = new string[] { "Administrator", "User" };

        public static async Task Initialize(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // 1. Создание ролей
            await EnsureRolesCreated(roleManager);

            // 2. Создание пользователя-администратора
            await EnsureAdminUser(userManager);
        }

        private static async Task EnsureRolesCreated(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task EnsureAdminUser(UserManager<ApplicationUser> userManager)
        {
            // Проверка: Если уже есть пользователь с ролью "Administrator", ничего не делаем
            var adminUsers = await userManager.GetUsersInRoleAsync("Administrator");
            if (adminUsers.Any())
            {
                return;
            }

            // Создание пользователя Администратора по умолчанию
            var adminUser = new ApplicationUser
            {
                UserName = "admin@novoyahope.com",
                Email = "admin@novoyahope.com",
                EmailConfirmed = true // Сразу подтверждаем email
            };

            // ВАЖНО: Пароль для Администратора! Измените его перед деплоем.
            var result = await userManager.CreateAsync(adminUser, "SecureAdmin123!");

            if (result.Succeeded)
            {
                // Назначение роли "Administrator"
                await userManager.AddToRoleAsync(adminUser, "Administrator");
            }
        }
    }
}