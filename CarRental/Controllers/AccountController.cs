using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.Models;
using CarRental.Data;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.ComponentModel.DataAnnotations;
using BCrypt.Net;

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

        // ==================== ОБЫЧНЫЙ ВХОД ====================
        
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string login, string password, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Введите email/телефон и пароль";
                return View();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == login || c.Phone == login);

            if (customer == null)
            {
                ViewBag.Error = "Неверный email/телефон или пароль";
                return View();
            }

            if (string.IsNullOrEmpty(customer.PasswordHash) || 
                !BCrypt.Net.BCrypt.Verify(password, customer.PasswordHash))
            {
                ViewBag.Error = "Неверный email/телефон или пароль";
                return View();
            }

            HttpContext.Session.SetInt32("CustomerId", customer.Id);
            HttpContext.Session.SetString("CustomerName", customer.FullName);
            HttpContext.Session.SetString("CustomerRole", customer.Role ?? "user");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "User");
        }

        // ==================== ОБЫЧНАЯ РЕГИСТРАЦИЯ ====================
        
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Customer customer, string ConfirmPassword, IFormFile? CustomerPhoto)
        {
            if (customer.Password != ConfirmPassword)
            {
                ViewBag.PasswordMismatch = "Пароли не совпадают";
                return View(customer);
            }

            if (string.IsNullOrEmpty(customer.Password) || customer.Password.Length < 6)
            {
                ModelState.AddModelError("Password", "Пароль должен быть не менее 6 символов");
                return View(customer);
            }

            if (ModelState.IsValid)
            {
                if (await _context.Customers.AnyAsync(c => c.Email == customer.Email))
                {
                    ModelState.AddModelError("Email", "Email уже зарегистрирован");
                    return View(customer);
                }

                if (await _context.Customers.AnyAsync(c => c.Phone == customer.Phone))
                {
                    ModelState.AddModelError("Phone", "Телефон уже зарегистрирован");
                    return View(customer);
                }

                if (await _context.Customers.AnyAsync(c => c.PassportNumber == customer.PassportNumber))
                {
                    ModelState.AddModelError("PassportNumber", "Паспорт уже зарегистрирован");
                    return View(customer);
                }

                customer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(customer.Password);
                customer.Password = null;
                customer.Role = "user";

                if (CustomerPhoto != null && CustomerPhoto.Length > 0)
                {
                    // ... код сохранения фото
                }

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetInt32("CustomerId", customer.Id);
                HttpContext.Session.SetString("CustomerName", customer.FullName);
                HttpContext.Session.SetString("CustomerRole", customer.Role);

                return RedirectToAction("Index", "User");
            }

            return View(customer);
        }

        // ==================== ВОССТАНОВЛЕНИЕ ПАРОЛЯ ====================
        
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Введите email";
                return View();
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            
            if (customer != null)
            {
                var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                    .Replace("/", "_").Replace("+", "-").TrimEnd('=');
                
                customer.PasswordResetToken = token;
                customer.PasswordResetTokenExpires = DateTime.Now.AddHours(2);
                
                await _context.SaveChangesAsync();
                
                var resetLink = $"{Request.Scheme}://{Request.Host}/Account/ResetPassword?email={Uri.EscapeDataString(customer.Email)}&token={Uri.EscapeDataString(token)}";
                
                ViewBag.ResetLink = resetLink;
                ViewBag.ShowLink = true;
                ViewBag.Message = "Ссылка для сброса пароля отправлена на ваш email.";
                
                return View("ForgotPasswordConfirmation");
            }
            
            ViewBag.Message = "Если этот email зарегистрирован, мы отправили ссылку для сброса пароля.";
            ViewBag.ShowLink = false;
            return View("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Email == email);
            
            if (customer == null || 
                customer.PasswordResetToken != token || 
                customer.PasswordResetTokenExpires < DateTime.Now)
            {
                ViewBag.Error = "Ссылка для сброса пароля недействительна или истекла.";
                return View("ResetPasswordInvalid");
            }
            
            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == model.Email);
            
            if (customer == null || 
                customer.PasswordResetToken != model.Token || 
                customer.PasswordResetTokenExpires < DateTime.Now)
            {
                ViewBag.Error = "Ссылка для сброса пароля недействительна или истекла.";
                return View("ResetPasswordInvalid");
            }
            
            if (string.IsNullOrEmpty(model.NewPassword) || model.NewPassword.Length < 6)
            {
                ModelState.AddModelError("NewPassword", "Пароль должен быть не менее 6 символов");
                return View(model);
            }
            
            customer.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            customer.PasswordResetToken = null;
            customer.PasswordResetTokenExpires = null;
            
            await _context.SaveChangesAsync();
            
            ViewBag.Message = "Пароль успешно изменен! Теперь вы можете войти с новым паролем.";
            return View("ResetPasswordConfirmation");
        }

        // ==================== АДМИНИСТРАТОР: РЕГИСТРАЦИЯ (если нет админа) ====================
        
        [HttpGet]
        public IActionResult AdminRegister()
        {
            var adminExists = _context.Customers.Any(c => c.Role == "admin");
            if (adminExists)
            {
                return RedirectToAction("AdminLogin");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AdminRegister(string FullName, string Email, string Phone, string PassportNumber, string Password, string ConfirmPassword)
        {
            if (Password != ConfirmPassword)
            {
                ViewBag.Error = "Пароли не совпадают";
                return View();
            }

            if (string.IsNullOrEmpty(Password) || Password.Length < 6)
            {
                ViewBag.Error = "Пароль должен быть не менее 6 символов";
                return View();
            }

            if (await _context.Customers.AnyAsync(c => c.Email == Email))
            {
                ViewBag.Error = "Email уже зарегистрирован";
                return View();
            }

            var customer = new Customer
            {
                FullName = FullName,
                Email = Email,
                Phone = Phone,
                PassportNumber = PassportNumber,
                Role = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password)
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("CustomerId", customer.Id);
            HttpContext.Session.SetString("CustomerName", customer.FullName);
            HttpContext.Session.SetString("CustomerRole", customer.Role);

            return RedirectToAction("Index", "Cars");
        }

        // ==================== АДМИНИСТРАТОР: ВХОД ====================
        
        [HttpGet]
        public IActionResult AdminLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AdminLogin(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Введите email/телефон и пароль";
                return View();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == login || c.Phone == login);

            if (customer == null)
            {
                ViewBag.Error = "Неверный email/телефон или пароль";
                return View();
            }

            if (string.IsNullOrEmpty(customer.PasswordHash) || 
                !BCrypt.Net.BCrypt.Verify(password, customer.PasswordHash))
            {
                ViewBag.Error = "Неверный email/телефон или пароль";
                return View();
            }

            if (customer.Role != "admin" && customer.Role != "manager")
            {
                ViewBag.Error = "У вас нет прав доступа к панели администратора";
                return View();
            }

            HttpContext.Session.SetInt32("CustomerId", customer.Id);
            HttpContext.Session.SetString("CustomerName", customer.FullName);
            HttpContext.Session.SetString("CustomerRole", customer.Role);

            return RedirectToAction("Index", "Cars");
        }

        // ==================== ВЫХОД ====================
        
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "MainHome");
        }
    }

    // Модель для сброса пароля
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        public string Email { get; set; }
        
        public string Token { get; set; }
        
        [Required(ErrorMessage = "Введите новый пароль")]
        [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }
        
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; }
    }
}