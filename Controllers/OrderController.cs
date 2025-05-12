using Microsoft.AspNetCore.Mvc;
using Pastar.Data;
using Pastar.Models;
using Pastar.ViewModels;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Linq;

namespace Pastar.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Create([FromBody] OrderViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.FirstName) ||
                string.IsNullOrWhiteSpace(model.LastName) ||
                string.IsNullOrWhiteSpace(model.Phone) ||
                string.IsNullOrWhiteSpace(model.ContactMethod))
            {
                return BadRequest("Обязательные поля не заполнены");
            }

            var phonePattern = @"^\+7 \(\d{3}\) \d{3}-\d{2}-\d{2}$";
            if (!Regex.IsMatch(model.Phone, phonePattern))
            {
                return BadRequest("Некорректный формат номера телефона");
            }

            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
            {
                return BadRequest("Корзина пуста");
            }

            var cart = JsonSerializer.Deserialize<Dictionary<long, int>>(cartJson);
            if (cart == null || !cart.Any())
            {
                return BadRequest("Корзина пуста");
            }

            var connectionMethodId = _context.WayOfConnections
                .Where(w => w.ConnectionMethod == model.ContactMethod)
                .Select(w => (long?)w.Id)
                .FirstOrDefault();

            if (connectionMethodId == null)
            {
                return BadRequest("Выбранный способ связи недоступен");
            }

            long? promoCodeId = null;
            if (!string.IsNullOrEmpty(model.PromoCode))
            {
                promoCodeId = _context.Promocodes
                    .Where(p => p.PromocodeName == model.PromoCode)
                    .Select(p => (long?)p.Id)
                    .FirstOrDefault();
            }

            var order = new Order
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                MiddleName = model.MiddleName,
                CustomerPhone = model.Phone,
                Comment = model.Comment,
                ConnectionMethodId = connectionMethodId,
                PromocodeId = promoCodeId,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var item in cart)
            {
                order.Items.Add(new OrderItem
                {
                    BoxId = item.Key,
                    Quantity = item.Value
                });
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            HttpContext.Session.Remove("Cart");

            return Ok(new { message = "Заказ успешно оформлен!" });
        }
    }
}
