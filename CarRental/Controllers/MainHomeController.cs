using Microsoft.AspNetCore.Mvc;

namespace CarRental.Controllers
{
    public class MainHomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}