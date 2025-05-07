namespace Pastar.Models
{
    public class BookTable
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public long ContactPhone { get; set; }
        public long? ConnectionMethodId { get; set; }
        public DateTime BookingDateTime { get; set; }
        public int NumberOfPeople { get; set; }

        public WayOfConnection? ConnectionMethod { get; set; }
    }
}
