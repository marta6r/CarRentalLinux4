using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CarRental.Data;
using CarRental.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.Feature)
                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.FeatureValue)
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
                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.Feature)
                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.FeatureValue)
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
        public async Task<IActionResult> Create(Car car, IFormCollection form)
        {
            Console.WriteLine("=== ДЕБАГ СОЗДАНИЯ АВТО ===");
            Console.WriteLine($"Марка: {car.Brand ?? "NULL"}");
            Console.WriteLine($"Модель: {car.Model ?? "NULL"}");
            Console.WriteLine($"Год: {car.Year}");
            Console.WriteLine($"Цена: {car.DailyPrice}");
            Console.WriteLine($"Доступен: {car.IsAvailable}");
            Console.WriteLine($"Категория ID: {car.CategoryId}");

            // Удаляем валидацию для навигационных свойств
            ModelState.Remove("Category");
            ModelState.Remove("CarFeatures");

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
                // Сохраняем автомобиль
                _context.Add(car);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Автомобиль добавлен! ID: {car.Id}");
                
                // Сохраняем характеристики для выбранной категории
                if (car.CategoryId.HasValue)
                {
                    var categoryFeatures = await _context.CategoryFeatures
                        .Where(cf => cf.CategoryId == car.CategoryId.Value)
                        .Include(cf => cf.Feature)
                        .ToListAsync();
                    
                    foreach (var categoryFeature in categoryFeatures)
                    {
                        string key = $"feature_{categoryFeature.FeatureId}";
                        if (form.ContainsKey(key) && int.TryParse(form[key], out int featureValueId) && featureValueId > 0)
                        {
                            var carFeature = new CarFeature
                            {
                                CarId = car.Id,
                                FeatureId = categoryFeature.FeatureId,
                                FeatureValueId = featureValueId
                            };
                            _context.CarFeatures.Add(carFeature);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                
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
                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.Feature)
                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.FeatureValue)
                .FirstOrDefaultAsync(c => c.Id == id);
                
            if (car == null)
            {
                return NotFound();
            }
            
            ViewBag.Categories = new SelectList(await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync(), "Id", "Name", car.CategoryId);
            
            // Создаем словарь выбранных значений
            var selectedValues = new Dictionary<int, int>();
            foreach (var cf in car.CarFeatures)
            {
                selectedValues[cf.FeatureId] = cf.FeatureValueId;
            }
            ViewBag.SelectedFeatureValues = selectedValues;
                
            return View(car);
        }

        // POST: Cars/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Car car, IFormCollection form)
        {
            if (id != car.Id)
            {
                return NotFound();
            }

            // Удаляем валидацию для навигационных свойств
            ModelState.Remove("Category");
            ModelState.Remove("CarFeatures");

            // Валидация года выпуска
            if (car.Year < 1885 || car.Year > 2026)
            {
                ModelState.AddModelError("Year", "Год должен быть между 1885 и 2026");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Обновляем автомобиль
                    _context.Update(car);
                    await _context.SaveChangesAsync();
                    
                    // Удаляем старые характеристики
                    var oldFeatures = await _context.CarFeatures
                        .Where(cf => cf.CarId == car.Id)
                        .ToListAsync();
                    _context.CarFeatures.RemoveRange(oldFeatures);
                    
                    // Добавляем новые характеристики для выбранной категории
                    if (car.CategoryId.HasValue)
                    {
                        var categoryFeatures = await _context.CategoryFeatures
                            .Where(cf => cf.CategoryId == car.CategoryId.Value)
                            .Include(cf => cf.Feature)
                            .ToListAsync();
                        
                        foreach (var categoryFeature in categoryFeatures)
                        {
                            string key = $"feature_{categoryFeature.FeatureId}";
                            if (form.ContainsKey(key) && int.TryParse(form[key], out int featureValueId) && featureValueId > 0)
                            {
                                var carFeature = new CarFeature
                                {
                                    CarId = car.Id,
                                    FeatureId = categoryFeature.FeatureId,
                                    FeatureValueId = featureValueId
                                };
                                _context.CarFeatures.Add(carFeature);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                    
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
                
            // Создаем словарь выбранных значений для повторного отображения
            var selectedValues = new Dictionary<int, int>();
            foreach (var cf in car.CarFeatures)
            {
                selectedValues[cf.FeatureId] = cf.FeatureValueId;
            }
            ViewBag.SelectedFeatureValues = selectedValues;
                
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
                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.Feature)
                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.FeatureValue)
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

        // GET: Cars/GetFeaturesByCategory
        [HttpGet]
        public async Task<IActionResult> GetFeaturesByCategory(int categoryId)
        {
            var categoryFeatures = await _context.CategoryFeatures
                .Where(cf => cf.CategoryId == categoryId)
                .Include(cf => cf.Feature)
                    .ThenInclude(f => f.FeatureValues)
                .OrderBy(cf => cf.DisplayOrder)
                .ToListAsync();
            
            var result = categoryFeatures.Select(cf => new
            {
                id = cf.Feature.Id,
                name = cf.Feature.Name,
                values = cf.Feature.FeatureValues.Select(fv => new
                {
                    id = fv.Id,
                    value = fv.Value
                }).OrderBy(v => v.value)
            });
            
            return Json(result);
        }

        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.Id == id);
        }
    }
}