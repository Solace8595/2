using Microsoft.AspNetCore.Mvc;
using Pastar.Data;
using Pastar.Models;
using Pastar.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Pastar.Helpers;

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
            if (cart.ContainsKey(boxId))
            {
                cart[boxId]++;
            }
            else
            {
                cart[boxId] = 1;
            }
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

        [HttpPost]
        public IActionResult UpdateQuantity([FromBody] CartUpdateModel model)
        {
            var cart = HttpContext.Session.GetObjectFromJson<Dictionary<long, int>>("Cart") ?? new Dictionary<long, int>();
            var box = _context.Boxes.FirstOrDefault(b => b.BoxId == model.BoxId);

            if (box == null)
                return Json(new { error = "Бокс не найден" });

            if (cart.ContainsKey(model.BoxId))
            {
                cart[model.BoxId] = Math.Max(1, cart[model.BoxId] + model.QuantityChange);
            }
            else
            {
                cart[model.BoxId] = 1;
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            var boxIds = cart.Keys.ToList();
            var boxesInCart = _context.Boxes
                .Where(b => boxIds.Contains(b.BoxId))
                .ToDictionary(b => b.BoxId);

            var total = cart.Sum(c =>
            {
                if (boxesInCart.TryGetValue(c.Key, out var currentBox))
                    return currentBox.BoxPrice * c.Value;
                return 0;
            });

            return Json(new
            {
                newQuantity = cart[model.BoxId],
                newTotal = total
            });
        }

        [HttpPost]
        public IActionResult RemoveFromCart(long boxId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<Dictionary<long, int>>("Cart") ?? new Dictionary<long, int>();

            if (cart.ContainsKey(boxId))
            {
                cart.Remove(boxId);
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            // Пересчитываем общую стоимость после удаления
            decimal total = 0;
            foreach (var cartItem in cart)
            {
                var box = _context.Boxes.FirstOrDefault(b => b.BoxId == cartItem.Key);
                if (box != null)
                {
                    total += (decimal)box.BoxPrice * (decimal)cartItem.Value;
                }
            }

            return Json(new { total });
        }
        [HttpGet]
        public IActionResult RefreshAntiforgeryToken()
        {
            return PartialView("_AntiForgeryToken");
        }

        public class CartUpdateModel
        {
            public long BoxId { get; set; }
            public int QuantityChange { get; set; }
        }

        public IActionResult ViewCart()
        {
            var cartItems = GetCartItemsFromSession();
            return View(cartItems);
        }

        [HttpGet]
        public IActionResult GetCartItems()
        {
            var cartItems = GetCartItemsFromSession();

            var result = cartItems.Select(ci => new
            {
                BoxId = ci.Box.BoxId,
                Name = ci.Box.BoxName,
                Price = ci.Box.BoxPrice,
                Quantity = ci.Quantity,
                Total = ci.Box.BoxPrice * ci.Quantity
            });

            return Json(result);
        }

        private List<CartItem> GetCartItemsFromSession()
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

            return cartItems;
        }

        public class CartItem
        {
            public Box Box { get; set; }
            public int Quantity { get; set; }
        }
    }
}
