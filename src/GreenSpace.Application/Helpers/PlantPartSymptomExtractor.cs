using GreenSpace.Domain.Models;

namespace GreenSpace.Application.Helpers;

/// <summary>
/// Extracts symptoms from user description with plant part context (scope-based)
/// </summary>
public class PlantPartSymptomExtractor
{
    /// <summary>
    /// Default plant part when no anchor is found
    /// </summary>
    public const string DefaultPlantPart = "general";

    /// <summary>
    /// Extract symptoms from description with plant part context
    /// </summary>
    /// <param name="description">User's description of plant symptoms</param>
    /// <param name="symptomDictionary">List of known symptoms from database</param>
    /// <returns>Dictionary of plant part -> list of extracted symptoms</returns>
    public Dictionary<string, List<ExtractedSymptom>> ExtractWithScope(
        string description,
        List<SymptomDictionary> symptomDictionary)
    {
        var result = new Dictionary<string, List<ExtractedSymptom>>();

        if (string.IsNullOrWhiteSpace(description) || symptomDictionary == null || symptomDictionary.Count == 0)
            return result;

        // Step 1: Split into clauses
        var clauses = VietnameseTextHelper.SplitIntoClauses(description);

        if (clauses.Count == 0)
        {
            // Fallback: treat entire description as one clause
            clauses = new List<string> { description };
        }

        // Step 2: Process each clause
        string? inheritedAnchor = null;

        foreach (var clause in clauses)
        {
            // Find anchor (plant part) in this clause
            var anchor = FindAnchorInClause(clause);

            // Check for modifier pattern like "trên lá"
            var modifierAnchor = VietnameseTextHelper.FindPlantPartAfterModifier(clause);
            if (modifierAnchor != null)
            {
                anchor = modifierAnchor;
            }

            // If no anchor, inherit from previous clause or use default
            if (anchor == null)
            {
                anchor = inheritedAnchor ?? DefaultPlantPart;
            }
            else
            {
                // Update inherited anchor for next clause
                inheritedAnchor = anchor;
            }

            // Find symptoms in this clause
            var symptoms = FindSymptomsInClause(clause, anchor, symptomDictionary);

            // Add to result
            foreach (var symptom in symptoms)
            {
                if (!result.ContainsKey(symptom.PlantPart))
                {
                    result[symptom.PlantPart] = new List<ExtractedSymptom>();
                }

                // Avoid duplicates
                if (!result[symptom.PlantPart].Any(s => s.SymptomName == symptom.SymptomName))
                {
                    result[symptom.PlantPart].Add(symptom);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Find plant part anchor in a clause
    /// </summary>
    private string? FindAnchorInClause(string clause)
    {
        var (plantPart, _) = VietnameseTextHelper.FindPlantPartInText(clause);
        return plantPart;
    }

    /// <summary>
    /// Find symptoms in a clause and assign them to the given plant part
    /// Context-aware matching: prefer symptoms where category matches anchor
    /// </summary>
    private List<ExtractedSymptom> FindSymptomsInClause(
        string clause,
        string plantPart,
        List<SymptomDictionary> symptomDictionary)
    {
        var candidates = new List<(ExtractedSymptom Symptom, bool CategoryMatchesAnchor, int MatchLength)>();
        var normalizedClause = VietnameseTextHelper.Normalize(clause);

        foreach (var symptom in symptomDictionary)
        {
            bool matched = false;
            double confidence = 0.8; // Default confidence
            int matchLength = 0;

            // Check canonical name first (highest priority)
            var normalizedCanonical = VietnameseTextHelper.Normalize(symptom.CanonicalName);
            if (normalizedClause.Contains(normalizedCanonical))
            {
                matched = true;
                confidence = 1.0; // Full confidence for canonical match
                matchLength = normalizedCanonical.Length;
            }

            // Check synonyms
            if (!matched)
            {
                foreach (var synonym in symptom.Synonyms)
                {
                    var normalizedSynonym = VietnameseTextHelper.Normalize(synonym);
                    if (normalizedClause.Contains(normalizedSynonym))
                    {
                        matched = true;
                        confidence = 0.9; // Slightly lower for synonym
                        matchLength = normalizedSynonym.Length;
                        break;
                    }
                }
            }

            if (matched)
            {
                // Check if symptom category matches the clause anchor
                bool categoryMatchesAnchor = false;
                if (!string.IsNullOrWhiteSpace(symptom.Category) && plantPart != DefaultPlantPart)
                {
                    categoryMatchesAnchor = symptom.Category.Equals(plantPart, StringComparison.OrdinalIgnoreCase);
                }

                // Determine final plant part
                var finalPlantPart = plantPart;

                // If anchor is general but symptom has a valid category, use it
                if (plantPart == DefaultPlantPart && !string.IsNullOrWhiteSpace(symptom.Category))
                {
                    if (VietnameseTextHelper.PlantPartKeywords.ContainsKey(symptom.Category))
                    {
                        finalPlantPart = symptom.Category;
                        confidence *= 0.9; // Slightly reduce confidence when inferred
                    }
                }

                // Boost confidence if category matches anchor
                if (categoryMatchesAnchor)
                {
                    confidence = Math.Min(confidence * 1.1, 1.0);
                }

                candidates.Add((
                    new ExtractedSymptom
                    {
                        SymptomName = symptom.CanonicalName,
                        PlantPart = finalPlantPart,
                        SymptomId = symptom.Id,
                        Category = symptom.Category,
                        Confidence = confidence
                    },
                    categoryMatchesAnchor,
                    matchLength
                ));
            }
        }

        // Filter out duplicates: when multiple symptoms match the same single word (e.g., "thối")
        // prefer the one whose category matches the anchor
        var result = new List<ExtractedSymptom>();
        var groupedByName = candidates.GroupBy(c => c.Symptom.SymptomName);

        foreach (var group in groupedByName)
        {
            if (group.Count() == 1)
            {
                result.Add(group.First().Symptom);
            }
            else
            {
                // Multiple symptoms with same name - shouldn't happen, just take first
                result.Add(group.First().Symptom);
            }
        }

        // Handle overlapping single-word matches (e.g., "thối" matches both "thối thân" and "thối rễ")
        // Group by the matched word and prefer category matching anchor
        var singleWordSymptoms = candidates
            .Where(c => c.MatchLength <= 5) // Single Vietnamese word typically <= 5 chars normalized
            .ToList();

        if (singleWordSymptoms.Count > 1)
        {
            // Find conflicts: same match length but different symptoms
            var conflicts = singleWordSymptoms
                .GroupBy(c => c.MatchLength)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .ToList();

            if (conflicts.Any())
            {
                // Remove all conflicts from result
                foreach (var conflict in conflicts)
                {
                    result.RemoveAll(r => r.SymptomName == conflict.Symptom.SymptomName);
                }

                // Re-add only the one with matching category, or highest confidence
                var bestMatch = conflicts
                    .OrderByDescending(c => c.CategoryMatchesAnchor ? 1 : 0)
                    .ThenByDescending(c => c.Symptom.Confidence)
                    .First();

                if (!result.Any(r => r.SymptomName == bestMatch.Symptom.SymptomName))
                {
                    result.Add(bestMatch.Symptom);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Flatten extracted symptoms to a simple list (for backward compatibility)
    /// </summary>
    public List<string> FlattenSymptoms(Dictionary<string, List<ExtractedSymptom>> scopedSymptoms)
    {
        return scopedSymptoms.Values
            .SelectMany(s => s)
            .Select(s => s.SymptomName)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Get all extracted symptoms with their plant parts as a flat list
    /// </summary>
    public List<ExtractedSymptom> GetAllSymptoms(Dictionary<string, List<ExtractedSymptom>> scopedSymptoms)
    {
        return scopedSymptoms.Values
            .SelectMany(s => s)
            .GroupBy(s => s.SymptomName)
            .Select(g => g.First()) // Remove duplicates, keep first occurrence
            .ToList();
    }
}

/// <summary>
/// Represents an extracted symptom with plant part context
/// </summary>
public class ExtractedSymptom
{
    /// <summary>
    /// Canonical symptom name (e.g., "đốm nâu")
    /// </summary>
    public string SymptomName { get; set; } = null!;

    /// <summary>
    /// Plant part category (leaf, stem, root, fruit, flower, general)
    /// </summary>
    public string PlantPart { get; set; } = "general";

    /// <summary>
    /// FK to symptom_dictionary (if matched from dictionary)
    /// </summary>
    public Guid? SymptomId { get; set; }

    /// <summary>
    /// Original category from symptom dictionary
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Confidence score (0.0 - 1.0)
    /// </summary>
    public double Confidence { get; set; } = 1.0;
}
