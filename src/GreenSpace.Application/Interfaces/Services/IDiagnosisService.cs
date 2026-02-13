using GreenSpace.Application.DTOs.Diagnosis;
using GreenSpace.Domain.Interfaces;

namespace GreenSpace.Application.Interfaces.Services;

/// <summary>
/// Service for plant disease diagnosis using AI
/// </summary>
public interface IDiagnosisService
{
    /// <summary>
    /// Check if diagnosis service is available
    /// </summary>
    /// <returns>True if service is ready</returns>
    bool IsAvailable();

    /// <summary>
    /// Diagnose plant disease from image
    /// </summary>
    /// <param name="request">Diagnosis request with image</param>
    /// <param name="userId">Optional user ID for history tracking</param>
    /// <returns>Diagnosis result with treatment recommendations</returns>
    Task<IServiceResult<DiagnosisResponseDto>> DiagnoseAsync(DiagnosisRequestDto request, Guid? userId = null);
}
