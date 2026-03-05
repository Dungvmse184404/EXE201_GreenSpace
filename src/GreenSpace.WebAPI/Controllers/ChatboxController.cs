using GreenSpace.Application.DTOs.Chatbox;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.WebAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.WebAPI.Controllers;

/// <summary>
/// AI-powered chatbox for plant care assistance
/// Supports general Q&A, disease diagnosis, care advice, and product recommendations
/// Uses the same AI Vision Service as Diagnosis (Groq, Gemini, etc.)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatboxController : ControllerBase
{
    private readonly IChatboxService _chatboxService;
    private readonly ILogger<ChatboxController> _logger;

    public ChatboxController(
        IChatboxService chatboxService,
        ILogger<ChatboxController> logger)
    {
        _chatboxService = chatboxService;
        _logger = logger;
    }

    /// <summary>
    /// Check if chatbox AI service is available
    /// </summary>
    /// <returns>Service availability status with model information</returns>
    /// <response code="200">Service status returned</response>
    [HttpGet("status")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus()
    {
        var status = await _chatboxService.GetServiceStatusAsync();
        return Ok(status);
    }

    /// <summary>
    /// Send a message to the chatbox AI (anonymous)
    /// </summary>
    /// <param name="request">Chatbox request with user message and optional image</param>
    /// <returns>AI response with message, recommendations, and analysis</returns>
    /// <remarks>
    /// **Cách 1: Tin nhắn văn bản (câu hỏi chung chung):**
    ///
    ///     POST /api/chatbox/message
    ///     {
    ///         "message": "Cách chăm sóc cây lưỡi hổ như thế nào?",
    ///         "language": "vi"
    ///     }
    ///
    /// **Cách 2: Tin nhắn với hình ảnh (phân tích hình ảnh):**
    ///
    ///     POST /api/chatbox/message
    ///     {
    ///         "message": "Cây của tôi bị gì thế?",
    ///         "imageBase64": "data:image/jpeg;base64,/9j/4AAQ...",
    ///         "language": "vi"
    ///     }
    ///
    /// **Cách 3: Kết hợp hình ảnh + URL:**
    ///
    ///     POST /api/chatbox/message
    ///     {
    ///         "message": "Cây này bị bệnh gì?",
    ///         "imageUrl": "https://example.com/plant.jpg",
    ///         "language": "vi"
    ///     }
    ///
    /// **Response:**
    ///
    ///     {
    ///         "isSuccessful": true,
    ///         "message": "Cây Lưỡi Hổ là một loại cây dễ chăm sóc...",
    ///         "responseType": "care_advice",
    ///         "recommendedProducts": [...],
    ///         "suggestedFollowUps": ["Câu hỏi tiếp theo?"],
    ///         "respondedAt": "2026-03-04T10:00:00Z"
    ///     }
    ///
    /// **Lưu ý:**
    /// - Hỗ trợ Tiếng Việt (vi) và Tiếng Anh (en)
    /// - Có thể hỏi về chăm sóc cây, bệnh tật, hoặc sản phẩm
    /// - Kèm hình ảnh để phân tích chính xác hơn
    /// - Tự động đề xuất sản phẩm hỗ trợ
    /// </remarks>
    /// <response code="200">Message processed successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="503">AI service temporarily unavailable</response>
    [HttpPost("message")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ChatboxResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SendMessage([FromBody] ChatboxRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Check service availability first
        if (!_chatboxService.IsAvailable())
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                isSuccess = false,
                message = "Dich vu chatbox AI tam thoi khong kha dung"
            });
        }

        _logger.LogInformation("Chatbox message received from anonymous user");

        var result = await _chatboxService.SendMessageAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Send a message to the chatbox AI (authenticated - saves to history)
    /// </summary>
    /// <param name="request">Chatbox request with user message and optional image</param>
    /// <returns>AI response with message, recommendations, and analysis</returns>
    /// <remarks>
    /// Endpoint dành cho người dùng đã đăng nhập.
    /// Tin nhắn sẽ được lưu vào lịch sử trò chuyện của người dùng.
    /// Hỗ trợ tất cả các tính năng như endpoint anonymous.
    /// </remarks>
    /// <response code="200">Message processed successfully and saved to history</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized - user not authenticated</response>
    /// <response code="503">AI service temporarily unavailable</response>
    [HttpPost("message/authenticated")]
    [Authorize]
    [ProducesResponseType(typeof(ChatboxResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SendMessageAuthenticated([FromBody] ChatboxRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!_chatboxService.IsAvailable())
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                isSuccess = false,
                message = "Dich vu chatbox AI tam thoi khong kha dung"
            });
        }

        Guid? userId = null;
        try
        {
            userId = User.GetUserId();
        }
        catch
        {
            return Unauthorized("Unable to identify user");
        }

        _logger.LogInformation("Authenticated chatbox message from user: {UserId}", userId);

        var result = await _chatboxService.SendMessageAsync(request, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Quick plant care tip (no AI processing, just for demo)
    /// </summary>
    /// <param name="plantName">Name of the plant</param>
    /// <param name="language">Language (vi or en)</param>
    /// <returns>Quick care tip</returns>
    /// <response code="200">Care tip returned</response>
    [HttpGet("tip/{plantName}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetQuickTip([FromRoute] string plantName, [FromQuery] string language = "vi")
    {
        _logger.LogInformation("Quick tip requested for plant: {PlantName}", plantName);

        var tip = language == "vi"
            ? GetVietnameseTip(plantName)
            : GetEnglishTip(plantName);

        return Ok(new
        {
            success = true,
            plant = plantName,
            tip = tip,
            language = language
        });
    }

    /// <summary>
    /// Vietnamese quick care tips
    /// </summary>
    private string GetVietnameseTip(string plantName)
    {
        return plantName.ToLower() switch
        {
            "lưởi hổ" or "snake plant" => "Lưỡi Hổ rất dễ chăm sóc. Tuới nuốc khi đất khô hoàn toàn, tối thiểu 2-3 tuần 1 lần. Yêu cầu ánh sáng gián tiếp.",
            "trầu ba" or "monstera" => "Trầu Ba yêu thích ánh sáng gián tiếp sáng và thích ẩm. Tuới nuốc khi lớp trên đất khô, đảm bảo thoáng khí tốt.",
            "lan" or "orchid" => "Lan yêu cầu tuới nuốc định kỳ và độ ẩm cao. Đặt gần cửa sổ có ánh sáng gián tiếp. Tránh tuới trực tiếp vào hoa.",
            _ => "Vui lòng tìm hiểu thêm bằng cách gửi tin nhắn trực tiếp với hình ảnh cây của bạn!"
        };
    }

    /// <summary>
    /// English quick care tips
    /// </summary>
    private string GetEnglishTip(string plantName)
    {
        return plantName.ToLower() switch
        {
            "lưởi hổ" or "snake plant" => "Snake Plant is very easy to care for. Water when soil is completely dry, at least once every 2-3 weeks. Prefers indirect light.",
            "trầu ba" or "monstera" => "Monstera loves bright, indirect light and moisture. Water when the top of soil is dry, ensure good drainage.",
            "lan" or "orchid" => "Orchids require regular watering and high humidity. Place near a window with indirect light. Avoid watering directly on flowers.",
            _ => "Please learn more by sending a direct message with a photo of your plant!"
        };
    }
}
