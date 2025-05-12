using Microsoft.AspNetCore.Mvc;
using Pastar.Data;
using Pastar.Models;
using Pastar.Helpers;
using System.Linq;
using Pastar.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Pastar.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult AddToCart(long boxId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<Dictionary<long, int>>("Cart") ?? new Dictionary<long, int>();

            // Проверяем, есть ли уже этот бокс в корзине
            if (cart.ContainsKey(boxId))
            {
                // Если есть, увеличиваем количество
                cart[boxId]++;
            }
            else
            {
                // Если нет, добавляем бокс с количеством 1
                cart[boxId] = 1;
            }

            // Сохраняем обновленную корзину в сессии
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return Ok(new { success = true });
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
        // Метод для обновления количества бокса в корзине
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity([FromBody] UpdateCartItemRequest request)
        {
            var cart = HttpContext.Session.GetObjectFromJson<Dictionary<long, int>>("Cart") ?? new Dictionary<long, int>();

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

        // Метод для удаления бокса из корзины
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart([FromBody] UpdateCartItemRequest request)
        {
            var cart = HttpContext.Session.GetObjectFromJson<Dictionary<long, int>>("Cart") ?? new Dictionary<long, int>();

            cart.Remove(request.BoxId);
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return Ok();
        }
        public class CartItem
        {
            public Box Box { get; set; }
            public int Quantity { get; set; }
        }
        public IActionResult ViewCart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<Dictionary<long, int>>("Cart") ?? new Dictionary<long, int>();

            var cartItems = new List<CartItem>();

            foreach (var item in cart)
            {
                var box = _context.Boxes.FirstOrDefault(b => b.BoxId == item.Key);
                if (box != null)
                {
                    cartItems.Add(new CartItem
                    {
                        Box = box,
                        Quantity = item.Value
                    });
                }
            }

            return View(cartItems);
        }

    }
}

