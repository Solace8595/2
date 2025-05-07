namespace Pastar.Models
{
    public class Order
    {
        public long Id { get; set; }
        public string LastName { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public long CustomerPhone { get; set; }
        public long? PromocodeId { get; set; }
        public long? ConnectionMethodId { get; set; }
        public string? Comment { get; set; }
        public long BoxId { get; set; }

        public Promocode? Promocode { get; set; }
        public WayOfConnection? ConnectionMethod { get; set; }
        public Box Box { get; set; } = null!;
    }
}
