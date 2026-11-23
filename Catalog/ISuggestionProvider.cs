using KingdomExplorer.Models;

namespace KingdomExplorer.Catalog;

/// <summary>
/// EXTENSION POINT: optional "Did you mean...?" suggestions shown when a search finds nothing.
/// The default (NoSuggestionProvider) returns none. A FuzzySharp-based class could implement this later.
/// </summary>
public interface ISuggestionProvider
{
    IReadOnlyList<string> Suggest(IEnumerable<Organism> organisms, string query);
}

public sealed class NoSuggestionProvider : ISuggestionProvider
{
    public IReadOnlyList<string> Suggest(IEnumerable<Organism> organisms, string query) =>
        Array.Empty<string>();
}
