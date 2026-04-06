using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.Models;
using CarRental.Data;

namespace CarRental.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // Список автомобилей
        public async Task<IActionResult> Index()
        {
            var cars = await _context.Cars
                .Include(c => c.Category)
                .Where(c => c.IsAvailable)
                .ToListAsync();

            return View(cars);
        }

        // Детали автомобиля с характеристиками
        public async Task<IActionResult> Details(int id)
        {
            var car = await _context.Cars
                .Include(c => c.Category)
                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.Feature)
                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.FeatureValue)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
                return NotFound();

            return View(car);
        }
    }
}