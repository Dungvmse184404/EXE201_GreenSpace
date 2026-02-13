using GreenSpace.Domain.Models;
using GreenSpace.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeder for Vietnamese symptom dictionary
/// Used for semantic matching in plant disease diagnosis
/// </summary>
public class SymptomDictionarySeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<SymptomDictionarySeeder> _logger;

    public SymptomDictionarySeeder(
        AppDbContext context,
        ILogger<SymptomDictionarySeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Check if already seeded
            var hasData = await _context.SymptomDictionaries.AnyAsync();
            if (hasData)
            {
                _logger.LogInformation("Symptom dictionary already seeded. Skipping.");
                return;
            }

            var symptoms = GetDefaultSymptoms();
            await _context.SymptomDictionaries.AddRangeAsync(symptoms);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded {Count} symptoms to dictionary", symptoms.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding symptom dictionary");
        }
    }

    private List<SymptomDictionary> GetDefaultSymptoms()
    {
        return new List<SymptomDictionary>
        {
            // LEAF SYMPTOMS
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "đốm nâu",
                Synonyms = new List<string> { "dom nau", "vết nâu", "chấm nâu", "đốm màu nâu", "la dom nau" },
                Category = "leaf",
                EnglishName = "brown spots",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "đốm đen",
                Synonyms = new List<string> { "dom den", "vết đen", "chấm đen", "đốm màu đen" },
                Category = "leaf",
                EnglishName = "black spots",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "đốm vàng",
                Synonyms = new List<string> { "dom vang", "vết vàng", "chấm vàng", "đốm màu vàng" },
                Category = "leaf",
                EnglishName = "yellow spots",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "lá vàng",
                Synonyms = new List<string> {
                    "la vang", "lá ngả vàng", "vàng úa", "lá úa", "la ua", "vang la",
                    // Single words for context-aware matching
                    "vàng", "bị vàng", "ngả vàng", "úa"
                },
                Category = "leaf",
                EnglishName = "yellowing leaves",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "lá xoăn",
                Synonyms = new List<string> {
                    "la xoan", "lá cuốn", "lá quăn", "xoăn lá", "cuon la",
                    // Single words for context-aware matching
                    "xoăn", "bị xoăn", "cuốn", "quăn"
                },
                Category = "leaf",
                EnglishName = "curling leaves",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "lá khô",
                Synonyms = new List<string> {
                    "la kho", "lá héo", "lá cháy", "khô cháy", "heo la", "chay la",
                    // Single words for context-aware matching
                    "khô", "bị khô", "héo", "bị héo"
                },
                Category = "leaf",
                EnglishName = "dry leaves",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "lá rụng",
                Synonyms = new List<string> { "la rung", "rụng lá", "lá rơi", "roi la" },
                Category = "leaf",
                EnglishName = "leaf drop",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "lá đen",
                Synonyms = new List<string> { "la den", "đen lá", "lá thối đen" },
                Category = "leaf",
                EnglishName = "black leaves",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "lá nhăn",
                Synonyms = new List<string> { "la nhan", "nhăn lá", "lá nhàu" },
                Category = "leaf",
                EnglishName = "wrinkled leaves",
                CreatedAt = DateTime.UtcNow
            },

            // STEM SYMPTOMS
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "chồi xoăn",
                Synonyms = new List<string> { "choi xoan", "đọt xoăn", "ngọn xoăn", "chồi cuốn", "dot xoan" },
                Category = "stem",
                EnglishName = "curled shoots",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "thối thân",
                Synonyms = new List<string> {
                    "thoi than", "thân thối", "thân nhũn", "muc than",
                    // Single words for context-aware matching (when anchor is "thân")
                    "thối", "bị thối", "nhũn", "bị nhũn"
                },
                Category = "stem",
                EnglishName = "stem rot",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "thân khô",
                Synonyms = new List<string> { "than kho", "khô thân", "thân héo" },
                Category = "stem",
                EnglishName = "dry stem",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "cành gãy",
                Synonyms = new List<string> { "canh gay", "gãy cành", "cành yếu" },
                Category = "stem",
                EnglishName = "broken branches",
                CreatedAt = DateTime.UtcNow
            },

            // ROOT SYMPTOMS
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "thối rễ",
                Synonyms = new List<string> {
                    "thoi re", "rễ thối", "rễ mục", "rễ nhũn", "re thoi", "muc re",
                    // Single words for context-aware matching (when anchor is "rễ")
                    "mục", "bị mục", "thối", "bị thối", "nhũn"
                },
                Category = "root",
                EnglishName = "root rot",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "rễ đen",
                Synonyms = new List<string> { "re den", "đen rễ" },
                Category = "root",
                EnglishName = "black roots",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "rễ khô",
                Synonyms = new List<string> { "re kho", "khô rễ", "rễ héo" },
                Category = "root",
                EnglishName = "dry roots",
                CreatedAt = DateTime.UtcNow
            },

            // FLOWER SYMPTOMS
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "hoa đen",
                Synonyms = new List<string> { "hoa den", "hoa thối", "hoa khô đen", "den hoa" },
                Category = "flower",
                EnglishName = "black flowers",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "hoa rụng",
                Synonyms = new List<string> { "hoa rung", "rụng hoa", "hoa rơi" },
                Category = "flower",
                EnglishName = "flower drop",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "hoa héo",
                Synonyms = new List<string> { "hoa heo", "héo hoa", "hoa khô" },
                Category = "flower",
                EnglishName = "wilted flowers",
                CreatedAt = DateTime.UtcNow
            },

            // GENERAL/DISEASE SYMPTOMS
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "nấm mốc",
                Synonyms = new List<string> { "nam moc", "mốc trắng", "phấn trắng", "moc trang", "nam trang" },
                Category = "general",
                EnglishName = "mold/mildew",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "sâu bệnh",
                Synonyms = new List<string> { "sau benh", "sâu đục", "sâu ăn", "sau an", "con sau" },
                Category = "pest",
                EnglishName = "pest damage",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "bọ trĩ",
                Synonyms = new List<string> { "bo tri", "bọ xít", "rệp", "rep", "bo xit" },
                Category = "pest",
                EnglishName = "thrips",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "nhện đỏ",
                Synonyms = new List<string> { "nhen do", "nhện", "con nhện" },
                Category = "pest",
                EnglishName = "spider mites",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "rỉ sắt",
                Synonyms = new List<string> { "ri sat", "bệnh rỉ", "dom ri" },
                Category = "general",
                EnglishName = "rust disease",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "cháy lá",
                Synonyms = new List<string> { "chay la", "lá cháy nắng", "la chay" },
                Category = "leaf",
                EnglishName = "leaf burn",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "thiếu nước",
                Synonyms = new List<string> { "thieu nuoc", "khô hạn", "heo ua", "héo úa" },
                Category = "general",
                EnglishName = "water deficiency",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "thừa nước",
                Synonyms = new List<string> { "thua nuoc", "úng nước", "ngập nước", "ung nuoc" },
                Category = "general",
                EnglishName = "overwatering",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CanonicalName = "thiếu dinh dưỡng",
                Synonyms = new List<string> { "thieu dinh duong", "thiếu phân", "cây còi", "coi coc" },
                Category = "general",
                EnglishName = "nutrient deficiency",
                CreatedAt = DateTime.UtcNow
            }
        };
    }
}
