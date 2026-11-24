using System.Text;
using KingdomExplorer.Data;

namespace KingdomExplorer.DevTools;

/// <summary>
/// Development helpers. DELETE THE WHOLE DevTools FOLDER (and Data/organisms.csv) BEFORE SUBMITTING.
/// The project then builds with the records in Data/OrganismData.cs only.
///   dotnet run                         uses Data/organisms.csv if it exists
///   dotnet run -- --data file.csv      uses another CSV file
///   dotnet run -- --export-cs          writes Data/OrganismData.cs from the CSV (pure C#)
///   dotnet run -- --export-cs -        prints the C# instead of writing a file
/// </summary>
internal static class DevCommands
{
    public static string DefaultCsvPath =>
        Path.Combine(Directory.GetCurrentDirectory(), "Data", "organisms.csv");

    /// <summary>The CSV repository to use, or null to fall back to OrganismData.</summary>
    public static IOrganismRepository? CreateRepository(string[] args)
    {
        string? path = ArgValue(args, "--data");
        if (path is not null) return new CsvOrganismRepository(path);
        return File.Exists(DefaultCsvPath) ? new CsvOrganismRepository(DefaultCsvPath) : null;
    }

    /// <summary>Handles --export-cs. Returns true when the command was given (the app then exits).</summary>
    public static bool TryRunExport(string[] args)
    {
        int at = Array.IndexOf(args, "--export-cs");
        if (at < 0) return false;

        string csvPath = ArgValue(args, "--data") ?? DefaultCsvPath;
        string output = at + 1 < args.Length && !args[at + 1].StartsWith("--") && args[at + 1] != ""
            ? args[at + 1]
            : Path.Combine(Directory.GetCurrentDirectory(), "Data", "OrganismData.cs");

        LoadResult loaded = new CsvOrganismRepository(csvPath).Load();
        foreach (string note in loaded.Notes) Console.Error.WriteLine("  " + note);

        // Duplicate IDs: keep the first, warn about the rest.
        var organisms = new List<KingdomExplorer.Models.Organism>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var o in loaded.Organisms)
        {
            if (seen.Add(o.Id)) organisms.Add(o);
            else Console.Error.WriteLine($"  {o.Id} appears more than once; only the first is exported.");
        }

        if (organisms.Count == 0)
        {
            Console.Error.WriteLine("Nothing to export. Fix the problems above first.");
            return true;
        }

        string code = OrganismDataExporter.Generate(organisms);
        if (output == "-")
        {
            Console.Write(code);
            return true;
        }

        File.WriteAllText(output, code, new UTF8Encoding(false));
        Console.WriteLine($"Wrote {organisms.Count} records to {output}");
        Console.WriteLine("Next: build and run once to check it. Before submitting, delete the DevTools");
        Console.WriteLine("folder and Data/organisms.csv, then build and run again (pure C# data).");
        return true;
    }

    public static string? ArgValue(string[] args, string name)
    {
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }
}
