using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AiChatBackend.Sevices
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly Random _random;
        private readonly string[] _fallbackModels;

        public GeminiService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = config["Gemini:ApiKey"] ?? throw new Exception("Gemini API key missing");
            _random = new Random();

            _fallbackModels = new[]
            {
                "gemini-2.5-flash",
                "gemini-2.5-pro",
                "gemini-2.0-flash"
            };
        }

        public async Task<string> GetResponseAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "Please provide a message.";

            foreach (var model in _fallbackModels)
            {
                var result = await TryCallModelAsync(message, model);

                if (!result.Contains("Error"))
                    return result;
            }

            return "AI service unavailable. Try again later.";
        }

        private async Task<string> TryCallModelAsync(string message, string model)
        {
            try
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";

                var body = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = message }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return $"Error: {response.StatusCode}";

                using var doc = JsonDocument.Parse(result);

                return doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "No response";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}