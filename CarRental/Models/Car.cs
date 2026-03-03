using System.ComponentModel.DataAnnotations;

namespace CarRental.Models
{
    public class Car
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле 'Марка' обязательно")]
        [Display(Name = "Марка")]
        [StringLength(100, ErrorMessage = "Максимум 100 символов")]
        public string Brand { get; set; }

        [Required(ErrorMessage = "Поле 'Модель' обязательно")]
        [Display(Name = "Модель")]
        [StringLength(100, ErrorMessage = "Максимум 100 символов")]
        public string Model { get; set; }

        [Required(ErrorMessage = "Поле 'Год выпуска' обязательно")]
        [Range(1885, 2025, ErrorMessage = "Некорректный год")]
        [Display(Name = "Год выпуска")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Поле 'Цена за день' обязательно")]
        [Range(1, 10000, ErrorMessage = "Цена должна быть от 1 до 10000")]
        [Display(Name = "Цена за день (BYN)")]
        public decimal DailyPrice { get; set; }

        [Display(Name = "Доступен для аренды")]
        public bool IsAvailable { get; set; } = true;

        public string FullName => $"{Brand} {Model} ({Year}) - {DailyPrice} BYN/день";
    }
}