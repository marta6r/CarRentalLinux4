// using CarRental.Data;
// using Microsoft.EntityFrameworkCore;
// using System.Globalization;
// using Microsoft.AspNetCore.Mvc.ModelBinding;
// using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
// #nullable disable
// var builder = WebApplication.CreateBuilder(args);

// // Настройка культуры для корректной работы с decimal
// var cultureInfo = new CultureInfo("ru-RU");
// cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
// CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
// CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// // Регистрация сервисов в контейнере DI
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// // Регистрация кастомного биндера для decimal
// builder.Services.AddControllersWithViews(options =>
// {
//     options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
// });

// var app = builder.Build();

// // Создание базы данных при запуске
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     db.Database.EnsureCreated();
//     Console.WriteLine("✅ База данных SQLite создана!");
// }

// // Настройка HTTP pipeline
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Home/Error");
//     app.UseHsts();
// }

// app.UseHttpsRedirection();
// app.UseStaticFiles();
// app.UseRouting();
// app.UseAuthorization();

// // Маршрутизация
// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}");

// app.Run();

// // Кастомный биндер для decimal (добавь в этот же файл или создай отдельный)
// public class DecimalModelBinder : IModelBinder
// {
//     public Task BindModelAsync(ModelBindingContext bindingContext)
//     {
//         var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        
//         if (valueProviderResult == ValueProviderResult.None)
//             return Task.CompletedTask;

//         var value = valueProviderResult.FirstValue?.Trim();
        
//         if (string.IsNullOrEmpty(value))
//             return Task.CompletedTask;

//         // Пытаемся распарсить разные форматы
//         // 1. Стандартный формат с точкой
//         if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
//         {
//             bindingContext.Result = ModelBindingResult.Success(result);
//         }
//         // 2. Русский формат с запятой
//         else if (decimal.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
//         {
//             bindingContext.Result = ModelBindingResult.Success(result);
//         }
//         else
//         {
//             bindingContext.ModelState.TryAddModelError(
//                 bindingContext.ModelName,
//                 $"Некорректное значение. Используйте формат: 350, 350.50 или 350,00");
//         }

//         return Task.CompletedTask;
//     }
// }

// public class DecimalModelBinderProvider : IModelBinderProvider
// {
//     public IModelBinder GetBinder(ModelBinderProviderContext context)
//     {
//         if (context == null) throw new ArgumentNullException(nameof(context));

//         // Применяем биндер только для decimal и decimal?
//         if (context.Metadata.ModelType == typeof(decimal) || context.Metadata.ModelType == typeof(decimal?))
//         {
//             return new DecimalModelBinder();
//         }

//         return null;
//     }
// }




















// using CarRental.Data;
// using Microsoft.EntityFrameworkCore;
// using System.Globalization;
// using Microsoft.AspNetCore.Mvc.ModelBinding;
// using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
// using System.IO;  // ← ДОБАВЬ ЭТУ СТРОКУ

// #nullable disable

// var builder = WebApplication.CreateBuilder(args);

// // Настройка культуры для корректной работы с decimal
// var cultureInfo = new CultureInfo("ru-RU");
// cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
// CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
// CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// // Регистрация сервисов в контейнере DI
// builder.Services.AddDbContext<AppDbContext>(options =>
// {
//     var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//     Console.WriteLine($"Подключение к БД: {connectionString}");
//     options.UseSqlite(connectionString);
// });

// // Регистрация кастомного биндера для decimal
// builder.Services.AddControllersWithViews(options =>
// {
//     options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
// });

// var app = builder.Build();

// // Создание базы данных при запуске
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
//     // Для Docker: проверяем, существует ли папка для базы данных
//     var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//     if (connectionString.Contains("/app/data/"))
//     {
//         var dataDir = Path.GetDirectoryName(connectionString.Replace("Data Source=", ""));
//         if (!Directory.Exists(dataDir))
//         {
//             Directory.CreateDirectory(dataDir);
//             Console.WriteLine($"Создана папка для БД: {dataDir}");
//         }
//     }
    
//     db.Database.EnsureCreated();
//     Console.WriteLine("База данных SQLite создана/проверена!");
    
//     // Выводим путь к БД для отладки
//     Console.WriteLine($"Путь к БД: {connectionString}");
// }

// // Настройка HTTP pipeline
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Home/Error");
//     app.UseHsts();
// }

// app.UseHttpsRedirection();
// app.UseStaticFiles();
// app.UseRouting();
// app.UseAuthorization();

// // Маршрутизация
// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}");

// // Вывод информации о запуске
// Console.WriteLine("Приложение запущено!");
// Console.WriteLine($"Окружение: {app.Environment.EnvironmentName}");
// Console.WriteLine($"wwwroot путь: {Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")}");

// app.Run();

// // Кастомный биндер для decimal
// public class DecimalModelBinder : IModelBinder
// {
//     public Task BindModelAsync(ModelBindingContext bindingContext)
//     {
//         var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        
//         if (valueProviderResult == ValueProviderResult.None)
//             return Task.CompletedTask;

//         var value = valueProviderResult.FirstValue?.Trim();
        
//         if (string.IsNullOrEmpty(value))
//             return Task.CompletedTask;

//         // Пытаемся распарсить разные форматы
//         // 1. Стандартный формат с точкой
//         if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
//         {
//             bindingContext.Result = ModelBindingResult.Success(result);
//         }
//         // 2. Русский формат с запятой
//         else if (decimal.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
//         {
//             bindingContext.Result = ModelBindingResult.Success(result);
//         }
//         else
//         {
//             bindingContext.ModelState.TryAddModelError(
//                 bindingContext.ModelName,
//                 $"Некорректное значение. Используйте формат: 350, 350.50 или 350,00");
//         }

//         return Task.CompletedTask;
//     }
// }

// public class DecimalModelBinderProvider : IModelBinderProvider
// {
//     public IModelBinder GetBinder(ModelBinderProviderContext context)
//     {
//         if (context == null) throw new ArgumentNullException(nameof(context));

//         // Применяем биндер только для decimal и decimal?
//         if (context.Metadata.ModelType == typeof(decimal) || context.Metadata.ModelType == typeof(decimal?))
//         {
//             return new DecimalModelBinder();
//         }

//         return null;
//     }
// }
































// using CarRental.Data;
// using Microsoft.EntityFrameworkCore;
// using System.Globalization;
// using Microsoft.AspNetCore.Mvc.ModelBinding;
// using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
// using System.IO;

// #nullable disable

// var builder = WebApplication.CreateBuilder(args);

// // Настройка культуры для корректной работы с decimal
// var cultureInfo = new CultureInfo("ru-RU");
// cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
// CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
// CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// // Регистрация сервисов в контейнере DI
// builder.Services.AddDbContext<AppDbContext>(options =>
// {
//     var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//     Console.WriteLine($"Подключение к БД: {connectionString}");
//     options.UseSqlite(connectionString);
// });

// // Регистрация кастомного биндера для decimal
// builder.Services.AddControllersWithViews(options =>
// {
//     options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
// });

// var app = builder.Build();

// // Создание базы данных при запуске
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
//     // Для Docker: проверяем, существует ли папка для базы данных
//     var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//     if (connectionString.Contains("/app/data/"))
//     {
//         var dataDir = Path.GetDirectoryName(connectionString.Replace("Data Source=", ""));
//         if (!Directory.Exists(dataDir))
//         {
//             Directory.CreateDirectory(dataDir);
//             Console.WriteLine($"Создана папка для БД: {dataDir}");
//         }
//     }
    
//     db.Database.EnsureCreated();
//     Console.WriteLine("База данных SQLite создана/проверена!");
    
//     // Выводим путь к БД для отладки
//     Console.WriteLine($"Путь к БД: {connectionString}");
// }

// // Настройка HTTP pipeline
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Home/Error");
//     app.UseHsts();
// }

// app.UseHttpsRedirection();
// app.UseStaticFiles();
// app.UseRouting();
// app.UseAuthorization();

// // ========== МАРШРУТИЗАЦИЯ ==========
// // Главная страница - MainHome (открывается при запуске)
// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=MainHome}/{action=Index}/{id?}");
// // ===================================

// // Вывод информации о запуске
// Console.WriteLine("Приложение запущено!");
// Console.WriteLine($"Окружение: {app.Environment.EnvironmentName}");
// Console.WriteLine($"wwwroot путь: {Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")}");

// app.Run();

// // Кастомный биндер для decimal
// public class DecimalModelBinder : IModelBinder
// {
//     public Task BindModelAsync(ModelBindingContext bindingContext)
//     {
//         var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        
//         if (valueProviderResult == ValueProviderResult.None)
//             return Task.CompletedTask;

//         var value = valueProviderResult.FirstValue?.Trim();
        
//         if (string.IsNullOrEmpty(value))
//             return Task.CompletedTask;

//         // Пытаемся распарсить разные форматы
//         // 1. Стандартный формат с точкой
//         if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
//         {
//             bindingContext.Result = ModelBindingResult.Success(result);
//         }
//         // 2. Русский формат с запятой
//         else if (decimal.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
//         {
//             bindingContext.Result = ModelBindingResult.Success(result);
//         }
//         else
//         {
//             bindingContext.ModelState.TryAddModelError(
//                 bindingContext.ModelName,
//                 $"Некорректное значение. Используйте формат: 350, 350.50 или 350,00");
//         }

//         return Task.CompletedTask;
//     }
// }

// public class DecimalModelBinderProvider : IModelBinderProvider
// {
//     public IModelBinder GetBinder(ModelBinderProviderContext context)
//     {
//         if (context == null) throw new ArgumentNullException(nameof(context));

//         // Применяем биндер только для decimal и decimal?
//         if (context.Metadata.ModelType == typeof(decimal) || context.Metadata.ModelType == typeof(decimal?))
//         {
//             return new DecimalModelBinder();
//         }

//         return null;
//     }
// }






































using CarRental.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.IO;

#nullable disable

var builder = WebApplication.CreateBuilder(args);

// Настройка культуры для корректной работы с decimal
var cultureInfo = new CultureInfo("ru-RU");
cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Регистрация сервисов в контейнере DI
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"Подключение к БД: {connectionString}");
    options.UseSqlite(connectionString);
});




builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ClearTempDataFilter>();
});




// Регистрация кастомного биндера для decimal
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
});

// ========== ДОБАВИТЬ НАСТРОЙКУ СЕССИЙ ==========
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "CarRental.Session";
});
// ===============================================

var app = builder.Build();

// Создание базы данных при запуске
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Для Docker: проверяем, существует ли папка для базы данных
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (connectionString.Contains("/app/data/"))
    {
        var dataDir = Path.GetDirectoryName(connectionString.Replace("Data Source=", ""));
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
            Console.WriteLine($"Создана папка для БД: {dataDir}");
        }
    }
    
    db.Database.EnsureCreated();
    Console.WriteLine("База данных SQLite создана/проверена!");
    
    // Выводим путь к БД для отладки
    Console.WriteLine($"Путь к БД: {connectionString}");
}

// Настройка HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ========== ДОБАВИТЬ ПОДКЛЮЧЕНИЕ СЕССИЙ ==========
app.UseSession();
// =================================================

app.UseAuthorization();

// Маршрутизация
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=MainHome}/{action=Index}/{id?}");

// Вывод информации о запуске
Console.WriteLine("Приложение запущено!");
Console.WriteLine($"Окружение: {app.Environment.EnvironmentName}");
Console.WriteLine($"wwwroot путь: {Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")}");

app.Run();

// Кастомный биндер для decimal
public class DecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        
        if (valueProviderResult == ValueProviderResult.None)
            return Task.CompletedTask;

        var value = valueProviderResult.FirstValue?.Trim();
        
        if (string.IsNullOrEmpty(value))
            return Task.CompletedTask;

        // Пытаемся распарсить разные форматы
        // 1. Стандартный формат с точкой
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            bindingContext.Result = ModelBindingResult.Success(result);
        }
        // 2. Русский формат с запятой
        else if (decimal.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
        {
            bindingContext.Result = ModelBindingResult.Success(result);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                $"Некорректное значение. Используйте формат: 350, 350.50 или 350,00");
        }

        return Task.CompletedTask;
    }
}

public class DecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        // Применяем биндер только для decimal и decimal?
        if (context.Metadata.ModelType == typeof(decimal) || context.Metadata.ModelType == typeof(decimal?))
        {
            return new DecimalModelBinder();
        }

        return null;
    }
}



