namespace Pastar.Models
{
    public class Promocode
    {
        public long Id { get; set; }
        public string PromocodeName { get; set; } = null!;
        public string? Description { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}