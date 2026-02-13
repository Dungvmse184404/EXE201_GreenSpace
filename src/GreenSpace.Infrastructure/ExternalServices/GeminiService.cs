using System.Text;
using System.Text.Json;
using GreenSpace.Application.Common.Settings;
using GreenSpace.Application.DTOs.Diagnosis;
using GreenSpace.Application.Interfaces.External;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GreenSpace.Infrastructure.ExternalServices;

/// <summary>
/// Google Gemini AI service implementation
/// </summary>
public class GeminiService : IAIVisionService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiSettings _settings;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(
        HttpClient httpClient,
        IOptions<GeminiSettings> settings,
        ILogger<GeminiService> logger)
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
    public string GetProviderName() => "Gemini";

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
                Provider = "Gemini",
                Model = _settings.Model,
                HasImage = !string.IsNullOrWhiteSpace(imageBase64) || !string.IsNullOrWhiteSpace(imageUrl)
            }
        };

        if (!IsAvailable())
        {
            _logger.LogWarning("Gemini service is not available. Check API key configuration.");
            result.DebugInfo.ErrorSource = "App";
            result.DebugInfo.ErrorMessage = "Service not available. API key not configured or service disabled.";
            return result;
        }

        try
        {
            var hasImage = result.DebugInfo.HasImage;
            var prompt = BuildDiagnosisPrompt(userDescription, language, hasImage);
            var requestBody = BuildRequestBody(prompt, imageBase64, imageUrl);

            var url = $"{_settings.BaseUrl}/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";

            var jsonContent = JsonSerializer.Serialize(requestBody);
            _logger.LogInformation("Calling Gemini API. HasImage: {HasImage}, Model: {Model}", hasImage, _settings.Model);

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
                _logger.LogError("Gemini API error: {StatusCode} - {Error}", response.StatusCode, responseContent);
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
                        result.DebugInfo.ErrorCode = error.TryGetProperty("code", out var code)
                            ? code.GetInt32()
                            : null;

                        if (result.DebugInfo.ErrorCode == 429)
                        {
                            _logger.LogWarning("Gemini API quota exceeded. Please wait or check billing.");
                        }
                        else if (result.DebugInfo.ErrorCode == 404)
                        {
                            _logger.LogError("Gemini model not found. Please check Model setting in appsettings.json");
                        }
                    }
                }
                catch { /* Ignore parse errors */ }

                return result;
            }

            _logger.LogInformation("Gemini API response received. Length: {Length}", responseContent.Length);

            var extractedContent = ExtractTextFromGeminiResponse(responseContent);
            if (extractedContent != null)
            {
                result.IsSuccess = true;
                result.Content = extractedContent;
            }
            else
            {
                result.DebugInfo.ErrorSource = "App";
                result.DebugInfo.ErrorMessage = "Failed to extract text from Gemini response";
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API");
            result.DebugInfo.ErrorSource = "App";
            result.DebugInfo.ErrorMessage = $"Exception: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// Build the diagnosis prompt for Gemini
    /// </summary>
    private string BuildDiagnosisPrompt(string? userDescription, string language, bool hasImage)
    {
        var languageInstruction = language == "vi"
            ? "Tra loi bang tieng Viet."
            : "Respond in English.";

        string analysisInstruction;
        if (hasImage && !string.IsNullOrWhiteSpace(userDescription))
        {
            // Có cả hình ảnh và mô tả
            analysisInstruction = $@"Hay phan tich hinh anh cay nay KET HOP voi mo ta cua nguoi dung de chan doan chinh xac hon.

Mo ta cua nguoi dung: {userDescription}

Dua tren hinh anh va mo ta tren, hay chan doan tinh trang cay";
        }
        else if (hasImage)
        {
            // Chỉ có hình ảnh
            analysisInstruction = "Hay phan tich hinh anh cay nay de chan doan tinh trang suc khoe va benh (neu co)";
        }
        else
        {
            // Chỉ có mô tả text (không có hình ảnh)
            analysisInstruction = $@"KHONG CO HINH ANH. Hay chan doan tinh trang cay CHI DUA TREN mo ta cua nguoi dung.

Mo ta cua nguoi dung: {userDescription}

Dua tren mo ta tren, hay phan tich va chan doan tinh trang cay. Neu thong tin chua du, hay dua ra chan doan so bo va ghi chu nhung thong tin can bo sung";
        }

        return $@"Ban la chuyen gia chan doan benh cay trong voi nhieu nam kinh nghiem.

{analysisInstruction}

Tra loi theo format JSON chinh xac nhu sau:

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
    ""notes"": ""Ghi chu them hoac thong tin can bo sung""
  }},
  ""treatment"": {{
    ""immediateActions"": [""Hanh dong can lam ngay 1"", ""Hanh dong can lam ngay 2""],
    ""longTermCare"": [""Cham soc dai han 1"", ""Cham soc dai han 2""],
    ""preventionTips"": [""Cach phong ngua 1"", ""Cach phong ngua 2""],
    ""wateringAdvice"": ""Huong dan tuoi nuoc cu the"",
    ""lightingAdvice"": ""Huong dan anh sang cu the"",
    ""fertilizingAdvice"": ""Huong dan bon phan cu the""
  }},
  ""confidenceScore"": 85,
  ""productKeywords"": [""phan bon"", ""thuoc tru sau"", ""dat trong""]
}}

{languageInstruction}

LUU Y QUAN TRONG:
- CHI TRA LOI JSON, KHONG CO TEXT KHAC
- Dam bao JSON hop le, co the parse duoc
- confidenceScore tu 0-100 (neu chi co mo ta khong co hinh thi giam do chinh xac)
- productKeywords la cac tu khoa de tim san pham phu hop trong cua hang";
    }

    /// <summary>
    /// Build request body for Gemini API
    /// </summary>
    private object BuildRequestBody(string prompt, string? imageBase64, string? imageUrl)
    {
        var parts = new List<object>
        {
            new { text = prompt }
        };

        var hasImage = false;

        // Add image part
        if (!string.IsNullOrWhiteSpace(imageBase64))
        {
            hasImage = true;
            // Remove data URL prefix if present
            var base64Data = imageBase64;
            var mimeType = "image/jpeg";

            if (imageBase64.Contains(","))
            {
                var splitParts = imageBase64.Split(',');
                base64Data = splitParts[1];

                // Extract mime type
                if (splitParts[0].Contains("image/png"))
                    mimeType = "image/png";
                else if (splitParts[0].Contains("image/webp"))
                    mimeType = "image/webp";
            }

            parts.Add(new
            {
                inline_data = new
                {
                    mime_type = mimeType,
                    data = base64Data
                }
            });
        }
        else if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            hasImage = true;
            // For URL, we need to fetch and convert to base64
            // Gemini API doesn't directly support external URL, so we'll use file_data for Google Cloud Storage
            // For simplicity, we'll require base64 for now
            parts.Add(new
            {
                file_data = new
                {
                    mime_type = "image/jpeg",
                    file_uri = imageUrl
                }
            });
        }

        // Build generation config - only use responseMimeType for text-only requests
        // Some models work better without it for vision tasks
        var generationConfig = new Dictionary<string, object>
        {
            { "temperature", _settings.Temperature },
            { "maxOutputTokens", _settings.MaxOutputTokens }
        };

        // Add responseMimeType for text-only to ensure JSON output
        if (!hasImage)
        {
            generationConfig["responseMimeType"] = "application/json";
        }

        return new
        {
            contents = new[]
            {
                new
                {
                    parts = parts.ToArray()
                }
            },
            generationConfig
        };
    }

    /// <summary>
    /// Extract text content from Gemini response
    /// </summary>
    private string? ExtractTextFromGeminiResponse(string response)
    {
        try
        {
            _logger.LogDebug("Parsing Gemini response: {Response}", response);

            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            // Check for error in response
            if (root.TryGetProperty("error", out var error))
            {
                var errorMessage = error.TryGetProperty("message", out var msg) ? msg.GetString() : "Unknown error";
                _logger.LogError("Gemini API returned error: {Error}", errorMessage);
                return null;
            }

            if (root.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];

                // Check for blocked content
                if (firstCandidate.TryGetProperty("finishReason", out var finishReason))
                {
                    var reason = finishReason.GetString();
                    if (reason != "STOP")
                    {
                        _logger.LogWarning("Gemini response blocked. Reason: {Reason}", reason);
                    }
                }

                if (firstCandidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0)
                {
                    var firstPart = parts[0];
                    if (firstPart.TryGetProperty("text", out var text))
                    {
                        var textContent = text.GetString();
                        _logger.LogInformation("Successfully extracted text from Gemini. Length: {Length}", textContent?.Length ?? 0);
                        return textContent;
                    }
                }
            }

            _logger.LogWarning("Unexpected Gemini response format: {Response}", response);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Gemini response: {Response}", response);
            return null;
        }
    }
}
