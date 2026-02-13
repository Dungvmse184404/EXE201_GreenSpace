using GreenSpace.Domain.Models;
using GreenSpace.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSpace.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeder for Disease Knowledge Base
/// Seeds plant types, diseases, and their symptoms
/// </summary>
public class DiseaseKnowledgeSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<DiseaseKnowledgeSeeder> _logger;

    public DiseaseKnowledgeSeeder(
        AppDbContext context,
        ILogger<DiseaseKnowledgeSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Check if already seeded
            var hasPlantTypes = await _context.PlantTypes.AnyAsync();
            var hasDiseases = await _context.Diseases.AnyAsync();

            if (hasPlantTypes && hasDiseases)
            {
                _logger.LogInformation("Disease Knowledge Base already seeded. Skipping.");
                return;
            }

            // Get symptom dictionary for linking
            var symptoms = await _context.SymptomDictionaries.ToListAsync();
            if (symptoms.Count == 0)
            {
                _logger.LogWarning("Symptom dictionary is empty. Please seed symptoms first.");
                return;
            }

            var symptomLookup = symptoms.ToDictionary(s => s.CanonicalName, s => s.Id);

            // Seed plant types
            var plantTypes = await SeedPlantTypesAsync();

            // Seed diseases with symptoms
            await SeedDiseasesAsync(symptomLookup, plantTypes);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Disease Knowledge Base seeded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding Disease Knowledge Base");
        }
    }

    private async Task<Dictionary<string, Guid>> SeedPlantTypesAsync()
    {
        var plantTypes = new List<PlantType>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CommonName = "Cây dừa",
                ScientificName = "Cocos nucifera",
                Family = "Arecaceae",
                Description = "Cây dừa là loại cây nhiệt đới thuộc họ Cau, thường được trồng ở vùng khí hậu ấm áp.",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CommonName = "Cây lúa",
                ScientificName = "Oryza sativa",
                Family = "Poaceae",
                Description = "Cây lương thực quan trọng nhất ở Việt Nam và châu Á.",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CommonName = "Cây cà phê",
                ScientificName = "Coffea",
                Family = "Rubiaceae",
                Description = "Cây công nghiệp quan trọng, trồng nhiều ở Tây Nguyên.",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CommonName = "Cây trầu bà",
                ScientificName = "Epipremnum aureum",
                Family = "Araceae",
                Description = "Cây cảnh phổ biến trong nhà, dễ chăm sóc.",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                CommonName = "Cây hoa hồng",
                ScientificName = "Rosa",
                Family = "Rosaceae",
                Description = "Cây hoa cảnh phổ biến, nhiều giống và màu sắc.",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.PlantTypes.AddRangeAsync(plantTypes);
        return plantTypes.ToDictionary(p => p.CommonName, p => p.Id);
    }

    private async Task SeedDiseasesAsync(Dictionary<string, Guid> symptomLookup, Dictionary<string, Guid> plantTypeLookup)
    {
        var diseases = new List<Disease>();
        var diseaseSymptoms = new List<DiseaseSymptom>();
        var plantTypeDiseases = new List<PlantTypeDisease>();

        // =====================================================
        // DISEASE 1: Bệnh đốm nâu lá
        // =====================================================
        var dotNauLa = new Disease
        {
            Id = Guid.NewGuid(),
            DiseaseName = "Bệnh đốm nâu lá",
            EnglishName = "Brown Leaf Spot",
            Description = "Bệnh do nấm gây ra, xuất hiện các đốm nâu trên lá, làm lá khô và rụng.",
            Severity = "High",
            Causes = new List<string> { "Nấm Pestalotiopsis", "Độ ẩm cao", "Thông gió kém" },
            ImmediateActions = new List<string>
            {
                "Cắt bỏ lá bị bệnh",
                "Tiêu hủy lá bị nhiễm",
                "Phun thuốc trừ nấm"
            },
            LongTermCare = new List<string>
            {
                "Tưới nước hợp lý, tránh ướt lá",
                "Bón phân cân đối",
                "Tăng cường thông gió"
            },
            PreventionTips = new List<string>
            {
                "Chọn giống kháng bệnh",
                "Trồng cây đúng khoảng cách",
                "Vệ sinh vườn thường xuyên"
            },
            WateringAdvice = "Tưới nước vào gốc, tránh tưới lên lá. Tưới vào buổi sáng.",
            LightingAdvice = "Đảm bảo cây nhận đủ ánh sáng mặt trời.",
            FertilizingAdvice = "Bón phân hữu cơ và kali để tăng sức đề kháng.",
            ProductKeywords = new List<string> { "thuốc trừ nấm", "phân bón lá", "phân kali" },
            Notes = "Bệnh phổ biến vào mùa mưa, cần kiểm tra thường xuyên.",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        diseases.Add(dotNauLa);

        // Add symptoms for đốm nâu lá (all affect leaves)
        if (symptomLookup.TryGetValue("đốm nâu", out var dotNauId))
            diseaseSymptoms.Add(new DiseaseSymptom { Id = Guid.NewGuid(), DiseaseId = dotNauLa.Id, SymptomId = dotNauId, IsPrimary = true, Weight = 2.0m, AffectedPart = "leaf" });
        if (symptomLookup.TryGetValue("lá vàng", out var laVangId))
            diseaseSymptoms.Add(new DiseaseSymptom { Id = Guid.NewGuid(), DiseaseId = dotNauLa.Id, SymptomId = laVangId, IsPrimary = false, Weight = 1.0m, AffectedPart = "leaf" });
        if (symptomLookup.TryGetValue("lá khô", out var laKhoId))
            diseaseSymptoms.Add(new DiseaseSymptom { Id = Guid.NewGuid(), DiseaseId = dotNauLa.Id, SymptomId = laKhoId, IsPrimary = false, Weight = 1.5m, AffectedPart = "leaf" });
        if (symptomLookup.TryGetValue("lá rụng", out var laRungId))
            diseaseSymptoms.Add(new DiseaseSymptom { Id = Guid.NewGuid(), DiseaseId = dotNauLa.Id, SymptomId = laRungId, IsPrimary = false, Weight = 1.0m, AffectedPart = "leaf" });

        // Link to plant types
        if (plantTypeLookup.TryGetValue("Cây dừa", out var cayDuaId))
            plantTypeDiseases.Add(new PlantTypeDisease { Id = Guid.NewGuid(), PlantTypeId = cayDuaId, DiseaseId = dotNauLa.Id, Prevalence = "Common" });
        if (plantTypeLookup.TryGetValue("Cây cà phê", out var cayCaPheId))
            plantTypeDiseases.Add(new PlantTypeDisease { Id = Guid.NewGuid(), PlantTypeId = cayCaPheId, DiseaseId = dotNauLa.Id, Prevalence = "Common" });

        // =====================================================
        // DISEASE 2: Bệnh xoăn lá virus
        // =====================================================
        var xoanLaVirus = new Disease
        {
            Id = Guid.NewGuid(),
            DiseaseName = "Bệnh xoăn lá virus",
            EnglishName = "Leaf Curl Virus",
            Description = "Bệnh do virus gây ra, làm lá xoăn, biến dạng, cây còi cọc.",
            Severity = "Critical",
            Causes = new List<string> { "Virus TYLCV", "Côn trùng truyền bệnh (bọ phấn, rầy)" },
            ImmediateActions = new List<string>
            {
                "Nhổ bỏ cây bị bệnh nặng",
                "Phun thuốc diệt côn trùng",
                "Cách ly cây bệnh"
            },
            LongTermCare = new List<string>
            {
                "Kiểm soát côn trùng thường xuyên",
                "Trồng cây giống sạch bệnh",
                "Tăng cường dinh dưỡng cho cây"
            },
            PreventionTips = new List<string>
            {
                "Sử dụng giống kháng virus",
                "Che phủ nilon bạc để xua côn trùng",
                "Luân canh cây trồng"
            },
            WateringAdvice = "Tưới nước đều đặn, tránh stress cho cây.",
            LightingAdvice = "Đảm bảo ánh sáng đầy đủ.",
            FertilizingAdvice = "Bón phân NPK cân đối.",
            ProductKeywords = new List<string> { "thuốc trừ rầy", "thuốc trừ bọ phấn", "phân bón" },
            Notes = "Bệnh không có thuốc chữa, cần phòng ngừa là chính.",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        diseases.Add(xoanLaVirus);

        // Add symptoms for xoăn lá virus (affects leaves and stems)
        if (symptomLookup.TryGetValue("lá xoăn", out var laXoanId))
            diseaseSymptoms.Add(new DiseaseSymptom { Id = Guid.NewGuid(), DiseaseId = xoanLaVirus.Id, SymptomId = laXoanId, IsPrimary = true, Weight = 2.5m, AffectedPart = "leaf" });
        if (symptomLookup.TryGetValue("chồi xoăn", out var choiXoanId))
            diseaseSymptoms.Add(new DiseaseSymptom { Id = Guid.NewGuid(), DiseaseId = xoanLaVirus.Id, SymptomId = choiXoanId, IsPrimary = true, Weight = 2.0m, AffectedPart = "stem" });
        if (symptomLookup.TryGetValue("lá vàng", out var laVangId2))
            diseaseSymptoms.Add(new DiseaseSymptom { Id = Guid.NewGuid(), DiseaseId = xoanLaVirus.Id, SymptomId = laVangId2, IsPrimary = false, Weight = 1.0m, AffectedPart = "leaf" });

        // Link to plant types
        if (plantTypeLookup.TryGetValue("Cây dừa", out var cayDuaId2))
            plantTypeDiseases.Add(new PlantTypeDisease { Id = Guid.NewGuid(), PlantTypeId = cayDuaId2, DiseaseId = xoanLaVirus.Id, Prevalence = "Rare" });

        // =====================================================
        // DISEASE 3: Bệnh thối rễ
        // =====================================================
        var thoiRe = new Disease
        {
            Id = Guid.NewGuid(),
            DiseaseName = "Bệnh thối rễ",
            EnglishName = "Root Rot",
            Description = "Bệnh do nấm hoặc vi khuẩn gây thối rễ, cây héo úa và chết.",
            Severity = "High",
            Causes = new List<string> { "Nấm Phytophthora", "Nấm Pythium", "Tưới nước quá nhiều", "Đất thoát nước kém" },
            ImmediateActions = new List<string>
            {
                "Ngừng tưới nước ngay",
                "Đào bỏ phần rễ thối",
                "Xử lý gốc bằng thuốc trừ nấm"
            },
            LongTermCare = new List<string>
            {
                "Cải tạo đất thoát nước tốt",
                "Tưới nước hợp lý",
                "Bón phân hữu cơ cải tạo đất"
            },
            PreventionTips = new List<string>
            {
                "Không tưới quá nhiều nước",
                "Trồng cây ở nơi thoát nước tốt",
                "Sử dụng đất trồng sạch"
            },
            WateringAdvice = "Chỉ tưới khi đất khô, kiểm tra độ ẩm đất trước khi tưới.",
            LightingAdvice = "Đủ ánh sáng giúp đất khô nhanh hơn.",
            FertilizingAdvice = "Bón phân hữu cơ, tránh phân đạm quá nhiều.",
            ProductKeywords = new List<string> { "thuốc trừ nấm", "đất trồng", "phân hữu cơ" },
            Notes = "Phát hiện sớm là chìa khóa để cứu cây.",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        diseases.Add(thoiRe);

        // Add symptoms for thối rễ (affects roots and shows on leaves)
        if (symptomLookup.TryGetValue("thối rễ", out var thoiReId))
            diseaseSymptoms.Add(new DiseaseSymptom { Id = Guid.NewGuid(), DiseaseId = thoiRe.Id, SymptomId = thoiReId, IsPrimary = true, Weight = 2.5m, AffectedPart = "root" });
        if (symptomLookup.TryGetValue("lá vàng", out var laVangId3))
            diseaseSymptoms.Add(new DiseaseSymptom { Id = Guid.NewGuid(), DiseaseId = thoiRe.Id, SymptomId = laVangId3, IsPrimary = false, Weight = 1.5m, AffectedPart = "leaf" });
        if (symptomLookup.TryGetValue("thừa nước", out var thuaNuocId))
            diseaseSymptoms.Add(new DiseaseSymptom { Id = Guid.NewGuid(), DiseaseId = thoiRe.Id, SymptomId = thuaNuocId, IsPrimary = false, Weight = 1.5m, AffectedPart = null }); // general condition

        // Link to all plant types
        foreach (var pt in plantTypeLookup)
        {
            plantTypeDiseases.Add(new PlantTypeDisease { Id = Guid.NewGuid(), PlantTypeId = pt.Value, DiseaseId = thoiRe.Id, Prevalence = "Common" });
        }

        // =====================================================
        // DISEASE 4: Bệnh nấm mốc trắng
        // =====================================================
        var namMocTrang = new Disease
        {
            Id = Guid.NewGuid(),
            DiseaseName = "Bệnh nấm mốc trắng",
            EnglishName = "Powdery Mildew",
            Description = "Bệnh do nấm gây ra, tạo lớp phấn trắng trên lá, ảnh hưởng quang hợp.",
            Severity = "Medium",
            Causes = new List<string> { "Nấm Erysiphe", "Độ ẩm cao", "Thiếu ánh sáng", "Thông gió kém" },
            ImmediateActions = new List<string>
            {
                "Cắt bỏ lá bị nấm",
                "Phun thuốc trừ nấm",
                "Tăng thông gió cho cây"
            },
            LongTermCare = new List<string>
            {
                "Tránh tưới nước lên lá",
                "Bố trí cây đủ khoảng cách",
                "Kiểm tra thường xuyên"
            },
            PreventionTips = new List<string>
            {
                "Trồng nơi thông thoáng",
                "Tránh độ ẩm quá cao",
                "Phun thuốc phòng định kỳ"
            },
            WateringAdvice = "Tưới vào gốc, buổi sáng sớm.",
            LightingAdvice = "Đảm bảo cây có đủ ánh sáng.",
            FertilizingAdvice = "Bón phân kali tăng sức đề kháng.",
            ProductKeywords = new List<string> { "thuốc trừ nấm", "phân kali" },
            Notes = "Bệnh thường xuất hiện vào mùa mưa hoặc thời tiết ẩm ướt.",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        diseases.Add(namMocTrang);

        // Add symptoms (affects leaves primarily)
        if (symptomLookup.TryGetValue("nấm mốc", out var namMocId))
            diseaseSymptoms.Add(new DiseaseSymptom { Id = Guid.NewGuid(), DiseaseId = namMocTrang.Id, SymptomId = namMocId, IsPrimary = true, Weight = 2.5m, AffectedPart = "leaf" });
        if (symptomLookup.TryGetValue("lá vàng", out var laVangId4))
            diseaseSymptoms.Add(new DiseaseSymptom { Id = Guid.NewGuid(), DiseaseId = namMocTrang.Id, SymptomId = laVangId4, IsPrimary = false, Weight = 1.0m, AffectedPart = "leaf" });

        // Link to plant types
        if (plantTypeLookup.TryGetValue("Cây hoa hồng", out var cayHoaHongId))
            plantTypeDiseases.Add(new PlantTypeDisease { Id = Guid.NewGuid(), PlantTypeId = cayHoaHongId, DiseaseId = namMocTrang.Id, Prevalence = "Common" });
        if (plantTypeLookup.TryGetValue("Cây trầu bà", out var cayTrauBaId))
            plantTypeDiseases.Add(new PlantTypeDisease { Id = Guid.NewGuid(), PlantTypeId = cayTrauBaId, DiseaseId = namMocTrang.Id, Prevalence = "Rare" });

        // Save all
        await _context.Diseases.AddRangeAsync(diseases);
        await _context.DiseaseSymptoms.AddRangeAsync(diseaseSymptoms);
        await _context.PlantTypeDiseases.AddRangeAsync(plantTypeDiseases);

        _logger.LogInformation("Seeded {DiseaseCount} diseases, {SymptomCount} disease-symptom links, {PlantTypeCount} plant-type-disease links",
            diseases.Count, diseaseSymptoms.Count, plantTypeDiseases.Count);
    }
}
