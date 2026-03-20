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
        public DbSet<Category> Categories { get; set; }  // ← ДОБАВЛЕНО

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурация для Customer (snake_case)
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("customer");
                
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                    
                entity.Property(e => e.FullName)
                    .HasMaxLength(100)
                    .HasColumnName("full_name");
                    
                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .HasColumnName("email");
                    
                entity.Property(e => e.Phone)
                    .HasMaxLength(20)
                    .HasColumnName("phone");
                    
                entity.Property(e => e.PassportNumber)
                    .HasMaxLength(9)
                    .HasColumnName("passport_number");
            });

            // Конфигурация для Car (snake_case)
            modelBuilder.Entity<Car>(entity =>
            {
                entity.ToTable("car");
                
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                    
                entity.Property(e => e.Brand)
                    .HasMaxLength(100)
                    .HasColumnName("brand");
                    
                entity.Property(e => e.Model)
                    .HasMaxLength(100)
                    .HasColumnName("model");
                    
                entity.Property(e => e.Year)
                    .HasColumnName("year");
                    
                entity.Property(e => e.DailyPrice)
                    .HasColumnType("decimal")
                    .HasColumnName("daily_price");
                    
                entity.Property(e => e.IsAvailable)
                    .HasColumnName("is_available");
                    
                // ДОБАВЛЕНО: CategoryId
                entity.Property(e => e.CategoryId)
                    .HasColumnName("category_id");
                    
                // ДОБАВЛЕНО: связь с Category
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Cars)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Конфигурация для Rental (snake_case)
            modelBuilder.Entity<Rental>(entity =>
            {
                entity.ToTable("rental");
                
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                    
                entity.Property(e => e.CarId)
                    .HasColumnName("car_id");
                    
                entity.Property(e => e.CustomerId)
                    .HasColumnName("customer_id");
                    
                entity.Property(e => e.RentDate)
                    .HasColumnName("rent_date");
                    
                entity.Property(e => e.ReturnDate)
                    .HasColumnName("return_date");
                    
                entity.Property(e => e.TotalPrice)
                    .HasColumnName("total_price");
            });

            // ДОБАВЛЕНО: Конфигурация для Category (snake_case)
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("category");
                
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                    
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");
            });
        }
    }
}