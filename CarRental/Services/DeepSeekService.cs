using System.Text;
using System.Text.Json;
using CarRental.Models;

namespace CarRental.Services
{
    public class DeepSeekService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly Dictionary<string, string> _cache = new(); // Кэш для ответов

        public DeepSeekService(IConfiguration configuration)
        {
            _apiKey = configuration["DeepSeek:ApiKey"];
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "CarRental/1.0");
        }

        public async Task<string> GetCarRecommendationAsync(string userQuery, List<Car> availableCars)
        {
            // Проверяем кэш
            var cacheKey = $"{userQuery}_{availableCars.Count}";
            if (_cache.ContainsKey(cacheKey))
            {
                Console.WriteLine($"=== Используем кэш для запроса: {userQuery} ===");
                return _cache[cacheKey];
            }

            // Пытаемся выполнить API-запрос с повторными попытками
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    Console.WriteLine($"=== DeepSeek API Request (OpenRouter) - попытка {attempt} ===");
                    Console.WriteLine($"Запрос: {userQuery}");
                    Console.WriteLine($"Количество автомобилей: {availableCars.Count}");

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

                    var prompt = $@"Ты — ассистент по подбору автомобилей.
Список машин:
{carList}

Запрос: '{userQuery}'

Верни **только ID** подходящих авто через запятую. Если ничего не подходит — '0'.
Без пояснений. Пример: '27,28,29'";

                    var requestBody = new
                    {
                        model = "deepseek/deepseek-chat",
                        messages = new[] { new { role = "user", content = prompt } },
                        temperature = 0.1,
                        max_tokens = 200
                    };

                    var json = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
                    var responseBody = await response.Content.ReadAsStringAsync();

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
                            var resultIds = string.Join(",", ids);
                            _cache[cacheKey] = resultIds;
                            return resultIds;
                        }
                        
                        // Если ID не найдены, возвращаем "0" (пустой результат)
                        _cache[cacheKey] = "0";
                        return "0";
                    }
                    else
                    {
                        Console.WriteLine($"API Error: {responseBody}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Attempt {attempt} failed: {ex.Message}");
                    if (attempt == 3)
                    {
                        Console.WriteLine("Все попытки API не удались, используем ключевой поиск");
                    }
                }
                
                // Ждём перед повторной попыткой
                if (attempt < 3) await Task.Delay(1000);
            }

            // Если API не работает, используем ключевой поиск
            var keywordResult = await KeywordSearchAsync(userQuery, availableCars);
            _cache[cacheKey] = keywordResult;
            return keywordResult;
        }

        private async Task<string> KeywordSearchAsync(string userQuery, List<Car> availableCars)
        {
            var keywords = userQuery.ToLower()
                .Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(k => k.Length > 2); // Игнорируем слишком короткие слова
            
            var matchedIds = availableCars
                .Where(c => keywords.Any(k => 
                    c.Brand.ToLower().Contains(k) || 
                    c.Model.ToLower().Contains(k)))
                .Select(c => c.Id)
                .ToList();
            
            // ИСПРАВЛЕНО: возвращаем "0", а не все автомобили
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