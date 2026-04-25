// using System.Text;
// using System.Text.Json;
// using CarRental.Models;

// namespace CarRental.Services
// {
//     public class DeepSeekService
//     {
//         private readonly HttpClient _httpClient;
//         private readonly string _apiKey;
//         private readonly Dictionary<string, string> _cache = new(); // Кэш для ответов

//         public DeepSeekService(IConfiguration configuration)
//         {
//             _apiKey = configuration["DeepSeek:ApiKey"];
//             _httpClient = new HttpClient();
//             _httpClient.Timeout = TimeSpan.FromSeconds(30);
//             _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
//             _httpClient.DefaultRequestHeaders.Add("User-Agent", "CarRental/1.0");
//         }

//         public async Task<string> GetCarRecommendationAsync(string userQuery, List<Car> availableCars)
//         {
//             // Проверяем кэш
//             var cacheKey = $"{userQuery}_{availableCars.Count}";
//             if (_cache.ContainsKey(cacheKey))
//             {
//                 Console.WriteLine($"=== Используем кэш для запроса: {userQuery} ===");
//                 return _cache[cacheKey];
//             }

//             // Пытаемся выполнить API-запрос с повторными попытками
//             for (int attempt = 1; attempt <= 3; attempt++)
//             {
//                 try
//                 {
//                     Console.WriteLine($"=== DeepSeek API Request (OpenRouter) - попытка {attempt} ===");
//                     Console.WriteLine($"Запрос: {userQuery}");
//                     Console.WriteLine($"Количество автомобилей: {availableCars.Count}");

//                     var carList = string.Join("\n", availableCars.Select(c => 
//                     {
//                         var features = "";
//                         if (c.CarFeatures != null && c.CarFeatures.Any())
//                         {
//                             var featureStrings = c.CarFeatures.Select(cf =>
//                                 $"{cf.Feature?.Name ?? "?"}: {cf.FeatureValue?.Value ?? "?"}");
//                             features = string.Join(", ", featureStrings);
//                         }
//                         var categoryName = c.Category?.Name ?? "не указана";
//                         return $"- ID: {c.Id}, {c.Brand} {c.Model}, {c.Year} год, категория: {categoryName}, характеристики: [{features}], цена: {c.DailyPrice} BYN";
//                     }));

//                     var prompt = $@"Ты — ассистент по подбору автомобилей.
// Список машин:
// {carList}

// Запрос: '{userQuery}'

// Верни **только ID** подходящих авто через запятую. Если ничего не подходит — '0'.
// Без пояснений. Пример: '27,28,29'";

//                     var requestBody = new
//                     {
//                         model = "deepseek/deepseek-chat",
//                         messages = new[] { new { role = "user", content = prompt } },
//                         temperature = 0.1,
//                         max_tokens = 200
//                     };

//                     var json = JsonSerializer.Serialize(requestBody);
//                     var content = new StringContent(json, Encoding.UTF8, "application/json");

//                     var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
//                     var responseBody = await response.Content.ReadAsStringAsync();

//                     Console.WriteLine($"Response Status: {response.StatusCode}");

//                     if (response.IsSuccessStatusCode)
//                     {
//                         var result = JsonSerializer.Deserialize<OpenRouterResponse>(responseBody);
//                         var answer = result?.choices?[0]?.message?.content?.Trim() ?? "0";
//                         Console.WriteLine($"DeepSeek Answer: {answer}");

//                         var ids = System.Text.RegularExpressions.Regex.Matches(answer, @"\d+")
//                             .Select(m => int.Parse(m.Value))
//                             .ToList();

//                         if (ids.Count > 0)
//                         {
//                             var resultIds = string.Join(",", ids);
//                             _cache[cacheKey] = resultIds;
//                             return resultIds;
//                         }
                        
//                         // Если ID не найдены, возвращаем "0" (пустой результат)
//                         _cache[cacheKey] = "0";
//                         return "0";
//                     }
//                     else
//                     {
//                         Console.WriteLine($"API Error: {responseBody}");
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine($"Attempt {attempt} failed: {ex.Message}");
//                     if (attempt == 3)
//                     {
//                         Console.WriteLine("Все попытки API не удались, используем ключевой поиск");
//                     }
//                 }
                
//                 // Ждём перед повторной попыткой
//                 if (attempt < 3) await Task.Delay(1000);
//             }

//             // Если API не работает, используем ключевой поиск
//             var keywordResult = await KeywordSearchAsync(userQuery, availableCars);
//             _cache[cacheKey] = keywordResult;
//             return keywordResult;
//         }

//         private async Task<string> KeywordSearchAsync(string userQuery, List<Car> availableCars)
//         {
//             var keywords = userQuery.ToLower()
//                 .Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
//                 .Where(k => k.Length > 2); // Игнорируем слишком короткие слова
            
//             var matchedIds = availableCars
//                 .Where(c => keywords.Any(k => 
//                     c.Brand.ToLower().Contains(k) || 
//                     c.Model.ToLower().Contains(k)))
//                 .Select(c => c.Id)
//                 .ToList();
            
//             // ИСПРАВЛЕНО: возвращаем "0", а не все автомобили
//             if (matchedIds.Count == 0)
//             {
//                 Console.WriteLine("Ничего не найдено по ключевым словам");
//                 return "0";
//             }
            
//             Console.WriteLine($"Ключевой поиск: найдено {matchedIds.Count} авто");
//             return string.Join(",", matchedIds);
//         }
//     }

//     public class OpenRouterResponse
//     {
//         public List<OpenRouterChoice> choices { get; set; }
//     }

//     public class OpenRouterChoice
//     {
//         public OpenRouterMessage message { get; set; }
//     }

//     public class OpenRouterMessage
//     {
//         public string content { get; set; }
//     }
// }












































// using System.Text;
// using System.Text.Json;
// using CarRental.Models;

// namespace CarRental.Services
// {
//     public class DeepSeekService
//     {
//         private readonly HttpClient _httpClient;
//         private readonly string _apiKey;
//         private readonly Dictionary<string, string> _cache = new(); // Кэш для ответов

//         public DeepSeekService(IConfiguration configuration)
//         {
//             _apiKey = configuration["DeepSeek:ApiKey"];
//             _httpClient = new HttpClient();
//             _httpClient.Timeout = TimeSpan.FromSeconds(30);
//             _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
//             _httpClient.DefaultRequestHeaders.Add("User-Agent", "CarRental/1.0");
//         }

//         // Вспомогательный метод для декодирования Unicode
//         private string DecodeUnicode(string input)
//         {
//             if (string.IsNullOrEmpty(input)) return input;
//             return System.Text.RegularExpressions.Regex.Unescape(input);
//         }

//         public async Task<string> GetCarRecommendationAsync(string userQuery, List<Car> availableCars)
//         {
//             // Проверяем кэш
//             var cacheKey = $"{userQuery}_{availableCars.Count}";
//             if (_cache.ContainsKey(cacheKey))
//             {
//                 Console.WriteLine($"=== Используем кэш для запроса: {userQuery} ===");
//                 return _cache[cacheKey];
//             }

//             // Пытаемся выполнить API-запрос с повторными попытками
//             for (int attempt = 1; attempt <= 3; attempt++)
//             {
//                 try
//                 {
//                     Console.WriteLine($"=== DeepSeek API Request (OpenRouter) - попытка {attempt} ===");
//                     Console.WriteLine($"Запрос: {userQuery}");
//                     Console.WriteLine($"Количество автомобилей: {availableCars.Count}");

//                     var carList = string.Join("\n", availableCars.Select(c => 
//                     {
//                         var features = "";
//                         if (c.CarFeatures != null && c.CarFeatures.Any())
//                         {
//                             var featureStrings = c.CarFeatures.Select(cf =>
//                                 $"{cf.Feature?.Name ?? "?"}: {cf.FeatureValue?.Value ?? "?"}");
//                             features = string.Join(", ", featureStrings);
//                         }
//                         var categoryName = c.Category?.Name ?? "не указана";
//                         return $"- ID: {c.Id}, {c.Brand} {c.Model}, {c.Year} год, категория: {categoryName}, характеристики: [{features}], цена: {c.DailyPrice} BYN";
//                     }));

//                     var prompt = $@"Ты — ассистент по подбору автомобилей.
// Список машин:
// {carList}

// Запрос: '{userQuery}'

// Верни **только ID** подходящих авто через запятую. Если ничего не подходит — '0'.
// Без пояснений. Пример: '27,28,29'";

//                     var requestBody = new
//                     {
//                         model = "deepseek/deepseek-chat",
//                         messages = new[] { new { role = "user", content = prompt } },
//                         temperature = 0.1,
//                         max_tokens = 200
//                     };

//                     // ========== ВЫВОД ПОЛНОГО JSON ЗАПРОСА (в читаемом виде) ==========
//                     var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
//                     string fullJsonRequest = JsonSerializer.Serialize(requestBody, jsonOptions);
//                     var decodedRequest = DecodeUnicode(fullJsonRequest);
//                     Console.WriteLine("=== ПОЛНЫЙ JSON ЗАПРОС К DEEPSEEK ===");
//                     Console.WriteLine(decodedRequest);
//                     Console.WriteLine("========================================");
//                     // ====================================================

//                     var json = JsonSerializer.Serialize(requestBody);
//                     var content = new StringContent(json, Encoding.UTF8, "application/json");

//                     var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
//                     var responseBody = await response.Content.ReadAsStringAsync();

//                     // ========== ВЫВОД ПОЛНОГО JSON ОТВЕТА (в читаемом виде) ==========
//                     var decodedResponse = DecodeUnicode(responseBody);
//                     Console.WriteLine("=== ПОЛНЫЙ JSON ОТВЕТ ОТ DEEPSEEK ===");
//                     Console.WriteLine(decodedResponse);
//                     Console.WriteLine("========================================");
//                     // ====================================================

//                     Console.WriteLine($"Response Status: {response.StatusCode}");

//                     if (response.IsSuccessStatusCode)
//                     {
//                         var result = JsonSerializer.Deserialize<OpenRouterResponse>(responseBody);
//                         var answer = result?.choices?[0]?.message?.content?.Trim() ?? "0";
//                         Console.WriteLine($"DeepSeek Answer: {answer}");

//                         var ids = System.Text.RegularExpressions.Regex.Matches(answer, @"\d+")
//                             .Select(m => int.Parse(m.Value))
//                             .ToList();

//                         if (ids.Count > 0)
//                         {
//                             var resultIds = string.Join(",", ids);
//                             _cache[cacheKey] = resultIds;
//                             return resultIds;
//                         }
                        
//                         // Если ID не найдены, возвращаем "0" (пустой результат)
//                         _cache[cacheKey] = "0";
//                         return "0";
//                     }
//                     else
//                     {
//                         Console.WriteLine($"API Error: {responseBody}");
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine($"Attempt {attempt} failed: {ex.Message}");
//                     if (attempt == 3)
//                     {
//                         Console.WriteLine("Все попытки API не удались, используем ключевой поиск");
//                     }
//                 }
                
//                 // Ждём перед повторной попыткой
//                 if (attempt < 3) await Task.Delay(1000);
//             }

//             // Если API не работает, используем ключевой поиск
//             var keywordResult = await KeywordSearchAsync(userQuery, availableCars);
//             _cache[cacheKey] = keywordResult;
//             return keywordResult;
//         }

//         private async Task<string> KeywordSearchAsync(string userQuery, List<Car> availableCars)
//         {
//             var keywords = userQuery.ToLower()
//                 .Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
//                 .Where(k => k.Length > 2);
            
//             var matchedIds = availableCars
//                 .Where(c => keywords.Any(k => 
//                     c.Brand.ToLower().Contains(k) || 
//                     c.Model.ToLower().Contains(k)))
//                 .Select(c => c.Id)
//                 .ToList();
            
//             if (matchedIds.Count == 0)
//             {
//                 Console.WriteLine("Ничего не найдено по ключевым словам");
//                 return "0";
//             }
            
//             Console.WriteLine($"Ключевой поиск: найдено {matchedIds.Count} авто");
//             return string.Join(",", matchedIds);
//         }
//     }

//     public class OpenRouterResponse
//     {
//         public List<OpenRouterChoice> choices { get; set; }
//     }

//     public class OpenRouterChoice
//     {
//         public OpenRouterMessage message { get; set; }
//     }

//     public class OpenRouterMessage
//     {
//         public string content { get; set; }
//     }
// }



























































// using System.Text;
// using System.Text.Json;
// using CarRental.Models;

// namespace CarRental.Services
// {
//     public class DeepSeekService
//     {
//         private readonly HttpClient _httpClient;
//         private readonly string _apiKey;

//         public DeepSeekService(IConfiguration configuration)
//         {
//             _apiKey = configuration["DeepSeek:ApiKey"];
//             _httpClient = new HttpClient();
//             _httpClient.Timeout = TimeSpan.FromSeconds(30);
//             _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
//             _httpClient.DefaultRequestHeaders.Add("User-Agent", "CarRental/1.0");
//         }

//         // Вспомогательный метод для декодирования Unicode
//         private string DecodeUnicode(string input)
//         {
//             if (string.IsNullOrEmpty(input)) return input;
//             return System.Text.RegularExpressions.Regex.Unescape(input);
//         }

//         // Метод для извлечения цвета из запроса
//         private string ExtractColor(string userQuery)
//         {
//             var colors = new[] { "красный", "синий", "зеленый", "желтый", "черный", "белый", "серебристый", "серый", "оранжевый", "фиолетовый", "розовый", "коричневый" };
//             var lowerQuery = userQuery.ToLower();
//             foreach (var color in colors)
//             {
//                 if (lowerQuery.Contains(color))
//                     return color;
//             }
//             return null;
//         }

//         // Метод для извлечения цены из запроса
//         private (decimal? exactPrice, decimal? maxPrice, decimal? minPrice) ExtractPrice(string userQuery)
//         {
//             var lowerQuery = userQuery.ToLower();
            
//             // Проверка на точную цену: "за 350", "ровно 350"
//             var exactMatch = System.Text.RegularExpressions.Regex.Match(lowerQuery, @"(за|ровно|точно)\s+(\d+)");
//             if (exactMatch.Success)
//             {
//                 return (decimal.Parse(exactMatch.Groups[2].Value), null, null);
//             }
            
//             // Проверка на диапазон: "от 200 до 350"
//             var rangeMatch = System.Text.RegularExpressions.Regex.Match(lowerQuery, @"от\s+(\d+)\s+до\s+(\d+)");
//             if (rangeMatch.Success)
//             {
//                 return (null, decimal.Parse(rangeMatch.Groups[2].Value), decimal.Parse(rangeMatch.Groups[1].Value));
//             }
            
//             // Проверка на максимальную цену: "до 350", "не дороже 350"
//             var maxMatch = System.Text.RegularExpressions.Regex.Match(lowerQuery, @"(до|не дороже|не более)\s+(\d+)");
//             if (maxMatch.Success)
//             {
//                 return (null, decimal.Parse(maxMatch.Groups[2].Value), null);
//             }
            
//             // Простое упоминание цены: "350 рублей"
//             var simpleMatch = System.Text.RegularExpressions.Regex.Match(lowerQuery, @"(\d+)\s*белорусских?\s*руб");
//             if (simpleMatch.Success)
//             {
//                 return (decimal.Parse(simpleMatch.Groups[1].Value), null, null);
//             }
            
//             return (null, null, null);
//         }

//         // Метод для пост-фильтрации по цвету
//         private List<Car> FilterByColor(List<Car> cars, string color)
//         {
//             if (string.IsNullOrEmpty(color)) return cars;
            
//             return cars.Where(c => c.CarFeatures != null && c.CarFeatures.Any(cf => 
//                 cf.Feature?.Name?.ToLower() == "цвет" && 
//                 cf.FeatureValue?.Value?.ToLower() == color)).ToList();
//         }

//         // Метод для пост-фильтрации по цене
//         private List<Car> FilterByPrice(List<Car> cars, decimal? exactPrice, decimal? maxPrice, decimal? minPrice)
//         {
//             if (exactPrice.HasValue)
//             {
//                 return cars.Where(c => Math.Abs(c.DailyPrice - exactPrice.Value) < 0.01m).ToList();
//             }
//             if (minPrice.HasValue && maxPrice.HasValue)
//             {
//                 return cars.Where(c => c.DailyPrice >= minPrice.Value && c.DailyPrice <= maxPrice.Value).ToList();
//             }
//             if (maxPrice.HasValue)
//             {
//                 return cars.Where(c => c.DailyPrice <= maxPrice.Value).ToList();
//             }
//             return cars;
//         }

//         public async Task<string> GetCarRecommendationAsync(string userQuery, List<Car> availableCars)
//         {
//             // Сохраняем параметры для пост-фильтрации
//             var requestedColor = ExtractColor(userQuery);
//             var (exactPrice, maxPrice, minPrice) = ExtractPrice(userQuery);
            
//             try
//             {
//                 Console.WriteLine($"=== DeepSeek API Request ===");
//                 Console.WriteLine($"Запрос пользователя: {userQuery}");
//                 Console.WriteLine($"Количество автомобилей в БД: {availableCars.Count}");

//                 // Формируем список автомобилей с подробными характеристиками
//                 var carList = string.Join("\n", availableCars.Select(c => 
//                 {
//                     var features = "";
//                     if (c.CarFeatures != null && c.CarFeatures.Any())
//                     {
//                         var featureStrings = c.CarFeatures.Select(cf =>
//                             $"{cf.Feature?.Name ?? "?"}: {cf.FeatureValue?.Value ?? "?"}");
//                         features = string.Join(", ", featureStrings);
//                     }
//                     var categoryName = c.Category?.Name ?? "не указана";
//                     return $"- ID: {c.Id}, {c.Brand} {c.Model}, {c.Year} год, категория: {categoryName}, характеристики: [{features}], цена: {c.DailyPrice} BYN";
//                 }));

//                 // ========== ВЫВОД СПИСКА АВТОМОБИЛЕЙ ДЛЯ ПРОМТА ==========
//                 Console.WriteLine("=== СПИСОК АВТОМОБИЛЕЙ, ПЕРЕДАВАЕМЫХ В ПРОМТ ===");
//                 Console.WriteLine(carList);
//                 Console.WriteLine("=================================================");

//                 var prompt = $@"Ты — строгий ассистент по подбору автомобилей.

// **Список доступных машин:**
// {carList}

// **Запрос пользователя:** '{userQuery}'

// **Правила отбора (очень важно!):**
// 1. Внимательно проанализируй запрос и выдели ВСЕ указанные характеристики (марка, модель, цвет, тип кузова, год, коробка передач, ЦЕНА и т.д.)
// 2. Автомобиль ДОЛЖЕН соответствовать ВСЕМ характеристикам из запроса
// 3. **Правила обработки цены:**
//    - Если пользователь сказал ""за 350 рублей"" или ""ровно 350"" — верни ТОЛЬКО автомобили с ценой ровно 350
//    - Если пользователь сказал ""до 350"" или ""не дороже 350"" — верни автомобили с ценой МЕНЬШЕ или РАВНО 350
//    - Если пользователь сказал ""от 200 до 350"" — верни автомобили с ценой между 200 и 350
//    - НЕ возвращай автомобили с другой ценой, если пользователь указал конкретную сумму
// 4. Если в запросе указан цвет — возвращай ТОЛЬКО автомобили этого цвета. Автомобили другого цвета НЕ включай в ответ
// 5. Если в запросе указана марка — возвращай ТОЛЬКО автомобили этой марки
// 6. Если в запросе указан тип кузова (внедорожник, седан, хэтчбек) — возвращай ТОЛЬКО автомобили этого типа
// 7. Если в запросе указана коробка передач (автомат, механика, робот) — возвращай ТОЛЬКО автомобили с этой коробкой
// 8. Если автомобиль не соответствует хотя бы одному условию — НЕ включай его в ответ
// 9. Если подходящих автомобилей нет — верни '0'
// 10. Верни **ТОЛЬКО ID** подходящих автомобилей через запятую. Пример правильного ответа: '56,61'
// 11. НЕ пиши никаких пояснений, только ID

// Помни: точность важнее количества. Лучше вернуть 0, чем неподходящий автомобиль.";

//                 // ========== ВЫВОД ПОЛНОГО ТЕКСТА ПРОМТА ==========
//                 Console.WriteLine("=== ПОЛНЫЙ ТЕКСТ ПРОМТА (ЧТО ВИДИТ НЕЙРОСЕТЬ) ===");
//                 Console.WriteLine(prompt);
//                 Console.WriteLine("=================================================");

//                 var requestBody = new
//                 {
//                     model = "deepseek/deepseek-chat",
//                     messages = new[] { new { role = "user", content = prompt } },
//                     temperature = 0.1,
//                     max_tokens = 200
//                 };

//                 // ========== ВЫВОД ПОЛНОГО JSON ЗАПРОСА ==========
//                 var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
//                 string fullJsonRequest = JsonSerializer.Serialize(requestBody, jsonOptions);
//                 Console.WriteLine("=== ПОЛНЫЙ JSON ЗАПРОС К DEEPSEEK ===");
//                 Console.WriteLine(DecodeUnicode(fullJsonRequest));
//                 Console.WriteLine("========================================");

//                 var json = JsonSerializer.Serialize(requestBody);
//                 var content = new StringContent(json, Encoding.UTF8, "application/json");

//                 var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
//                 var responseBody = await response.Content.ReadAsStringAsync();

//                 // ========== ВЫВОД ПОЛНОГО JSON ОТВЕТА ==========
//                 Console.WriteLine("=== ПОЛНЫЙ JSON ОТВЕТ ОТ DEEPSEEK ===");
//                 Console.WriteLine(DecodeUnicode(responseBody));
//                 Console.WriteLine("========================================");
//                 Console.WriteLine($"Response Status: {response.StatusCode}");

//                 if (response.IsSuccessStatusCode)
//                 {
//                     var result = JsonSerializer.Deserialize<OpenRouterResponse>(responseBody);
//                     var answer = result?.choices?[0]?.message?.content?.Trim() ?? "0";
//                     Console.WriteLine($"DeepSeek Answer: {answer}");

//                     var ids = System.Text.RegularExpressions.Regex.Matches(answer, @"\d+")
//                         .Select(m => int.Parse(m.Value))
//                         .ToList();

//                     if (ids.Count > 0)
//                     {
//                         var recommendedCars = availableCars.Where(c => ids.Contains(c.Id)).ToList();
                        
//                         // Пост-фильтрация по цвету
//                         if (!string.IsNullOrEmpty(requestedColor))
//                         {
//                             var colorFilteredCars = FilterByColor(recommendedCars, requestedColor);
//                             if (colorFilteredCars.Count < recommendedCars.Count)
//                             {
//                                 Console.WriteLine($"Пост-фильтрация по цвету '{requestedColor}': удалено {recommendedCars.Count - colorFilteredCars.Count} автомобилей");
//                             }
//                             recommendedCars = colorFilteredCars;
//                         }
                        
//                         // Пост-фильтрация по цене
//                         if (exactPrice.HasValue || maxPrice.HasValue || minPrice.HasValue)
//                         {
//                             var priceFilteredCars = FilterByPrice(recommendedCars, exactPrice, maxPrice, minPrice);
//                             if (priceFilteredCars.Count < recommendedCars.Count)
//                             {
//                                 Console.WriteLine($"Пост-фильтрация по цене: удалено {recommendedCars.Count - priceFilteredCars.Count} автомобилей");
//                             }
//                             recommendedCars = priceFilteredCars;
//                         }
                        
//                         if (recommendedCars.Count > 0)
//                         {
//                             var resultIds = string.Join(",", recommendedCars.Select(c => c.Id));
//                             Console.WriteLine($"Итоговые ID: {resultIds}");
//                             return resultIds;
//                         }
//                     }
                    
//                     Console.WriteLine("Не найдено подходящих автомобилей");
//                     return "0";
//                 }
//                 else
//                 {
//                     Console.WriteLine($"API Error: {responseBody}");
//                     return await KeywordSearchAsync(userQuery, availableCars);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"DeepSeek Exception: {ex.Message}");
//                 return await KeywordSearchAsync(userQuery, availableCars);
//             }
//         }

//         private async Task<string> KeywordSearchAsync(string userQuery, List<Car> availableCars)
//         {
//             var keywords = userQuery.ToLower()
//                 .Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
//                 .Where(k => k.Length > 2);
            
//             var matchedIds = availableCars
//                 .Where(c => keywords.Any(k => 
//                     c.Brand.ToLower().Contains(k) || 
//                     c.Model.ToLower().Contains(k) ||
//                     (c.Category?.Name?.ToLower().Contains(k) ?? false)))
//                 .Select(c => c.Id)
//                 .ToList();
            
//             // Дополнительная фильтрация по цвету
//             var requestedColor = ExtractColor(userQuery);
//             if (!string.IsNullOrEmpty(requestedColor) && matchedIds.Count > 0)
//             {
//                 var cars = availableCars.Where(c => matchedIds.Contains(c.Id)).ToList();
//                 var colorFilteredCars = FilterByColor(cars, requestedColor);
//                 if (colorFilteredCars.Count > 0)
//                 {
//                     matchedIds = colorFilteredCars.Select(c => c.Id).ToList();
//                 }
//             }
            
//             // Дополнительная фильтрация по цене
//             var (exactPrice, maxPrice, minPrice) = ExtractPrice(userQuery);
//             if ((exactPrice.HasValue || maxPrice.HasValue || minPrice.HasValue) && matchedIds.Count > 0)
//             {
//                 var cars = availableCars.Where(c => matchedIds.Contains(c.Id)).ToList();
//                 var priceFilteredCars = FilterByPrice(cars, exactPrice, maxPrice, minPrice);
//                 if (priceFilteredCars.Count > 0)
//                 {
//                     matchedIds = priceFilteredCars.Select(c => c.Id).ToList();
//                 }
//             }
            
//             if (matchedIds.Count == 0)
//             {
//                 Console.WriteLine("Ничего не найдено по ключевым словам, возвращаем все автомобили");
//                 return string.Join(",", availableCars.Select(c => c.Id));
//             }
            
//             Console.WriteLine($"Ключевой поиск: найдено {matchedIds.Count} авто");
//             return string.Join(",", matchedIds);
//         }
//     }

//     public class OpenRouterResponse
//     {
//         public List<OpenRouterChoice> choices { get; set; }
//     }

//     public class OpenRouterChoice
//     {
//         public OpenRouterMessage message { get; set; }
//     }

//     public class OpenRouterMessage
//     {
//         public string content { get; set; }
//     }
// }
























































using System.Text;
using System.Text.Json;
using CarRental.Models;

namespace CarRental.Services
{
    public class DeepSeekService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public DeepSeekService(IConfiguration configuration)
        {
            _apiKey = configuration["DeepSeek:ApiKey"];
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "CarRental/1.0");
        }

        // Вспомогательный метод для декодирования Unicode
        private string DecodeUnicode(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return System.Text.RegularExpressions.Regex.Unescape(input);
        }

        // Метод для извлечения цвета из запроса
        private string ExtractColor(string userQuery)
        {
            var colors = new[] { "красный", "синий", "зеленый", "желтый", "черный", "белый", "серебристый", "серый", "оранжевый", "фиолетовый", "розовый", "коричневый" };
            var lowerQuery = userQuery.ToLower();
            foreach (var color in colors)
            {
                if (lowerQuery.Contains(color))
                    return color;
            }
            return null;
        }

        // Метод для пост-фильтрации по цвету
        private List<Car> FilterByColor(List<Car> cars, string color)
        {
            if (string.IsNullOrEmpty(color)) return cars;
            
            return cars.Where(c => c.CarFeatures != null && c.CarFeatures.Any(cf => 
                cf.Feature?.Name?.ToLower() == "цвет" && 
                cf.FeatureValue?.Value?.ToLower() == color)).ToList();
        }

        public async Task<string> GetCarRecommendationAsync(string userQuery, List<Car> availableCars)
        {
            // Сохраняем цвет из запроса для пост-фильтрации
            var requestedColor = ExtractColor(userQuery);
            
            try
            {
                Console.WriteLine($"=== DeepSeek API Request ===");
                Console.WriteLine($"Запрос: {userQuery}");
                Console.WriteLine($"Количество автомобилей: {availableCars.Count}");

                // Формируем список автомобилей с подробными характеристиками
                var carList = string.Join("\n", availableCars.Select(c => 
                {
                    var features = "";
                    if (c.CarFeatures != null && c.CarFeatures.Any())
                    {
                        var featureStrings = c.CarFeatures.Select(cf =>
                            $"{cf.Feature?.Name ?? "?"}: {cf.FeatureValue?.Value ?? "?"}");
                        features = string.Join(", ", featureStrings);
                    }
                    var categoryName = c.Category?.Name ?? "не указана";
                    return $"- ID: {c.Id}, {c.Brand} {c.Model}, {c.Year} год, категория: {categoryName}, характеристики: [{features}], цена: {c.DailyPrice} BYN";
                }));

                // Улучшенный промт для точного соответствия
                var prompt = $@"Ты — строгий ассистент по подбору автомобилей.

**Список доступных машин:**
{carList}

**Запрос пользователя:** '{userQuery}'

**Правила отбора (очень важно!):**
1. Внимательно проанализируй запрос и выдели ВСЕ указанные характеристики (марка, модель, цвет, тип кузова, год, коробка передач и т.д.)
2. Автомобиль ДОЛЖЕН соответствовать ВСЕМ характеристикам из запроса
3. Если в запросе указан цвет — возвращай ТОЛЬКО автомобили этого цвета. Автомобили другого цвета НЕ включай в ответ
4. Если в запросе указана марка — возвращай ТОЛЬКО автомобили этой марки
5. Если в запросе указан тип кузова — возвращай ТОЛЬКО автомобили этого типа кузова
6. Если автомобиль не соответствует хотя бы одному условию — НЕ включай его в ответ
7. Если подходящих автомобилей нет — верни '0'
8. Верни **ТОЛЬКО ID** подходящих автомобилей через запятую. Пример правильного ответа: '56,61'
9. НЕ пиши никаких пояснений, только ID

Помни: точность важнее количества. Лучше вернуть 0, чем неподходящий автомобиль.";

                var requestBody = new
                {
                    model = "deepseek/deepseek-chat",
                    messages = new[] { new { role = "user", content = prompt } },
                    temperature = 0.1,
                    max_tokens = 200
                };

                // ========== ВЫВОД JSON ЗАПРОСА ==========
                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                string fullJsonRequest = JsonSerializer.Serialize(requestBody, jsonOptions);
                Console.WriteLine("=== ПОЛНЫЙ JSON ЗАПРОС К DEEPSEEK ===");
                Console.WriteLine(DecodeUnicode(fullJsonRequest));
                Console.WriteLine("========================================");
                // ========================================

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                // ========== ВЫВОД JSON ОТВЕТА ==========
                Console.WriteLine("=== ПОЛНЫЙ JSON ОТВЕТ ОТ DEEPSEEK ===");
                Console.WriteLine(DecodeUnicode(responseBody));
                Console.WriteLine("========================================");
                // ========================================

                Console.WriteLine($"Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<OpenRouterResponse>(responseBody);
                    var answer = result?.choices?[0]?.message?.content?.Trim() ?? "0";
                    Console.WriteLine($"DeepSeek Answer: {answer}");

                    var ids = System.Text.RegularExpressions.Regex.Matches(answer, @"\d+")
                        .Select(m => int.Parse(m.Value))
                        .ToList();

                    if (ids.Count > 0)
                    {
                        // Получаем автомобили по ID
                        var recommendedCars = availableCars.Where(c => ids.Contains(c.Id)).ToList();
                        
                        // Пост-фильтрация по цвету (дополнительная проверка)
                        if (!string.IsNullOrEmpty(requestedColor))
                        {
                            var colorFilteredCars = FilterByColor(recommendedCars, requestedColor);
                            if (colorFilteredCars.Count < recommendedCars.Count)
                            {
                                Console.WriteLine($"Пост-фильтрация по цвету '{requestedColor}': удалено {recommendedCars.Count - colorFilteredCars.Count} автомобилей");
                            }
                            recommendedCars = colorFilteredCars;
                        }
                        
                        if (recommendedCars.Count > 0)
                        {
                            var resultIds = string.Join(",", recommendedCars.Select(c => c.Id));
                            return resultIds;
                        }
                    }
                    
                    Console.WriteLine("Не найдено подходящих автомобилей");
                    return "0";
                }
                else
                {
                    Console.WriteLine($"API Error: {responseBody}");
                    return await KeywordSearchAsync(userQuery, availableCars);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeepSeek Exception: {ex.Message}");
                return await KeywordSearchAsync(userQuery, availableCars);
            }
        }

        private async Task<string> KeywordSearchAsync(string userQuery, List<Car> availableCars)
        {
            var keywords = userQuery.ToLower()
                .Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(k => k.Length > 2);
            
            var matchedIds = availableCars
                .Where(c => keywords.Any(k => 
                    c.Brand.ToLower().Contains(k) || 
                    c.Model.ToLower().Contains(k)))
                .Select(c => c.Id)
                .ToList();
            
            // Дополнительная фильтрация по цвету
            var requestedColor = ExtractColor(userQuery);
            if (!string.IsNullOrEmpty(requestedColor) && matchedIds.Count > 0)
            {
                var cars = availableCars.Where(c => matchedIds.Contains(c.Id)).ToList();
                var colorFilteredCars = FilterByColor(cars, requestedColor);
                if (colorFilteredCars.Count > 0)
                {
                    matchedIds = colorFilteredCars.Select(c => c.Id).ToList();
                }
            }
            
            if (matchedIds.Count == 0)
            {
                Console.WriteLine("Ничего не найдено по ключевым словам");
                return "0";
            }
            
            Console.WriteLine($"Ключевой поиск: найдено {matchedIds.Count} авто");
            return string.Join(",", matchedIds);
        }
    }

    public class OpenRouterResponse
    {
        public List<OpenRouterChoice> choices { get; set; }
    }

    public class OpenRouterChoice
    {
        public OpenRouterMessage message { get; set; }
    }

    public class OpenRouterMessage
    {
        public string content { get; set; }
    }
}
