using KingdomExplorer.Models;

namespace KingdomExplorer.Catalog;

/// <summary>
/// EXTENSION POINT: how a name query is matched against organisms.
/// NameSearchStrategy does plain case-insensitive matching; FuzzySearchStrategy adds typo
/// tolerance with FuzzySharp. Program.cs chooses which one the catalog uses.
/// </summary>
public interface ISearchStrategy
{
    IReadOnlyList<Organism> Search(IEnumerable<Organism> organisms, string query, bool exactMatch);
}
