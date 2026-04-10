using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using BCrypt.Net;

namespace CarRental.Controllers
{
    public class AdminProfileController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminProfileController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // Личный кабинет администратора
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var adminId = HttpContext.Session.GetInt32("CustomerId");
            if (adminId == null)
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            var admin = await _context.Customers.FindAsync(adminId);
            if (admin == null)
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            return View(admin);
        }

        // Редактирование профиля - GET
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var adminId = HttpContext.Session.GetInt32("CustomerId");
            if (adminId == null)
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            var admin = await _context.Customers.FindAsync(adminId);
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // Редактирование профиля - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(string FullName, string Email, string Phone, string PassportNumber, IFormFile? ImageFile)
        {
            var adminId = HttpContext.Session.GetInt32("CustomerId");
            if (adminId == null)
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            var admin = await _context.Customers.FindAsync(adminId);
            if (admin == null)
            {
                return NotFound();
            }

            // Обновляем поля
            admin.FullName = FullName;
            admin.Email = Email;
            admin.Phone = Phone;
            admin.PassportNumber = PassportNumber;

            // Обновляем фото
            if (ImageFile != null && ImageFile.Length > 0)
            {
                // Удаляем старое фото
                if (!string.IsNullOrEmpty(admin.ImagePath))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, admin.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                // Сохраняем новое фото
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "admins");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                admin.ImagePath = "/uploads/admins/" + fileName;
            }

            await _context.SaveChangesAsync();

            // Обновляем имя в сессии
            HttpContext.Session.SetString("CustomerName", admin.FullName);

            TempData["Success"] = "Профиль успешно обновлен!";
            return RedirectToAction("Index");
        }

        // Смена пароля - GET
        [HttpGet]
        public IActionResult ChangePassword()
        {
            var adminId = HttpContext.Session.GetInt32("CustomerId");
            if (adminId == null)
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            return View();
        }

        // Смена пароля - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var adminId = HttpContext.Session.GetInt32("CustomerId");
            if (adminId == null)
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            var admin = await _context.Customers.FindAsync(adminId);
            if (admin == null)
            {
                return NotFound();
            }

            // Проверка текущего пароля
            if (string.IsNullOrEmpty(admin.PasswordHash) || 
                !BCrypt.Net.BCrypt.Verify(currentPassword, admin.PasswordHash))
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
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Пароль успешно изменен!";
            return View();
        }
    }
}