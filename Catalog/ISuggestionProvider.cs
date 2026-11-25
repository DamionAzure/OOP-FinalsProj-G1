using KingdomExplorer.Models;

namespace KingdomExplorer.Catalog;

/// <summary>
/// EXTENSION POINT: optional "Did you mean...?" suggestions shown when a search finds nothing.
/// NoSuggestionProvider returns none; FuzzySuggestionProvider (FuzzySharp) returns close names.
/// Program.cs chooses which one the catalog uses.
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
