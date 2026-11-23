using KingdomExplorer.Models;

namespace KingdomExplorer.Catalog;

/// <summary>
/// ENCAPSULATION: owns the private organism list. The outside world can only
/// add, list, search, and filter through the controlled methods below.
/// </summary>
public sealed class OrganismCatalog
{
    private readonly List<Organism> _organisms = new();   // hidden state
    private readonly ISearchStrategy _searchStrategy;
    private readonly ISuggestionProvider _suggestionProvider;

    public OrganismCatalog(ISearchStrategy? searchStrategy = null,
                           ISuggestionProvider? suggestionProvider = null)
    {
        _searchStrategy = searchStrategy ?? new NameSearchStrategy();
        _suggestionProvider = suggestionProvider ?? new NoSuggestionProvider();
    }

    public int Count => _organisms.Count;

    /// <summary>Adds a record. Rejects null and duplicate IDs (case-insensitive).</summary>
    public void Add(Organism organism)
    {
        if (organism is null)
            throw new ArgumentNullException(nameof(organism), "Cannot add a null organism.");

        if (_organisms.Any(o => o.Id.Equals(organism.Id, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"An organism with ID '{organism.Id}' already exists.", nameof(organism));

        _organisms.Add(organism);
    }

    /// <summary>Returns a copy, so callers cannot modify the internal list.</summary>
    public IReadOnlyList<Organism> GetAll() => _organisms.ToList();

    public Organism? FindById(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return _organisms.FirstOrDefault(o => o.Id.Equals(id.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<Organism> Search(string query, bool exactMatch = false) =>
        _searchStrategy.Search(_organisms, query, exactMatch);

    public IReadOnlyList<string> SuggestSimilar(string query) =>
        _suggestionProvider.Suggest(_organisms, query);

    public IReadOnlyList<Organism> FilterByKingdom(Kingdom kingdom) =>
        _organisms.Where(o => o.Kingdom == kingdom).ToList();

    public IReadOnlyList<Organism> FilterByGroup(string? group)
    {
        if (string.IsNullOrWhiteSpace(group)) return Array.Empty<Organism>();
        return _organisms
            .Where(o => o.Group.Equals(group.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public IReadOnlyList<string> GetGroups() =>
        _organisms.Select(o => o.Group).Distinct().OrderBy(g => g).ToList();
}
