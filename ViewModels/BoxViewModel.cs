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
        public long BoxId { get; set; }  
        public int Quantity { get; set; }  
    }
}
