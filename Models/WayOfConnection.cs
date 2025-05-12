namespace Pastar.Models
{
    public class WayOfConnection
    {
        public long Id { get; set; }
        public string ConnectionMethod { get; set; } = null!;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<BookTable> BookTables { get; set; } = new List<BookTable>();
    }
}