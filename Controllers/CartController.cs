using Microsoft.AspNetCore.Mvc;
using Pastar.Data;
using Pastar.Helpers;
using Pastar.Models;
using Pastar.ViewModels;
using System.Linq;

namespace Pastar.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [ResponseCache(NoStore = true)]
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<Dictionary<long, int>>("Cart") ?? new Dictionary<long, int>();

            if (cart.Count == 0)
            {
                ViewBag.IsEmpty = true;
                ViewBag.ConnectionMethods = _context.WayOfConnections.ToList();
                return View(new List<BoxViewModel>());
            }

            var boxIds = cart.Keys.ToList();
            var boxesInCart = _context.Boxes
                .Where(b => boxIds.Contains(b.BoxId))
                .ToDictionary(b => b.BoxId);

            var boxes = cart.Select(c =>
            {
                var boxId = c.Key;
                var quantity = c.Value;

                if (!boxesInCart.TryGetValue(boxId, out var box))
                    return null;

                return new BoxViewModel
                {
                    BoxId = box.BoxId,
                    BoxName = box.BoxName,
                    BoxPrice = box.BoxPrice,
                    BoxDescription = box.BoxDescription,
                    Quantity = quantity
                };
            })
            .Where(vm => vm != null)
            .ToList();

            ViewBag.IsEmpty = false;
            ViewBag.ConnectionMethods = _context.WayOfConnections.ToList();
            return View(boxes);
        }

        [HttpPost]
        public IActionResult AddToCart(long boxId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<Dictionary<long, int>>("Cart") ?? new Dictionary<long, int>();

            if (cart.ContainsKey(boxId))
                cart[boxId]++;
            else
                cart[boxId] = 1;

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return Ok(new { success = true });
        }

        [HttpPost]
        public IActionResult UpdateQuantity(long boxId, int quantityChange)
        {
            var cart = HttpContext.Session.GetObjectFromJson<Dictionary<long, int>>("Cart") ?? new Dictionary<long, int>();
            var box = _context.Boxes.FirstOrDefault(b => b.BoxId == boxId);

            if (box == null)
                return Json(new { error = "Бокс не найден" });

            if (cart.ContainsKey(boxId))
            {
                cart[boxId] += quantityChange;
                if (cart[boxId] <= 0)
                {
                    cart.Remove(boxId);
                }
            }
            else
            {
                cart[boxId] = 1;
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            // Подсчёт суммы
            var boxIds = cart.Keys.ToList();
            var boxesInCart = _context.Boxes
                .Where(b => boxIds.Contains(b.BoxId))
                .ToDictionary(b => b.BoxId);

            var total = cart.Sum(c => boxesInCart.TryGetValue(c.Key, out var currentBox)
                                        ? currentBox.BoxPrice * c.Value
                                        : 0);

            return Json(new
            {
                newQuantity = cart.TryGetValue(boxId, out var qty) ? qty : 0,
                newTotal = total
            });
        }

        [HttpPost]
        public IActionResult RemoveFromCart(long boxId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<Dictionary<long, int>>("Cart") ?? new Dictionary<long, int>();

            if (cart.Remove(boxId))
            {
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            return Ok(new { success = true });
        }
    }
}
