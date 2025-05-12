using Microsoft.EntityFrameworkCore;
using Pastar.Data;

var builder = WebApplication.CreateBuilder(args);

// Добавляем контроллеры и представления
builder.Services.AddControllersWithViews();

// Подключаем PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Host=localhost;Port=5432;Database=Pastar;Username=postgres;Password=13092005";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Для работы сессий нужен Distributed Cache
builder.Services.AddDistributedMemoryCache();

// Настройка сессий
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Время жизни сессии
    options.Cookie.HttpOnly = true;                // Защита от XSS
    options.Cookie.IsEssential = true;             // Не требует согласия на куки
});

var app = builder.Build();

// Конвейер обработки запросов
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Обслуживание статических файлов (например, CSS, JS)
app.UseHttpsRedirection();
app.UseStaticFiles();

// Важно: UseSession должен быть до UseRouting и UseAuthorization
app.UseSession();

app.UseRouting();

app.UseAuthorization();

// Регистрация маршрутов
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Запуск приложения
app.Run();