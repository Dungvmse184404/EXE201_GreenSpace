using GreenSpace.Application.Common.Constants;
using GreenSpace.Application.DTOs.Diagnosis;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.WebAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.WebAPI.Controllers;

/// <summary>
/// AI-powered plant disease diagnosis endpoints
/// Uses Google Gemini Vision API for image analysis
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DiagnosisController : ControllerBase
{
    private readonly IDiagnosisService _diagnosisService;
    private readonly ILogger<DiagnosisController> _logger;

    public DiagnosisController(
        IDiagnosisService diagnosisService,
        ILogger<DiagnosisController> logger)
    {
        _diagnosisService = diagnosisService;
        _logger = logger;
    }

    /// <summary>
    /// Check if diagnosis service is available
    /// </summary>
    /// <returns>Service availability status</returns>
    /// <response code="200">Service status returned</response>
    [HttpGet("status")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetStatus()
    {
        var isAvailable = _diagnosisService.IsAvailable();
        return Ok(new
        {
            isAvailable,
            message = isAvailable
                ? "Dich vu chan doan AI dang hoat dong"
                : "Dich vu chan doan AI tam thoi khong kha dung"
        });
    }

    /// <summary>
    /// Diagnose plant disease from image or text description
    /// </summary>
    /// <param name="request">Diagnosis request with image and/or description</param>
    /// <returns>Diagnosis result with treatment recommendations</returns>
    /// <remarks>
    /// **Cach 1: Su dung hinh anh (chinh xac nhat):**
    ///
    ///     POST /api/diagnosis
    ///     {
    ///         "imageBase64": "data:image/jpeg;base64,/9j/4AAQ...",  // Anh Base64
    ///         "description": "La cay bi vang va co dom nau",         // Mo ta them (optional)
    ///         "language": "vi"
    ///     }
    ///
    /// **Cach 2: Chi dung mo ta text (khong can hinh anh):**
    ///
    ///     POST /api/diagnosis
    ///     {
    ///         "description": "Cay trau ba cua toi bi vang la, da trong trong nha 6 thang, tuoi nuoc 2 lan/tuan, dat trong co ve qua am",
    ///         "language": "vi"
    ///     }
    ///
    /// **Cach 3: Ket hop hinh anh + mo ta (chinh xac nhat):**
    ///
    ///     POST /api/diagnosis
    ///     {
    ///         "imageUrl": "https://example.com/plant.jpg",
    ///         "description": "Cay trong cham soc tot nhung van bi benh, da thay doi dat 1 thang truoc",
    ///         "language": "vi"
    ///     }
    ///
    /// **Response:**
    ///
    ///     {
    ///         "isSuccessful": true,
    ///         "plantInfo": {
    ///             "commonName": "Cay trau ba",
    ///             "scientificName": "Epipremnum aureum"
    ///         },
    ///         "diseaseInfo": {
    ///             "isHealthy": false,
    ///             "diseaseName": "Benh dom la",
    ///             "severity": "Medium",
    ///             "symptoms": ["La vang", "Dom nau tren la"]
    ///         },
    ///         "treatment": {
    ///             "immediateActions": ["Cat bo la bi benh"],
    ///             "longTermCare": ["Giam luong nuoc tuoi"]
    ///         },
    ///         "recommendedProducts": [
    ///             { "productId": "...", "name": "Thuoc tri nam" }
    ///         ],
    ///         "confidenceScore": 85
    ///     }
    ///
    /// **Luu y:**
    /// - Co the chan doan bang hinh anh, mo ta text, hoac ca hai
    /// - Ket hop hinh anh + mo ta cho ket qua chinh xac nhat
    /// - Mo ta chi tiet giup AI hieu ro hon: ten cay, trieu chung, moi truong, lich su cham soc
    /// - Ho tro dinh dang hinh: JPG, PNG, WEBP (toi da 4MB)
    /// </remarks>
    /// <response code="200">Diagnosis successful</response>
    /// <response code="400">Invalid request or image cannot be analyzed</response>
    /// <response code="503">AI service temporarily unavailable</response>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(DiagnosisResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Diagnose([FromBody] DiagnosisRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Check service availability first
        if (!_diagnosisService.IsAvailable())
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                isSuccess = false,
                message = ApiMessages.Diagnostic.ServiceUnavailable
            });
        }

        // Get user ID if authenticated
        Guid? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            try
            {
                userId = User.GetUserId();
            }
            catch
            {
                // Anonymous user, continue without userId
            }
        }

        _logger.LogInformation("Plant diagnosis request received. UserId: {UserId}", userId);

        var result = await _diagnosisService.DiagnoseAsync(request, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Diagnose plant with authentication (saves to history)
    /// </summary>
    /// <param name="request">Diagnosis request with image</param>
    /// <returns>Diagnosis result with treatment recommendations</returns>
    /// <response code="200">Diagnosis successful, saved to history</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="503">AI service unavailable</response>
    [HttpPost("authenticated")]
    [Authorize]
    [ProducesResponseType(typeof(DiagnosisResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> DiagnoseAuthenticated([FromBody] DiagnosisRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!_diagnosisService.IsAvailable())
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                isSuccess = false,
                message = "Dich vu chan doan AI tam thoi khong kha dung."
            });
        }

        var userId = User.GetUserId();
        _logger.LogInformation("Authenticated diagnosis request from user: {UserId}", userId);

        var result = await _diagnosisService.DiagnoseAsync(request, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
