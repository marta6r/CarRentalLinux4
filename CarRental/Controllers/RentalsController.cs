// using System;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Rendering;
// using Microsoft.EntityFrameworkCore;
// using CarRental.Data;
// using CarRental.Models;

// namespace CarRental.Controllers
// {
//     public class RentalsController : Controller
//     {
//         private readonly AppDbContext _context;

//         public RentalsController(AppDbContext context)
//         {
//             _context = context;
//         }

//         // GET: Rentals
//         public async Task<IActionResult> Index()
//         {
//             var rentals = await _context.Rentals
//                 .Include(r => r.Car)
//                 .Include(r => r.Customer)
//                 .ToListAsync();
//             return View(rentals);
//         }

//         // GET: Rentals/Details/5
//         public async Task<IActionResult> Details(int? id)
//         {
//             if (id == null)
//             {
//                 return NotFound();
//             }

//             var rental = await _context.Rentals
//                 .Include(r => r.Car)
//                 .Include(r => r.Customer)
//                 .FirstOrDefaultAsync(m => m.Id == id);

//             if (rental == null)
//             {
//                 return NotFound();
//             }

//             return View(rental);
//         }

//         // GET: Rentals/Create
//         public IActionResult Create()
//         {
//             ViewData["CarId"] = new SelectList(_context.Cars, "Id", "Id");
//             ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id");
//             return View();
//         }

//         // POST: Rentals/Create
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Create([Bind("Id,CarId,CustomerId,RentDate,ReturnDate,TotalPrice")] Rental rental)
//         {
//             // Проверка доступности автомобиля
//             if (!IsCarAvailable(rental.CarId, rental.RentDate, rental.ReturnDate))
//             {
//                 ModelState.AddModelError("", "Этот автомобиль уже арендован на выбранные даты");
//                 ViewData["CarId"] = new SelectList(_context.Cars, "Id", "Id", rental.CarId);
//                 ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id", rental.CustomerId);
//                 return View(rental);
//             }

//             if (ModelState.IsValid)
//             {
//                 // Получаем автомобиль для расчета стоимости
//                 var car = await _context.Cars.FindAsync(rental.CarId);
//                 if (car == null)
//                 {
//                     ModelState.AddModelError("CarId", "Автомобиль не найден");
//                     return View(rental);
//                 }

//                 // Проверяем даты
//                 if (rental.ReturnDate <= rental.RentDate)
//                 {
//                     ModelState.AddModelError("ReturnDate", "Дата возврата должна быть позже даты начала");
//                     return View(rental);
//                 }

//                 // Рассчитываем количество дней и стоимость
//                 var days = (rental.ReturnDate - rental.RentDate).Days;
//                 rental.TotalPrice = days * car.DailyPrice;

//                 _context.Add(rental);
//                 await _context.SaveChangesAsync();
//                 return RedirectToAction(nameof(Index));
//             }

//             ViewData["CarId"] = new SelectList(_context.Cars, "Id", "Id", rental.CarId);
//             ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id", rental.CustomerId);
//             return View(rental);
//         }

//         // GET: Rentals/Edit/5
//         public async Task<IActionResult> Edit(int? id)
//         {
//             if (id == null)
//             {
//                 return NotFound();
//             }

//             var rental = await _context.Rentals
//                 .Include(r => r.Car)
//                 .Include(r => r.Customer)
//                 .FirstOrDefaultAsync(r => r.Id == id);

//             if (rental == null)
//             {
//                 return NotFound();
//             }

//             // Добавляем список всех автомобилей в ViewBag
//             ViewBag.AllCars = await _context.Cars.ToListAsync();
//             ViewBag.AllCustomers = await _context.Customers.ToListAsync();

//             return View(rental);
//         }

//         // POST: Rentals/Edit/5
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Edit(int id, [Bind("Id,CarId,CustomerId,RentDate,ReturnDate,TotalPrice")] Rental rental)
//         {
//             if (id != rental.Id)
//             {
//                 return NotFound();
//             }

//             // Проверка доступности автомобиля (исключая текущую аренду)
//             if (!IsCarAvailable(rental.CarId, rental.RentDate, rental.ReturnDate, rental.Id))
//             {
//                 ModelState.AddModelError("", "Этот автомобиль уже арендован на выбранные даты");
//                 ViewBag.AllCars = await _context.Cars.ToListAsync();
//                 ViewBag.AllCustomers = await _context.Customers.ToListAsync();
//                 return View(rental);
//             }

//             if (ModelState.IsValid)
//             {
//                 try
//                 {
//                     // Пересчитываем стоимость при редактировании
//                     var car = await _context.Cars.FindAsync(rental.CarId);
//                     if (car != null)
//                     {
//                         var days = (rental.ReturnDate - rental.RentDate).Days;
//                         rental.TotalPrice = days * car.DailyPrice;
//                     }

//                     _context.Update(rental);
//                     await _context.SaveChangesAsync();
//                 }
//                 catch (DbUpdateConcurrencyException)
//                 {
//                     if (!RentalExists(rental.Id))
//                     {
//                         return NotFound();
//                     }
//                     else
//                     {
//                         throw;
//                     }
//                 }
//                 return RedirectToAction(nameof(Index));
//             }

//             ViewBag.AllCars = await _context.Cars.ToListAsync();
//             ViewBag.AllCustomers = await _context.Customers.ToListAsync();
//             return View(rental);
//         }

//         // GET: Rentals/Delete/5
//         public async Task<IActionResult> Delete(int? id)
//         {
//             if (id == null)
//             {
//                 return NotFound();
//             }

//             var rental = await _context.Rentals
//                 .Include(r => r.Car)
//                 .Include(r => r.Customer)
//                 .FirstOrDefaultAsync(m => m.Id == id);

//             if (rental == null)
//             {
//                 return NotFound();
//             }

//             return View(rental);
//         }

//         // POST: Rentals/Delete/5
//         [HttpPost, ActionName("Delete")]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> DeleteConfirmed(int id)
//         {
//             var rental = await _context.Rentals.FindAsync(id);
//             _context.Rentals.Remove(rental);
//             await _context.SaveChangesAsync();
//             return RedirectToAction(nameof(Index));
//         }

//         private bool RentalExists(int id)
//         {
//             return _context.Rentals.Any(e => e.Id == id);
//         }

//         // AJAX метод для поиска автомобилей
//         [HttpGet]
//         public IActionResult SearchCars(string term)
//         {
//             var cars = _context.Cars
//                 .Where(c => c.Brand.Contains(term) || c.Model.Contains(term))
//                 .Select(c => new {
//                     id = c.Id,
//                     text = $"{c.Brand} {c.Model} ({c.Year}) - {c.DailyPrice} BYN/день",
//                     brand = c.Brand,
//                     model = c.Model,
//                     year = c.Year,
//                     dailyPrice = c.DailyPrice
//                 })
//                 .Take(10)
//                 .ToList();

//             return Json(cars);
//         }

//         // AJAX метод для поиска клиентов
//         [HttpGet]
//         public IActionResult SearchCustomers(string term)
//         {
//             var customers = _context.Customers
//                 .Where(c => c.FullName.Contains(term))
//                 .Select(c => new {
//                     id = c.Id,
//                     text = c.FullName
//                 })
//                 .Take(10)
//                 .ToList();

//             return Json(customers);
//         }

//         // AJAX метод для получения цены автомобиля
//         [HttpGet]
//         public IActionResult GetCarPrice(int id)
//         {
//             var car = _context.Cars
//                 .Where(c => c.Id == id)
//                 .Select(c => new {
//                     dailyPrice = c.DailyPrice
//                 })
//                 .FirstOrDefault();

//             if (car == null)
//             {
//                 return NotFound();
//             }

//             return Json(car);
//         }

//         // AJAX метод для проверки доступности автомобиля
//         [HttpGet]
//         public IActionResult CheckCarAvailability(int carId, DateTime rentDate, DateTime returnDate, int? excludeRentalId = null)
//         {
//             return Json(IsCarAvailable(carId, rentDate, returnDate, excludeRentalId));
//         }

//         // Метод проверки доступности автомобиля
//         private bool IsCarAvailable(int carId, DateTime rentDate, DateTime returnDate, int? excludeRentalId = null)
//         {
//             return !_context.Rentals // Берем все аренды из БД и инвертируем результат (нам нужно "не занят")
//                 .Where(r => r.CarId == carId) // Фильтруем только аренды нужного автомобиля
//                 .Where(r => r.Id != excludeRentalId) // Исключаем текущую аренду (если редактируем)
//                 .Any(r => (rentDate <= r.ReturnDate) && (returnDate >= r.RentDate)); // Проверяем, есть ли хотя бы одна аренда,
//                                                                                      // где: (rentDate <= r.ReturnDate) &&   // Начало новой аренды <= окончание
//                                                                                      // существующей (returnDate >= r.RentDate)    
//                                                                                      // Конец новой аренды >= начало существующей
//         }
//     }
// }

















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
        public async Task<IActionResult> Index()
        {
            var rentals = await _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .ToListAsync();
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: Rentals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CarId,CustomerId,RentDate,ReturnDate,TotalPrice")] Rental rental)
        {
            // Удаляем проверку CarId и CustomerId из ModelState, так как они приходят из скрытых полей
            ModelState.Remove("Car");
            ModelState.Remove("Customer");

            // Проверка доступности автомобиля
            if (!IsCarAvailable(rental.CarId, rental.RentDate, rental.ReturnDate))
            {
                ModelState.AddModelError("", "Этот автомобиль уже арендован на выбранные даты");
                return View(rental);
            }

            // Проверка дат
            if (rental.ReturnDate <= rental.RentDate)
            {
                ModelState.AddModelError("ReturnDate", "Дата возврата должна быть позже даты начала");
                return View(rental);
            }

            // Получаем автомобиль для расчета стоимости
            var car = await _context.Cars.FindAsync(rental.CarId);
            if (car == null)
            {
                ModelState.AddModelError("CarId", "Автомобиль не найден");
                return View(rental);
            }

            // Рассчитываем количество дней и стоимость
            var days = (rental.ReturnDate - rental.RentDate).Days;
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

            // Добавляем список всех автомобилей в ViewBag
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

            // Проверка доступности автомобиля (исключая текущую аренду)
            if (!IsCarAvailable(rental.CarId, rental.RentDate, rental.ReturnDate, rental.Id))
            {
                ModelState.AddModelError("", "Этот автомобиль уже арендован на выбранные даты");
                ViewBag.AllCars = await _context.Cars.ToListAsync();
                ViewBag.AllCustomers = await _context.Customers.ToListAsync();
                return View(rental);
            }

            // Проверка дат
            if (rental.ReturnDate <= rental.RentDate)
            {
                ModelState.AddModelError("ReturnDate", "Дата возврата должна быть позже даты начала");
                ViewBag.AllCars = await _context.Cars.ToListAsync();
                ViewBag.AllCustomers = await _context.Customers.ToListAsync();
                return View(rental);
            }

            // Получаем автомобиль для расчета стоимости
            var car = await _context.Cars.FindAsync(rental.CarId);
            if (car == null)
            {
                ModelState.AddModelError("CarId", "Автомобиль не найден");
                ViewBag.AllCars = await _context.Cars.ToListAsync();
                ViewBag.AllCustomers = await _context.Customers.ToListAsync();
                return View(rental);
            }

            // Пересчитываем стоимость
            var days = (rental.ReturnDate - rental.RentDate).Days;
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

        // AJAX метод для поиска автомобилей
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
                    dailyPrice = c.DailyPrice
                })
                .Take(10)
                .ToListAsync();

            return Json(cars);
        }

        // AJAX метод для поиска клиентов
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

        // AJAX метод для получения цены автомобиля
        [HttpGet]
        public async Task<IActionResult> GetCarPrice(int id)
        {
            var car = await _context.Cars
                .Where(c => c.Id == id)
                .Select(c => new {
                    dailyPrice = c.DailyPrice
                })
                .FirstOrDefaultAsync();

            if (car == null)
            {
                return NotFound();
            }

            return Json(car);
        }

        // AJAX метод для проверки доступности автомобиля
        [HttpGet]
        public async Task<IActionResult> CheckCarAvailability(int carId, DateTime rentDate, DateTime returnDate, int? excludeRentalId = null)
        {
            var isAvailable = !await _context.Rentals
                .Where(r => r.CarId == carId)
                .Where(r => !excludeRentalId.HasValue || r.Id != excludeRentalId.Value)
                .AnyAsync(r => r.RentDate < returnDate && r.ReturnDate > rentDate);

            return Json(isAvailable);
        }

        // Метод проверки доступности автомобиля
        private bool IsCarAvailable(int carId, DateTime rentDate, DateTime returnDate, int? excludeRentalId = null)
        {
            return !_context.Rentals
                .Where(r => r.CarId == carId)
                .Where(r => !excludeRentalId.HasValue || r.Id != excludeRentalId.Value)
                .Any(r => r.RentDate < returnDate && r.ReturnDate > rentDate);
        }
    }
}