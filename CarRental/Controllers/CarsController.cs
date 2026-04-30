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
using System.IO;

#nullable disable

namespace CarRental.Controllers
{
    public class CarsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CarsController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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

            ModelState.Remove("Category");
            ModelState.Remove("CarFeatures");

            if (car.Year < 1885 || car.Year > 2026)
            {
                ModelState.AddModelError("Year", "Год должен быть между 1885 и 2026");
                Console.WriteLine("Ошибка валидации: Неверный год");
            }

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
                if (car.ImageFile != null && car.ImageFile.Length > 0)
                {
                    if (car.ImageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImageFile", "Размер файла не должен превышать 5MB");
                        ViewBag.Categories = new SelectList(await _context.Categories
                            .OrderBy(c => c.Name)
                            .ToListAsync(), "Id", "Name", car.CategoryId);
                        return View(car);
                    }
                    
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    string fileExtension = Path.GetExtension(car.ImageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ImageFile", "Поддерживаются только форматы: JPG, JPEG, PNG, GIF, WEBP");
                        ViewBag.Categories = new SelectList(await _context.Categories
                            .OrderBy(c => c.Name)
                            .ToListAsync(), "Id", "Name", car.CategoryId);
                        return View(car);
                    }
                    
                    string fileName = Guid.NewGuid().ToString() + fileExtension;
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "cars");
                    
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    
                    string filePath = Path.Combine(uploadsFolder, fileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await car.ImageFile.CopyToAsync(fileStream);
                    }
                    
                    car.ImagePath = "/images/cars/" + fileName;
                }
                
                _context.Add(car);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Автомобиль добавлен! ID: {car.Id}");
                
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
                
                Console.WriteLine("=== ВСЕ КЛЮЧИ ИЗ ФОРМЫ ===");
                foreach (var key in form.Keys)
                {
                    Console.WriteLine($"КЛЮЧ: {key} = {form[key]}");
                }
                
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
                                
                                var existingFeature = await _context.Features
                                    .FirstOrDefaultAsync(f => f.Name == featureName);
                                
                                int actualFeatureId;
                                FeatureValue featureValue = null;
                                
                                bool isValueId = int.TryParse(value, out int parsedValueId);
                                
                                if (existingFeature != null)
                                {
                                    actualFeatureId = existingFeature.Id;
                                    Console.WriteLine($"Найдена существующая характеристика: {featureName} (ID: {actualFeatureId})");
                                    
                                    if (isValueId)
                                    {
                                        featureValue = await _context.FeatureValues
                                            .FirstOrDefaultAsync(fv => fv.Id == parsedValueId && fv.FeatureId == actualFeatureId);
                                        
                                        if (featureValue != null)
                                        {
                                            Console.WriteLine($"Найдено существующее значение по ID {parsedValueId}: {featureValue.Value}");
                                        }
                                        else
                                        {
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
                                    Console.WriteLine($"Создаем новую характеристику: {featureName}");
                                    var newFeature = new Feature
                                    {
                                        Name = featureName
                                    };
                                    _context.Features.Add(newFeature);
                                    await _context.SaveChangesAsync();
                                    actualFeatureId = newFeature.Id;
                                    Console.WriteLine($"Создана новая характеристика: {featureName} (ID: {actualFeatureId})");
                                    
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
                                    // Проверяем, нет ли уже такой связи
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
                                        Console.WriteLine($"Создана связь: CarId={car.Id}, FeatureId={actualFeatureId}, ValueId={featureValue.Id}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Связь уже существует: CarId={car.Id}, FeatureId={actualFeatureId}");
                                    }
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
                        .ThenInclude(f => f.FeatureValues)
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
            
            return View(car);
        }

        // POST: Cars/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Car car, IFormCollection form, string DeletePhoto)
        {
            if (id != car.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Category");
            ModelState.Remove("CarFeatures");

            if (car.Year < 1885 || car.Year > 2026)
            {
                ModelState.AddModelError("Year", "Год должен быть между 1885 и 2026");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCar = await _context.Cars
                        .Include(c => c.CarFeatures)
                        .FirstOrDefaultAsync(c => c.Id == id);
                        
                    if (existingCar == null)
                    {
                        return NotFound();
                    }
                    
                    existingCar.Brand = car.Brand;
                    existingCar.Model = car.Model;
                    existingCar.Year = car.Year;
                    existingCar.DailyPrice = car.DailyPrice;
                    existingCar.IsAvailable = car.IsAvailable;
                    existingCar.CategoryId = car.CategoryId;
                    
                    // Обработка удаления фото
                    if (DeletePhoto == "true" && !string.IsNullOrEmpty(existingCar.ImagePath))
                    {
                        string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingCar.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                        existingCar.ImagePath = null;
                    }
                    
                    // Сохраняем новое изображение
                    if (car.ImageFile != null && car.ImageFile.Length > 0)
                    {
                        if (car.ImageFile.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("ImageFile", "Размер файла не должен превышать 5MB");
                            ViewBag.Categories = new SelectList(await _context.Categories
                                .OrderBy(c => c.Name)
                                .ToListAsync(), "Id", "Name", car.CategoryId);
                            return View(car);
                        }
                        
                        string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                        string fileExtension = Path.GetExtension(car.ImageFile.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("ImageFile", "Поддерживаются только форматы: JPG, JPEG, PNG, GIF, WEBP");
                            ViewBag.Categories = new SelectList(await _context.Categories
                                .OrderBy(c => c.Name)
                                .ToListAsync(), "Id", "Name", car.CategoryId);
                            return View(car);
                        }
                        
                        if (!string.IsNullOrEmpty(existingCar.ImagePath) && DeletePhoto != "true")
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingCar.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }
                        
                        string fileName = Guid.NewGuid().ToString() + fileExtension;
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "cars");
                        
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        
                        string filePath = Path.Combine(uploadsFolder, fileName);
                        
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await car.ImageFile.CopyToAsync(fileStream);
                        }
                        
                        existingCar.ImagePath = "/images/cars/" + fileName;
                    }
                    
                    // Сохраняем основные данные автомобиля
                    _context.Update(existingCar);
                    await _context.SaveChangesAsync();
                    
                    // ========== ОБНОВЛЕНИЕ ХАРАКТЕРИСТИК ==========
                    
                    // Получаем ID обязательных характеристик для текущей категории
                    var requiredFeatureIds = new List<int>();
                    if (existingCar.CategoryId.HasValue)
                    {
                        requiredFeatureIds = await _context.CategoryFeatures
                            .Where(cf => cf.CategoryId == existingCar.CategoryId.Value)
                            .Select(cf => cf.FeatureId)
                            .ToListAsync();
                    }
                    
                    // Получаем список существующих характеристик автомобиля
                    var existingFeatures = await _context.CarFeatures
                        .Where(cf => cf.CarId == existingCar.Id)
                        .ToListAsync();
                    
                    // 1. ОБНОВЛЕНИЕ ОБЯЗАТЕЛЬНЫХ ХАРАКТЕРИСТИК (категорийных)
                    if (existingCar.CategoryId.HasValue)
                    {
                        // Удаляем старые обязательные характеристики
                        var oldRequiredFeatures = existingFeatures
                            .Where(cf => requiredFeatureIds.Contains(cf.FeatureId))
                            .ToList();
                        if (oldRequiredFeatures.Any())
                        {
                            _context.CarFeatures.RemoveRange(oldRequiredFeatures);
                        }
                        
                        // Добавляем новые значения для обязательных характеристик
                        var categoryFeatures = await _context.CategoryFeatures
                            .Where(cf => cf.CategoryId == existingCar.CategoryId.Value)
                            .Include(cf => cf.Feature)
                            .ToListAsync();
                        
                        foreach (var categoryFeature in categoryFeatures)
                        {
                            string key = $"category_feature_{categoryFeature.FeatureId}";
                            if (form.ContainsKey(key) && int.TryParse(form[key], out int featureValueId) && featureValueId > 0)
                            {
                                var carFeature = new CarFeature
                                {
                                    CarId = existingCar.Id,
                                    FeatureId = categoryFeature.FeatureId,
                                    FeatureValueId = featureValueId
                                };
                                _context.CarFeatures.Add(carFeature);
                                Console.WriteLine($"Обязательная характеристика добавлена: {categoryFeature.Feature.Name} = {featureValueId}");
                            }
                        }
                    }
                    
                    // 2. ОБНОВЛЕНИЕ/ДОБАВЛЕНИЕ ДОПОЛНИТЕЛЬНЫХ ХАРАКТЕРИСТИК
                    foreach (var key in form.Keys)
                    {
                        // Пропускаем служебные ключи и обязательные (они уже обработаны)
                        if (key.StartsWith("additional_feature_") || 
                            key.Contains("_name_") || 
                            key == "DeletePhoto" ||
                            key == "__RequestVerificationToken" ||
                            key.StartsWith("category_feature_"))
                        {
                            continue;
                        }
                        
                        // Обрабатываем дополнительные характеристики (формат: feature_123)
                        if (key.StartsWith("feature_") && !key.StartsWith("delete_feature_"))
                        {
                            string featureIdStr = key.Replace("feature_", "");
                            if (int.TryParse(featureIdStr, out int featureId))
                            {
                                var valueIdStr = form[key].ToString();
                                if (int.TryParse(valueIdStr, out int valueId) && valueId > 0)
                                {
                                    var existingFeature = existingFeatures.FirstOrDefault(cf => cf.FeatureId == featureId);
                                    if (existingFeature != null)
                                    {
                                        existingFeature.FeatureValueId = valueId;
                                        Console.WriteLine($"Обновлена дополнительная характеристика {featureId}: новое значение ID = {valueId}");
                                    }
                                    else
                                    {
                                        var carFeature = new CarFeature
                                        {
                                            CarId = existingCar.Id,
                                            FeatureId = featureId,
                                            FeatureValueId = valueId
                                        };
                                        _context.CarFeatures.Add(carFeature);
                                        Console.WriteLine($"Добавлена новая дополнительная характеристика {featureId}: значение ID = {valueId}");
                                    }
                                }
                            }
                        }
                    }
                    
                    // 3. УДАЛЕНИЕ ХАРАКТЕРИСТИК
                    foreach (var key in form.Keys)
                    {
                        if (key.StartsWith("delete_feature_"))
                        {
                            var featureIdStr = key.Replace("delete_feature_", "");
                            if (int.TryParse(featureIdStr, out int featureId))
                            {
                                var carFeature = existingFeatures.FirstOrDefault(cf => cf.FeatureId == featureId);
                                if (carFeature != null)
                                {
                                    _context.CarFeatures.Remove(carFeature);
                                    Console.WriteLine($"Удалена характеристика {featureId}");
                                }
                            }
                        }
                    }
                    
                    // 4. ДОБАВЛЕНИЕ НОВЫХ ДОПОЛНИТЕЛЬНЫХ ХАРАКТЕРИСТИК (через кнопку "Добавить характеристику")
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
                                    
                                    // Проверяем, нет ли уже такой характеристики
                                    var existingFeatureInCar = existingFeatures
                                        .FirstOrDefault(cf => cf.Feature != null && cf.Feature.Name == featureName);
                                    
                                    if (existingFeatureInCar != null)
                                    {
                                        Console.WriteLine($"Характеристика {featureName} уже существует, пропускаем");
                                        continue;
                                    }
                                    
                                    // Ищем или создаем характеристику
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
                                        var carFeature = new CarFeature
                                        {
                                            CarId = existingCar.Id,
                                            FeatureId = actualFeatureId,
                                            FeatureValueId = featureValue.Id
                                        };
                                        _context.CarFeatures.Add(carFeature);
                                        Console.WriteLine($"Добавлена новая характеристика: {featureName} = {value}");
                                    }
                                }
                            }
                        }
                    }
                    
                    // Сохраняем все изменения характеристик
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Данные автомобиля обновлены";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"DbUpdateException: {ex.Message}");
                    Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                    ModelState.AddModelError("", $"Ошибка базы данных: {ex.InnerException?.Message ?? ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка сохранения: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    ModelState.AddModelError("", $"Ошибка сохранения: {ex.Message}");
                }
            }
            
            // Перезагружаем данные для повторного отображения формы
            var reloadedCar = await _context.Cars
                .Include(c => c.CarFeatures)
                    .ThenInclude(cf => cf.Feature)
                .FirstOrDefaultAsync(c => c.Id == id);
                
            if (reloadedCar != null)
            {
                ViewBag.Categories = new SelectList(await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync(), "Id", "Name", car.CategoryId);
            }
                
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
                if (!string.IsNullOrEmpty(car.ImagePath))
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, car.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                
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

        // GET: Cars/GetCarFeatures (для AJAX)
        [HttpGet]
        public async Task<IActionResult> GetCarFeatures(int carId)
        {
            try
            {
                var car = await _context.Cars
                    .Include(c => c.CarFeatures)
                        .ThenInclude(cf => cf.Feature)
                    .Include(c => c.CarFeatures)
                        .ThenInclude(cf => cf.FeatureValue)
                    .FirstOrDefaultAsync(c => c.Id == carId);
                
                if (car == null)
                {
                    return Json(new List<object>());
                }
                
                var result = car.CarFeatures.Select(cf => new
                {
                    id = cf.Id,
                    featureId = cf.FeatureId,
                    featureName = cf.Feature.Name,
                    valueId = cf.FeatureValueId,
                    valueText = cf.FeatureValue?.Value ?? ""
                });
                
                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в GetCarFeatures: {ex.Message}");
                return Json(new List<object>());
            }
        }
        
        // GET: Cars/GetFeatureValues
        [HttpGet]
        public async Task<IActionResult> GetFeatureValues(int featureId)
        {
            try
            {
                var values = await _context.FeatureValues
                    .Where(v => v.FeatureId == featureId)
                    .Select(v => new { id = v.Id, value = v.Value })
                    .OrderBy(v => v.value)
                    .ToListAsync();
                
                return Json(values);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в GetFeatureValues: {ex.Message}");
                return Json(new List<object>());
            }
        }

        private bool CarExists(int id)
        {
            return _context.Cars.Any(e => e.Id == id);
        }
    }
}