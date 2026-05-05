using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AiChatBackend.Sevices
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private const string GroqApiUrl = "https://api.groq.com/openai/v1/chat/completions";

        public GeminiService(IConfiguration config)
        {
            _httpClient = new HttpClient();

            // Read Groq API key from configuration
            _apiKey = config["Groq:ApiKey"]
                ?? throw new Exception("Groq API key is missing. Please add it to appsettings.json");

            // Default model - fast and free tier
            _model = config["Groq:Model"] ?? "llama-3.1-8b-instant";

            // Set authorization header for Groq
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> GetResponseAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "Please enter a message.";

            int maxRetries = 3;
            int retryDelayMs = 1000;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    // Prepare request body in OpenAI-compatible format
                    var requestBody = new
                    {
                        model = _model,
                        messages = new[]
                        {
                            new
                            {
                                role = "user",
                                content = message
                            }
                        },
                        max_tokens = 150,
                        temperature = 0.7
                    };

                    var json = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Send request to Groq API
                    var response = await _httpClient.PostAsync(GroqApiUrl, content);
                    var result = await response.Content.ReadAsStringAsync();

                    // Check for rate limiting (429)
                    if ((int)response.StatusCode == 429)
                    {
                        Console.WriteLine($"Rate limited by Groq. Attempt {attempt + 1} of {maxRetries}");

                        if (attempt < maxRetries - 1)
                        {
                            // Exponential backoff: 1s, 2s, 4s
                            int delay = retryDelayMs * (int)Math.Pow(2, attempt);
                            await Task.Delay(delay);
                            continue;
                        }
                        return "AI service is currently rate limited. Please try again in a moment.";
                    }

                    // Check for other HTTP errors
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Groq API error: {response.StatusCode} - {result}");
                        return $"AI service error: {response.StatusCode}";
                    }

                    // Parse the response
                    using var doc = JsonDocument.Parse(result);

                    // Extract the assistant's reply from OpenAI-compatible response format
                    var assistantMessage = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    return assistantMessage ?? "Sorry, I couldn't generate a response.";
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("429"))
                {
                    Console.WriteLine($"Rate limit exception. Attempt {attempt + 1}");
                    if (attempt < maxRetries - 1)
                    {
                        int delay = retryDelayMs * (int)Math.Pow(2, attempt);
                        await Task.Delay(delay);
                        continue;
                    }
                    return "AI service is busy. Please try again.";
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"JSON parsing error: {ex.Message}");
                    return "Error processing AI response.";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    return $"Error: {ex.Message}";
                }
            }

            return "AI service is temporarily unavailable. Please try again later.";
        }
    }
}