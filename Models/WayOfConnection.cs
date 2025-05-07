namespace Pastar.Models
{
    public class WayOfConnection
    {
        public long Id { get; set; }
        public string ConnectionMethod { get; set; } = null!;

        public ICollection<Order>? Orders { get; set; }
        public BookTable? BookTable { get; set; }
    }

}
