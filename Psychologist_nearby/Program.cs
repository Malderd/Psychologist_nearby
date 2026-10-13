using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Psychologist_nearby.Components;
using Psychologist_nearby.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Подключаем Razor Components и Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 2. Настраиваем кастомное состояние авторизации для Blazor
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<TestingService>();

// 3. Настраиваем Cookie-аутентификацию (без Identity)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "PsychologistNearby.Auth";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

// 4. Подключаем DbContext для MySQL (Pomelo)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Не задана строка подключения к базе данных."
    );
}

// Заменяем обычный AddDbContext на AddDbContextFactory:
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Включаем аутентификацию и авторизацию
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Вывод регламента разработки
var reglamentPath = Path.Combine(app.Environment.ContentRootPath, "..", "reglament.txt");
if (File.Exists(reglamentPath))
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("========================================");
    Console.WriteLine("         РЕГЛАМЕНТ РАЗРАБОТКИ            ");
    Console.WriteLine("========================================");
    Console.ResetColor();

    Console.WriteLine(File.ReadAllText(reglamentPath));

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("========================================\n");
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"[Предупреждение]: Файл регламента не найден по пути: {reglamentPath}\n");
    Console.ResetColor();
}

app.Run();