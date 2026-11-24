using KingdomExplorer.Models;

namespace KingdomExplorer.Data;

/// <summary>What a data source returns: the records it could create, plus plain-language notes.</summary>
public sealed class LoadResult
{
    public List<Organism> Organisms { get; } = new();

    /// <summary>Problems and reminders to show the user (skipped entries, missing sources, ...).</summary>
    public List<string> Notes { get; } = new();

    /// <summary>Where the records came from (shown in the startup message).</summary>
    public string Source { get; set; } = "";

    /// <summary>Adds reminders about missing or unchecked sources (the brief requires sources for every record).</summary>
    public void AddSourceNotes()
    {
        var noSources = Organisms.Where(o => o.Sources.Count == 0).Select(o => o.Id).ToList();
        if (noSources.Count > 0)
            Notes.Add($"No sources listed for: {string.Join(", ", noSources)}.");

        var suggested = Organisms
            .Where(o => o.Sources.Any(s => s.StartsWith("SUGGESTED", StringComparison.OrdinalIgnoreCase)))
            .Select(o => o.Id).ToList();
        if (suggested.Count > 0)
            Notes.Add($"{suggested.Count} record(s) still have SUGGESTED (unchecked) sources. " +
                      "Open each source, confirm the facts, then replace the text.");
    }
}

/// <summary>
/// EXTENSION POINT: where organism records come from. The submitted app uses OrganismData
/// (records written in C#). Another source could implement this interface without changing the catalog.
/// </summary>
public interface IOrganismRepository
{
    LoadResult Load();
}
