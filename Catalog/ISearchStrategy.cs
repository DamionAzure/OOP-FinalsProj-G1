using KingdomExplorer.Models;

namespace KingdomExplorer.Catalog;

/// <summary>
/// EXTENSION POINT: how a name query is matched against organisms.
/// The default is NameSearchStrategy (plain string matching).
/// A FuzzySharp-based class could implement this interface later.
/// </summary>
public interface ISearchStrategy
{
    IReadOnlyList<Organism> Search(IEnumerable<Organism> organisms, string query, bool exactMatch);
}
