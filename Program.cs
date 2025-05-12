using Microsoft.EntityFrameworkCore;
using Pastar.Data;
using Pastar.Helpers;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddViewOptions(options =>
    {
        options.HtmlHelperOptions.ClientValidationEnabled = true;
    });

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "XSRF-TOKEN";
    options.Cookie.Name = "XSRF-COOKIE";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Host=localhost;Port=5432;Database=Pastar;Username=postgres;Password=13092005";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton<TelegramBot>(provider =>
{
    var botToken = builder.Configuration["TelegramBot:Token"]
                   ?? "7868892437:AAHy9H4FBUxL7vaz5VXneZ2dHyZBRxrumRE";
    return new TelegramBot(botToken);
});

builder.Services.AddSingleton<TelegramBotService>();

builder.Services.AddHostedService<TelegramBotBackgroundService>();

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

app.Run();