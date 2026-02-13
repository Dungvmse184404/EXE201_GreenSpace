using System.Text;
using System.Text.Json;
using GreenSpace.Application.Common.Settings;
using GreenSpace.Application.Interfaces.External;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GreenSpace.Infrastructure.ExternalServices;

/// <summary>
/// Groq AI service implementation for plant diagnosis
/// Uses Llama 3.2 Vision model
/// </summary>
public class GroqService : IAIVisionService
{
    private readonly HttpClient _httpClient;
    private readonly GroqSettings _settings;
    private readonly ILogger<GroqService> _logger;

    public GroqService(
        HttpClient httpClient,
        IOptions<GroqSettings> settings,
        ILogger<GroqService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public bool IsAvailable()
    {
        return _settings.IsEnabled && !string.IsNullOrWhiteSpace(_settings.ApiKey);
    }

    /// <inheritdoc/>
    public string GetModelName() => _settings.Model ?? "unknown";

    /// <inheritdoc/>
    public string GetProviderName() => "Groq";

    /// <inheritdoc/>
    public async Task<AIAnalysisResult> AnalyzePlantImageAsync(
        string? imageBase64,
        string? imageUrl,
        string? userDescription,
        string language = "vi")
    {
        var result = new AIAnalysisResult
        {
            DebugInfo = new AIDebugInfo
            {
                Provider = "Groq",
                Model = _settings.Model,
                HasImage = !string.IsNullOrWhiteSpace(imageBase64) || !string.IsNullOrWhiteSpace(imageUrl)
            }
        };

        if (!IsAvailable())
        {
            _logger.LogWarning("Groq service is not available. Check API key configuration.");
            result.DebugInfo.ErrorSource = "App";
            result.DebugInfo.ErrorMessage = "Service not available. API key not configured or service disabled.";
            return result;
        }

        try
        {
            var hasImage = result.DebugInfo.HasImage;
            var messages = BuildMessages(userDescription, language, hasImage, imageBase64, imageUrl);
            var requestBody = BuildRequestBody(messages);

            var url = $"{_settings.BaseUrl}/chat/completions";

            var jsonContent = JsonSerializer.Serialize(requestBody);
            _logger.LogInformation("Calling Groq API. HasImage: {HasImage}, Model: {Model}", hasImage, _settings.Model);

            // Set Authorization header
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");

            var response = await _httpClient.PostAsync(url,
                new StringContent(jsonContent, Encoding.UTF8, "application/json"));

            var responseContent = await response.Content.ReadAsStringAsync();
            result.DebugInfo.HttpStatusCode = (int)response.StatusCode;

            // Truncate raw response for debug (max 500 chars)
            result.DebugInfo.RawResponse = responseContent.Length > 500
                ? responseContent.Substring(0, 500) + "..."
                : responseContent;

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Groq API error: {StatusCode} - {Error}", response.StatusCode, responseContent);
                result.DebugInfo.ErrorSource = "AI";

                // Parse error for better info
                try
                {
                    using var errorDoc = JsonDocument.Parse(responseContent);
                    if (errorDoc.RootElement.TryGetProperty("error", out var error))
                    {
                        result.DebugInfo.ErrorMessage = error.TryGetProperty("message", out var msg)
                            ? msg.GetString()
                            : "Unknown error";

                        if ((int)response.StatusCode == 429)
                        {
                            _logger.LogWarning("Groq API rate limit exceeded. Please wait.");
                        }
                    }
                }
                catch { /* Ignore parse errors */ }

                return result;
            }

            _logger.LogInformation("Groq API response received. Length: {Length}", responseContent.Length);

            var extractedContent = ExtractContentFromResponse(responseContent);
            if (extractedContent != null)
            {
                result.IsSuccess = true;
                result.Content = extractedContent;
            }
            else
            {
                result.DebugInfo.ErrorSource = "App";
                result.DebugInfo.ErrorMessage = "Failed to extract content from Groq response";
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Groq API");
            result.DebugInfo.ErrorSource = "App";
            result.DebugInfo.ErrorMessage = $"Exception: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// Build messages array for Groq API (OpenAI-compatible format)
    /// </summary>
    private List<object> BuildMessages(string? userDescription, string language, bool hasImage, string? imageBase64, string? imageUrl)
    {
        var systemPrompt = BuildSystemPrompt(language);
        var userPrompt = BuildUserPrompt(userDescription, hasImage);

        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        // Build user message with optional image
        if (hasImage)
        {
            var contentParts = new List<object>
            {
                new { type = "text", text = userPrompt }
            };

            // Add image
            if (!string.IsNullOrWhiteSpace(imageBase64))
            {
                var imageData = imageBase64;
                if (!imageBase64.StartsWith("data:"))
                {
                    imageData = $"data:image/jpeg;base64,{imageBase64}";
                }

                contentParts.Add(new
                {
                    type = "image_url",
                    image_url = new { url = imageData }
                });
            }
            else if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                contentParts.Add(new
                {
                    type = "image_url",
                    image_url = new { url = imageUrl }
                });
            }

            messages.Add(new { role = "user", content = contentParts.ToArray() });
        }
        else
        {
            messages.Add(new { role = "user", content = userPrompt });
        }

        return messages;
    }

    /// <summary>
    /// Build system prompt for plant diagnosis
    /// </summary>
    private string BuildSystemPrompt(string language)
    {
        var languageInstruction = language == "vi"
            ? "Tra loi bang tieng Viet."
            : "Respond in English.";

        return $@"Ban la chuyen gia chan doan benh cay trong voi nhieu nam kinh nghiem.

{languageInstruction}

Tra loi theo format JSON chinh xac nhu sau (KHONG co text khac ngoai JSON):

{{
  ""plantInfo"": {{
    ""commonName"": ""Ten thong thuong cua cay"",
    ""scientificName"": ""Ten khoa hoc (neu biet)"",
    ""family"": ""Ho cay (neu biet)"",
    ""description"": ""Mo ta ngan ve cay""
  }},
  ""diseaseInfo"": {{
    ""isHealthy"": true hoac false,
    ""diseaseName"": ""Ten benh hoac 'Khoe manh' neu cay khoe"",
    ""severity"": ""None/Low/Medium/High/Critical"",
    ""symptoms"": [""Trieu chung 1"", ""Trieu chung 2""],
    ""causes"": [""Nguyen nhan 1"", ""Nguyen nhan 2""],
    ""notes"": ""Ghi chu them""
  }},
  ""treatment"": {{
    ""immediateActions"": [""Hanh dong 1"", ""Hanh dong 2""],
    ""longTermCare"": [""Cham soc 1"", ""Cham soc 2""],
    ""preventionTips"": [""Phong ngua 1"", ""Phong ngua 2""],
    ""wateringAdvice"": ""Huong dan tuoi nuoc"",
    ""lightingAdvice"": ""Huong dan anh sang"",
    ""fertilizingAdvice"": ""Huong dan bon phan""
  }},
  ""confidenceScore"": 85,
  ""productKeywords"": [""phan bon"", ""thuoc tru sau""]
}}";
    }

    /// <summary>
    /// Build user prompt based on input
    /// </summary>
    private string BuildUserPrompt(string? userDescription, bool hasImage)
    {
        if (hasImage && !string.IsNullOrWhiteSpace(userDescription))
        {
            return $"Phan tich hinh anh cay nay ket hop voi mo ta: {userDescription}";
        }
        else if (hasImage)
        {
            return "Phan tich hinh anh cay nay va chan doan tinh trang suc khoe.";
        }
        else
        {
            return $"Chan doan tinh trang cay dua tren mo ta sau: {userDescription}";
        }
    }

    /// <summary>
    /// Build request body for Groq API
    /// </summary>
    private object BuildRequestBody(List<object> messages)
    {
        return new
        {
            model = _settings.Model,
            messages = messages.ToArray(),
            max_tokens = _settings.MaxTokens,
            temperature = _settings.Temperature,
            response_format = new { type = "json_object" }
        };
    }

    /// <summary>
    /// Extract content from Groq response (OpenAI-compatible format)
    /// </summary>
    private string? ExtractContentFromResponse(string response)
    {
        try
        {
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            if (root.TryGetProperty("choices", out var choices) &&
                choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];
                if (firstChoice.TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var content))
                {
                    var textContent = content.GetString();
                    _logger.LogInformation("Successfully extracted content from Groq. Length: {Length}", textContent?.Length ?? 0);
                    return textContent;
                }
            }

            _logger.LogWarning("Unexpected Groq response format: {Response}", response);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Groq response");
            return null;
        }
    }
}
