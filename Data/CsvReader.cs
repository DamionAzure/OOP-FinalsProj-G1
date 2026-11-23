using System.Text;

namespace KingdomExplorer.Data;

/// <summary>
/// Small CSV parser (what Excel and Google Sheets export): commas separate columns,
/// "double quotes" wrap values that contain commas or line breaks, and "" is a quote inside a value.
/// </summary>
internal static class CsvReader
{
    public static List<List<string>> Parse(string text)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else inQuotes = false;
                }
                else field.Append(c);
            }
            else if (c == '"') inQuotes = true;
            else if (c == ',') { row.Add(field.ToString()); field.Clear(); }
            else if (c == '\r') { /* ignore; the \n ends the row */ }
            else if (c == '\n')
            {
                row.Add(field.ToString()); field.Clear();
                rows.Add(row); row = new List<string>();
            }
            else field.Append(c);
        }

        if (inQuotes)
            throw new FormatException("A quoted value is missing its closing quote (\").");

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }

        // drop completely blank rows
        rows.RemoveAll(r => r.All(string.IsNullOrWhiteSpace));
        return rows;
    }
}
