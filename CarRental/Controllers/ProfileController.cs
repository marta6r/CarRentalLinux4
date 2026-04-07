using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.Models;
using CarRental.Data;
using Microsoft.AspNetCore.Http;

namespace CarRental.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        // Личный кабинет
        public async Task<IActionResult> Index()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            var customer = await _context.Customers.FindAsync(customerId.Value);
            return View(customer);
        }

        // Мои бронирования
        public async Task<IActionResult> MyBookings()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            var bookings = await _context.Rentals
                .Include(r => r.Car)
                .Where(r => r.CustomerId == customerId.Value)
                .OrderByDescending(r => r.RentDate)
                .ToListAsync();

            return View(bookings);
        }

        // История аренд
        public async Task<IActionResult> RentalHistory()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            var history = await _context.Rentals
                .Include(r => r.Car)
                .Where(r => r.CustomerId == customerId.Value && r.ReturnDate < DateTime.Today)
                .OrderByDescending(r => r.ReturnDate)
                .ToListAsync();

            return View(history);
        }
    }
}