namespace Pastar.Models
{
    public class OrderItem
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long BoxId { get; set; }
        public int Quantity { get; set; }
        public Order Order { get; set; } = null!;
        public Box Box { get; set; } = null!;
    }
}