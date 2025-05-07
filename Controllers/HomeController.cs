using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PastaBar.Data;
using Pastar.Models;
using Pastar.ViewModels;
using System.Diagnostics;

namespace Pastar.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Menu()
        {
            return View();
        }
        public IActionResult Boxes()
        {
            var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img");
            var directories = Directory.GetDirectories(rootPath);

            // —начала загружаем все боксы из Ѕƒ
            var dbBoxes = _context.Boxes.ToList(); // _context Ч это твой DbContext

            var boxes = new List<BoxViewModel>();

            foreach (var dir in directories)
            {
                var folderName = Path.GetFileName(dir);

                // »щем в Ѕƒ бокс с таким же именем, как папка
                var matchingBox = dbBoxes.FirstOrDefault(b => b.BoxName == folderName);

                if (matchingBox != null)
                {
                    var images = Directory.GetFiles(dir)
                        .Where(f => f.EndsWith(".jpg") || f.EndsWith(".png"))
                        .Select(f => "/img/" + folderName + "/" + Path.GetFileName(f))
                        .ToList();

                    boxes.Add(new BoxViewModel
                    {
                        BoxId = matchingBox.BoxId,
                        BoxName = matchingBox.BoxName,
                        BoxDescription = matchingBox.BoxDescription,
                        BoxPrice = matchingBox.BoxPrice,
                    });
                }
            }

            return View(boxes);
        }

    }
}
