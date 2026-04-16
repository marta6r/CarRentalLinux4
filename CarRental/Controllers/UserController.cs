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

        // ==================== AI ПОИСК АВТОМОБИЛЕЙ ====================
        
        /// <summary>
        /// Поиск автомобилей с использованием DeepSeek AI
        /// </summary>
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

            // Получаем все доступные автомобили с ПОЛНОЙ загрузкой связанных данных
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
                // Получаем рекомендацию от DeepSeek AI
                var aiResponse = await _aiService.GetCarRecommendationAsync(query, cars);
                Console.WriteLine($"Ответ от DeepSeek: {aiResponse}");
                
                // Парсим ID автомобилей
                var recommendedIds = aiResponse
                    .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => id.Trim())
                    .Where(id => int.TryParse(id, out _))
                    .Select(int.Parse)
                    .ToList();
                
                Console.WriteLine($"Распарсенные ID: {string.Join(",", recommendedIds)}");

                // Фильтруем автомобили
                var recommendedCars = cars
                    .Where(c => recommendedIds.Contains(c.Id))
                    .ToList();
                
                Console.WriteLine($"Рекомендовано автомобилей: {recommendedCars.Count}");
                
                // Преобразуем в DTO (без циклических ссылок)
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