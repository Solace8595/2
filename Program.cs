using Microsoft.EntityFrameworkCore;
using PastaBar.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Регистрируем MVC
builder.Services.AddControllersWithViews();

// 2. Регистрируем DbContext для PostgreSQL
string connectionString = "Host=localhost;Port=5432;Database=Pastar;Username=postgres;Password=13092005";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 3. Регистрируем кэш в памяти для сессий
builder.Services.AddDistributedMemoryCache();

// 4. Регистрируем сессии
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Время жизни сессии
    options.Cookie.HttpOnly = true; // Защита от XSS атак
    options.Cookie.IsEssential = true; // Сессии важны для работы приложения
});

var app = builder.Build();

// 5. Конвейер обработки запросов
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// 6. Подключаем сессии ДО Authorization и MapController
app.UseSession();

// 7. Маршруты и авторизация
app.UseRouting();
app.UseAuthorization();

// 8. Настроим стандартный маршрут
app.MapDefaultControllerRoute();

// 9. Запуск приложения
app.Run();
