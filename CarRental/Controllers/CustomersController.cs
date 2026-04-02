using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;

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
            return View(await _context.Customers.ToListAsync());
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.Id == id);

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
        public async Task<IActionResult> Create(Customer customer)
        {
            // ДЕБАГ: проверяем, приходит ли файл
            if (customer.ImageFile == null)
            {
                Console.WriteLine("ImageFile = NULL");
            }
            else
            {
                Console.WriteLine($"ImageFile получен: {customer.ImageFile.FileName}, размер: {customer.ImageFile.Length}");
            }

            if (ModelState.IsValid)
            {
                // Сохраняем файл изображения
                if (customer.ImageFile != null && customer.ImageFile.Length > 0)
                {
                    // Проверяем размер файла (макс 5MB)
                    if (customer.ImageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImageFile", "Размер файла не должен превышать 5MB");
                        return View(customer);
                    }

                    // Проверяем расширение файла
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    string fileExtension = Path.GetExtension(customer.ImageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ImageFile", "Поддерживаются только форматы: JPG, JPEG, PNG, GIF, WEBP");
                        return View(customer);
                    }

                    // Генерируем уникальное имя файла
                    string fileName = Guid.NewGuid().ToString() + fileExtension;
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "customers");

                    // Создаем папку, если её нет
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await customer.ImageFile.CopyToAsync(fileStream);
                    }

                    customer.ImagePath = "/images/customers/" + fileName;
                    Console.WriteLine($"Файл сохранен: {customer.ImagePath}");
                }

                _context.Add(customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Клиент успешно добавлен!";
                return RedirectToAction(nameof(Index));
            }

            // Выводим ошибки валидации
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Ошибка валидации: {error.ErrorMessage}");
            }

            return View(customer);
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
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Получаем существующего клиента из базы
                    var existingCustomer = await _context.Customers.FindAsync(id);
                    if (existingCustomer == null)
                    {
                        return NotFound();
                    }

                    // Обновляем поля
                    existingCustomer.FullName = customer.FullName;
                    existingCustomer.Email = customer.Email;
                    existingCustomer.Phone = customer.Phone;
                    existingCustomer.PassportNumber = customer.PassportNumber;

                    // Сохраняем новое изображение
                    if (customer.ImageFile != null && customer.ImageFile.Length > 0)
                    {
                        // Проверяем размер файла
                        if (customer.ImageFile.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("ImageFile", "Размер файла не должен превышать 5MB");
                            return View(customer);
                        }

                        // Проверяем расширение файла
                        string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                        string fileExtension = Path.GetExtension(customer.ImageFile.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("ImageFile", "Поддерживаются только форматы: JPG, JPEG, PNG, GIF, WEBP");
                            return View(customer);
                        }

                        // Удаляем старое изображение, если оно есть
                        if (!string.IsNullOrEmpty(existingCustomer.ImagePath))
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingCustomer.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Генерируем уникальное имя файла
                        string fileName = Guid.NewGuid().ToString() + fileExtension;
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "customers");

                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string filePath = Path.Combine(uploadsFolder, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await customer.ImageFile.CopyToAsync(fileStream);
                        }

                        existingCustomer.ImagePath = "/images/customers/" + fileName;
                    }

                    _context.Update(existingCustomer);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Данные клиента обновлены";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.Id == id);

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
            if (customer == null)
            {
                return NotFound();
            }

            // Удаляем изображение, если оно есть
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
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.PassportNumber == passportNumber);

            if (customer == null)
            {
                ViewBag.Message = "Клиент не найден";
                return View();
            }

            return View("SearchResult", customer);
        }
    }
}