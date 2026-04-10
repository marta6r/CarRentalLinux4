using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;
using Microsoft.AspNetCore.Http;
using BCrypt.Net;

namespace CarRental.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Index
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("CustomerRole");
            if (role != "admin" && role != "manager")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            return RedirectToAction("Index", "Cars");
        }

        // GET: /Admin/Dashboard
        public IActionResult Dashboard()
        {
            var role = HttpContext.Session.GetString("CustomerRole");
            if (role != "admin" && role != "manager")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            return View();
        }

        // GET: /Admin/Employees - список сотрудников
        [HttpGet]
        public async Task<IActionResult> Employees()
        {
            var role = HttpContext.Session.GetString("CustomerRole");
            if (role != "admin")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            var employees = await _context.Customers
                .Where(c => c.Role == "admin" || c.Role == "manager")
                .ToListAsync();
            return View(employees);
        }

        // GET: /Admin/CreateEmployee - создание сотрудника
        [HttpGet]
        public IActionResult CreateEmployee()
        {
            var role = HttpContext.Session.GetString("CustomerRole");
            if (role != "admin")
            {
                return RedirectToAction("AdminLogin", "Account");
            }
            return View();
        }

        // POST: /Admin/CreateEmployee - создание сотрудника
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(string FullName, string Email, string Phone, string PassportNumber, string Password, string ConfirmPassword, string UserRole)
        {
            var currentRole = HttpContext.Session.GetString("CustomerRole");
            if (currentRole != "admin")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            // Проверка паролей
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

            // Проверка уникальности email
            if (await _context.Customers.AnyAsync(c => c.Email == Email))
            {
                ViewBag.Error = "Сотрудник с таким email уже существует";
                return View();
            }

            // Проверка уникальности телефона
            if (await _context.Customers.AnyAsync(c => c.Phone == Phone))
            {
                ViewBag.Error = "Сотрудник с таким телефоном уже существует";
                return View();
            }

            // Проверка уникальности паспорта
            if (await _context.Customers.AnyAsync(c => c.PassportNumber == PassportNumber))
            {
                ViewBag.Error = "Сотрудник с таким паспортом уже существует";
                return View();
            }

            // Создаем сотрудника
            var customer = new Customer
            {
                FullName = FullName,
                Email = Email,
                Phone = Phone,
                PassportNumber = PassportNumber,
                Role = UserRole,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password)
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Сотрудник {FullName} с ролью {(UserRole == "admin" ? "Администратор" : "Менеджер")} успешно создан!";
            return RedirectToAction("Employees");
        }

        // POST: /Admin/DeleteEmployee/5
        [HttpPost]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var role = HttpContext.Session.GetString("CustomerRole");
            if (role != "admin")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            var employee = await _context.Customers.FindAsync(id);
            if (employee != null && employee.Role != "admin")
            {
                _context.Customers.Remove(employee);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Сотрудник удален";
            }
            else if (employee != null && employee.Role == "admin")
            {
                TempData["Error"] = "Нельзя удалить администратора";
            }

            return RedirectToAction("Employees");
        }
    }
}