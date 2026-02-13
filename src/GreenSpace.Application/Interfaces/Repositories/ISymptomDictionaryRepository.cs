using GreenSpace.Domain.Models;

namespace GreenSpace.Application.Interfaces.Repositories;

/// <summary>
/// Repository for symptom dictionary operations
/// </summary>
public interface ISymptomDictionaryRepository : IGenericRepository<SymptomDictionary>
{
    /// <summary>
    /// Get all symptoms for caching in memory
    /// </summary>
    Task<List<SymptomDictionary>> GetAllSymptomsAsync();

    /// <summary>
    /// Find symptoms by category
    /// </summary>
    Task<List<SymptomDictionary>> GetByCategoryAsync(string category);
}
