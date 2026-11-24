using System.Reflection;
using System.Text;
using KingdomExplorer.Models;

namespace KingdomExplorer.DevTools;

/// <summary>
/// Turns loaded organisms into the source code of Data/OrganismData.cs (pure C#, no data files).
/// Works for any Organism subclass: each constructor parameter is matched to the property with
/// the same name, so a new group class needs no changes here.
/// </summary>
internal static class OrganismDataExporter
{
    public static string Generate(IEnumerable<Organism> organisms)
    {
        var sb = new StringBuilder();
        sb.Append(Header);

        string? lastKingdom = null, lastGroup = null;
        foreach (Organism o in organisms)
        {
            if (o.Kingdom.ToString() != lastKingdom)
            {
                if (lastKingdom is not null) sb.AppendLine();
                sb.AppendLine($"        // ===================== {o.Kingdom.ToString().ToUpperInvariant()}S =====================");
                sb.AppendLine();
                lastKingdom = o.Kingdom.ToString();
                lastGroup = null;
            }
            if (o.Group != lastGroup)
            {
                sb.AppendLine($"        // ----- {o.Group} -----");
                lastGroup = o.Group;
            }
            AppendRecord(sb, o);
            sb.AppendLine();
        }

        sb.Append(Footer);
        return sb.ToString();
    }

    private static void AppendRecord(StringBuilder sb, Organism o)
    {
        Type t = o.GetType();
        ConstructorInfo ctor = t.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();
        ParameterInfo[] ps = ctor.GetParameters();

        sb.AppendLine($"        () => new {t.Name}(");
        for (int i = 0; i < ps.Length; i++)
        {
            string prop = char.ToUpperInvariant(ps[i].Name![0]) + ps[i].Name![1..];
            PropertyInfo? pi = t.GetProperty(prop);
            if (pi is null)
                throw new InvalidOperationException(
                    $"{t.Name} has a constructor parameter '{ps[i].Name}' but no property '{prop}', so it cannot be exported.");

            string end = i < ps.Length - 1 ? "," : "),";
            sb.AppendLine($"            {ps[i].Name}: {Literal(pi.GetValue(o))}{end}");
        }
    }

    private static string Literal(object? value) => value switch
    {
        null => "null",
        string s => Quote(s),
        bool b => b ? "true" : "false",
        IEnumerable<string> list => list.Any()
            ? "new[] { " + string.Join(", ", list.Select(Quote)) + " }"
            : "Array.Empty<string>()",
        _ => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? "null"
    };

    private static string Quote(string s)
    {
        var sb = new StringBuilder("\"");
        foreach (char c in s)
            sb.Append(c switch
            {
                '\\' => "\\\\",
                '"' => "\\\"",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                _ => c.ToString()
            });
        return sb.Append('"').ToString();
    }

    private const string Header = """
using KingdomExplorer.Models;

namespace KingdomExplorer.Data;

/// <summary>
/// ALL ORGANISM RECORDS LIVE HERE. Each record is one block starting with "() => new ...(".
///
/// TO ADD AN ORGANISM: copy a whole block of the same type, paste it right after that block
/// (before the closing "};"), and change the values. Keep the quotes and commas. The app lists any
/// entry that was skipped (with its number and the reason) when it starts.
///
/// Every type needs:  id (unique), commonName, scientificName, habitat, distinguishingTrait, sources.
/// Extra fields:      Mammal diet, bodyCovering | Bird diet, canFly | FloweringPlant flowerColor |
///                    Fern frondDescription
///
/// SOURCES: replace each "SUGGESTED - verify: ..." text with the real citation after you have
/// opened the source and confirmed the facts.
/// </summary>
public sealed class OrganismData : IOrganismRepository
{
    // Each entry is wrapped in "() =>" so one bad entry can be skipped and reported
    // without stopping the others.
    private static readonly List<Func<Organism>> Records = new()
    {

""";

    private const string Footer = """
    };

    public LoadResult Load()
    {
        var result = new LoadResult { Source = "OrganismData.cs" };

        int entry = 0;
        foreach (Func<Organism> create in Records)
        {
            entry++;
            try
            {
                result.Organisms.Add(create());
            }
            catch (ArgumentException ex)    // thrown by the organism classes' validation
            {
                int p = ex.Message.IndexOf(" (Parameter", StringComparison.Ordinal);
                result.Notes.Add($"Entry #{entry} skipped: {(p >= 0 ? ex.Message[..p] : ex.Message)}");
            }
        }

        result.AddSourceNotes();
        return result;
    }
}

""";
}
