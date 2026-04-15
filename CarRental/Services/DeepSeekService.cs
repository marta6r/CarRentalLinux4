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
//             _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
//         }

//         public async Task<string> GetCarRecommendationAsync(string userQuery, List<Car> availableCars)
//         {
//             try
//             {
//                 Console.WriteLine($"=== DeepSeek API Request ===");
//                 Console.WriteLine($"Запрос: {userQuery}");
//                 Console.WriteLine($"Количество автомобилей: {availableCars.Count}");

//                 // Формируем список автомобилей с характеристиками
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

//                 var prompt = $@"Ты — ассистент по подбору автомобилей.
// Вот список доступных машин:
// {carList}

// Запрос пользователя: '{userQuery}'

// Важно: Учитывай все характеристики: цвет, коробку передач, двигатель, привод, количество мест и т.д.
// Верни **только ID** автомобилей (через запятую), которые лучше всего подходят под описание пользователя.
// Если ни одна машина не подходит, верни '0'.
// Не пиши никаких пояснений, только ID машин. Пример ответа: '27,28,29'";

//                 var requestBody = new
//                 {
//                     model = "deepseek-chat",
//                     messages = new[]
//                     {
//                         new { role = "system", content = "Ты полезный ассистент по подбору автомобилей." },
//                         new { role = "user", content = prompt }
//                     },
//                     temperature = 0.1,
//                     max_tokens = 500
//                 };

//                 var json = JsonSerializer.Serialize(requestBody);
//                 var content = new StringContent(json, Encoding.UTF8, "application/json");

//                 var response = await _httpClient.PostAsync("https://api.deepseek.com/v1/chat/completions", content);
//                 var responseBody = await response.Content.ReadAsStringAsync();

//                 Console.WriteLine($"Response Status: {response.StatusCode}");

//                 if (!response.IsSuccessStatusCode)
//                 {
//                     Console.WriteLine($"DeepSeek Error: {responseBody}");
//                     return string.Join(",", availableCars.Select(c => c.Id));
//                 }

//                 var result = JsonSerializer.Deserialize<DeepSeekResponse>(responseBody);
//                 var answer = result?.choices?[0]?.message?.content?.Trim() ?? "0";
//                 Console.WriteLine($"DeepSeek Answer: {answer}");

//                 var ids = System.Text.RegularExpressions.Regex.Matches(answer, @"\d+")
//                     .Select(m => int.Parse(m.Value))
//                     .ToList();

//                 if (ids.Count == 0)
//                 {
//                     Console.WriteLine("Не удалось извлечь ID, возвращаем все автомобили");
//                     return string.Join(",", availableCars.Select(c => c.Id));
//                 }

//                 Console.WriteLine($"Найденные ID: {string.Join(",", ids)}");
//                 return string.Join(",", ids);
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"DeepSeek Exception: {ex.Message}");
//                 return string.Join(",", availableCars.Select(c => c.Id));
//             }
//         }
//     }

//     // Классы для десериализации ответа
//     public class DeepSeekResponse
//     {
//         public List<Choice> choices { get; set; }
//     }

//     public class Choice
//     {
//         public Message message { get; set; }
//     }

//     public class Message
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
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<string> GetCarRecommendationAsync(string userQuery, List<Car> availableCars)
        {
            try
            {
                Console.WriteLine($"=== DeepSeek API Request (OpenRouter) ===");
                Console.WriteLine($"Запрос: {userQuery}");
                Console.WriteLine($"Количество автомобилей: {availableCars.Count}");

                // Формируем список автомобилей с характеристиками
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
Вот список доступных машин:
{carList}

Запрос пользователя: '{userQuery}'

Важно: Учитывай все характеристики: цвет, коробку передач, двигатель, привод, количество мест и т.д.
Верни **только ID** автомобилей (через запятую), которые лучше всего подходят под описание пользователя.
Если ни одна машина не подходит, верни '0'.
Не пиши никаких пояснений, только ID машин. Пример ответа: '27,28,29'";

                var requestBody = new
                {
                    model = "deepseek/deepseek-chat",
                    messages = new[]
                    {
                        new { role = "system", content = "Ты полезный ассистент по подбору автомобилей." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.1,
                    max_tokens = 500
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // URL OpenRouter API
                var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Response Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"OpenRouter Error: {responseBody}");
                    return string.Join(",", availableCars.Select(c => c.Id));
                }

                var result = JsonSerializer.Deserialize<OpenRouterResponse>(responseBody);
                var answer = result?.choices?[0]?.message?.content?.Trim() ?? "0";
                Console.WriteLine($"DeepSeek Answer: {answer}");

                var ids = System.Text.RegularExpressions.Regex.Matches(answer, @"\d+")
                    .Select(m => int.Parse(m.Value))
                    .ToList();

                if (ids.Count == 0)
                {
                    Console.WriteLine("Не удалось извлечь ID, возвращаем все автомобили");
                    return string.Join(",", availableCars.Select(c => c.Id));
                }

                Console.WriteLine($"Найденные ID: {string.Join(",", ids)}");
                return string.Join(",", ids);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeepSeek Exception: {ex.Message}");
                return string.Join(",", availableCars.Select(c => c.Id));
            }
        }
    }

    // Классы для десериализации ответа OpenRouter
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