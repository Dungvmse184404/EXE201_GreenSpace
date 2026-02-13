using GreenSpace.Application.Interfaces.Repositories;
using GreenSpace.Domain.Models;
using GreenSpace.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for disease knowledge base operations
/// </summary>
public class DiseaseRepository : GenericRepository<Disease>, IDiseaseRepository
{
    private readonly AppDbContext _context;

    public DiseaseRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<List<Disease>> GetAllWithSymptomsAsync()
    {
        return await _context.Diseases
            .Include(d => d.DiseaseSymptoms)
                .ThenInclude(ds => ds.Symptom)
            .Where(d => d.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<DiseaseWithMatchInfo>> FindBySymptomIdsAsync(List<Guid> symptomIds)
    {
        if (symptomIds.Count == 0)
            return new List<DiseaseWithMatchInfo>();

        // Get diseases that have at least one matching symptom
        var diseases = await _context.Diseases
            .Include(d => d.DiseaseSymptoms)
                .ThenInclude(ds => ds.Symptom)
            .Where(d => d.IsActive &&
                d.DiseaseSymptoms.Any(ds => symptomIds.Contains(ds.SymptomId)))
            .AsNoTracking()
            .ToListAsync();

        // Calculate match info for each disease
        var result = diseases.Select(d =>
        {
            var matchedSymptoms = d.DiseaseSymptoms
                .Where(ds => symptomIds.Contains(ds.SymptomId))
                .ToList();

            return new DiseaseWithMatchInfo
            {
                Disease = d,
                MatchedSymptoms = matchedSymptoms,
                TotalSymptomCount = d.DiseaseSymptoms.Count,
                TotalWeight = d.DiseaseSymptoms.Sum(ds => ds.Weight),
                MatchedWeight = matchedSymptoms.Sum(ds => ds.Weight)
            };
        }).ToList();

        return result;
    }

    /// <inheritdoc/>
    public async Task<List<Disease>> GetByPlantTypeIdAsync(Guid plantTypeId)
    {
        return await _context.PlantTypeDiseases
            .Where(ptd => ptd.PlantTypeId == plantTypeId)
            .Select(ptd => ptd.Disease)
            .Where(d => d.IsActive)
            .Include(d => d.DiseaseSymptoms)
                .ThenInclude(ds => ds.Symptom)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<Disease>> GetByPlantTypeNameAsync(string plantTypeName)
    {
        if (string.IsNullOrWhiteSpace(plantTypeName))
            return new List<Disease>();

        var normalizedName = plantTypeName.ToLowerInvariant().Trim();

        // Find matching plant type
        var plantType = await _context.PlantTypes
            .FirstOrDefaultAsync(pt => pt.IsActive &&
                (pt.CommonName.ToLower().Contains(normalizedName) ||
                 (pt.ScientificName != null && pt.ScientificName.ToLower().Contains(normalizedName))));

        if (plantType == null)
            return new List<Disease>();

        return await GetByPlantTypeIdAsync(plantType.Id);
    }

    /// <inheritdoc/>
    public async Task<Disease?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Diseases
            .Include(d => d.DiseaseSymptoms)
                .ThenInclude(ds => ds.Symptom)
            .Include(d => d.PlantTypeDiseases)
                .ThenInclude(ptd => ptd.PlantType)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    /// <inheritdoc/>
    public async Task<Disease?> FindByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var normalizedName = name.ToLowerInvariant().Trim();

        return await _context.Diseases
            .Where(d => d.IsActive)
            .Where(d => d.DiseaseName.ToLower().Contains(normalizedName)
                     || (d.EnglishName != null && d.EnglishName.ToLower().Contains(normalizedName)))
            .FirstOrDefaultAsync();
    }
}
