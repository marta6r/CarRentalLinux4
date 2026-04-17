using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using BCrypt.Net;

namespace CarRental.Controllers
{
    public class CustomersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CustomersController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers.ToListAsync();
            return View(customers);
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string FullName, string Email, string Phone, string PassportNumber, IFormFile? ImageFile)
        {
            // Отладка
            Console.WriteLine("=== CREATE POST CALLED ===");
            Console.WriteLine($"FullName: {FullName}");
            Console.WriteLine($"Email: {Email}");
            Console.WriteLine($"Phone: {Phone}");
            Console.WriteLine($"PassportNumber: {PassportNumber}");

            // Проверка обязательных полей
            if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Email) || 
                string.IsNullOrEmpty(Phone) || string.IsNullOrEmpty(PassportNumber))
            {
                TempData["Error"] = "Все поля обязательны для заполнения";
                return View();
            }

            // Проверка формата телефона (+375XXXXXXXXX)
            if (!Phone.StartsWith("+375") || Phone.Length != 13)
            {
                TempData["Error"] = "Телефон должен быть в формате +375XXXXXXXXX (13 символов)";
                return View();
            }

            // Проверка формата паспорта (2 буквы + 7 цифр)
            if (PassportNumber.Length != 9)
            {
                TempData["Error"] = "Номер паспорта должен содержать 9 символов (2 буквы и 7 цифр)";
                return View();
            }

            // Проверка уникальности email
            if (await _context.Customers.AnyAsync(c => c.Email == Email))
            {
                TempData["Error"] = "Клиент с таким email уже существует";
                return View();
            }

            // Проверка уникальности телефона
            if (await _context.Customers.AnyAsync(c => c.Phone == Phone))
            {
                TempData["Error"] = "Клиент с таким телефоном уже существует";
                return View();
            }

            // Проверка уникальности паспорта
            if (await _context.Customers.AnyAsync(c => c.PassportNumber == PassportNumber))
            {
                TempData["Error"] = "Клиент с таким паспортом уже существует";
                return View();
            }

            try
            {
                var customer = new Customer
                {
                    FullName = FullName,
                    Email = Email,
                    Phone = Phone,
                    PassportNumber = PassportNumber,
                    Role = "user",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456")
                };

                // Обработка фото
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Проверка размера
                    if (ImageFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "Размер файла не должен превышать 5MB";
                        return View();
                    }

                    // Проверка формата
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    string fileExtension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["Error"] = "Поддерживаются только форматы: JPG, JPEG, PNG, GIF, WEBP";
                        return View();
                    }

                    // Сохраняем фото
                    string fileName = Guid.NewGuid().ToString() + fileExtension;
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "customers");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string filePath = Path.Combine(uploadsFolder, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    customer.ImagePath = "/images/customers/" + fileName;
                }

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Customer saved successfully! ID: {customer.Id}");
                TempData["Success"] = $"Клиент {FullName} успешно добавлен! Временный пароль: 123456";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR saving customer: {ex.Message}");
                TempData["Error"] = $"Ошибка при сохранении: {ex.Message}";
                return View();
            }
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string FullName, string Email, string Phone, string PassportNumber, IFormFile? ImageFile, string DeletePhoto)
        {
            // Находим клиента в базе данных
            var existingCustomer = await _context.Customers.FindAsync(id);
            if (existingCustomer == null)
            {
                return NotFound();
            }

            // Проверка уникальности email (если изменился)
            if (existingCustomer.Email != Email && await _context.Customers.AnyAsync(c => c.Email == Email))
            {
                TempData["Error"] = "Клиент с таким email уже существует";
                return View(existingCustomer);
            }

            // Проверка уникальности телефона (если изменился)
            if (existingCustomer.Phone != Phone && await _context.Customers.AnyAsync(c => c.Phone == Phone))
            {
                TempData["Error"] = "Клиент с таким телефоном уже существует";
                return View(existingCustomer);
            }

            // Проверка уникальности паспорта (если изменился)
            if (existingCustomer.PassportNumber != PassportNumber && await _context.Customers.AnyAsync(c => c.PassportNumber == PassportNumber))
            {
                TempData["Error"] = "Клиент с таким паспортом уже существует";
                return View(existingCustomer);
            }

            // Обновляем поля
            existingCustomer.FullName = FullName;
            existingCustomer.Email = Email;
            existingCustomer.Phone = Phone;
            existingCustomer.PassportNumber = PassportNumber;

            // Обработка удаления фото
            if (DeletePhoto == "true")
            {
                if (!string.IsNullOrEmpty(existingCustomer.ImagePath))
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingCustomer.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                    existingCustomer.ImagePath = null;
                }
            }

            // Обработка загрузки нового фото
            if (ImageFile != null && ImageFile.Length > 0)
            {
                // Проверка размера
                if (ImageFile.Length > 5 * 1024 * 1024)
                {
                    TempData["Error"] = "Размер файла не должен превышать 5MB";
                    return View(existingCustomer);
                }

                // Проверка формата
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                string fileExtension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["Error"] = "Поддерживаются только форматы: JPG, JPEG, PNG, GIF, WEBP";
                    return View(existingCustomer);
                }

                // Удаляем старое фото (если есть и если не было удалено)
                if (!string.IsNullOrEmpty(existingCustomer.ImagePath) && DeletePhoto != "true")
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingCustomer.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Сохраняем новое фото
                string fileName = Guid.NewGuid().ToString() + fileExtension;
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "customers");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string filePath = Path.Combine(uploadsFolder, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                existingCustomer.ImagePath = "/images/customers/" + fileName;
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Данные клиента успешно обновлены!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при сохранении: {ex.Message}";
                return View(existingCustomer);
            }
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                // Удаляем фото
                if (!string.IsNullOrEmpty(customer.ImagePath))
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, customer.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Клиент успешно удален";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }

        // GET: Customers/Search
        public IActionResult Search()
        {
            return View();
        }

        // POST: Customers/Search
        [HttpPost]
        public async Task<IActionResult> Search(string passportNumber)
        {
            if (string.IsNullOrEmpty(passportNumber))
            {
                ViewBag.Message = "Введите номер паспорта для поиска";
                return View();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.PassportNumber == passportNumber);

            if (customer == null)
            {
                ViewBag.Message = "Клиент с таким номером паспорта не найден";
                return View();
            }

            return View("SearchResult", customer);
        }
    }
}