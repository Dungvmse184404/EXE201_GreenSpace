using GreenSpace.Application.DTOs.Diagnosis;
using GreenSpace.Application.Helpers;
using GreenSpace.Application.Interfaces;
using GreenSpace.Application.Interfaces.Services;
using GreenSpace.Domain.Models;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Application.Services;

/// <summary>
/// Service for matching symptoms against disease knowledge base
/// </summary>
public class DiseaseKnowledgeService : IDiseaseKnowledgeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DiseaseKnowledgeService> _logger;
    private readonly PlantPartSymptomExtractor _symptomExtractor;

    // Matching threshold - disease must match at least 60% of weighted symptoms
    private const double MatchThreshold = 0.6;

    // Bonus for primary symptoms
    private const double PrimarySymptomBonus = 0.2;

    // Bonus when user's plant part matches disease symptom's affected part
    private const double PlantPartMatchBonus = 0.3;

    public DiseaseKnowledgeService(
        IUnitOfWork unitOfWork,
        ILogger<DiseaseKnowledgeService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _symptomExtractor = new PlantPartSymptomExtractor();
    }

    /// <inheritdoc/>
    public async Task<DiseaseMatchResult?> FindMatchingDiseaseAsync(string description, string? plantType)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        try
        {
            // 1. Extract symptoms with plant part context (scope-based)
            var allSymptoms = await _unitOfWork.SymptomDictionaryRepository.GetAllSymptomsAsync();
            var scopedSymptoms = _symptomExtractor.ExtractWithScope(description, allSymptoms);
            var extractedSymptomsList = _symptomExtractor.GetAllSymptoms(scopedSymptoms);

            if (extractedSymptomsList.Count == 0)
            {
                _logger.LogDebug("No symptoms extracted from description");
                return null;
            }

            // Log extracted symptoms with their plant parts
            _logger.LogDebug("Extracted {Count} symptoms with scope: {Symptoms}",
                extractedSymptomsList.Count,
                string.Join(", ", extractedSymptomsList.Select(s => $"{s.SymptomName}[{s.PlantPart}]")));

            var symptomIds = extractedSymptomsList
                .Where(s => s.SymptomId.HasValue)
                .Select(s => s.SymptomId!.Value)
                .ToList();

            // 2. Find diseases with matching symptoms
            var matchedDiseases = await _unitOfWork.DiseaseRepository.FindBySymptomIdsAsync(symptomIds);

            if (matchedDiseases.Count == 0)
            {
                _logger.LogDebug("No diseases found matching the symptoms");
                return null;
            }

            // 3. Filter by plant type if provided
            if (!string.IsNullOrWhiteSpace(plantType))
            {
                var plantTypeDiseases = await _unitOfWork.DiseaseRepository.GetByPlantTypeNameAsync(plantType);
                var plantTypeDiseaseIds = plantTypeDiseases.Select(d => d.Id).ToHashSet();

                // Prioritize diseases linked to the plant type
                matchedDiseases = matchedDiseases
                    .OrderByDescending(m => plantTypeDiseaseIds.Contains(m.Disease.Id) ? 1 : 0)
                    .ThenByDescending(m => CalculateMatchScoreWithPlantPart(m, extractedSymptomsList))
                    .ToList();
            }

            // 4. Calculate match scores and find best match (with plant part context)
            var scoredDiseases = matchedDiseases
                .Select(m => new
                {
                    MatchInfo = m,
                    Score = CalculateMatchScoreWithPlantPart(m, extractedSymptomsList)
                })
                .OrderByDescending(x => x.Score)
                .ToList();

            _logger.LogDebug("Scored {Count} diseases. Top match: {Disease} with score {Score:F2}",
                scoredDiseases.Count,
                scoredDiseases.FirstOrDefault()?.MatchInfo.Disease.DiseaseName,
                scoredDiseases.FirstOrDefault()?.Score);

            // 5. Return best match if above threshold
            var bestMatch = scoredDiseases.FirstOrDefault();
            if (bestMatch == null || bestMatch.Score < MatchThreshold)
            {
                _logger.LogDebug("Best match score {Score:F2} is below threshold {Threshold}",
                    bestMatch?.Score ?? 0, MatchThreshold);
                return null;
            }

            // 6. Build response
            var disease = bestMatch.MatchInfo.Disease;
            var matchedSymptomNames = bestMatch.MatchInfo.MatchedSymptoms
                .Select(ds => ds.Symptom.CanonicalName)
                .ToList();

            var response = BuildDiagnosisResponse(disease, matchedSymptomNames);

            _logger.LogInformation(
                "KB match found! Disease: {Disease}, Score: {Score:F2}, Matched symptoms: {Symptoms}",
                disease.DiseaseName, bestMatch.Score, string.Join(", ", matchedSymptomNames));

            return new DiseaseMatchResult
            {
                DiseaseId = disease.Id,
                MatchScore = bestMatch.Score,
                DiagnosisResponse = response,
                MatchedSymptoms = matchedSymptomNames,
                TotalSymptoms = disease.DiseaseSymptoms.Count,
                ProductKeywords = disease.ProductKeywords
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding matching disease from knowledge base");
            return null;
        }
    }

    /// <summary>
    /// Calculate match score for a disease with plant part context
    /// Gives bonus when user's plant part matches disease symptom's affected part
    /// </summary>
    private double CalculateMatchScoreWithPlantPart(
        GreenSpace.Application.Interfaces.Repositories.DiseaseWithMatchInfo matchInfo,
        List<ExtractedSymptom> userSymptoms)
    {
        if (matchInfo.TotalWeight == 0)
            return 0;

        double baseScore = (double)(matchInfo.MatchedWeight / matchInfo.TotalWeight);
        double plantPartBonus = 0;
        int plantPartMatches = 0;

        // Check each matched symptom for plant part alignment
        foreach (var diseaseSymptom in matchInfo.MatchedSymptoms)
        {
            // Find the user's extracted symptom that matched this disease symptom
            var userSymptom = userSymptoms.FirstOrDefault(us =>
                us.SymptomId == diseaseSymptom.SymptomId);

            if (userSymptom != null && !string.IsNullOrEmpty(diseaseSymptom.AffectedPart))
            {
                // If disease symptom specifies an affected part and user's plant part matches
                if (diseaseSymptom.AffectedPart.Equals(userSymptom.PlantPart, StringComparison.OrdinalIgnoreCase))
                {
                    plantPartMatches++;
                }
            }
        }

        // Add bonus proportional to plant part matches
        if (matchInfo.MatchedSymptoms.Count > 0 && plantPartMatches > 0)
        {
            plantPartBonus = (double)plantPartMatches / matchInfo.MatchedSymptoms.Count * PlantPartMatchBonus;
        }

        // Add bonus for primary symptoms matched
        var primaryMatched = matchInfo.MatchedSymptoms.Count(ds => ds.IsPrimary);
        var totalPrimary = matchInfo.Disease.DiseaseSymptoms.Count(ds => ds.IsPrimary);
        double primaryBonus = 0;

        if (totalPrimary > 0 && primaryMatched > 0)
        {
            primaryBonus = (double)primaryMatched / totalPrimary * PrimarySymptomBonus;
        }

        var finalScore = baseScore + plantPartBonus + primaryBonus;

        _logger.LogDebug(
            "Disease {Disease}: base={Base:F2}, plantPartBonus={PlantBonus:F2} ({PlantMatches} matches), primaryBonus={PrimaryBonus:F2}, final={Final:F2}",
            matchInfo.Disease.DiseaseName, baseScore, plantPartBonus, plantPartMatches, primaryBonus, finalScore);

        return Math.Min(finalScore, 1.0); // Cap at 1.0
    }

    /// <summary>
    /// Build DiagnosisResponseDto from disease entity
    /// </summary>
    private DiagnosisResponseDto BuildDiagnosisResponse(Disease disease, List<string> matchedSymptoms)
    {
        return new DiagnosisResponseDto
        {
            IsSuccessful = true,
            FromCache = false, // Will be set to indicate KB source
            PlantInfo = new PlantInfoDto
            {
                CommonName = "Xác định từ ngân hàng bệnh",
                ScientificName = null,
                Family = null,
                Description = "Thông tin bệnh từ cơ sở dữ liệu chuyên gia"
            },
            DiseaseInfo = new DiseaseInfoDto
            {
                IsHealthy = false,
                DiseaseName = disease.DiseaseName,
                Severity = disease.Severity,
                Symptoms = matchedSymptoms,
                Causes = disease.Causes,
                Notes = disease.Notes
            },
            Treatment = new TreatmentInfoDto
            {
                ImmediateActions = disease.ImmediateActions,
                LongTermCare = disease.LongTermCare,
                PreventionTips = disease.PreventionTips,
                WateringAdvice = disease.WateringAdvice,
                LightingAdvice = disease.LightingAdvice,
                FertilizingAdvice = disease.FertilizingAdvice
            },
            ConfidenceScore = 85, // High confidence from KB
            DiagnosedAt = DateTime.UtcNow,
            RecommendedProducts = new List<RecommendedProductDto>()
        };
    }

    /// <inheritdoc/>
    public async Task<List<PlantTypeDto>> GetAllPlantTypesAsync()
    {
        var plantTypes = await _unitOfWork.PlantTypeRepository.GetAllActiveAsync();

        return plantTypes.Select(pt => new PlantTypeDto
        {
            Id = pt.Id,
            CommonName = pt.CommonName,
            ScientificName = pt.ScientificName,
            Family = pt.Family,
            Description = pt.Description,
            ImageUrl = pt.ImageUrl
        }).ToList();
    }

    /// <inheritdoc/>
    public async Task<List<DiseaseDto>> GetAllDiseasesAsync()
    {
        var diseases = await _unitOfWork.DiseaseRepository.GetAllWithSymptomsAsync();

        return diseases.Select(d => new DiseaseDto
        {
            Id = d.Id,
            DiseaseName = d.DiseaseName,
            EnglishName = d.EnglishName,
            Description = d.Description,
            Severity = d.Severity,
            Causes = d.Causes,
            Symptoms = d.DiseaseSymptoms.Select(ds => ds.Symptom.CanonicalName).ToList(),
            SymptomCount = d.DiseaseSymptoms.Count
        }).ToList();
    }
}
