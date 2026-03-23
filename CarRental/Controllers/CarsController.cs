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
                
                // Сохраняем характеристики для выбранной категории (обязательные)
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
                            Console.WriteLine($"Обязательная характеристика добавлена: {categoryFeature.Feature.Name}");
                        }
                    }
                }
                
                // Выводим все ключи для дебага
                Console.WriteLine("=== ВСЕ КЛЮЧИ ИЗ ФОРМЫ ===");
                foreach (var key in form.Keys)
                {
                    Console.WriteLine($"КЛЮЧ: {key} = {form[key]}");
                }
                
                // Сохраняем дополнительные характеристики
                Console.WriteLine("=== ОБРАБОТКА ДОПОЛНИТЕЛЬНЫХ ХАРАКТЕРИСТИК ===");
                foreach (var key in form.Keys)
                {
                    if (key.StartsWith("additional_feature_") && !key.Contains("_name_"))
                    {
                        var featureIdStr = key.Replace("additional_feature_", "");
                        Console.WriteLine($"featureIdStr: {featureIdStr}");
                        
                        if (long.TryParse(featureIdStr, out long tempFeatureId))
                        {
                            var value = form[key].ToString();
                            Console.WriteLine($"value: {value}");
                            
                            if (!string.IsNullOrEmpty(value))
                            {
                                string featureName = form["additional_feature_name_" + tempFeatureId].ToString();
                                if (string.IsNullOrEmpty(featureName))
                                {
                                    featureName = "Новая характеристика";
                                }
                                
                                Console.WriteLine($"Найдена характеристика: {featureName} = {value}");
                                
                                // Ищем существующую характеристику по имени
                                var existingFeature = await _context.Features
                                    .FirstOrDefaultAsync(f => f.Name == featureName);
                                
                                int actualFeatureId;
                                FeatureValue featureValue = null;
                                
                                // ПРОВЕРЯЕМ: является ли value числом (ID) или текстом
                                bool isValueId = int.TryParse(value, out int parsedValueId);
                                
                                if (existingFeature != null)
                                {
                                    actualFeatureId = existingFeature.Id;
                                    Console.WriteLine($"Найдена существующая характеристика: {featureName} (ID: {actualFeatureId})");
                                    
                                    if (isValueId)
                                    {
                                        // Если value - это ID, ищем существующее значение по ID
                                        featureValue = await _context.FeatureValues
                                            .FirstOrDefaultAsync(fv => fv.Id == parsedValueId && fv.FeatureId == actualFeatureId);
                                        
                                        if (featureValue != null)
                                        {
                                            Console.WriteLine($"Найдено существующее значение по ID {parsedValueId}: {featureValue.Value}");
                                        }
                                        else
                                        {
                                            // Если ID не найден, создаем новое значение с текстом
                                            featureValue = new FeatureValue
                                            {
                                                FeatureId = actualFeatureId,
                                                Value = value
                                            };
                                            _context.FeatureValues.Add(featureValue);
                                            await _context.SaveChangesAsync();
                                            Console.WriteLine($"Создано новое значение: {value} (был передан ID, но не найден)");
                                        }
                                    }
                                    else
                                    {
                                        // Если value - это текст, ищем существующее значение по тексту
                                        featureValue = await _context.FeatureValues
                                            .FirstOrDefaultAsync(fv => fv.FeatureId == actualFeatureId && fv.Value == value);
                                        
                                        if (featureValue == null)
                                        {
                                            // Создаем новое значение
                                            featureValue = new FeatureValue
                                            {
                                                FeatureId = actualFeatureId,
                                                Value = value
                                            };
                                            _context.FeatureValues.Add(featureValue);
                                            await _context.SaveChangesAsync();
                                            Console.WriteLine($"Создано новое значение: {value} для характеристики {featureName}");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Найдено существующее значение: {value}");
                                        }
                                    }
                                }
                                else
                                {
                                    // Создаем новую характеристику
                                    Console.WriteLine($"Создаем новую характеристику: {featureName}");
                                    var newFeature = new Feature
                                    {
                                        Name = featureName
                                    };
                                    _context.Features.Add(newFeature);
                                    await _context.SaveChangesAsync();
                                    actualFeatureId = newFeature.Id;
                                    Console.WriteLine($"Создана новая характеристика: {featureName} (ID: {actualFeatureId})");
                                    
                                    // Создаем значение для новой характеристики
                                    featureValue = new FeatureValue
                                    {
                                        FeatureId = actualFeatureId,
                                        Value = value
                                    };
                                    _context.FeatureValues.Add(featureValue);
                                    await _context.SaveChangesAsync();
                                    Console.WriteLine($"Создано значение: {value} для новой характеристики");
                                }
                                
                                if (featureValue != null)
                                {
                                    var carFeature = new CarFeature
                                    {
                                        CarId = car.Id,
                                        FeatureId = actualFeatureId,
                                        FeatureValueId = featureValue.Id
                                    };
                                    _context.CarFeatures.Add(carFeature);
                                    Console.WriteLine($"Создана связь: CarId={car.Id}, FeatureId={actualFeatureId}, ValueId={featureValue.Id}");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"НЕ УДАЛОСЬ ПАРСИТЬ ID (long): {featureIdStr}");
                        }
                    }
                }
                
                await _context.SaveChangesAsync();
                Console.WriteLine("=== ВСЕ ХАРАКТЕРИСТИКИ СОХРАНЕНЫ ===");
                
                TempData["Success"] = "Автомобиль успешно добавлен!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
            
            // Получаем ID обязательных характеристик для категории
            var requiredFeatureIds = new List<int>();
            if (car.CategoryId.HasValue)
            {
                requiredFeatureIds = await _context.CategoryFeatures
                    .Where(cf => cf.CategoryId == car.CategoryId.Value)
                    .Select(cf => cf.FeatureId)
                    .ToListAsync();
            }
            
            // Создаем словарь выбранных значений для обязательных характеристик
            var selectedValues = new Dictionary<int, int>();
            foreach (var cf in car.CarFeatures.Where(cf => requiredFeatureIds.Contains(cf.FeatureId)))
            {
                selectedValues[cf.FeatureId] = cf.FeatureValueId;
            }
            ViewBag.SelectedFeatureValues = selectedValues;
            
            // Создаем список дополнительных характеристик (которые не входят в обязательные)
            var additionalFeatures = car.CarFeatures
                .Where(cf => !requiredFeatureIds.Contains(cf.FeatureId))
                .Select(cf => new
                {
                    featureId = cf.FeatureId,
                    featureName = cf.Feature.Name,
                    valueId = cf.FeatureValueId,
                    valueText = cf.FeatureValue.Value
                })
                .ToList();
            ViewBag.AdditionalFeatures = additionalFeatures;
                
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
                    
                    // 1. ОБНОВЛЯЕМ обязательные характеристики
                    var requiredFeatureIds = new List<int>();
                    if (car.CategoryId.HasValue)
                    {
                        requiredFeatureIds = await _context.CategoryFeatures
                            .Where(cf => cf.CategoryId == car.CategoryId.Value)
                            .Select(cf => cf.FeatureId)
                            .ToListAsync();
                        
                        // Удаляем только обязательные характеристики
                        var oldRequiredFeatures = await _context.CarFeatures
                            .Where(cf => cf.CarId == car.Id && requiredFeatureIds.Contains(cf.FeatureId))
                            .ToListAsync();
                        _context.CarFeatures.RemoveRange(oldRequiredFeatures);
                        
                        // Добавляем новые значения для обязательных характеристик
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
                    }
                    
                    // 2. ОБНОВЛЯЕМ существующие дополнительные характеристики
                    Console.WriteLine("=== ОБНОВЛЕНИЕ СУЩЕСТВУЮЩИХ ДОПОЛНИТЕЛЬНЫХ ХАРАКТЕРИСТИК ===");
                    foreach (var key in form.Keys)
                    {
                        if (key.StartsWith("existing_feature_value_"))
                        {
                            var featureIdStr = key.Replace("existing_feature_value_", "");
                            if (int.TryParse(featureIdStr, out int featureId))
                            {
                                var valueIdStr = form[key].ToString();
                                if (int.TryParse(valueIdStr, out int valueId) && valueId > 0)
                                {
                                    var existingCarFeature = await _context.CarFeatures
                                        .FirstOrDefaultAsync(cf => cf.CarId == car.Id && cf.FeatureId == featureId);
                                    
                                    if (existingCarFeature != null)
                                    {
                                        existingCarFeature.FeatureValueId = valueId;
                                        Console.WriteLine($"Обновлена характеристика: FeatureId={featureId}, NewValueId={valueId}");
                                    }
                                    else
                                    {
                                        var carFeature = new CarFeature
                                        {
                                            CarId = car.Id,
                                            FeatureId = featureId,
                                            FeatureValueId = valueId
                                        };
                                        _context.CarFeatures.Add(carFeature);
                                        Console.WriteLine($"Добавлена характеристика: FeatureId={featureId}, ValueId={valueId}");
                                    }
                                }
                            }
                        }
                    }
                    
                    // 3. УДАЛЯЕМ характеристики, которые были отмечены на удаление
                    Console.WriteLine("=== УДАЛЕНИЕ ХАРАКТЕРИСТИК ===");
                    foreach (var key in form.Keys)
                    {
                        if (key.StartsWith("remove_feature_"))
                        {
                            var featureIdStr = key.Replace("remove_feature_", "");
                            if (int.TryParse(featureIdStr, out int featureId))
                            {
                                var carFeature = await _context.CarFeatures
                                    .FirstOrDefaultAsync(cf => cf.CarId == car.Id && cf.FeatureId == featureId);
                                
                                if (carFeature != null)
                                {
                                    _context.CarFeatures.Remove(carFeature);
                                    Console.WriteLine($"Удалена характеристика: FeatureId={featureId}");
                                }
                            }
                        }
                    }
                    
                    // 4. ДОБАВЛЯЕМ новые дополнительные характеристики
                    Console.WriteLine("=== ДОБАВЛЕНИЕ НОВЫХ ДОПОЛНИТЕЛЬНЫХ ХАРАКТЕРИСТИК ===");
                    foreach (var key in form.Keys)
                    {
                        if (key.StartsWith("additional_feature_") && !key.Contains("_name_"))
                        {
                            var featureIdStr = key.Replace("additional_feature_", "");
                            if (long.TryParse(featureIdStr, out long tempFeatureId))
                            {
                                var value = form[key].ToString();
                                if (!string.IsNullOrEmpty(value))
                                {
                                    string featureName = form["additional_feature_name_" + tempFeatureId].ToString();
                                    if (string.IsNullOrEmpty(featureName))
                                    {
                                        featureName = "Новая характеристика";
                                    }
                                    
                                    Console.WriteLine($"Найдена новая характеристика: {featureName} = {value}");
                                    
                                    // Ищем существующую характеристику по имени
                                    var existingFeature = await _context.Features
                                        .FirstOrDefaultAsync(f => f.Name == featureName);
                                    
                                    int actualFeatureId;
                                    FeatureValue featureValue = null;
                                    
                                    bool isValueId = int.TryParse(value, out int parsedValueId);
                                    
                                    if (existingFeature != null)
                                    {
                                        actualFeatureId = existingFeature.Id;
                                        
                                        if (isValueId)
                                        {
                                            featureValue = await _context.FeatureValues
                                                .FirstOrDefaultAsync(fv => fv.Id == parsedValueId && fv.FeatureId == actualFeatureId);
                                            
                                            if (featureValue == null)
                                            {
                                                featureValue = new FeatureValue
                                                {
                                                    FeatureId = actualFeatureId,
                                                    Value = value
                                                };
                                                _context.FeatureValues.Add(featureValue);
                                                await _context.SaveChangesAsync();
                                            }
                                        }
                                        else
                                        {
                                            featureValue = await _context.FeatureValues
                                                .FirstOrDefaultAsync(fv => fv.FeatureId == actualFeatureId && fv.Value == value);
                                            
                                            if (featureValue == null)
                                            {
                                                featureValue = new FeatureValue
                                                {
                                                    FeatureId = actualFeatureId,
                                                    Value = value
                                                };
                                                _context.FeatureValues.Add(featureValue);
                                                await _context.SaveChangesAsync();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var newFeature = new Feature
                                        {
                                            Name = featureName
                                        };
                                        _context.Features.Add(newFeature);
                                        await _context.SaveChangesAsync();
                                        actualFeatureId = newFeature.Id;
                                        
                                        featureValue = new FeatureValue
                                        {
                                            FeatureId = actualFeatureId,
                                            Value = value
                                        };
                                        _context.FeatureValues.Add(featureValue);
                                        await _context.SaveChangesAsync();
                                    }
                                    
                                    if (featureValue != null)
                                    {
                                        // Проверяем, не существует ли уже такая характеристика у автомобиля
                                        var existingCarFeature = await _context.CarFeatures
                                            .FirstOrDefaultAsync(cf => cf.CarId == car.Id && cf.FeatureId == actualFeatureId);
                                        
                                        if (existingCarFeature == null)
                                        {
                                            var carFeature = new CarFeature
                                            {
                                                CarId = car.Id,
                                                FeatureId = actualFeatureId,
                                                FeatureValueId = featureValue.Id
                                            };
                                            _context.CarFeatures.Add(carFeature);
                                            Console.WriteLine($"Добавлена новая характеристика: FeatureId={actualFeatureId}, ValueId={featureValue.Id}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
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
            try
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
                    values = cf.Feature.FeatureValues
                        .Select(fv => new
                        {
                            id = fv.Id,
                            value = fv.Value
                        })
                        .OrderBy(v => v.value)
                        .ToList()
                });
                
                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в GetFeaturesByCategory: {ex.Message}");
                return Json(new List<object>());
            }
        }

        // GET: Cars/GetAllFeatures
        [HttpGet]
        public async Task<IActionResult> GetAllFeatures()
        {
            try
            {
                var features = await _context.Features
                    .Include(f => f.FeatureValues)
                    .OrderBy(f => f.Name)
                    .Select(f => new
                    {
                        id = f.Id,
                        name = f.Name,
                        values = f.FeatureValues
                            .Select(v => new { id = v.Id, value = v.Value })
                            .OrderBy(v => v.value)
                            .ToList()
                    })
                    .ToListAsync();
                
                return Json(features);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в GetAllFeatures: {ex.Message}");
                return Json(new List<object>());
            }
        }

        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.Id == id);
        }
    }
}