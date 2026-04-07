using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.Models;
using CarRental.Data;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;

namespace CarRental.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // Страница входа
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Обработка входа
        [HttpPost]
        public async Task<IActionResult> Login(string email, string phone, string returnUrl = null)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email && c.Phone == phone);

            if (customer == null)
            {
                ViewBag.Error = "Неверный email или телефон";
                return View();
            }

            HttpContext.Session.SetInt32("CustomerId", customer.Id);
            HttpContext.Session.SetString("CustomerName", customer.FullName);
            HttpContext.Session.SetString("CustomerRole", customer.Role ?? "user");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "User");
        }

        // Страница регистрации
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Обработка регистрации с фото
        [HttpPost]
        public async Task<IActionResult> Register(Customer customer, IFormFile CustomerPhoto)
        {
            if (ModelState.IsValid)
            {
                // Проверка уникальности email
                if (await _context.Customers.AnyAsync(c => c.Email == customer.Email))
                {
                    ModelState.AddModelError("Email", "Email уже зарегистрирован");
                    return View(customer);
                }

                // Проверка уникальности телефона
                if (await _context.Customers.AnyAsync(c => c.Phone == customer.Phone))
                {
                    ModelState.AddModelError("Phone", "Телефон уже зарегистрирован");
                    return View(customer);
                }

                // Обработка загрузки фото
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

                    // Создаём уникальное имя файла
                    var fileName = Guid.NewGuid().ToString() + fileExtension;
                    
                    // Путь к папке uploads/customers (создаём, если не существует)
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

                    // Сохраняем относительный путь в БД (поле image_path)
                    customer.ImagePath = "/uploads/customers/" + fileName;
                }

                // Новый пользователь получает роль "user"
                customer.Role = "user";

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // Автоматический вход после регистрации
                HttpContext.Session.SetInt32("CustomerId", customer.Id);
                HttpContext.Session.SetString("CustomerName", customer.FullName);
                HttpContext.Session.SetString("CustomerRole", customer.Role);

                return RedirectToAction("Index", "User");
            }

            return View(customer);
        }

        // Выход
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "MainHome");
        }
    }
}