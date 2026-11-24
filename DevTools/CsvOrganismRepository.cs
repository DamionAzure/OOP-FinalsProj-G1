using KingdomExplorer.Data;

namespace KingdomExplorer.DevTools;

/// <summary>
/// Development only: loads organisms from a CSV file (one row per organism) so teammates can edit
/// data in Excel or Google Sheets. Bad rows are skipped and reported by row number.
/// </summary>
internal sealed class CsvOrganismRepository : IOrganismRepository
{
    private static readonly string[] RequiredColumns =
        { "type", "id", "commonName", "scientificName", "habitat", "distinguishingTrait" };

    private readonly string _path;

    public CsvOrganismRepository(string path) => _path = path;

    public LoadResult Load()
    {
        var result = new LoadResult { Source = Path.GetFileName(_path) };

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

        result.AddSourceNotes();
        return result;
    }

    // ArgumentException messages end with " (Parameter 'x')"; the column name is already in the text.
    private static string Clean(string message)
    {
        int i = message.IndexOf(" (Parameter", StringComparison.Ordinal);
        return i >= 0 ? message[..i] : message;
    }
}
