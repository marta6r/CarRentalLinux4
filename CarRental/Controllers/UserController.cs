// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using CarRental.Models;
// using CarRental.Data;

// namespace CarRental.Controllers
// {
//     public class UserController : Controller
//     {
//         private readonly AppDbContext _context;

//         public UserController(AppDbContext context)
//         {
//             _context = context;
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









// /// <summary>
// /// Возвращает информацию о бронировании автомобиля по его идентификатору.
// /// </summary>
//         [HttpGet]
// public async Task<IActionResult> GetRentalDetails(int id)
// {
//     var rental = await _context.Rentals
//         .Include(r => r.Car)
//         .Include(r => r.Customer)
//         .FirstOrDefaultAsync(r => r.Id == id);
    
//     if (rental == null)
//         return NotFound();
    
//     return Json(new
//     {
//         id = rental.Id,
//         carBrand = rental.Car?.Brand ?? "Не указано",
//         carModel = rental.Car?.Model ?? "Не указано",
//         carYear = rental.Car?.Year.ToString() ?? "Не указано",
//         carIsAvailable = rental.Car?.IsAvailable ?? false,
//         customerFullName = rental.Customer?.FullName ?? "Не указано",
//         customerPhone = rental.Customer?.Phone ?? "Не указано",
//         customerEmail = rental.Customer?.Email ?? "Не указано",
//         customerPassport = rental.Customer?.PassportNumber ?? "Не указано",
//         rentDate = rental.RentDate.ToString("dd.MM.yyyy"),
//         returnDate = rental.ReturnDate.ToString("dd.MM.yyyy"),
//         durationDays = (rental.ReturnDate - rental.RentDate).Days,
//         isActive = rental.ReturnDate >= DateTime.Now,
//         totalPrice = rental.TotalPrice.ToString("N2"),
//         dailyPrice = rental.Car?.DailyPrice.ToString("N2") ?? "0.00"
//     });
// }
//     }
// }





































// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using CarRental.Models;
// using CarRental.Data;

// namespace CarRental.Controllers
// {
//     public class UserController : Controller
//     {
//         private readonly AppDbContext _context;

//         public UserController(AppDbContext context)
//         {
//             _context = context;
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

//         // Мои бронирования
//         public async Task<IActionResult> MyBooking()
//         {
//             // Здесь нужно получить ID текущего пользователя
//             // Для примера используем заглушку, замените на реальную логику
//             var currentUserId = GetCurrentUserId();
            
//             var rentals = await _context.Rentals
//                 .Include(r => r.Car)
//                 .Include(r => r.Customer)
//                 .Where(r => r.CustomerId == currentUserId)
//                 .OrderByDescending(r => r.RentDate)
//                 .ToListAsync();

//             return View(rentals);
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
            
//             // Проверяем, что аренда принадлежит текущему пользователю
//             var currentUserId = GetCurrentUserId();
//             if (rental.CustomerId != currentUserId)
//             {
//                 return Forbid();
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
            
//             // Проверяем, что аренда принадлежит текущему пользователю
//             var currentUserId = GetCurrentUserId();
//             if (rental.CustomerId != currentUserId)
//             {
//                 return Forbid();
//             }
            
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

//         // Вспомогательный метод для получения ID текущего пользователя
//         // Замените на вашу реальную логику аутентификации
//         private int GetCurrentUserId()
//         {
//             // Вариант 1: Если используете сессию
//             // var userId = HttpContext.Session.GetInt32("UserId");
//             // return userId ?? 0;
            
//             // Вариант 2: Если используете Claims (Identity)
//             // var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
//             // return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
            
//             // Вариант 3: Заглушка - замените на реальную логику
//             // Здесь нужно вернуть ID текущего авторизованного пользователя
//             // Пока возвращаем 1 для примера
//             return 1;
//         }
//     }
// }

























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
    }
}