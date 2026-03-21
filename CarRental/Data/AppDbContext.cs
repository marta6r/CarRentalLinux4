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
        public DbSet<Category> Categories { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<FeatureValue> FeatureValues { get; set; }
        public DbSet<CarFeature> CarFeatures { get; set; }
        public DbSet<CategoryFeature> CategoryFeatures { get; set; }

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
                    
                entity.Property(e => e.CategoryId)
                    .HasColumnName("category_id");
                    
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Cars)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasMany(e => e.CarFeatures)
                    .WithOne(cf => cf.Car)
                    .HasForeignKey(cf => cf.CarId)
                    .OnDelete(DeleteBehavior.Cascade);
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

            // Конфигурация для Category (snake_case)
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("category");
                
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                    
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");
                    
                entity.HasMany(e => e.Cars)
                    .WithOne(c => c.Category)
                    .HasForeignKey(c => c.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasMany(e => e.CategoryFeatures)
                    .WithOne(cf => cf.Category)
                    .HasForeignKey(cf => cf.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация для Feature (snake_case)
            modelBuilder.Entity<Feature>(entity =>
            {
                entity.ToTable("feature");
                
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                    
                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");
                    
                entity.HasMany(e => e.FeatureValues)
                    .WithOne(fv => fv.Feature)
                    .HasForeignKey(fv => fv.FeatureId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasMany(e => e.CarFeatures)
                    .WithOne(cf => cf.Feature)
                    .HasForeignKey(cf => cf.FeatureId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasMany(e => e.CategoryFeatures)
                    .WithOne(cf => cf.Feature)
                    .HasForeignKey(cf => cf.FeatureId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация для FeatureValue (snake_case)
            modelBuilder.Entity<FeatureValue>(entity =>
            {
                entity.ToTable("feature_value");
                
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                    
                entity.Property(e => e.Value)
                    .HasMaxLength(100)
                    .HasColumnName("value");
                    
                entity.Property(e => e.FeatureId)
                    .HasColumnName("feature_id");
                    
                entity.HasOne(e => e.Feature)
                    .WithMany(f => f.FeatureValues)
                    .HasForeignKey(e => e.FeatureId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasMany(e => e.CarFeatures)
                    .WithOne(cf => cf.FeatureValue)
                    .HasForeignKey(cf => cf.FeatureValueId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация для CarFeature (snake_case)
            modelBuilder.Entity<CarFeature>(entity =>
            {
                entity.ToTable("car_feature");
                
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                    
                entity.Property(e => e.CarId)
                    .HasColumnName("car_id");
                    
                entity.Property(e => e.FeatureId)
                    .HasColumnName("feature_id");
                    
                entity.Property(e => e.FeatureValueId)
                    .HasColumnName("feature_value_id");
                    
                entity.HasOne(e => e.Car)
                    .WithMany(c => c.CarFeatures)
                    .HasForeignKey(e => e.CarId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Feature)
                    .WithMany(f => f.CarFeatures)
                    .HasForeignKey(e => e.FeatureId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.FeatureValue)
                    .WithMany(fv => fv.CarFeatures)
                    .HasForeignKey(e => e.FeatureValueId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Уникальный индекс: один автомобиль не может иметь две одинаковые характеристики
                entity.HasIndex(e => new { e.CarId, e.FeatureId })
                    .IsUnique();
            });

            // Конфигурация для CategoryFeature (snake_case)
            modelBuilder.Entity<CategoryFeature>(entity =>
            {
                entity.ToTable("category_feature");
                
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                    
                entity.Property(e => e.CategoryId)
                    .HasColumnName("category_id");
                    
                entity.Property(e => e.FeatureId)
                    .HasColumnName("feature_id");
                    
                entity.Property(e => e.DisplayOrder)
                    .HasColumnName("display_order");
                    
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.CategoryFeatures)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Feature)
                    .WithMany(f => f.CategoryFeatures)
                    .HasForeignKey(e => e.FeatureId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Уникальный индекс: одна категория не может иметь две одинаковые характеристики
                entity.HasIndex(e => new { e.CategoryId, e.FeatureId })
                    .IsUnique();
            });
        }
    }
}