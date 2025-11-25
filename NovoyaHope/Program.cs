using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NovoyaHope.Data;
using NovoyaHope.Models;
using NovoyaHope.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using NovoyaHope.Models.DataModels;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Настройка Контекста Базы Данных (EF Core) ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- 2. Настройка Identity (Аутентификация и Авторизация) ---
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Настройки пароля (можно перенести в appsettings.json)
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


// --- 3. Регистрация Пользовательских Сервисов (DI) ---
builder.Services.AddScoped<ISurveyService, SurveyService>();


// --- 4. Настройка MVC с Авторизацией по умолчанию ---
builder.Services.AddControllersWithViews(options =>
{
    // Установка политики: по умолчанию все контроллеры и действия требуют авторизации, 
    // за исключением тех, что помечены [AllowAnonymous].
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});


// --- КОНЕЦ КОНФИГУРАЦИИ СЕРВИСОВ ---
var app = builder.Build();


// --- КОНВЕЙЕР ОБРАБОТКИ ЗАПРОСОВ (Middleware) ---

// 1. Настройка окружения
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 2. Аутентификация и Авторизация
app.UseAuthentication();
app.UseAuthorization();


// --- 3. ИНИЦИАЛИЗАЦИЯ БД (СОЗДАНИЕ РОЛЕЙ И АДМИНА) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Вызов статического инициализатора
        await NovoyaHope.Data.DbInitializer.Initialize(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}


// --- 4. Определение маршрутов MVC ---
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();