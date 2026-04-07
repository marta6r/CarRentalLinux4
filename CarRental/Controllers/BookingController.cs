using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.Models;
using CarRental.Data;
using Microsoft.AspNetCore.Http;

namespace CarRental.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(int carId, DateTime rentDate, DateTime returnDate)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            var car = await _context.Cars.FindAsync(carId);
            if (car == null || !car.IsAvailable)
            {
                TempData["Error"] = "Автомобиль недоступен";
                return RedirectToAction("Details", "User", new { id = carId });
            }

            // Проверка на пересечение дат
            var isBooked = await _context.Rentals
                .AnyAsync(r => r.CarId == carId &&
                              rentDate < r.ReturnDate &&
                              returnDate > r.RentDate);

            if (isBooked)
            {
                TempData["Error"] = "Автомобиль уже забронирован на выбранные даты";
                return RedirectToAction("Details", "User", new { id = carId });
            }

            var days = (returnDate - rentDate).Days;
            var totalPrice = days * car.DailyPrice;

            var rental = new Rental
            {
                CarId = carId,
                CustomerId = customerId.Value,
                RentDate = rentDate,
                ReturnDate = returnDate,
                TotalPrice = totalPrice
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Автомобиль успешно забронирован!";
            return RedirectToAction("MyBookings", "Profile");
        }
    }
}
