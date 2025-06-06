﻿namespace Pastar.Models
{
    public class Box
    {
        public long BoxId { get; set; }
        public string BoxName { get; set; } = null!;
        public double BoxPrice { get; set; }
        public string? BoxDescription { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}