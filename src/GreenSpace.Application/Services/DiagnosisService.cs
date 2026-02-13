using System.Text.Json;
using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.DTOs.Diagnosis;
using GreenSpace.Application.Interfaces.External;
using GreenSpace.Application.Interfaces.Repositories;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Application.Services;

/// <summary>
/// Service for plant disease diagnosis using AI Vision (Groq, Gemini, etc.)
/// Supports knowledge base matching and caching to reduce AI API calls
/// Priority: 1. Knowledge Base → 2. Cache → 3. AI API
/// </summary>
public class DiagnosisService : IDiagnosisService
{
    private readonly IAIVisionService _aiVisionService;
    private readonly IProductRepository _productRepository;
    private readonly IDiagnosisCacheService _cacheService;
    private readonly IDiseaseKnowledgeService _knowledgeService;
    private readonly ILogger<DiagnosisService> _logger;

    public DiagnosisService(
        IAIVisionService aiVisionService,
        IProductRepository productRepository,
        IDiagnosisCacheService cacheService,
        IDiseaseKnowledgeService knowledgeService,
        ILogger<DiagnosisService> logger)
    {
        _aiVisionService = aiVisionService;
        _productRepository = productRepository;
        _cacheService = cacheService;
        _knowledgeService = knowledgeService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public bool IsAvailable()
    {
        return _aiVisionService.IsAvailable();
    }

    /// <inheritdoc/>
    public async Task<IServiceResult<DiagnosisResponseDto>> DiagnoseAsync(
        DiagnosisRequestDto request,
        Guid? userId = null)
    {
        // Clean up invalid placeholder values from Swagger
        if (request.ImageBase64 == "string") request.ImageBase64 = null;
        if (request.ImageUrl == "string") request.ImageUrl = null;
        if (request.Description == "string") request.Description = null;

        // Validate request - need either image OR description
        var hasImage = !string.IsNullOrWhiteSpace(request.ImageBase64) || !string.IsNullOrWhiteSpace(request.ImageUrl);
        var hasDescription = !string.IsNullOrWhiteSpace(request.Description);

        if (!hasImage && !hasDescription)
        {
            return ServiceResult<DiagnosisResponseDto>.Failure(400,
                "Vui long cung cap hinh anh (Base64 hoac URL) hoac mo ta trieu chung cay");
        }

        // Check service availability
        if (!IsAvailable())
        {
            return ServiceResult<DiagnosisResponseDto>.Failure(503,
                ApiMessages.Diagnostic.ServiceUnavailable);
        }

        try
        {
            // =====================================================
            // KNOWLEDGE BASE LOOKUP (highest priority)
            // Only for text-based diagnosis without image
            // =====================================================
            if (!request.SkipCache && hasDescription && !hasImage)
            {
                var kbMatch = await _knowledgeService.FindMatchingDiseaseAsync(
                    request.Description!,
                    request.PlantType);

                if (kbMatch != null)
                {
                    _logger.LogInformation(
                        "Knowledge Base hit! Disease: {Disease}, Score: {Score:F2}",
                        kbMatch.DiagnosisResponse.DiseaseInfo?.DiseaseName, kbMatch.MatchScore);

                    // Update response with KB info
                    var kbResponse = kbMatch.DiagnosisResponse;
                    kbResponse.FromKnowledgeBase = true;
                    kbResponse.KnowledgeBaseMatchScore = kbMatch.MatchScore;
                    kbResponse.IsSuccessful = true;
                    kbResponse.DiagnosedAt = DateTime.UtcNow;

                    // Add debug info
                    kbResponse.DebugInfo = new GeminiDebugInfo
                    {
                        Model = "knowledge-base",
                        HasImage = false,
                        ErrorSource = null
                    };

                    // Find recommended products using ProductKeywords from KB
                    if (kbResponse.DiseaseInfo != null && !kbResponse.DiseaseInfo.IsHealthy)
                    {
                        kbResponse.RecommendedProducts = await FindRecommendedProductsByKeywordsAsync(
                            kbMatch.ProductKeywords);
                    }

                    return ServiceResult<DiagnosisResponseDto>.Success(
                        kbResponse,
                        $"Diagnosic successfully (from catch, match rate: {kbMatch.MatchScore:P0})");
                }
            }

            // =====================================================
            // STEP 2: CACHE LOOKUP (fallback from KB)
            // Only for text-based diagnosis without image
            // =====================================================
            if (!request.SkipCache && hasDescription && !hasImage)
            {
                var cacheMatch = await _cacheService.FindMatchAsync(
                    request.Description!,
                    request.PlantType,
                    hasImage);

                if (cacheMatch != null)
                {
                    _logger.LogInformation(
                        "Cache hit for diagnosis. Disease: {Disease}, Score: {Score:F2}",
                        cacheMatch.DiseaseName, cacheMatch.MatchScore);

                    // Increment hit count asynchronously (fire and forget)
                    _ = _cacheService.IncrementHitCountAsync(cacheMatch.CacheId);

                    // Update response with cache info
                    var cachedResponse = cacheMatch.DiagnosisResponse;
                    cachedResponse.FromCache = true;
                    cachedResponse.CacheMatchScore = cacheMatch.MatchScore;
                    cachedResponse.CacheId = cacheMatch.CacheId;
                    cachedResponse.IsSuccessful = true;
                    cachedResponse.DiagnosedAt = DateTime.UtcNow;

                    // Add debug info
                    cachedResponse.DebugInfo = new GeminiDebugInfo
                    {
                        Model = "cache",
                        HasImage = false,
                        ErrorSource = null
                    };

                    return ServiceResult<DiagnosisResponseDto>.Success(
                        cachedResponse,
                        $"Chan doan thanh cong (tu cache, do chinh xac: {cacheMatch.MatchScore:P0})");
                }
            }

            // =====================================================
            // CALL AI API (cache miss or image-based diagnosis)
            // =====================================================
            // Call AI Vision API
            var aiResult = await _aiVisionService.AnalyzePlantImageAsync(
                request.ImageBase64,
                request.ImageUrl,
                request.Description,
                request.Language);

            // If AI call failed, return error with debug info
            if (!aiResult.IsSuccess || string.IsNullOrWhiteSpace(aiResult.Content))
            {
                var errorMsg = hasImage
                    ? ApiMessages.Diagnostic.DiagnosicImageFailed
                    : ApiMessages.Diagnostic.DiagnosicTextFailed;

                var errorResponse = new DiagnosisResponseDto
                {
                    IsSuccessful = false,
                    ErrorMessage = errorMsg,
                    DebugInfo = MapToGeminiDebugInfo(aiResult.DebugInfo)
                };

                return ServiceResult<DiagnosisResponseDto>.SuccessWithData(errorResponse, 400, errorMsg);
            }

            // Parse AI response
            var diagnosis = ParseAIResponse(aiResult.Content);

            if (diagnosis == null)
            {
                _logger.LogWarning("Failed to parse AI response: {Response}", aiResult.Content);

                var parseErrorResponse = new DiagnosisResponseDto
                {
                    IsSuccessful = false,
                    ErrorMessage = ApiMessages.Diagnostic.DiagnosicFailed,
                    DebugInfo = new GeminiDebugInfo
                    {
                        ErrorSource = "App",
                        GeminiErrorMessage = "Failed to parse AI JSON response",
                        RawResponse = aiResult.Content?.Length > 500
                            ? aiResult.Content.Substring(0, 500) + "..."
                            : aiResult.Content,
                        Model = _aiVisionService.GetModelName(),
                        HasImage = hasImage
                    }
                };

                return ServiceResult<DiagnosisResponseDto>.SuccessWithData(parseErrorResponse, 500,
                    ApiMessages.Diagnostic.DiagnosicFailed);
            }

            // Find recommended products based on keywords
            if (diagnosis.DiseaseInfo != null && !diagnosis.DiseaseInfo.IsHealthy)
            {
                diagnosis.RecommendedProducts = await FindRecommendedProductsAsync(aiResult.Content);
            }

            diagnosis.IsSuccessful = true;
            diagnosis.DiagnosedAt = DateTime.UtcNow;
            diagnosis.FromCache = false;

            // Include debug info for successful response too (for development)
            diagnosis.DebugInfo = new GeminiDebugInfo
            {
                Model = $"{_aiVisionService.GetProviderName()}/{_aiVisionService.GetModelName()}",
                HasImage = hasImage,
                HttpStatusCode = aiResult.DebugInfo.HttpStatusCode,
                ErrorSource = null // No error
            };

            // =====================================================
            // SAVE TO CACHE (for text-based diagnosis without image)
            // =====================================================
            if (hasDescription && !hasImage && diagnosis.DiseaseInfo != null)
            {
                await _cacheService.SaveToCacheAsync(
                    request.Description!,
                    request.PlantType,
                    diagnosis,
                    hasImage);
            }

            // TODO: Save to diagnosis history if userId is provided

            return ServiceResult<DiagnosisResponseDto>.Success(diagnosis, ApiMessages.Diagnostic.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during plant diagnosis");

            var exceptionResponse = new DiagnosisResponseDto
            {
                IsSuccessful = false,
                ErrorMessage = ApiMessages.Diagnostic.DiagnosicError,
                DebugInfo = new GeminiDebugInfo
                {
                    ErrorSource = "App",
                    GeminiErrorMessage = $"Exception: {ex.Message}",
                    Model = $"{_aiVisionService.GetProviderName()}/{_aiVisionService.GetModelName()}",
                    HasImage = hasImage
                }
            };

            return ServiceResult<DiagnosisResponseDto>.SuccessWithData(exceptionResponse, StatusCodes.Status200OK,
                 ApiMessages.Diagnostic.DiagnosicError);
        }
    }

    /// <summary>
    /// Map AIDebugInfo to GeminiDebugInfo (for backward compatibility)
    /// </summary>
    private static GeminiDebugInfo MapToGeminiDebugInfo(AIDebugInfo aiDebugInfo)
    {
        return new GeminiDebugInfo
        {
            HttpStatusCode = aiDebugInfo.HttpStatusCode,
            GeminiErrorCode = aiDebugInfo.ErrorCode,
            GeminiErrorMessage = aiDebugInfo.ErrorMessage,
            RawResponse = aiDebugInfo.RawResponse,
            Model = $"{aiDebugInfo.Provider}/{aiDebugInfo.Model}",
            HasImage = aiDebugInfo.HasImage,
            ErrorSource = aiDebugInfo.ErrorSource
        };
    }

    /// <summary>
    /// Parse AI JSON response to DiagnosisResponseDto
    /// </summary>
    private DiagnosisResponseDto? ParseAIResponse(string jsonResponse)
    {
        try
        {
            // Clean JSON if needed
            var cleanJson = jsonResponse.Trim();
            if (cleanJson.StartsWith("```json"))
            {
                cleanJson = cleanJson.Substring(7);
            }
            if (cleanJson.StartsWith("```"))
            {
                cleanJson = cleanJson.Substring(3);
            }
            if (cleanJson.EndsWith("```"))
            {
                cleanJson = cleanJson.Substring(0, cleanJson.Length - 3);
            }
            cleanJson = cleanJson.Trim();

            using var doc = JsonDocument.Parse(cleanJson);
            var root = doc.RootElement;

            var response = new DiagnosisResponseDto();

            // Parse plantInfo
            if (root.TryGetProperty("plantInfo", out var plantInfo))
            {
                response.PlantInfo = new PlantInfoDto
                {
                    CommonName = GetStringProperty(plantInfo, "commonName") ?? "Khong xac dinh",
                    ScientificName = GetStringProperty(plantInfo, "scientificName"),
                    Family = GetStringProperty(plantInfo, "family"),
                    Description = GetStringProperty(plantInfo, "description")
                };
            }

            // Parse diseaseInfo
            if (root.TryGetProperty("diseaseInfo", out var diseaseInfo))
            {
                response.DiseaseInfo = new DiseaseInfoDto
                {
                    IsHealthy = GetBoolProperty(diseaseInfo, "isHealthy"),
                    DiseaseName = GetStringProperty(diseaseInfo, "diseaseName"),
                    Severity = GetStringProperty(diseaseInfo, "severity") ?? "None",
                    Symptoms = GetStringArrayProperty(diseaseInfo, "symptoms"),
                    Causes = GetStringArrayProperty(diseaseInfo, "causes"),
                    Notes = GetStringProperty(diseaseInfo, "notes")
                };
            }

            // Parse treatment
            if (root.TryGetProperty("treatment", out var treatment))
            {
                response.Treatment = new TreatmentInfoDto
                {
                    ImmediateActions = GetStringArrayProperty(treatment, "immediateActions"),
                    LongTermCare = GetStringArrayProperty(treatment, "longTermCare"),
                    PreventionTips = GetStringArrayProperty(treatment, "preventionTips"),
                    WateringAdvice = GetStringProperty(treatment, "wateringAdvice"),
                    LightingAdvice = GetStringProperty(treatment, "lightingAdvice"),
                    FertilizingAdvice = GetStringProperty(treatment, "fertilizingAdvice")
                };
            }

            // Parse confidence score
            if (root.TryGetProperty("confidenceScore", out var confidence))
            {
                response.ConfidenceScore = confidence.TryGetInt32(out var score) ? score : 70;
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing response JSON");
            return null;
        }
    }

    /// <summary>
    /// Find recommended products based on diagnosis keywords
    /// </summary>
    private async Task<List<RecommendedProductDto>> FindRecommendedProductsAsync(string geminiResponse)
    {
        var recommendations = new List<RecommendedProductDto>();

        try
        {
            // Extract product keywords from response
            var keywords = ExtractProductKeywords(geminiResponse);

            if (keywords.Count == 0)
            {
                // Generic fallback keywords for plant care products
                keywords = new List<string> { "phân bón", "thuốc bảo vệ thực vật" };//fallback keywords
            }

            // Search for products matching keywords
            var allProducts = await _productRepository.GetAllAsync();
            var matchedProducts = allProducts
                .Where(p => keywords.Any(k =>
                    (p.Name?.Contains(k, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Description?.Contains(k, StringComparison.OrdinalIgnoreCase) ?? false)))
                .Take(5)
                .ToList();

            foreach (var product in matchedProducts)
            {
                recommendations.Add(new RecommendedProductDto
                {
                    ProductId = product.ProductId,
                    Name = product.Name ?? "",
                    ThumbnailUrl = product.ThumbnailUrl,
                    Price = product.BasePrice,
                    RecommendationReason = "Sản phẩm phù hợp với tình trạng cây của bạn"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding recommended products");
        }

        return recommendations;
    }

    /// <summary>
    /// Find recommended products based on product keywords from Knowledge Base
    /// </summary>
    /// <param name="productKeywords">Keywords from Disease.ProductKeywords</param>
    private async Task<List<RecommendedProductDto>> FindRecommendedProductsByKeywordsAsync(List<string> productKeywords)
    {
        var recommendations = new List<RecommendedProductDto>();

        try
        {
            // Use keywords from KB, fallback to generic if empty
            var keywords = productKeywords?.Count > 0
                ? productKeywords
                : new List<string> { "phân bón", "thuốc bảo vệ thực vật" }; //fallback keywords

            var allProducts = await _productRepository.GetAllAsync();
            var matchedProducts = allProducts
                .Where(p => keywords.Any(k =>
                    (p.Name?.Contains(k, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Description?.Contains(k, StringComparison.OrdinalIgnoreCase) ?? false)))
                .Take(5)
                .ToList();

            foreach (var product in matchedProducts)
            {
                recommendations.Add(new RecommendedProductDto
                {
                    ProductId = product.ProductId,
                    Name = product.Name ?? "",
                    ThumbnailUrl = product.ThumbnailUrl,
                    Price = product.BasePrice,
                    RecommendationReason = "Sản phẩm phù hợp với tình trạng cây của bạn"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding recommended products by keywords");
        }

        return recommendations;
    }

    /// <summary>
    /// Extract product keywords from Gemini response
    /// </summary>
    private List<string> ExtractProductKeywords(string jsonResponse)
    {
        var keywords = new List<string>();

        try
        {
            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            if (root.TryGetProperty("productKeywords", out var keywordsArray))
            {
                foreach (var keyword in keywordsArray.EnumerateArray())
                {
                    var value = keyword.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        keywords.Add(value);
                    }
                }
            }
        }
        catch
        {
            // Ignore parsing errors, return empty list
        }

        return keywords;
    }

    #region JSON Helper Methods

    private static string? GetStringProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;
    }

    private static bool GetBoolProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.True;
    }

    private static List<string> GetStringArrayProperty(JsonElement element, string propertyName)
    {
        var list = new List<string>();
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in prop.EnumerateArray())
            {
                var value = item.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    list.Add(value);
                }
            }
        }
        return list;
    }

    #endregion
}
