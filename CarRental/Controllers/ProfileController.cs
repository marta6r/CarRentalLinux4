using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.Models;
using CarRental.Data;
using Microsoft.AspNetCore.Http;
using System.IO;
using BCrypt.Net;

namespace CarRental.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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

        // Редактирование профиля - GET
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            var customer = await _context.Customers.FindAsync(customerId.Value);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        // Редактирование профиля - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(string FullName, string Email, string Phone, string PassportNumber, string DeletePhoto, IFormFile? CustomerPhoto)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            var customer = await _context.Customers.FindAsync(customerId.Value);
            if (customer == null)
                return NotFound();

            // Обновляем поля
            customer.FullName = FullName;
            customer.Email = Email;
            customer.Phone = Phone;
            customer.PassportNumber = PassportNumber;

            // Обработка удаления фото
            if (DeletePhoto == "true" && !string.IsNullOrEmpty(customer.ImagePath))
            {
                var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, customer.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
                customer.ImagePath = null;
            }

            // Обработка загрузки нового фото
            if (CustomerPhoto != null && CustomerPhoto.Length > 0)
            {
                // Проверка размера файла (максимум 5MB)
                if (CustomerPhoto.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("CustomerPhoto", "Размер файла не должен превышать 5MB");
                    return View(customer);
                }

                // Проверка формата файла
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(CustomerPhoto.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("CustomerPhoto", "Поддерживаются только форматы: JPG, JPEG, PNG, GIF");
                    return View(customer);
                }

                // Удаляем старое фото, если оно есть и не было удалено
                if (!string.IsNullOrEmpty(customer.ImagePath) && DeletePhoto != "true")
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, customer.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Создаём уникальное имя файла
                var fileName = Guid.NewGuid().ToString() + fileExtension;
                
                // Путь к папке uploads/customers
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "customers");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Полный путь к файлу
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Сохраняем файл
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await CustomerPhoto.CopyToAsync(fileStream);
                }

                // Сохраняем относительный путь в БД
                customer.ImagePath = "/uploads/customers/" + fileName;
            }

            await _context.SaveChangesAsync();
            
            // Обновляем имя в сессии
            HttpContext.Session.SetString("CustomerName", customer.FullName);
            
            TempData["Success"] = "Профиль успешно обновлен!";
            return RedirectToAction("Index");
        }

        // Смена пароля - GET
        [HttpGet]
        public IActionResult ChangePassword()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            return View();
        }

        // Смена пароля - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            var customer = await _context.Customers.FindAsync(customerId.Value);
            if (customer == null)
                return NotFound();

            // Проверка текущего пароля
            if (string.IsNullOrEmpty(customer.PasswordHash) || 
                !BCrypt.Net.BCrypt.Verify(currentPassword, customer.PasswordHash))
            {
                ViewBag.Error = "Текущий пароль введен неверно";
                return View();
            }

            // Проверка нового пароля
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                ViewBag.Error = "Новый пароль должен содержать минимум 6 символов";
                return View();
            }

            // Проверка совпадения паролей
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Новый пароль и подтверждение не совпадают";
                return View();
            }

            // Хешируем и сохраняем новый пароль
            customer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Пароль успешно изменен!";
            return View();
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

        // Мои бронирования (алиас для ссылки из RentalDetails)
        public async Task<IActionResult> MyBooking()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            var bookings = await _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .Where(r => r.CustomerId == customerId.Value)
                .OrderByDescending(r => r.RentDate)
                .ToListAsync();

            return View("MyBookings", bookings);
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