// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using CarRental.Models;
// using CarRental.Data;
// using CarRental.Services;
// using System.Text.Json;

// namespace CarRental.Controllers
// {
//     public class UserController : Controller
//     {
//         private readonly AppDbContext _context;
//         private readonly DeepSeekService _aiService;

//         public UserController(AppDbContext context, DeepSeekService aiService)
//         {
//             _context = context;
//             _aiService = aiService;
//         }

//         // Список автомобилей
//         public async Task<IActionResult> Index()
//         {
//                 var cars = await _context.Cars
//                 .Include(c => c.Category)
//                 .Where(c => c.IsAvailable)
//                 .ToListAsync();

//             return View(cars);
//         }

//         // Детали автомобиля с характеристиками
//         public async Task<IActionResult> Details(int id)
//         {
//             var car = await _context.Cars
//                 .Include(c => c.Category)
//                 .Include(c => c.CarFeatures)
//                     .ThenInclude(cf => cf.Feature)
//                 .Include(c => c.CarFeatures)
//                     .ThenInclude(cf => cf.FeatureValue)
//                 .FirstOrDefaultAsync(c => c.Id == id);

//             if (car == null)
//                 return NotFound();

//             return View(car);
//         }

//         // Детали аренды (отдельная страница)
//         public async Task<IActionResult> RentalDetails(int id)
//         {
//             var rental = await _context.Rentals
//                 .Include(r => r.Car)
//                 .Include(r => r.Customer)
//                 .FirstOrDefaultAsync(r => r.Id == id);
            
//             if (rental == null)
//             {
//                 return NotFound();
//             }
            
//             return View(rental);
//         }

//         /// <summary>
//         /// Возвращает информацию о бронировании автомобиля по его идентификатору.
//         /// </summary>
//         [HttpGet]
//         public async Task<IActionResult> GetRentalDetails(int id)
//         {
//             var rental = await _context.Rentals
//                 .Include(r => r.Car)
//                 .Include(r => r.Customer)
//                 .FirstOrDefaultAsync(r => r.Id == id);
            
//             if (rental == null)
//                 return NotFound();
            
//             return Json(new
//             {
//                 id = rental.Id,
//                 carBrand = rental.Car?.Brand ?? "Не указано",
//                 carModel = rental.Car?.Model ?? "Не указано",
//                 carYear = rental.Car?.Year.ToString() ?? "Не указано",
//                 carIsAvailable = rental.Car?.IsAvailable ?? false,
//                 customerFullName = rental.Customer?.FullName ?? "Не указано",
//                 customerPhone = rental.Customer?.Phone ?? "Не указано",
//                 customerEmail = rental.Customer?.Email ?? "Не указано",
//                 customerPassport = rental.Customer?.PassportNumber ?? "Не указано",
//                 rentDate = rental.RentDate.ToString("dd.MM.yyyy"),
//                 returnDate = rental.ReturnDate.ToString("dd.MM.yyyy"),
//                 durationDays = (rental.ReturnDate - rental.RentDate).Days,
//                 isActive = rental.ReturnDate >= DateTime.Now,
//                 totalPrice = rental.TotalPrice.ToString("N2"),
//                 dailyPrice = rental.Car?.DailyPrice.ToString("N2") ?? "0.00"
//             });
//         }

//         // ==================== AI ПОИСК АВТОМОБИЛЕЙ ====================
        
//         /// <summary>
//         /// Поиск автомобилей с использованием DeepSeek AI
//         /// </summary>
//         [HttpPost]
//         public async Task<IActionResult> AISearch([FromBody] JsonElement request)
//         {
//             string query = "";
//             if (request.TryGetProperty("query", out var queryProperty))
//             {
//                 query = queryProperty.GetString() ?? "";
//             }
            
//             Console.WriteLine($"=== AISearch ВЫЗВАН ===");
//             Console.WriteLine($"Запрос: {query}");
            
//             if (string.IsNullOrWhiteSpace(query))
//             {
//                 Console.WriteLine("Запрос пустой");
//                 return Json(new List<CarDto>());
//             }

//             // Получаем все доступные автомобили с ПОЛНОЙ загрузкой связанных данных
//             var cars = await _context.Cars
//                 .Include(c => c.Category)
//                 .Include(c => c.CarFeatures)
//                     .ThenInclude(cf => cf.Feature)
//                 .Include(c => c.CarFeatures)
//                     .ThenInclude(cf => cf.FeatureValue)
//                 .Where(c => c.IsAvailable)
//                 .ToListAsync();
            
//             Console.WriteLine($"Найдено автомобилей в БД: {cars.Count}");
            
//             if (!cars.Any())
//             {
//                 return Json(new List<CarDto>());
//             }

//             try
//             {
//                 // Получаем рекомендацию от DeepSeek AI
//                 var aiResponse = await _aiService.GetCarRecommendationAsync(query, cars);
//                 Console.WriteLine($"Ответ от DeepSeek: {aiResponse}");
                
//                 // Парсим ID автомобилей
//                 var recommendedIds = aiResponse
//                     .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
//                     .Select(id => id.Trim())
//                     .Where(id => int.TryParse(id, out _))
//                     .Select(int.Parse)
//                     .ToList();
                
//                 Console.WriteLine($"Распарсенные ID: {string.Join(",", recommendedIds)}");

//                 // Фильтруем автомобили
//                 var recommendedCars = cars
//                     .Where(c => recommendedIds.Contains(c.Id))
//                     .ToList();
                
//                 Console.WriteLine($"Рекомендовано автомобилей: {recommendedCars.Count}");
                
//                 // Преобразуем в DTO (без циклических ссылок)
//                 var result = recommendedCars.Select(c => new CarDto
//                 {
//                     Id = c.Id,
//                     Brand = c.Brand,
//                     Model = c.Model,
//                     Year = c.Year,
//                     DailyPrice = c.DailyPrice,
//                     ImagePath = c.ImagePath,
//                     CategoryName = c.Category?.Name ?? "Без категории"
//                 }).ToList();
                
//                 return Json(result);
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"AI Search Error: {ex.Message}");
//                 Console.WriteLine($"Stack trace: {ex.StackTrace}");
//                 return Json(new List<CarDto>());
//             }
//         }
//     }
// }































































// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using CarRental.Models;
// using CarRental.Data;
// using CarRental.Services;
// using System.Text.Json;

// namespace CarRental.Controllers
// {
//     public class UserController : Controller
//     {
//         private readonly AppDbContext _context;
//         private readonly DeepSeekService _aiService;

//         public UserController(AppDbContext context, DeepSeekService aiService)
//         {
//             _context = context;
//             _aiService = aiService;
//         }

//         // Список автомобилей
//         public async Task<IActionResult> Index()
//         {
//             var cars = await _context.Cars
//                 .Include(c => c.Category)
//                 .Where(c => c.IsAvailable)
//                 .ToListAsync();

//             return View(cars);
//         }

//         // Детали автомобиля с характеристиками
//         public async Task<IActionResult> Details(int id)
//         {
//             var car = await _context.Cars
//                 .Include(c => c.Category)
//                 .Include(c => c.CarFeatures)
//                     .ThenInclude(cf => cf.Feature)
//                 .Include(c => c.CarFeatures)
//                     .ThenInclude(cf => cf.FeatureValue)
//                 .FirstOrDefaultAsync(c => c.Id == id);

//             if (car == null)
//                 return NotFound();

//             return View(car);
//         }

//         // Детали аренды (отдельная страница)
//         public async Task<IActionResult> RentalDetails(int id)
//         {
//             var rental = await _context.Rentals
//                 .Include(r => r.Car)
//                 .Include(r => r.Customer)
//                 .FirstOrDefaultAsync(r => r.Id == id);
            
//             if (rental == null)
//             {
//                 return NotFound();
//             }
            
//             return View(rental);
//         }

//         /// <summary>
//         /// Возвращает информацию о бронировании автомобиля по его идентификатору.
//         /// </summary>
//         [HttpGet]
//         public async Task<IActionResult> GetRentalDetails(int id)
//         {
//             var rental = await _context.Rentals
//                 .Include(r => r.Car)
//                 .Include(r => r.Customer)
//                 .FirstOrDefaultAsync(r => r.Id == id);
            
//             if (rental == null)
//                 return NotFound();
            
//             return Json(new
//             {
//                 id = rental.Id,
//                 carBrand = rental.Car?.Brand ?? "Не указано",
//                 carModel = rental.Car?.Model ?? "Не указано",
//                 carYear = rental.Car?.Year.ToString() ?? "Не указано",
//                 carIsAvailable = rental.Car?.IsAvailable ?? false,
//                 customerFullName = rental.Customer?.FullName ?? "Не указано",
//                 customerPhone = rental.Customer?.Phone ?? "Не указано",
//                 customerEmail = rental.Customer?.Email ?? "Не указано",
//                 customerPassport = rental.Customer?.PassportNumber ?? "Не указано",
//                 rentDate = rental.RentDate.ToString("dd.MM.yyyy"),
//                 returnDate = rental.ReturnDate.ToString("dd.MM.yyyy"),
//                 durationDays = (rental.ReturnDate - rental.RentDate).Days,
//                 isActive = rental.ReturnDate >= DateTime.Now,
//                 totalPrice = rental.TotalPrice.ToString("N2"),
//                 dailyPrice = rental.Car?.DailyPrice.ToString("N2") ?? "0.00"
//             });
//         }

//         // ==================== БРОНИРОВАНИЕ АВТОМОБИЛЯ ====================
        
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Book(int carId, DateTime startDate, DateTime endDate)
//         {
//             // Получаем автомобиль для расчёта
//             var car = await _context.Cars.FindAsync(carId);
//             if (car == null)
//             {
//                 TempData["Error"] = "Автомобиль не найден";
//                 return RedirectToAction("Index");
//             }

//             // Проверка корректности дат
//             if (startDate < DateTime.Today)
//             {
//                 TempData["Error"] = "Дата начала не может быть раньше сегодняшнего дня";
//                 return RedirectToAction("Details", new { id = carId });
//             }

//             if (endDate <= startDate)
//             {
//                 TempData["Error"] = "Дата возврата должна быть позже даты начала";
//                 return RedirectToAction("Details", new { id = carId });
//             }

//             // Проверка доступности автомобиля на выбранные даты
//             var isAvailable = !await _context.Rentals.AnyAsync(r => r.CarId == carId &&
//                 ((startDate >= r.RentDate && startDate < r.ReturnDate) ||
//                  (endDate > r.RentDate && endDate <= r.ReturnDate) ||
//                  (startDate <= r.RentDate && endDate >= r.ReturnDate)));

//             if (!isAvailable)
//             {
//                 TempData["Error"] = "Автомобиль уже забронирован на выбранные даты";
//                 return RedirectToAction("Details", new { id = carId });
//             }

//             // Получаем ID текущего пользователя
//             var customerId = HttpContext.Session.GetInt32("CustomerId");
//             if (!customerId.HasValue)
//             {
//                 return RedirectToAction("Login", "Account");
//             }

//             // Расчёт стоимости
//             var days = (endDate - startDate).Days;
//             var totalPrice = car.DailyPrice * days;

//             // Создаём бронирование
//             var rental = new Rental
//             {
//                 CarId = carId,
//                 CustomerId = customerId.Value,
//                 RentDate = startDate,
//                 ReturnDate = endDate,
//                 TotalPrice = totalPrice
//             };

//             _context.Rentals.Add(rental);
//             await _context.SaveChangesAsync();

//             TempData["Success"] = $"Автомобиль {car.Brand} {car.Model} успешно забронирован!";
//             return RedirectToAction("MyBookings", "Profile");
//         }

//         // ==================== ПРОВЕРКА ДОСТУПНОСТИ ====================
        
//         [HttpGet]
//         public async Task<IActionResult> CheckAvailability(int carId, DateTime startDate, DateTime endDate)
//         {
//             var hasConflict = await _context.Rentals
//                 .AnyAsync(r => r.CarId == carId &&
//                     ((startDate >= r.RentDate && startDate < r.ReturnDate) ||
//                      (endDate > r.RentDate && endDate <= r.ReturnDate) ||
//                      (startDate <= r.RentDate && endDate >= r.ReturnDate)));

//             var car = await _context.Cars.FindAsync(carId);
//             var days = (endDate - startDate).Days;
//             var totalPrice = car != null ? car.DailyPrice * days : 0;

//             return Json(new 
//             { 
//                 available = !hasConflict,
//                 days = days > 0 ? days : 0,
//                 totalPrice = totalPrice
//             });
//         }

//         [HttpGet]
//         public async Task<IActionResult> GetBookedDates(int carId)
//         {
//             var rentals = await _context.Rentals
//                 .Where(r => r.CarId == carId && r.ReturnDate >= DateTime.Now.Date)
//                 .Select(r => new { start = r.RentDate, end = r.ReturnDate })
//                 .ToListAsync();
            
//             var bookedDates = new List<string>();
//             foreach (var rental in rentals)
//             {
//                 for (var date = rental.start; date <= rental.end; date = date.AddDays(1))
//                 {
//                     bookedDates.Add(date.ToString("yyyy-MM-dd"));
//                 }
//             }
            
//             return Json(bookedDates);
//         }

//         // ==================== AI ПОИСК АВТОМОБИЛЕЙ ====================
        
//         /// <summary>
//         /// Поиск автомобилей с использованием DeepSeek AI
//         /// </summary>
//         [HttpPost]
//         public async Task<IActionResult> AISearch([FromBody] JsonElement request)
//         {
//             string query = "";
//             if (request.TryGetProperty("query", out var queryProperty))
//             {
//                 query = queryProperty.GetString() ?? "";
//             }
            
//             Console.WriteLine($"=== AISearch ВЫЗВАН ===");
//             Console.WriteLine($"Запрос: {query}");
            
//             if (string.IsNullOrWhiteSpace(query))
//             {
//                 Console.WriteLine("Запрос пустой");
//                 return Json(new List<CarDto>());
//             }

//             // Получаем все доступные автомобили с ПОЛНОЙ загрузкой связанных данных
//             var cars = await _context.Cars
//                 .Include(c => c.Category)
//                 .Include(c => c.CarFeatures)
//                     .ThenInclude(cf => cf.Feature)
//                 .Include(c => c.CarFeatures)
//                     .ThenInclude(cf => cf.FeatureValue)
//                 .Where(c => c.IsAvailable)
//                 .ToListAsync();
            
//             Console.WriteLine($"Найдено автомобилей в БД: {cars.Count}");
            
//             if (!cars.Any())
//             {
//                 return Json(new List<CarDto>());
//             }

//             try
//             {
//                 // Получаем рекомендацию от DeepSeek AI
//                 var aiResponse = await _aiService.GetCarRecommendationAsync(query, cars);
//                 Console.WriteLine($"Ответ от DeepSeek: {aiResponse}");
                
//                 // Парсим ID автомобилей
//                 var recommendedIds = aiResponse
//                     .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
//                     .Select(id => id.Trim())
//                     .Where(id => int.TryParse(id, out _))
//                     .Select(int.Parse)
//                     .ToList();
                
//                 Console.WriteLine($"Распарсенные ID: {string.Join(",", recommendedIds)}");

//                 // Фильтруем автомобили
//                 var recommendedCars = cars
//                     .Where(c => recommendedIds.Contains(c.Id))
//                     .ToList();
                
//                 Console.WriteLine($"Рекомендовано автомобилей: {recommendedCars.Count}");
                
//                 // Преобразуем в DTO (без циклических ссылок)
//                 var result = recommendedCars.Select(c => new CarDto
//                 {
//                     Id = c.Id,
//                     Brand = c.Brand,
//                     Model = c.Model,
//                     Year = c.Year,
//                     DailyPrice = c.DailyPrice,
//                     ImagePath = c.ImagePath,
//                     CategoryName = c.Category?.Name ?? "Без категории"
//                 }).ToList();
                
//                 return Json(result);
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"AI Search Error: {ex.Message}");
//                 Console.WriteLine($"Stack trace: {ex.StackTrace}");
//                 return Json(new List<CarDto>());
//             }
//         }
//     }
// }












































using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.Models;
using CarRental.Data;
using CarRental.Services;
using System.Text.Json;

namespace CarRental.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly DeepSeekService _aiService;

        public UserController(AppDbContext context, DeepSeekService aiService)
        {
            _context = context;
            _aiService = aiService;
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

        // Детали аренды (отдельная страница)
        public async Task<IActionResult> RentalDetails(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);
            
            if (rental == null)
            {
                return NotFound();
            }
            
            return View(rental);
        }

        /// <summary>
        /// Возвращает информацию о бронировании автомобиля по его идентификатору.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRentalDetails(int id)
        {
            var rental = await _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);
            
            if (rental == null)
                return NotFound();
            
            return Json(new
            {
                id = rental.Id,
                carBrand = rental.Car?.Brand ?? "Не указано",
                carModel = rental.Car?.Model ?? "Не указано",
                carYear = rental.Car?.Year.ToString() ?? "Не указано",
                carIsAvailable = rental.Car?.IsAvailable ?? false,
                customerFullName = rental.Customer?.FullName ?? "Не указано",
                customerPhone = rental.Customer?.Phone ?? "Не указано",
                customerEmail = rental.Customer?.Email ?? "Не указано",
                customerPassport = rental.Customer?.PassportNumber ?? "Не указано",
                rentDate = rental.RentDate.ToString("dd.MM.yyyy"),
                returnDate = rental.ReturnDate.ToString("dd.MM.yyyy"),
                durationDays = (rental.ReturnDate - rental.RentDate).Days,
                isActive = rental.ReturnDate >= DateTime.Now,
                totalPrice = rental.TotalPrice.ToString("N2"),
                dailyPrice = rental.Car?.DailyPrice.ToString("N2") ?? "0.00"
            });
        }

        // ==================== БРОНИРОВАНИЕ АВТОМОБИЛЯ ====================
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int carId, DateTime startDate, DateTime endDate)
        {
            // Получаем автомобиль для расчёта
            var car = await _context.Cars.FindAsync(carId);
            if (car == null)
            {
                TempData["Error"] = "Автомобиль не найден";
                return RedirectToAction("Index");
            }

            // Проверка корректности дат
            if (startDate < DateTime.Today)
            {
                TempData["Error"] = "Дата начала не может быть раньше сегодняшнего дня";
                return RedirectToAction("Details", new { id = carId });
            }

            if (endDate <= startDate)
            {
                TempData["Error"] = "Дата возврата должна быть позже даты начала";
                return RedirectToAction("Details", new { id = carId });
            }

            // Ограничение: максимум 365 дней
            var days = (endDate - startDate).Days;
            if (days > 365)
            {
                TempData["Error"] = "Максимальная продолжительность аренды не может превышать 365 дней (1 год)";
                return RedirectToAction("Details", new { id = carId });
            }

            // Проверка доступности автомобиля на выбранные даты
            var isAvailable = !await _context.Rentals.AnyAsync(r => r.CarId == carId &&
                ((startDate >= r.RentDate && startDate < r.ReturnDate) ||
                 (endDate > r.RentDate && endDate <= r.ReturnDate) ||
                 (startDate <= r.RentDate && endDate >= r.ReturnDate)));

            if (!isAvailable)
            {
                TempData["Error"] = "Автомобиль уже забронирован на выбранные даты";
                return RedirectToAction("Details", new { id = carId });
            }

            // Получаем ID текущего пользователя
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Расчёт стоимости
            var totalPrice = car.DailyPrice * days;

            // Создаём бронирование
            var rental = new Rental
            {
                CarId = carId,
                CustomerId = customerId.Value,
                RentDate = startDate,
                ReturnDate = endDate,
                TotalPrice = totalPrice
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Автомобиль {car.Brand} {car.Model} успешно забронирован!";
            return RedirectToAction("MyBookings", "Profile");
        }

        // ==================== ПРОВЕРКА ДОСТУПНОСТИ ====================
        
        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int carId, DateTime startDate, DateTime endDate)
        {
            var hasConflict = await _context.Rentals
                .AnyAsync(r => r.CarId == carId &&
                    ((startDate >= r.RentDate && startDate < r.ReturnDate) ||
                     (endDate > r.RentDate && endDate <= r.ReturnDate) ||
                     (startDate <= r.RentDate && endDate >= r.ReturnDate)));

            var car = await _context.Cars.FindAsync(carId);
            var days = (endDate - startDate).Days;
            var totalPrice = car != null ? car.DailyPrice * days : 0;

            // Проверка на максимальную продолжительность
            var isValidDuration = days <= 365 && days > 0;

            return Json(new 
            { 
                available = !hasConflict && isValidDuration,
                days = days > 0 ? days : 0,
                totalPrice = totalPrice,
                isValidDuration = isValidDuration,
                maxDays = 365
            });
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

        // ==================== AI ПОИСК АВТОМОБИЛЕЙ ====================
        
        [HttpPost]
        public async Task<IActionResult> AISearch([FromBody] JsonElement request)
        {
            string query = "";
            if (request.TryGetProperty("query", out var queryProperty))
            {
                query = queryProperty.GetString() ?? "";
            }
            
            Console.WriteLine($"=== AISearch ВЫЗВАН ===");
            Console.WriteLine($"Запрос: {query}");
            
            if (string.IsNullOrWhiteSpace(query))
            {
                Console.WriteLine("Запрос пустой");
                return Json(new List<CarDto>());
            }

            var cars = await _context.Cars
                .Include(c => c.Category)
                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.Feature)
                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.FeatureValue)
                .Where(c => c.IsAvailable)
                .ToListAsync();
            
            Console.WriteLine($"Найдено автомобилей в БД: {cars.Count}");
            
            if (!cars.Any())
            {
                return Json(new List<CarDto>());
            }

            try
            {
                var aiResponse = await _aiService.GetCarRecommendationAsync(query, cars);
                Console.WriteLine($"Ответ от DeepSeek: {aiResponse}");
                
                var recommendedIds = aiResponse
                    .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => id.Trim())
                    .Where(id => int.TryParse(id, out _))
                    .Select(int.Parse)
                    .ToList();
                
                Console.WriteLine($"Распарсенные ID: {string.Join(",", recommendedIds)}");

                var recommendedCars = cars
                    .Where(c => recommendedIds.Contains(c.Id))
                    .ToList();
                
                Console.WriteLine($"Рекомендовано автомобилей: {recommendedCars.Count}");
                
                var result = recommendedCars.Select(c => new CarDto
                {
                    Id = c.Id,
                    Brand = c.Brand,
                    Model = c.Model,
                    Year = c.Year,
                    DailyPrice = c.DailyPrice,
                    ImagePath = c.ImagePath,
                    CategoryName = c.Category?.Name ?? "Без категории"
                }).ToList();
                
                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Search Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new List<CarDto>());
            }
        }
    }
}