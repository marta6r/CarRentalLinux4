using Microsoft.AspNetCore.Mvc;
using CarRental.Data;
using Microsoft.AspNetCore.Http;

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

            // Перенаправляем на список автомобилей
            return RedirectToAction("Index", "Cars");
        }

        // GET: /Admin/Dashboard - панель управления (опционально)
        public IActionResult Dashboard()
        {
            var role = HttpContext.Session.GetString("CustomerRole");
            if (role != "admin" && role != "manager")
            {
                return RedirectToAction("AdminLogin", "Account");
            }

            return View();
        }
    }
}