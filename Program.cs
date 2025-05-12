using Microsoft.EntityFrameworkCore;
using Pastar.Data;
using Pastar.Helpers;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Добавление контроллеров и представлений
builder.Services.AddControllersWithViews();

// Подключение PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Host=localhost;Port=5432;Database=Pastar;Username=postgres;Password=13092005";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Добавление поддержки сессий
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Регистрация Telegram-бота
builder.Services.AddSingleton<TelegramBot>(provider =>
{
    var botToken = builder.Configuration["TelegramBot:Token"]
                   ?? "7868892437:AAHy9H4FBUxL7vaz5VXneZ2dHyZBRxrumRE";
    return new TelegramBot(botToken);
});

// Сервис для работы с ботом
builder.Services.AddSingleton<TelegramBotService>();

// Фоновая служба для запуска бота
builder.Services.AddHostedService<TelegramBotBackgroundService>();

// Построение приложения
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Запуск приложения
app.Run();