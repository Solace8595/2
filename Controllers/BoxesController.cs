using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PastaBar.Data;
using Pastar.Helpers;
using Pastar.ViewModels;

public class BoxesController : Controller
{
    private readonly IWebHostEnvironment _env;
    private readonly ApplicationDbContext _context;

    public BoxesController(IWebHostEnvironment env, ApplicationDbContext context)
    {
        _env = env;
        _context = context;
    }

    public IActionResult Index()
    {
        var boxes = _context.Boxes.ToList();

        var cart = HttpContext.Session.GetObjectFromJson<Dictionary<int, int>>("Cart") ?? new Dictionary<int, int>();

        var result = boxes.Select(box =>
        {
            var vm = new BoxViewModel
            {
                BoxId = box.BoxId,
                BoxName = box.BoxName,
                BoxPrice = box.BoxPrice,
                BoxDescription = box.BoxDescription,
                Quantity = cart.ContainsKey((int)box.BoxId) ? cart[(int)box.BoxId] : 0 // Загрузка количества товара из корзины
            };

            // Получение путей изображений для каждого бокса
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

    [HttpPost]
    public IActionResult AddToCart(int boxId)
    {
        // Получаем корзину из сессии
        var cart = HttpContext.Session.GetObjectFromJson<Dictionary<int, int>>("Cart") ?? new Dictionary<int, int>();

        // Добавляем товар в корзину или увеличиваем его количество
        if (cart.ContainsKey(boxId))
        {
            cart[boxId]++;
        }
        else
        {
            cart[boxId] = 1;
        }

        // Сохраняем обновленную корзину в сессию
        HttpContext.Session.SetObjectAsJson("Cart", cart);

        // Перенаправляем обратно на страницу с боксов
        return RedirectToAction("Index");
    }


    [HttpPost]
    public IActionResult RemoveFromCart(int boxId)
    {
        var cart = HttpContext.Session.GetObjectFromJson<Dictionary<int, int>>("Cart") ?? new Dictionary<int, int>();

        if (cart.ContainsKey(boxId))
        {
            cart.Remove(boxId);
        }

        HttpContext.Session.SetObjectAsJson("Cart", cart);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateQuantity([FromBody] UpdateCartItemRequest request)
    {
        // Получаем корзину из сессии (или создаем новую, если её нет)
        var cart = HttpContext.Session.GetObjectFromJson<Dictionary<int, int>>("Cart") ?? new Dictionary<int, int>();

        // Обновляем количество в корзине
        if (request.Quantity == 0)
            cart.Remove(request.BoxId);  // Удаляем товар из корзины
        else
            cart[request.BoxId] = request.Quantity;  // Обновляем количество товара

        // Сохраняем обновленную корзину в сессию
        HttpContext.Session.SetObjectAsJson("Cart", cart);

        return Ok();
    }

}
