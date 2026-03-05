using System.Diagnostics;
using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.DTOs.Chatbox;
using GreenSpace.Application.Interfaces.External;
using GreenSpace.Application.Interfaces.Repositories;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Common;
using GreenSpace.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Application.Services;

/// <summary>
/// Service for AI-powered chatbox conversations about plants
/// Supports general Q&A (CHAT MODE) and plant diagnosis (IMAGE MODE)
/// - Chat Mode: Uses IChatService for natural conversational responses (like ChatGPT)
/// - Image Mode: Uses IAIVisionService for structured plant diagnosis
/// </summary>
public class ChatboxService : IChatboxService
{
    private readonly IChatService _chatService;
    private readonly IAIVisionService _aiVisionService;
    private readonly IProductRepository _productRepository;
    private readonly IDiseaseKnowledgeService _knowledgeService;
    private readonly ILogger<ChatboxService> _logger;

    public ChatboxService(
        IChatService chatService,
        IAIVisionService aiVisionService,
        IProductRepository productRepository,
        IDiseaseKnowledgeService knowledgeService,
        ILogger<ChatboxService> logger)
    {
        _chatService = chatService;
        _aiVisionService = aiVisionService;
        _productRepository = productRepository;
        _knowledgeService = knowledgeService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public bool IsAvailable()
    {
        return _chatService.IsAvailable();
    }

    /// <inheritdoc/>
    public async Task<ServiceStatusDto> GetServiceStatusAsync()
    {
        return new ServiceStatusDto
        {
            IsAvailable = IsAvailable(),
            Provider = _chatService.GetProviderName(),
            Model = _chatService.GetModelName(),
            Message = IsAvailable()
                ? "Dich vu chatbox AI dang hoat dong binh thuong"
                : "Dich vu chatbox AI tam thoi khong kha dung"
        };
    }

    /// <inheritdoc/>
    public async Task<IServiceResult<ChatboxResponseDto>> SendMessageAsync(
        ChatboxRequestDto request,
        Guid? userId = null)
    {
        var stopwatch = Stopwatch.StartNew();

        // Clean up invalid placeholder values from Swagger
        if (request.Message == "string") request.Message = string.Empty;
        if (request.ImageBase64 == "string") request.ImageBase64 = null;
        if (request.ImageUrl == "string") request.ImageUrl = null;
        if (request.PlantType == "string") request.PlantType = null;

        // Validate request
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return ServiceResult<ChatboxResponseDto>.Failure(400,
                "Vui long cung cap tin nhan");
        }

        // Check service availability
        if (!IsAvailable())
        {
            return ServiceResult<ChatboxResponseDto>.Failure(503,
                "Dich vu chatbox AI tam thoi khong kha dung");
        }

        try
        {
            var hasImage = !string.IsNullOrWhiteSpace(request.ImageBase64) || 
                          !string.IsNullOrWhiteSpace(request.ImageUrl);

            ChatboxResponseDto response;

            // =====================================================
            // DECISION: Use appropriate AI mode
            // =====================================================
            if (hasImage)
            {
                // IMAGE ANALYSIS MODE: Use IAIVisionService (plant diagnosis)
                response = await HandleImageAnalysisAsync(request, stopwatch);
            }
            else
            {
                // CHAT MODE: Use IChatService (conversational like ChatGPT)
                response = await HandleChatModeAsync(request, stopwatch);
            }

            stopwatch.Stop();
            response.DebugInfo!.ResponseTimeMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Chatbox message processed. Type: {Type}, ResponseTime: {Time}ms, HasImage: {HasImage}",
                response.ResponseType, stopwatch.ElapsedMilliseconds, hasImage);

            return ServiceResult<ChatboxResponseDto>.Success(response, "Xu ly thong diep thanh cong");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex, "Error in ChatboxService.SendMessageAsync. UserId: {UserId}", userId);

            var errorResponse = new ChatboxResponseDto
            {
                IsSuccessful = false,
                ErrorMessage = "Co loi xay ra khi xu ly yeu cau cua ban. Vui long thu lai sau.",
                ResponseType = "error",
                DebugInfo = new ChatboxDebugInfoDto
                {
                    Provider = _chatService.GetProviderName(),
                    Model = _chatService.GetModelName(),
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    ErrorMessage = ex.Message,
                    CacheHit = false
                }
            };

            return ServiceResult<ChatboxResponseDto>.SuccessWithData(errorResponse, 500,
                "Internal server error");
        }
    }

    /// <summary>
    /// Handle image analysis mode (plant diagnosis with structure)
    /// </summary>
    private async Task<ChatboxResponseDto> HandleImageAnalysisAsync(
        ChatboxRequestDto request,
        Stopwatch stopwatch)
    {
        var aiResult = await _aiVisionService.AnalyzePlantImageAsync(
            request.ImageBase64,
            request.ImageUrl,
            request.Message,
            request.Language);

        if (!aiResult.IsSuccess || string.IsNullOrWhiteSpace(aiResult.Content))
        {
            _logger.LogError("Image analysis failed. Provider: {Provider}",
                _aiVisionService.GetProviderName());

            return new ChatboxResponseDto
            {
                IsSuccessful = false,
                ErrorMessage = "Khong the phan tich hinh anh. Vui long thu lai voi hinh anh khac.",
                ResponseType = "error",
                DebugInfo = MapToDebugInfo(aiResult.DebugInfo, stopwatch.ElapsedMilliseconds, false)
            };
        }

        // For image analysis, keep the structured response
        return new ChatboxResponseDto
        {
            IsSuccessful = true,
            Message = aiResult.Content,
            ResponseType = "diagnosis",
            PlantDetection = ExtractPlantDetection(aiResult.Content),
            DiseaseDetection = ExtractDiseaseDetection(aiResult.Content),
            DebugInfo = MapToDebugInfo(aiResult.DebugInfo, stopwatch.ElapsedMilliseconds, false),
            RespondedAt = DateTime.UtcNow,
            //RecommendedProducts = request.IncludeProductRecommendations
            //    ? await GetRecommendedProductsAsync(aiResult.Content, request.Language)
            //    : null,
            SuggestedFollowUps = GenerateSuggestedFollowUps("diagnosis", request.Language)
        };
    }

    /// <summary>
    /// Handle pure chat mode (conversational, like ChatGPT)
    /// </summary>
    private async Task<ChatboxResponseDto> HandleChatModeAsync(
        ChatboxRequestDto request,
        Stopwatch stopwatch)
    {
        var chatResult = await _chatService.GetChatResponseAsync(request.Message, request.Language);

        if (!chatResult.IsSuccess || string.IsNullOrWhiteSpace(chatResult.Response))
        {
            _logger.LogError("Chat API failed. Error: {Error}", chatResult.ErrorMessage);

            return new ChatboxResponseDto
            {
                IsSuccessful = false,
                ErrorMessage = "Xin loi, co loi xay ra. Vui long thu lai sau.",
                ResponseType = "error",
                DebugInfo = new ChatboxDebugInfoDto
                {
                    Provider = chatResult.DebugInfo?.Provider,
                    Model = chatResult.DebugInfo?.Model,
                    ResponseTimeMs = chatResult.DebugInfo?.ResponseTimeMs ?? 0,
                    ErrorMessage = chatResult.ErrorMessage,
                    HttpStatusCode = chatResult.DebugInfo?.HttpStatusCode
                }
            };
        }

        // Determine if the chat message relates to products
        var responseType = DetermineResponseType(chatResult.Response, request.Message);

        return new ChatboxResponseDto
        {
            IsSuccessful = true,
            Message = chatResult.Response,
            ResponseType = responseType,
            DebugInfo = new ChatboxDebugInfoDto
            {
                Provider = chatResult.DebugInfo?.Provider,
                Model = chatResult.DebugInfo?.Model,
                HasImage = false,
                ResponseTimeMs = chatResult.DebugInfo?.ResponseTimeMs ?? 0,
                HttpStatusCode = chatResult.DebugInfo?.HttpStatusCode,
                CacheHit = false
            },
            RespondedAt = DateTime.UtcNow,
            //RecommendedProducts = request.IncludeProductRecommendations && 
            //    (responseType == "product_recommendation" || responseType == "care_advice")
            //    ? await GetRecommendedProductsAsync(chatResult.Response, request.Language)
            //    : null,
            SuggestedFollowUps = GenerateSuggestedFollowUps(responseType, request.Language)
        };
    }

    /// <summary>
    /// Determine the type of response based on content analysis
    /// </summary>
    private string DetermineResponseType(string content, string userMessage)
    {
        var lowerContent = (content + " " + userMessage).ToLower();

        // Keywords for different response types
        var diagnosisKeywords = new[] { "benh", "disease", "infection", "problem", "issue", "bi" };
        var careKeywords = new[] { "cham soc", "care tips", "huong dan", "guide", "lam sao" };
        var recommendationKeywords = new[] { "dua ra", "recommend", "suggest", "khuyen nghi", "san pham" };

        if (diagnosisKeywords.Any(kw => lowerContent.Contains(kw)))
            return "diagnosis";

        if (careKeywords.Any(kw => lowerContent.Contains(kw)))
            return "care_advice";

        if (recommendationKeywords.Any(kw => lowerContent.Contains(kw)))
            return "product_recommendation";

        return "general";
    }

    /// <summary>
    /// Extract plant detection information from AI response
    /// </summary>
    private PlantDetectionDto? ExtractPlantDetection(string aiResponse)
    {
        // In a production scenario, you would parse the AI response more carefully
        // For now, return null as plants are typically detected in the response text
        return null;
    }

    /// <summary>
    /// Extract disease detection information from AI response
    /// </summary>
    private DiseaseDetectionDto? ExtractDiseaseDetection(string aiResponse)
    {
        // In a production scenario, you would parse the AI response more carefully
        // For now, return null as diseases are typically mentioned in the response text
        return null;
    }

    /// <summary>
    /// Get product recommendations based on AI response content
    /// </summary>
    //private async Task<List<ChatboxProductRecommendationDto>?> GetRecommendedProductsAsync(
    //    string aiContent, 
    //    string language)
    //{
    //    try
    //    {
    //        // Extract keywords from AI response that might match products
    //        var keywords = ExtractKeywordsForProducts(aiContent);

    //        if (!keywords.Any())
    //            return null;

    //        // Get products matching keywords (simplified approach)
    //        var products = await _productRepository.GetProductsByCategoryAsync("fertilizer", 0, 5);

    //        if (products == null || !products.Any())
    //            return null;

    //        var recommendations = products
    //            .Take(3)
    //            .Select(p => new ChatboxProductRecommendationDto
    //            {
    //                ProductId = p.Id,
    //                ProductName = p.Name,
    //                RecommendationReason = GetRecommendationReason(p.Name, language),
    //                Price = p.BasePrice,
    //                Category = p.CategoryId.ToString()
    //            })
    //            .ToList();

    //        return recommendations.Any() ? recommendations : null;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogWarning(ex, "Error getting product recommendations");
    //        return null;
    //    }
    //}

    /// <summary>
    /// Extract product-related keywords from AI response
    /// </summary>
    private List<string> ExtractKeywordsForProducts(string aiContent)
    {
        var keywords = new List<string>();

        // Product-related keywords (Vietnamese and English)
        var productKeywords = new Dictionary<string, string>
        {
            { "phan ban", "fertilizer" },
            { "thuoc", "pesticide" },
            { "dat", "soil" },
            { "vitamin", "vitamin" },
            { "treat", "treatment" },
            { "spray", "spray" },
            { "tan phan", "fertilizer" }
        };

        foreach (var (keyword, category) in productKeywords)
        {
            if (aiContent.ToLower().Contains(keyword))
                keywords.Add(category);
        }

        return keywords;
    }

    /// <summary>
    /// Get recommendation reason based on product and language
    /// </summary>
    private string GetRecommendationReason(string productName, string language)
    {
        return language == "vi"
            ? $"San pham nay co the giup ban cham soc cay hieu qua"
            : "This product can help you care for your plants effectively";
    }

    /// <summary>
    /// Generate suggested follow-up questions
    /// </summary>
    private List<string> GenerateSuggestedFollowUps(string responseType, string language)
    {
        return language == "vi" ? responseType switch
        {
            "diagnosis" => new List<string>
            {
                "Lam sao de xu ly tiep theo?",
                "Neu co san pham huy tro khong?",
                "Bao lau cay se khoe lai?"
            },
            "care_advice" => new List<string>
            {
                "Tan suat tuoi nuoc ra sao?",
                "Co can phan bon khong?",
                "Cay can anh sang bao nhieu?"
            },
            "product_recommendation" => new List<string>
            {
                "San pham nay co chat luong khong?",
                "Gia ca hợp ly khong?",
                "Co san pham nao re hon khong?"
            },
            _ => new List<string>
            {
                "Co cau hoi gi khac khong?",
                "Ban can ho tro gi them?",
                "Hay cho biet them chi tiet?"
            }
        }
        : responseType switch
        {
            "diagnosis" => new List<string>
            {
                "What should I do next?",
                "Are there any recommended products?",
                "How long until the plant recovers?"
            },
            "care_advice" => new List<string>
            {
                "How often should I water?",
                "Does it need fertilizer?",
                "How much sunlight does it need?"
            },
            "product_recommendation" => new List<string>
            {
                "Is this product of good quality?",
                "Is the price reasonable?",
                "Are there cheaper alternatives?"
            },
            _ => new List<string>
            {
                "Any other questions?",
                "What else can I help with?",
                "Tell me more details?"
            }
        };
    }

    /// <summary>
    /// Map AI debug info to chatbox debug info
    /// </summary>
    private ChatboxDebugInfoDto MapToDebugInfo(
        AIDebugInfo? aiDebugInfo, 
        long responseTimeMs, 
        bool cacheHit)
    {
        return new ChatboxDebugInfoDto
        {
            Provider = aiDebugInfo?.Provider ?? _aiVisionService.GetProviderName(),
            Model = aiDebugInfo?.Model ?? _aiVisionService.GetModelName(),
            HasImage = aiDebugInfo?.HasImage ?? false,
            ResponseTimeMs = responseTimeMs,
            CacheHit = cacheHit,
            ErrorMessage = aiDebugInfo?.ErrorMessage,
            HttpStatusCode = aiDebugInfo?.HttpStatusCode
        };
    }
}
