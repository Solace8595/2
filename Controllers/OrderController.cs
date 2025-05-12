using Microsoft.AspNetCore.Mvc;
using Pastar.Data;
using Pastar.Models;
using Pastar.ViewModels;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Linq;
using Pastar.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Pastar.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TelegramBotService _telegramBotService;

        public OrderController(ApplicationDbContext context, TelegramBotService telegramBotService)
        {
            _context = context;
            _telegramBotService = telegramBotService;
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderViewModel model)
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

            var connectionMethodName = model.ContactMethod?.Trim();

            var connectionMethod = await _context.WayOfConnections
                .FirstOrDefaultAsync(w => w.ConnectionMethod.Trim().ToLower() == connectionMethodName.ToLower());

            if (connectionMethod == null)
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
                ConnectionMethodId = connectionMethod.Id,
                PromocodeId = promoCodeId,
                CreatedAt = DateTime.UtcNow
            };

            var boxIds = cart.Keys.ToList();
            var boxes = await _context.Boxes
                .Where(b => boxIds.Contains(b.BoxId))
                .ToDictionaryAsync(b => b.BoxId);

            foreach (var item in cart)
            {
                if (boxes.TryGetValue(item.Key, out var box))
                {
                    order.Items.Add(new OrderItem
                    {
                        BoxId = item.Key,
                        Quantity = item.Value,
                        Box = box 
                    });
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await SendOrderNotificationAsync(order, cart, boxes);

            HttpContext.Session.Remove("Cart");

            return Ok(new { message = "Заказ успешно оформлен!" });
        }
        private async Task SendOrderNotificationAsync(Order order, Dictionary<long, int> cart, Dictionary<long, Box> boxes)
        {
            var message = $"<b>Новый заказ!</b>\n\n" +
                          $"<b>Имя:</b> {order.FirstName} {order.LastName}\n" +
                          $"<b>Телефон:</b> {order.CustomerPhone}\n" +
                          $"<b>Способ связи:</b> {order.ConnectionMethod?.ConnectionMethod ?? "Не указан"}\n" +
                          $"<b>Дата:</b> {order.CreatedAt:dd.MM.yyyy HH:mm}\n\n" +
                          "<b>Товары:</b>\n";

            decimal total = 0;

            foreach (var item in cart)
            {
                if (boxes.TryGetValue(item.Key, out var box))
                {
                    decimal price = (decimal)box.BoxPrice;
                    int quantity = item.Value;
                    decimal itemTotal = price * quantity;
                    total += itemTotal;

                    message += $"• {box.BoxName} x {quantity} — {itemTotal:F2} ₽\n";
                }
            }

            message += $"\n<b>Итого:</b> {total:F2} ₽";

            await _telegramBotService.NotifyAdminAsync(message);
        }
    }
}