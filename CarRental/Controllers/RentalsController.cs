using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;

namespace CarRental.Controllers
{
    public class RentalsController : Controller
    {
        private readonly AppDbContext _context;

        public RentalsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Rentals
        public async Task<IActionResult> Index(string status = "all")
        {
            var query = _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .AsQueryable();

            ViewBag.CurrentStatus = status;

            switch (status)
            {
                case "active":
                    query = query.Where(r => r.ReturnDate >= DateTime.Now);
                    ViewBag.StatusTitle = "Активные аренды";
                    break;
                case "completed":
                    query = query.Where(r => r.ReturnDate < DateTime.Now);
                    ViewBag.StatusTitle = "Завершенные аренды";
                    break;
                default:
                    ViewBag.StatusTitle = "Все аренды";
                    break;
            }

            // Сортировка по дате начала (новые сверху)
            var rentals = await query.OrderByDescending(r => r.RentDate).ToListAsync();
            return View(rentals);
        }

        // GET: Rentals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // GET: Rentals/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Cars = new SelectList(await _context.Cars
                .Where(c => c.IsAvailable == true)
                .ToListAsync(), "Id", "FullName");
            ViewBag.Customers = new SelectList(await _context.Customers.ToListAsync(), "Id", "FullName");
            return View();
        }

        // POST: Rentals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CarId,CustomerId,RentDate,ReturnDate,TotalPrice")] Rental rental)
        {
            // Отладка
            Console.WriteLine($"=== CREATE METHOD CALLED ===");
            Console.WriteLine($"CarId: {rental.CarId}");
            Console.WriteLine($"CustomerId: {rental.CustomerId}");
            Console.WriteLine($"RentDate: {rental.RentDate}");
            Console.WriteLine($"ReturnDate: {rental.ReturnDate}");

            ModelState.Remove("Car");
            ModelState.Remove("Customer");

            var car = await _context.Cars.FindAsync(rental.CarId);
            if (car == null)
            {
                ModelState.AddModelError("CarId", "Автомобиль не найден");
                await LoadSelectLists();
                return View(rental);
            }

            // Проверка технической доступности
            if (!car.IsAvailable)
            {
                ModelState.AddModelError("", $"Автомобиль {car.Brand} {car.Model} временно недоступен");
                await LoadSelectLists();
                return View(rental);
            }

            // Проверка корректности дат
            if (rental.RentDate == default || rental.ReturnDate == default)
            {
                ModelState.AddModelError("", "Пожалуйста, выберите даты аренды");
                await LoadSelectLists();
                return View(rental);
            }

            if (rental.ReturnDate <= rental.RentDate)
            {
                ModelState.AddModelError("ReturnDate", "Дата возврата должна быть позже даты начала");
                await LoadSelectLists();
                return View(rental);
            }

            // Проверка максимальной продолжительности (365 дней)
            var days = (rental.ReturnDate - rental.RentDate).Days;
            if (days > 365)
            {
                ModelState.AddModelError("ReturnDate", "Максимальная продолжительность аренды не может превышать 365 дней (1 год)");
                await LoadSelectLists();
                return View(rental);
            }

            // Проверка доступности автомобиля на даты
            if (!IsCarAvailable(rental.CarId, rental.RentDate, rental.ReturnDate))
            {
                ModelState.AddModelError("", $"Автомобиль {car.Brand} {car.Model} уже арендован на выбранные даты");
                await LoadSelectLists();
                return View(rental);
            }

            // Расчет стоимости
            rental.TotalPrice = days * car.DailyPrice;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(rental);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Аренда успешно создана!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ошибка при сохранении: " + ex.Message);
                }
            }

            await LoadSelectLists();
            return View(rental);
        }

        // GET: Rentals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null)
            {
                return NotFound();
            }

            ViewBag.AllCars = await _context.Cars.ToListAsync();
            ViewBag.AllCustomers = await _context.Customers.ToListAsync();

            return View(rental);
        }

        // POST: Rentals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CarId,CustomerId,RentDate,ReturnDate,TotalPrice")] Rental rental)
        {
            if (id != rental.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Car");
            ModelState.Remove("Customer");

            var car = await _context.Cars.FindAsync(rental.CarId);
            if (car == null)
            {
                ModelState.AddModelError("CarId", "Автомобиль не найден");
                ViewBag.AllCars = await _context.Cars.ToListAsync();
                ViewBag.AllCustomers = await _context.Customers.ToListAsync();
                return View(rental);
            }

            // Проверка технической доступности
            if (!car.IsAvailable)
            {
                var originalRental = await _context.Rentals.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
                if (originalRental != null && originalRental.CarId == rental.CarId)
                {
                    TempData["Warning"] = $"Внимание: Автомобиль {car.Brand} {car.Model} сейчас отмечен как технически недоступный.";
                }
                else
                {
                    ModelState.AddModelError("", $"Автомобиль {car.Brand} {car.Model} временно недоступен");
                    ViewBag.AllCars = await _context.Cars.ToListAsync();
                    ViewBag.AllCustomers = await _context.Customers.ToListAsync();
                    return View(rental);
                }
            }

            // Проверка корректности дат
            if (rental.RentDate == default || rental.ReturnDate == default)
            {
                ModelState.AddModelError("", "Пожалуйста, выберите даты аренды");
                ViewBag.AllCars = await _context.Cars.ToListAsync();
                ViewBag.AllCustomers = await _context.Customers.ToListAsync();
                return View(rental);
            }

            if (rental.ReturnDate <= rental.RentDate)
            {
                ModelState.AddModelError("ReturnDate", "Дата возврата должна быть позже даты начала");
                ViewBag.AllCars = await _context.Cars.ToListAsync();
                ViewBag.AllCustomers = await _context.Customers.ToListAsync();
                return View(rental);
            }

            // Проверка максимальной продолжительности
            var days = (rental.ReturnDate - rental.RentDate).Days;
            if (days > 365)
            {
                ModelState.AddModelError("ReturnDate", "Максимальная продолжительность аренды не может превышать 365 дней (1 год)");
                ViewBag.AllCars = await _context.Cars.ToListAsync();
                ViewBag.AllCustomers = await _context.Customers.ToListAsync();
                return View(rental);
            }

            // Проверка доступности автомобиля на даты (исключая текущую аренду)
            if (!IsCarAvailable(rental.CarId, rental.RentDate, rental.ReturnDate, rental.Id))
            {
                ModelState.AddModelError("", $"Автомобиль {car.Brand} {car.Model} уже арендован на выбранные даты");
                ViewBag.AllCars = await _context.Cars.ToListAsync();
                ViewBag.AllCustomers = await _context.Customers.ToListAsync();
                return View(rental);
            }

            rental.TotalPrice = days * car.DailyPrice;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rental);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Аренда успешно обновлена!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RentalExists(rental.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ошибка при сохранении: " + ex.Message);
                }
            }

            ViewBag.AllCars = await _context.Cars.ToListAsync();
            ViewBag.AllCustomers = await _context.Customers.ToListAsync();
            rental.Car = await _context.Cars.FindAsync(rental.CarId);
            rental.Customer = await _context.Customers.FindAsync(rental.CustomerId);
            
            return View(rental);
        }

        // GET: Rentals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // POST: Rentals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null)
            {
                _context.Rentals.Remove(rental);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Аренда успешно удалена!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool RentalExists(int id)
        {
            return _context.Rentals.Any(e => e.Id == id);
        }

        private async Task LoadSelectLists()
        {
            ViewBag.Cars = new SelectList(await _context.Cars
                .Where(c => c.IsAvailable == true)
                .ToListAsync(), "Id", "FullName");
            ViewBag.Customers = new SelectList(await _context.Customers.ToListAsync(), "Id", "FullName");
        }

        // AJAX методы
        [HttpGet]
        public async Task<IActionResult> SearchCars(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new List<object>());
            }

            var cars = await _context.Cars
                .Where(c => c.Brand.Contains(term) || c.Model.Contains(term))
                .Select(c => new {
                    id = c.Id,
                    text = $"{c.Brand} {c.Model} ({c.Year})",
                    brand = c.Brand,
                    model = c.Model,
                    year = c.Year,
                    dailyPrice = c.DailyPrice,
                    isAvailable = c.IsAvailable
                })
                .Take(10)
                .ToListAsync();

            return Json(cars);
        }

        [HttpGet]
        public async Task<IActionResult> SearchCustomers(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new List<object>());
            }

            var customers = await _context.Customers
                .Where(c => c.FullName.Contains(term))
                .Select(c => new {
                    id = c.Id,
                    text = $"{c.FullName} ({c.Phone})"
                })
                .Take(10)
                .ToListAsync();

            return Json(customers);
        }

        [HttpGet]
        public async Task<IActionResult> GetBookedDates(int carId)
        {
            var rentals = await _context.Rentals
                .Where(r => r.CarId == carId && r.ReturnDate >= DateTime.Now.Date)
                .Select(r => new { start = r.RentDate, end = r.ReturnDate })
                .ToListAsync();
            
            var bookedDates = new List<string>();
            foreach (var rental in rentals)
            {
                for (var date = rental.start; date <= rental.end; date = date.AddDays(1))
                {
                    bookedDates.Add(date.ToString("yyyy-MM-dd"));
                }
            }
            
            return Json(bookedDates);
        }

        [HttpGet]
        public async Task<IActionResult> CheckCarAvailability(int carId, DateTime rentDate, DateTime returnDate, int? excludeRentalId = null)
        {
            var car = await _context.Cars.FindAsync(carId);
            
            if (car == null || !car.IsAvailable)
            {
                return Json(new { isAvailable = false });
            }
            
            var days = (returnDate - rentDate).Days;
            if (days > 365)
            {
                return Json(new { isAvailable = false });
            }
            
            var isAvailableByDates = !await _context.Rentals
                .Where(r => r.CarId == carId)
                .Where(r => !excludeRentalId.HasValue || r.Id != excludeRentalId.Value)
                .AnyAsync(r => r.RentDate < returnDate && r.ReturnDate > rentDate);
            
            return Json(new { isAvailable = isAvailableByDates });
        }

        private bool IsCarAvailable(int carId, DateTime rentDate, DateTime returnDate, int? excludeRentalId = null)
        {
            var car = _context.Cars.Find(carId);
            if (car == null || !car.IsAvailable) return false;
            
            var days = (returnDate - rentDate).Days;
            if (days > 365) return false;
            
            return !_context.Rentals
                .Where(r => r.CarId == carId)
                .Where(r => !excludeRentalId.HasValue || r.Id != excludeRentalId.Value)
                .Any(r => r.RentDate < returnDate && r.ReturnDate > rentDate);
        }
    }
}