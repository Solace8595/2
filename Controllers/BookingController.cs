using Microsoft.AspNetCore.Mvc;
using Pastar.Models;
using Pastar.ViewModels;
using Microsoft.EntityFrameworkCore;
using Pastar.Data;
using Pastar.Helpers;

public class BookingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly TelegramBotService _telegramBotService;

    public BookingController(ApplicationDbContext context, TelegramBotService telegramBotService)
    {
        _context = context;
        _telegramBotService = telegramBotService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.ConnectionMethods = _context.WayOfConnections.ToList();
        return View(new BookingViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(BookingViewModel model)
    {
        ViewBag.ConnectionMethods = _context.WayOfConnections.ToList();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            DateTime bookingTime = model.BookingDateTime;
            if (bookingTime.Kind == DateTimeKind.Unspecified)
            {
                bookingTime = DateTime.SpecifyKind(bookingTime, DateTimeKind.Local);
            }
            bookingTime = TimeZoneInfo.ConvertTimeToUtc(bookingTime);

            var method = _context.WayOfConnections.Find(model.ConnectionMethodId);
            string methodName = method?.ConnectionMethod ?? "Не указан";

            var booking = new BookTable
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                ContactPhone = model.ContactPhone,
                ConnectionMethodId = model.ConnectionMethodId,
                BookingDateTime = bookingTime,
                NumberOfPeople = model.NumberOfPeople
            };

            _context.BookTables.Add(booking);
            await _context.SaveChangesAsync();

            string message = $@"
<b>Новое бронирование:</b>
Имя: {model.FirstName} {model.LastName}
Телефон: {model.ContactPhone}
Способ связи: {methodName}
Дата и время: {bookingTime:dd.MM.yyyy HH:mm} (UTC)
Количество человек: {model.NumberOfPeople}";

            await _telegramBotService.NotifyAdminAsync(message);

            ViewBag.Success = "Бронирование успешно оформлено!";
            return View(new BookingViewModel());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при сохранении бронирования: {ex.Message}");
            ViewBag.Error = "Произошла ошибка при оформлении бронирования.";
            return View(model);
        }
    }
}