namespace Pastar.ViewModels
{
    public class BoxViewModel
    {
        public long BoxId { get; set; }
        public string BoxName { get; set; } = null!;
        public double BoxPrice { get; set; }
        public string? BoxDescription { get; set; }

        public List<string> ImagePaths { get; set; } = new();
        public int Quantity { get; set; }
    }
    public class UpdateCartItemRequest
    {
        public int BoxId { get; set; }  // ID бокса
        public int Quantity { get; set; }  // Количество товара
    }
}
