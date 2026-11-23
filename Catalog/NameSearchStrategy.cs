using KingdomExplorer.Models;

namespace KingdomExplorer.Catalog;

/// <summary>Case-insensitive exact or partial match on common OR scientific name.</summary>
public sealed class NameSearchStrategy : ISearchStrategy
{
    public IReadOnlyList<Organism> Search(IEnumerable<Organism> organisms, string query, bool exactMatch)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<Organism>();

        string q = query.Trim();
        const StringComparison cmp = StringComparison.OrdinalIgnoreCase;

        return organisms
            .Where(o => exactMatch
                ? o.CommonName.Equals(q, cmp) || o.ScientificName.Equals(q, cmp)
                : o.CommonName.Contains(q, cmp) || o.ScientificName.Contains(q, cmp))
            .ToList();
    }
}
