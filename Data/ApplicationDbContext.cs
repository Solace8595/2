using Microsoft.EntityFrameworkCore;
using Pastar.Models;

namespace PastaBar.Data
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Явный маппинг для Box на PostgreSQL-таблицу с snake_case колонками
            modelBuilder.Entity<Box>(b =>
            {
                b.ToTable("boxes");
                b.HasKey(x => x.BoxId);
                b.Property(x => x.BoxId).HasColumnName("box_id");
                b.Property(x => x.BoxName).HasColumnName("box_name");
                b.Property(x => x.BoxPrice).HasColumnName("box_price");
                b.Property(x => x.BoxDescription).HasColumnName("box_description");
            });

            // Кастомные связи
            modelBuilder.Entity<BookTable>()
                .HasOne(b => b.ConnectionMethod)
                .WithOne(w => w.BookTable)
                .HasForeignKey<BookTable>(b => b.ConnectionMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Box)
                .WithMany(b => b.Orders)
                .HasForeignKey(o => o.BoxId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Promocode)
                .WithMany(p => p.Orders)
                .HasForeignKey(o => o.PromocodeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.ConnectionMethod)
                .WithMany(w => w.Orders)
                .HasForeignKey(o => o.ConnectionMethodId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
