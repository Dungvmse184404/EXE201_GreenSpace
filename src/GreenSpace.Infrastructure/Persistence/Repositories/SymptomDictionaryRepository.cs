using GreenSpace.Application.Interfaces.Repositories;
using GreenSpace.Domain.Models;
using GreenSpace.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for symptom dictionary operations
/// </summary>
public class SymptomDictionaryRepository : GenericRepository<SymptomDictionary>, ISymptomDictionaryRepository
{
    private readonly AppDbContext _context;

    public SymptomDictionaryRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<List<SymptomDictionary>> GetAllSymptomsAsync()
    {
        return await _context.SymptomDictionaries
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<List<SymptomDictionary>> GetByCategoryAsync(string category)
    {
        return await _context.SymptomDictionaries
            .Where(s => s.Category == category)
            .AsNoTracking()
            .ToListAsync();
    }
}
