using Microsoft.EntityFrameworkCore;
using CarRental.Models;

namespace CarRental.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Rental> Rentals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Дополнительная конфигурация модели
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.PassportNumber)
                .HasMaxLength(9)
                .HasColumnName("PassportNumber");
            });

            modelBuilder.Entity<Car>(entity =>
            {
                entity.Property(e => e.DailyPrice)
                .HasColumnType("decimal")
                .HasPrecision(10, 2);
            });
        }
    }
}