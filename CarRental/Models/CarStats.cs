// Models/CarStats.cs
using System.Text.RegularExpressions;

namespace CarRental.Models
{
    public class CarStats
    {
        public int CarId { get; set; }
        public string CarInfo { get; set; } // Марка, модель, год
        public int RentalDays { get; set; } // Общее количество дней аренды
        public int RentalCount { get; set; } // Количество аренд
        public decimal TotalRevenue { get; set; } // Общий доход

        public string CleanCarInfo =>
    string.IsNullOrEmpty(CarInfo)
        ? "Неизвестный автомобиль"
        : Regex.Replace(CarInfo, @"[^\w\sа-яА-ЯёЁ()-]", "");

    }
}