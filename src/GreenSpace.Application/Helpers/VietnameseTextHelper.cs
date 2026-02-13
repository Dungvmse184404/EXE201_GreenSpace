using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GreenSpace.Application.Helpers;

/// <summary>
/// Helper class for Vietnamese text processing and normalization
/// </summary>
public static class VietnameseTextHelper
{
    #region Plant Part Keywords

    /// <summary>
    /// Keywords for plant parts in Vietnamese
    /// Used for scope-based symptom extraction
    /// </summary>
    public static readonly Dictionary<string, List<string>> PlantPartKeywords = new()
    {
        ["leaf"] = new() { "lá", "la", "lá cây", "tán lá", "phiến lá" },
        ["stem"] = new() { "thân", "than", "cành", "canh", "nhánh", "chồi", "đọt", "ngọn", "cuống" },
        ["root"] = new() { "rễ", "re", "gốc", "goc", "củ" },
        ["fruit"] = new() { "quả", "qua", "trái", "trai", "hạt", "hat", "bông" },
        ["flower"] = new() { "hoa", "bông hoa", "nụ", "nu", "cánh hoa" }
    };

    /// <summary>
    /// Boundary keywords that separate clauses (scope boundaries)
    /// </summary>
    public static readonly List<string> BoundaryKeywords = new()
    {
        // Transition conjunctions (reset scope)
        "còn", "trong khi đó", "ở phần", "phần", "riêng",
        "ngoài ra", "bên cạnh đó", "đồng thời", "mặt khác",
        "về phần", "đối với"
    };

    /// <summary>
    /// Modifier keywords that link symptoms to plant parts
    /// Example: "đốm nâu TRÊN lá" → đốm nâu belongs to lá
    /// </summary>
    public static readonly List<string> ModifierKeywords = new()
    {
        "trên", "ở", "tại", "của", "thuộc", "nằm trên", "xuất hiện trên"
    };

    #endregion

    /// <summary>
    /// Vietnamese diacritics mapping for normalization
    /// </summary>
    private static readonly Dictionary<char, char> DiacriticsMap = new()
    {
        // a variants
        {'à', 'a'}, {'á', 'a'}, {'ả', 'a'}, {'ã', 'a'}, {'ạ', 'a'},
        {'ă', 'a'}, {'ằ', 'a'}, {'ắ', 'a'}, {'ẳ', 'a'}, {'ẵ', 'a'}, {'ặ', 'a'},
        {'â', 'a'}, {'ầ', 'a'}, {'ấ', 'a'}, {'ẩ', 'a'}, {'ẫ', 'a'}, {'ậ', 'a'},

        // e variants
        {'è', 'e'}, {'é', 'e'}, {'ẻ', 'e'}, {'ẽ', 'e'}, {'ẹ', 'e'},
        {'ê', 'e'}, {'ề', 'e'}, {'ế', 'e'}, {'ể', 'e'}, {'ễ', 'e'}, {'ệ', 'e'},

        // i variants
        {'ì', 'i'}, {'í', 'i'}, {'ỉ', 'i'}, {'ĩ', 'i'}, {'ị', 'i'},

        // o variants
        {'ò', 'o'}, {'ó', 'o'}, {'ỏ', 'o'}, {'õ', 'o'}, {'ọ', 'o'},
        {'ô', 'o'}, {'ồ', 'o'}, {'ố', 'o'}, {'ổ', 'o'}, {'ỗ', 'o'}, {'ộ', 'o'},
        {'ơ', 'o'}, {'ờ', 'o'}, {'ớ', 'o'}, {'ở', 'o'}, {'ỡ', 'o'}, {'ợ', 'o'},

        // u variants
        {'ù', 'u'}, {'ú', 'u'}, {'ủ', 'u'}, {'ũ', 'u'}, {'ụ', 'u'},
        {'ư', 'u'}, {'ừ', 'u'}, {'ứ', 'u'}, {'ử', 'u'}, {'ữ', 'u'}, {'ự', 'u'},

        // y variants
        {'ỳ', 'y'}, {'ý', 'y'}, {'ỷ', 'y'}, {'ỹ', 'y'}, {'ỵ', 'y'},

        // d variant
        {'đ', 'd'}
    };

    /// <summary>
    /// Normalize Vietnamese text for comparison
    /// - Lowercase
    /// - Remove diacritics
    /// - Remove extra whitespace
    /// - Remove punctuation
    /// </summary>
    public static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var result = new StringBuilder();
        var normalized = text.ToLowerInvariant().Normalize(NormalizationForm.FormC);

        foreach (var c in normalized)
        {
            if (DiacriticsMap.TryGetValue(c, out var replacement))
            {
                result.Append(replacement);
            }
            else if (char.IsLetterOrDigit(c))
            {
                result.Append(c);
            }
            else if (char.IsWhiteSpace(c))
            {
                // Avoid multiple spaces
                if (result.Length > 0 && result[^1] != ' ')
                {
                    result.Append(' ');
                }
            }
            // Skip punctuation and other characters
        }

        return result.ToString().Trim();
    }

    /// <summary>
    /// Remove Vietnamese diacritics but keep the base characters
    /// Example: "đốm nâu" -> "dom nau"
    /// </summary>
    public static string RemoveDiacritics(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var result = new StringBuilder();
        var normalized = text.Normalize(NormalizationForm.FormC);

        foreach (var c in normalized)
        {
            var lowerC = char.ToLowerInvariant(c);
            if (DiacriticsMap.TryGetValue(lowerC, out var replacement))
            {
                result.Append(char.IsUpper(c) ? char.ToUpperInvariant(replacement) : replacement);
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Tokenize text into words
    /// </summary>
    public static List<string> Tokenize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        var normalized = Normalize(text);
        return normalized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 1) // Remove single characters
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Calculate Jaccard similarity between two sets of tokens
    /// </summary>
    public static double CalculateJaccardSimilarity(IEnumerable<string> set1, IEnumerable<string> set2)
    {
        var list1 = set1.ToHashSet();
        var list2 = set2.ToHashSet();

        if (list1.Count == 0 && list2.Count == 0)
            return 1.0;

        if (list1.Count == 0 || list2.Count == 0)
            return 0.0;

        var intersection = list1.Intersect(list2).Count();
        var union = list1.Union(list2).Count();

        return union > 0 ? (double)intersection / union : 0.0;
    }

    /// <summary>
    /// Calculate token overlap score between two texts
    /// </summary>
    public static double CalculateTokenOverlap(string? text1, string? text2)
    {
        var tokens1 = Tokenize(text1);
        var tokens2 = Tokenize(text2);

        return CalculateJaccardSimilarity(tokens1, tokens2);
    }

    /// <summary>
    /// Check if text contains any of the given keywords
    /// </summary>
    public static bool ContainsAny(string? text, IEnumerable<string> keywords)
    {
        if (string.IsNullOrWhiteSpace(text) || keywords == null)
            return false;

        var normalizedText = Normalize(text);
        return keywords.Any(k => normalizedText.Contains(Normalize(k)));
    }

    /// <summary>
    /// Find matching keywords in text
    /// </summary>
    public static List<string> FindMatchingKeywords(string? text, IEnumerable<string> keywords)
    {
        if (string.IsNullOrWhiteSpace(text) || keywords == null)
            return new List<string>();

        var normalizedText = Normalize(text);
        return keywords
            .Where(k => normalizedText.Contains(Normalize(k)))
            .ToList();
    }

    /// <summary>
    /// Extract potential symptom phrases from text
    /// Common Vietnamese symptom patterns
    /// </summary>
    public static List<string> ExtractSymptomPhrases(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        var phrases = new List<string>();
        var normalized = text.ToLowerInvariant();

        // Common symptom patterns in Vietnamese
        var patterns = new[]
        {
            @"lá\s+\w+",           // lá vàng, lá xoăn, lá khô
            @"đốm\s+\w+",         // đốm nâu, đốm đen
            @"vết\s+\w+",         // vết nâu, vết đen
            @"chồi\s+\w+",        // chồi xoăn
            @"rễ\s+\w+",          // rễ thối
            @"thân\s+\w+",        // thân thối
            @"hoa\s+\w+",         // hoa héo
            @"\w+\s+úa",          // lá úa
            @"\w+\s+héo",         // lá héo, cành héo
            @"\w+\s+thối",        // rễ thối, thân thối
            @"\w+\s+xoăn",        // lá xoăn, chồi xoăn
            @"nấm\s+\w*",         // nấm mốc
            @"mốc\s*\w*",         // mốc trắng
            @"sâu\s+\w*",         // sâu đục
            @"bọ\s+\w*",          // bọ trĩ
        };

        foreach (var pattern in patterns)
        {
            var matches = Regex.Matches(normalized, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var phrase = match.Value.Trim();
                if (!string.IsNullOrWhiteSpace(phrase) && !phrases.Contains(phrase))
                {
                    phrases.Add(phrase);
                }
            }
        }

        return phrases;
    }

    #region Scope-Based Extraction Methods

    /// <summary>
    /// Split text into clauses based on boundary keywords and punctuation
    /// Each clause represents a separate scope for symptom extraction
    /// </summary>
    public static List<string> SplitIntoClauses(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        var result = new List<string>();
        var lowerText = text.ToLowerInvariant();

        // First, split by strong boundaries (boundary keywords)
        var workingText = lowerText;
        foreach (var boundary in BoundaryKeywords)
        {
            workingText = Regex.Replace(workingText, $@"\b{Regex.Escape(boundary)}\b", "|||BOUNDARY|||");
        }

        // Split by boundary markers and punctuation
        var parts = Regex.Split(workingText, @"\|\|\|BOUNDARY\|\|\||[;.]");

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                continue;

            // Further split by comma, but only if there's a new plant part after comma
            var subParts = trimmed.Split(',');
            var currentClause = "";

            foreach (var subPart in subParts)
            {
                var subTrimmed = subPart.Trim();
                if (string.IsNullOrWhiteSpace(subTrimmed))
                    continue;

                // Check if this sub-part starts with a new plant part
                var hasNewPlantPart = FindPlantPartInText(subTrimmed).plantPart != null &&
                                      FindPlantPartInText(subTrimmed).position < 3; // Plant part near start

                if (hasNewPlantPart && !string.IsNullOrWhiteSpace(currentClause))
                {
                    // Save current clause and start new one
                    result.Add(currentClause.Trim());
                    currentClause = subTrimmed;
                }
                else
                {
                    // Append to current clause
                    currentClause = string.IsNullOrWhiteSpace(currentClause)
                        ? subTrimmed
                        : currentClause + ", " + subTrimmed;
                }
            }

            if (!string.IsNullOrWhiteSpace(currentClause))
            {
                result.Add(currentClause.Trim());
            }
        }

        return result;
    }

    /// <summary>
    /// Find plant part keyword in text and return its category and position
    /// </summary>
    public static (string? plantPart, int position) FindPlantPartInText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (null, -1);

        var lowerText = text.ToLowerInvariant();
        var bestMatch = (plantPart: (string?)null, position: int.MaxValue, length: 0);

        foreach (var (category, keywords) in PlantPartKeywords)
        {
            foreach (var keyword in keywords)
            {
                var pattern = $@"\b{Regex.Escape(keyword)}\b";
                var match = Regex.Match(lowerText, pattern);

                if (match.Success)
                {
                    // Prefer earlier matches, and longer keywords for ties
                    if (match.Index < bestMatch.position ||
                        (match.Index == bestMatch.position && keyword.Length > bestMatch.length))
                    {
                        bestMatch = (category, match.Index, keyword.Length);
                    }
                }
            }
        }

        return bestMatch.plantPart != null ? (bestMatch.plantPart, bestMatch.position) : (null, -1);
    }

    /// <summary>
    /// Check if text contains a modifier pattern like "trên lá", "ở thân"
    /// Returns the plant part if found after modifier
    /// </summary>
    public static string? FindPlantPartAfterModifier(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var lowerText = text.ToLowerInvariant();

        foreach (var modifier in ModifierKeywords)
        {
            var pattern = $@"\b{Regex.Escape(modifier)}\s+(\w+)";
            var match = Regex.Match(lowerText, pattern);

            if (match.Success)
            {
                var wordAfterModifier = match.Groups[1].Value;

                // Check if this word is a plant part
                foreach (var (category, keywords) in PlantPartKeywords)
                {
                    if (keywords.Any(k => k.Equals(wordAfterModifier, StringComparison.OrdinalIgnoreCase) ||
                                          Normalize(k) == Normalize(wordAfterModifier)))
                    {
                        return category;
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Normalize plant part category from Vietnamese text
    /// </summary>
    public static string? NormalizePlantPart(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var normalized = Normalize(text);

        foreach (var (category, keywords) in PlantPartKeywords)
        {
            if (keywords.Any(k => Normalize(k) == normalized))
            {
                return category;
            }
        }

        return null;
    }

    #endregion
}
