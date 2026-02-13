using GreenSpace.Application.Interfaces.Repositories;
using GreenSpace.Domain.Models;
using GreenSpace.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for plant type operations
/// </summary>
public class PlantTypeRepository : GenericRepository<PlantType>, IPlantTypeRepository
{
    private readonly AppDbContext _context;

    public PlantTypeRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<PlantType?> FindByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var normalizedName = name.ToLowerInvariant().Trim();

        return await _context.PlantTypes
            .Where(pt => pt.IsActive)
            .FirstOrDefaultAsync(pt =>
                pt.CommonName.ToLower().Contains(normalizedName) ||
                (pt.ScientificName != null && pt.ScientificName.ToLower().Contains(normalizedName)));
    }

    /// <inheritdoc/>
    public async Task<List<PlantType>> GetAllActiveAsync()
    {
        return await _context.PlantTypes
            .Where(pt => pt.IsActive)
            .OrderBy(pt => pt.CommonName)
            .AsNoTracking()
            .ToListAsync();
    }
}
