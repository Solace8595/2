using Microsoft.AspNetCore.Mvc;
using Pastar.Data;
using Pastar.Helpers;
using Pastar.ViewModels;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace Pastar.Controllers
{
    public class BoxesController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public BoxesController(IWebHostEnvironment env, ApplicationDbContext context)
        {
            _env = env;
            _context = context;
        }

        // Метод отображения боксов на странице
        public IActionResult Index()
        {
            var boxes = _context.Boxes.ToList();
            var cart = HttpContext.Session.GetObjectFromJson<Dictionary<long, int>>("Cart") ?? new Dictionary<long, int>();

            var result = boxes.Select(box =>
            {
                var vm = new BoxViewModel
                {
                    BoxId = box.BoxId,
                    BoxName = box.BoxName,
                    BoxPrice = box.BoxPrice,
                    BoxDescription = box.BoxDescription,
                    Quantity = cart.ContainsKey(box.BoxId) ? cart[box.BoxId] : 0
                };

                var folderPath = Path.Combine(_env.WebRootPath, "img", box.BoxName);
                if (Directory.Exists(folderPath))
                {
                    vm.ImagePaths = Directory
                        .GetFiles(folderPath, "*.jpg")
                        .Select(f => "/img/" + box.BoxName + "/" + Path.GetFileName(f))
                        .ToList();
                }

                return vm;
            }).ToList();

            return View(result);
        }

        // Метод для добавления бокса в корзину
        [HttpPost]
        public IActionResult AddToCart(long boxId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<Dictionary<long, int>>("Cart") ?? new Dictionary<long, int>();

            // Если бокс уже есть в корзине, увеличиваем его количество, иначе добавляем новый
            if (cart.ContainsKey(boxId))
            {
                cart[boxId]++;
            }
            else
            {
                cart[boxId] = 1;
            }

            // Сохраняем корзину в сессию
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("Index"); // Возвращаем обратно на страницу боксов
        }

        // Метод для удаления бокса из корзины
        [HttpPost]
        public IActionResult RemoveFromCart(long boxId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<Dictionary<long, int>>("Cart") ?? new Dictionary<long, int>();

            // Удаляем бокс из корзины
            cart.Remove(boxId);
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToAction("Index");
        }

        // Метод для обновления количества в корзине
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity([FromBody] UpdateCartItemRequest request)
        {
            var cart = HttpContext.Session.GetObjectFromJson<Dictionary<long, int>>("Cart") ?? new Dictionary<long, int>();

            // Если количество меньше или равно 0, удаляем товар из корзины
            if (request.Quantity <= 0)
            {
                cart.Remove(request.BoxId);
            }
            else
            {
                cart[request.BoxId] = request.Quantity;
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return Ok();
        }
    }
}
