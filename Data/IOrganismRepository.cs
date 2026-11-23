using KingdomExplorer.Models;

namespace KingdomExplorer.Data;

/// <summary>What a data source returns: the records it could read, plus plain-language notes.</summary>
public sealed class LoadResult
{
    public List<Organism> Organisms { get; } = new();

    /// <summary>Problems and information to show the user (bad rows, missing sources, ...).</summary>
    public List<string> Notes { get; } = new();
}

/// <summary>
/// EXTENSION POINT: where organism records come from. The app uses CsvOrganismRepository
/// (a spreadsheet file). A JSON, database or web source could implement this later.
/// </summary>
public interface IOrganismRepository
{
    LoadResult Load();
}
