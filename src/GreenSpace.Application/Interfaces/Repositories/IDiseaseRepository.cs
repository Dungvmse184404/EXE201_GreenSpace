using GreenSpace.Domain.Models;

namespace GreenSpace.Application.Interfaces.Repositories;

/// <summary>
/// Repository for disease knowledge base operations
/// </summary>
public interface IDiseaseRepository : IGenericRepository<Disease>
{
    /// <summary>
    /// Get all active diseases with their symptoms
    /// </summary>
    Task<List<Disease>> GetAllWithSymptomsAsync();

    /// <summary>
    /// Get diseases by matching symptom IDs
    /// </summary>
    /// <param name="symptomIds">List of symptom IDs to match</param>
    /// <returns>Diseases that have any of the given symptoms</returns>
    Task<List<DiseaseWithMatchInfo>> FindBySymptomIdsAsync(List<Guid> symptomIds);

    /// <summary>
    /// Get diseases for a specific plant type
    /// </summary>
    /// <param name="plantTypeId">Plant type ID</param>
    Task<List<Disease>> GetByPlantTypeIdAsync(Guid plantTypeId);

    /// <summary>
    /// Get diseases for a plant type by name (fuzzy match)
    /// </summary>
    /// <param name="plantTypeName">Plant type name</param>
    Task<List<Disease>> GetByPlantTypeNameAsync(string plantTypeName);

    /// <summary>
    /// Get a disease by ID with all related data
    /// </summary>
    Task<Disease?> GetByIdWithDetailsAsync(Guid id);

    /// <summary>
    /// Find disease by name (fuzzy match for Vietnamese)
    /// </summary>
    /// <param name="name">Disease name to search for</param>
    /// <returns>Disease if found, null otherwise</returns>
    Task<Disease?> FindByNameAsync(string name);
}

/// <summary>
/// Repository for plant type operations
/// </summary>
public interface IPlantTypeRepository : IGenericRepository<PlantType>
{
    /// <summary>
    /// Find plant type by name (fuzzy match)
    /// </summary>
    Task<PlantType?> FindByNameAsync(string name);

    /// <summary>
    /// Get all active plant types
    /// </summary>
    Task<List<PlantType>> GetAllActiveAsync();
}

/// <summary>
/// Disease with match information for scoring
/// </summary>
public class DiseaseWithMatchInfo
{
    public Disease Disease { get; set; } = null!;
    public List<DiseaseSymptom> MatchedSymptoms { get; set; } = new();
    public int TotalSymptomCount { get; set; }
    public decimal TotalWeight { get; set; }
    public decimal MatchedWeight { get; set; }
}
