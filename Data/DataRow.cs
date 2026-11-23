namespace KingdomExplorer.Data;

/// <summary>One spreadsheet row, read by column name (case-insensitive). Missing columns read as empty.</summary>
internal sealed class DataRow
{
    private readonly Dictionary<string, string> _values;

    public DataRow(Dictionary<string, string> values) => _values = values;

    public string Get(string column) =>
        _values.TryGetValue(column, out string? v) ? v.Trim() : "";

    public bool GetBool(string column)
    {
        string v = Get(column).ToLowerInvariant();
        return v switch
        {
            "true" or "yes" or "y" or "1" => true,
            "false" or "no" or "n" or "0" => false,
            _ => throw new FormatException($"'{column}' must be true or false (found '{Get(column)}').")
        };
    }

    /// <summary>Several sources go in one cell, separated by | (pipe).</summary>
    public IEnumerable<string> Sources =>
        Get("sources").Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
