using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

#nullable disable

namespace CarRental.Controllers
{
    public class CarsController : Controller
    {
        private readonly AppDbContext _context;

        public CarsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Cars
        public async Task<IActionResult> Index()
        {
            var cars = await _context.Cars
                .Include(c => c.Category)
                .ToListAsync();
            return View(cars);
        }

        // GET: Cars/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .Include(c => c.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // GET: Cars/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync(), "Id", "Name");
            
            return View();
        }

        // POST: Cars/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Car car)
        {
            Console.WriteLine("=== ДЕБАГ СОЗДАНИЯ АВТО ===");
            Console.WriteLine($"Марка: {car.Brand ?? "NULL"}");
            Console.WriteLine($"Модель: {car.Model ?? "NULL"}");
            Console.WriteLine($"Год: {car.Year}");
            Console.WriteLine($"Цена: {car.DailyPrice}");
            Console.WriteLine($"Доступен: {car.IsAvailable}");
            Console.WriteLine($"Категория ID: {car.CategoryId}");

            // Удаляем валидацию для навигационного свойства Category
            ModelState.Remove("Category");

            // Валидация года выпуска
            if (car.Year < 1885 || car.Year > 2026)
            {
                ModelState.AddModelError("Year", "Год должен быть между 1885 и 2026");
                Console.WriteLine("Ошибка валидации: Неверный год");
            }

            // Проверяем ModelState
            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== ОШИБКИ VALIDATION ===");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Count > 0)
                    {
                        Console.WriteLine($"Поле '{key}':");
                        foreach (var error in state.Errors)
                        {
                            Console.WriteLine($"  - {error.ErrorMessage}");
                        }
                    }
                }
                
                ViewBag.Categories = new SelectList(await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync(), "Id", "Name", car.CategoryId);
                    
                return View(car);
            }

            try
            {
                _context.Add(car);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Автомобиль добавлен! ID: {car.Id}");
                TempData["Success"] = "Автомобиль успешно добавлен!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения: {ex.Message}");
                ModelState.AddModelError("", $"Ошибка сохранения: {ex.Message}");
                
                ViewBag.Categories = new SelectList(await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync(), "Id", "Name", car.CategoryId);
                    
                return View(car);
            }
        }

        // GET: Cars/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id);
                
            if (car == null)
            {
                return NotFound();
            }
            
            ViewBag.Categories = new SelectList(await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync(), "Id", "Name", car.CategoryId);
                
            return View(car);
        }

        // POST: Cars/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Car car)
        {
            if (id != car.Id)
            {
                return NotFound();
            }

            // Удаляем валидацию для навигационного свойства Category
            ModelState.Remove("Category");

            // Валидация года выпуска
            if (car.Year < 1885 || car.Year > 2026)
            {
                ModelState.AddModelError("Year", "Год должен быть между 1885 и 2026");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(car);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Данные автомобиля обновлены";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    Console.WriteLine($"Ошибка конкурентности: {ex.Message}");
                    if (!CarExists(car.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка сохранения: {ex.Message}");
                    ModelState.AddModelError("", $"Ошибка сохранения: {ex.Message}");
                }
            }
            
            ViewBag.Categories = new SelectList(await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync(), "Id", "Name", car.CategoryId);
                
            return View(car);
        }

        // GET: Cars/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .Include(c => c.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // POST: Cars/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }

            try
            {
                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Автомобиль успешно удален";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления: {ex.Message}");
                TempData["Error"] = $"Ошибка при удалении: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.Id == id);
        }
    }
}