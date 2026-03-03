using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;
using System.Linq;

namespace CarRental.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly AppDbContext _context;

        public StatisticsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Customers(int top = 5, string sortBy = "count")
        {
            // Загружаем все данные в память для работы с SQLite
            var rentals = _context.Rentals
                .Include(r => r.Customer)
                .Where(r => r.Customer != null)
                .ToList();

            // Группируем и считаем статистику с проверкой на null
            var customerStats = rentals
                .Where(r => r.Customer != null)
                .GroupBy(r => r.Customer!)
                .Select(g => new CustomerStats
                {
                    CustomerId = g.Key.Id,
                    FullName = g.Key.FullName ?? "Неизвестный клиент",
                    RentalsCount = g.Count(),
                    TotalSpent = g.Sum(r => r.TotalPrice)
                })
                .ToList();

            // Сортировка
            var sortedStats = sortBy == "revenue" 
                ? customerStats.OrderByDescending(c => c.TotalSpent)
                : customerStats.OrderByDescending(c => c.RentalsCount);

            // Ограничение количества
            var stats = (top == 0 ? sortedStats : sortedStats.Take(top)).ToList();

            ViewBag.TopCount = top;
            ViewBag.SortBy = sortBy;
            return View(stats);
        }

        public IActionResult Cars(int top = 5, string sortBy = "count")
        {
            // Загружаем все данные в память для работы с SQLite
            var rentals = _context.Rentals
                .Include(r => r.Car)
                .Where(r => r.Car != null)
                .ToList();

            // Группируем и считаем статистику с проверкой на null
            var carStats = rentals
                .Where(r => r.Car != null)
                .GroupBy(r => new 
                { 
                    r.Car!.Id, 
                    Brand = r.Car.Brand ?? "Неизвестная марка", 
                    Model = r.Car.Model ?? "Неизвестная модель", 
                    Year = r.Car.Year 
                })
                .Select(g => new CarStats
                {
                    CarId = g.Key.Id,
                    CarInfo = $"{g.Key.Brand} {g.Key.Model} ({g.Key.Year})",
                    RentalCount = g.Count(),
                    TotalRevenue = g.Sum(r => r.TotalPrice)
                })
                .ToList();

            // Сортировка
            var sortedStats = sortBy == "revenue"
                ? carStats.OrderByDescending(c => c.TotalRevenue)
                : carStats.OrderByDescending(c => c.RentalCount);

            // Ограничение количества
            var stats = (top == 0 ? sortedStats : sortedStats.Take(top)).ToList();

            ViewBag.TopCount = top;
            ViewBag.SortBy = sortBy;
            return View(stats);
        }
    }
}