using Microsoft.AspNetCore.Mvc;
using PastaBar.Data;
using Pastar.ViewModels;
using Pastar.Helpers;


public class CartController : Controller
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetObjectFromJson<Dictionary<int, int>>("Cart") ?? new Dictionary<int, int>();

        if (cart.Count == 0)
        {
            ViewBag.IsEmpty = true;
            return View(new List<BoxViewModel>());
        }

        var boxIds = cart.Keys.ToList();
        var boxes = _context.Boxes
            .Where(b => boxIds.Contains((int)b.BoxId))
            .Select(b => new BoxViewModel
            {
                BoxId = b.BoxId,
                BoxName = b.BoxName,
                BoxPrice = b.BoxPrice,
                BoxDescription = b.BoxDescription,
                Quantity = cart.ContainsKey((int)b.BoxId) ? cart[(int)b.BoxId] : 0
            })
            .ToList();

        ViewBag.IsEmpty = false;
        return View(boxes);
    }

    [HttpPost]
    public IActionResult AddToCart(int boxId)
    {
        // Получаем корзину из сессии (или создаём новую, если её нет)
        var cart = HttpContext.Session.GetObjectFromJson<Dictionary<int, int>>("Cart") ?? new Dictionary<int, int>();

        // Увеличиваем количество для данного бокса или добавляем его, если его нет в корзине
        if (cart.ContainsKey(boxId))
            cart[boxId]++;
        else
            cart[boxId] = 1;

        // Сохраняем обновлённую корзину в сессию
        HttpContext.Session.SetObjectAsJson("Cart", cart);

        // Отправляем ответ
        return Ok(); // Просто ответ OK, без редиректа
    }
    [HttpPost]
    public IActionResult UpdateQuantity([FromBody] CartUpdateModel model)
    {
        var cart = HttpContext.Session.GetObjectFromJson<Dictionary<long, int>>("Cart") ?? new();
        if (model.Quantity > 0)
            cart[model.BoxId] = model.Quantity;
        else
            cart.Remove(model.BoxId);
        HttpContext.Session.SetObjectAsJson("Cart", cart);
        return Ok();
    }

    [HttpPost]
    public IActionResult RemoveFromCart([FromBody] CartRemoveModel model)
    {
        var cart = HttpContext.Session.GetObjectFromJson<Dictionary<long, int>>("Cart") ?? new();
        cart.Remove(model.BoxId);
        HttpContext.Session.SetObjectAsJson("Cart", cart);
        return Ok();
    }

    public class CartUpdateModel
    {
        public long BoxId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartRemoveModel
    {
        public long BoxId { get; set; }
    }

}
