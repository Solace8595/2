using Microsoft.AspNetCore.Mvc;
using Pastar.Models;
using Pastar.ViewModels;
using Microsoft.EntityFrameworkCore;
using Pastar.Data;

public class BookingController : Controller
{
    private readonly ApplicationDbContext _context;

    public BookingController(ApplicationDbContext context)
    {
        _context = context;
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
            var booking = new BookTable
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                ContactPhone = model.ContactPhone,
                ConnectionMethodId = model.ConnectionMethodId,
                BookingDateTime = model.BookingDateTime,
                NumberOfPeople = model.NumberOfPeople
            };

            _context.BookTables.Add(booking);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Бронирование успешно оформлено!";
            return View(new BookingViewModel()); 
        }
        catch (Exception)
        {
            ViewBag.Error = "Произошла ошибка при оформлении бронирования.";
            return View(model);
        }
    }
}
