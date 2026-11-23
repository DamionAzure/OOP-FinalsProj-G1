using KingdomExplorer.Models;

namespace KingdomExplorer.Data;

/// <summary>
/// Loads organisms from a CSV file (one row per organism). Bad rows are skipped and reported
/// by row number, so the rest of the file still loads.
/// </summary>
public sealed class CsvOrganismRepository : IOrganismRepository
{
    private static readonly string[] RequiredColumns =
        { "type", "id", "commonName", "scientificName", "habitat", "distinguishingTrait" };

    private readonly string _path;

    public CsvOrganismRepository(string path) => _path = path;

    public LoadResult Load()
    {
        var result = new LoadResult();

        if (!File.Exists(_path))
        {
            result.Notes.Add($"Data file not found: {_path}");
            return result;
        }

        List<List<string>> rows;
        try
        {
            rows = CsvReader.Parse(File.ReadAllText(_path));
        }
        catch (Exception ex) when (ex is FormatException or IOException or UnauthorizedAccessException)
        {
            result.Notes.Add($"Could not read {Path.GetFileName(_path)}: {ex.Message}");
            return result;
        }

        if (rows.Count == 0)
        {
            result.Notes.Add($"{Path.GetFileName(_path)} is empty.");
            return result;
        }

        List<string> header = rows[0].Select(h => h.Trim()).ToList();
        var missing = RequiredColumns
            .Where(c => !header.Contains(c, StringComparer.OrdinalIgnoreCase)).ToList();
        if (missing.Count > 0)
        {
            result.Notes.Add($"The header row is missing column(s): {string.Join(", ", missing)}.");
            return result;
        }

        for (int r = 1; r < rows.Count; r++)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int c = 0; c < header.Count && c < rows[r].Count; c++)
                values[header[c]] = rows[r][c];

            var row = new DataRow(values);
            try
            {
                result.Organisms.Add(OrganismFactory.Create(row));
            }
            catch (Exception ex) when (ex is ArgumentException or FormatException)
            {
                string id = row.Get("id");
                result.Notes.Add($"Row {r + 1} ({(id.Length > 0 ? id : "no ID")}) skipped: {Clean(ex.Message)}");
            }
        }

        AddSourceNotes(result);
        return result;
    }

    private static void AddSourceNotes(LoadResult result)
    {
        var noSources = result.Organisms.Where(o => o.Sources.Count == 0).Select(o => o.Id).ToList();
        if (noSources.Count > 0)
            result.Notes.Add($"No sources listed for: {string.Join(", ", noSources)}.");

        var suggested = result.Organisms
            .Where(o => o.Sources.Any(s => s.StartsWith("SUGGESTED", StringComparison.OrdinalIgnoreCase)))
            .Select(o => o.Id).ToList();
        if (suggested.Count > 0)
            result.Notes.Add($"{suggested.Count} record(s) still have SUGGESTED (unchecked) sources. " +
                             "Open each source, confirm the facts, then replace the text.");
    }

    // ArgumentException messages end with " (Parameter 'x')"; the column name is already in the text.
    private static string Clean(string message)
    {
        int i = message.IndexOf(" (Parameter", StringComparison.Ordinal);
        return i >= 0 ? message[..i] : message;
    }
}
