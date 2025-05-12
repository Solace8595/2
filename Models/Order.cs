namespace Pastar.Models
{
    public class Order
    {
        public long Id { get; set; }
        public string LastName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public string CustomerPhone { get; set; } = null!;
        public long? PromocodeId { get; set; }
        public long? ConnectionMethodId { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public Promocode? Promocode { get; set; }
        public WayOfConnection? ConnectionMethod { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}