using Microsoft.EntityFrameworkCore;
using Pastar.Models;

namespace Pastar.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Box> Boxes { get; set; }
        public DbSet<WayOfConnection> WayOfConnections { get; set; }
        public DbSet<Promocode> Promocodes { get; set; }
        public DbSet<BookTable> BookTables { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Box>(b =>
            {
                b.ToTable("boxes");
                b.HasKey(x => x.BoxId);
                b.Property(x => x.BoxId).HasColumnName("box_id");
                b.Property(x => x.BoxName).HasColumnName("box_name");
                b.Property(x => x.BoxPrice).HasColumnName("box_price");
                b.Property(x => x.BoxDescription).HasColumnName("box_description");

                b.HasMany(x => x.OrderItems)
                 .WithOne(x => x.Box)
                 .HasForeignKey(x => x.BoxId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Order>(o =>
            {
                o.ToTable("orders");
                o.HasKey(x => x.Id);
                o.Property(x => x.Id).HasColumnName("id");
                o.Property(x => x.LastName).HasColumnName("last_name");
                o.Property(x => x.FirstName).HasColumnName("first_name");
                o.Property(x => x.MiddleName).HasColumnName("middle_name");
                o.Property(x => x.CustomerPhone).HasColumnName("customer_phone");
                o.Property(x => x.PromocodeId).HasColumnName("promocode_id");
                o.Property(x => x.ConnectionMethodId).HasColumnName("connection_method_id");
                o.Property(x => x.Comment).HasColumnName("comment");
                o.Property(x => x.CreatedAt).HasColumnName("created_at");

                o.HasOne(x => x.Promocode)
                 .WithMany(x => x.Orders)
                 .HasForeignKey(x => x.PromocodeId)
                 .OnDelete(DeleteBehavior.SetNull);

                o.HasOne(x => x.ConnectionMethod)
                 .WithMany(x => x.Orders)
                 .HasForeignKey(x => x.ConnectionMethodId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<OrderItem>(oi =>
            {
                oi.ToTable("order_items");
                oi.HasKey(x => x.Id);
                oi.Property(x => x.Id).HasColumnName("id");
                oi.Property(x => x.OrderId).HasColumnName("order_id");
                oi.Property(x => x.BoxId).HasColumnName("box_id");
                oi.Property(x => x.Quantity).HasColumnName("quantity");
                oi.HasOne(x => x.Order)
                  .WithMany(x => x.Items)
                  .HasForeignKey(x => x.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Promocode>(p =>
            {
                p.ToTable("promocodes");
                p.HasKey(x => x.Id);
                p.Property(x => x.Id).HasColumnName("id");
                p.Property(x => x.PromocodeName).HasColumnName("promocode_name");
                p.Property(x => x.Description).HasColumnName("description");
            });

            modelBuilder.Entity<WayOfConnection>(w =>
            {
                w.ToTable("way_of_connection");
                w.HasKey(x => x.Id);
                w.Property(x => x.Id).HasColumnName("id");
                w.Property(x => x.ConnectionMethod).HasColumnName("connection_method");
            });

            modelBuilder.Entity<BookTable>(bt =>
            {
                bt.ToTable("book_table");
                bt.HasKey(x => x.Id);
                bt.Property(x => x.Id).HasColumnName("id");
                bt.Property(x => x.FirstName).HasColumnName("first_name");
                bt.Property(x => x.LastName).HasColumnName("last_name");
                bt.Property(x => x.ContactPhone).HasColumnName("contact_phone");
                bt.Property(x => x.ConnectionMethodId).HasColumnName("connection_method_id");
                bt.Property(x => x.BookingDateTime).HasColumnName("booking_datetime");
                bt.Property(x => x.NumberOfPeople).HasColumnName("number_of_people");

                bt.HasOne(x => x.ConnectionMethod)
                  .WithMany(x => x.BookTables)
                  .HasForeignKey(x => x.ConnectionMethodId)
                  .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
