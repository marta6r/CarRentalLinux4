using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRental.Models
{
    [Table("car")]  // указываем имя таблицы в БД
    public class Car
    {
        [Column("id")]  // указываем имя колонки в БД
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле 'Марка' обязательно")]
        [Display(Name = "Марка")]
        [StringLength(100, ErrorMessage = "Максимум 100 символов")]
        [Column("brand")]
        public string Brand { get; set; }

        [Required(ErrorMessage = "Поле 'Модель' обязательно")]
        [Display(Name = "Модель")]
        [StringLength(100, ErrorMessage = "Максимум 100 символов")]
        [Column("model")]
        public string Model { get; set; }

        [Required(ErrorMessage = "Поле 'Год выпуска' обязательно")]
        [Range(1885, 2026, ErrorMessage = "Некорректный год")]
        [Display(Name = "Год выпуска")]
        [Column("year")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Поле 'Цена за день' обязательно")]
        [Range(1, 10000, ErrorMessage = "Цена должна быть от 1 до 10000")]
        [Display(Name = "Цена за день (BYN)")]
        [Column("daily_price")]
        public decimal DailyPrice { get; set; }

        [Display(Name = "Доступен для аренды")]
        [Column("is_available")]
        public bool IsAvailable { get; set; } = true;

        [NotMapped]  // это поле не сохраняется в БД, только для отображения
        public string FullName => $"{Brand} {Model} ({Year}) - {DailyPrice} BYN/день";




        [Column("category_id")]
        [Display(Name = "Категория")]
        public int? CategoryId { get; set; }

        // Навигационное свойство
        public Category? Category { get; set; }


        public ICollection<CarFeature>? CarFeatures { get; set; }

        [Column("image_path")]
        [Display(Name = "Фото автомобиля")]
        public string? ImagePath { get; set; }

        [NotMapped]
        [Display(Name = "Фото автомобиля")]
        public IFormFile? ImageFile { get; set; }
    }
}