using KingdomExplorer.Catalog;
using KingdomExplorer.Data;
using KingdomExplorer.Tools;
using KingdomExplorer.UI;
#if TUI
using KingdomExplorer.UI.TerminalGui;
#endif

// ---- Documentation helper: dotnet run -- --uml [full|table] ----
int umlAt = Array.IndexOf(args, "--uml");
if (umlAt >= 0)
{
    string mode = umlAt + 1 < args.Length ? args[umlAt + 1] : "";
    Console.WriteLine(mode == "table" ? UmlGenerator.ToMarkdownTable() : UmlGenerator.ToMermaid(mode == "full"));
    return;
}

// Composition root: the ONLY place that chooses concrete implementations.
// FuzzySharp is plugged in below. For plain matching, use NameSearchStrategy
// and NoSuggestionProvider instead.
var catalog = new OrganismCatalog(
    new FuzzySearchStrategy(),
    new FuzzySuggestionProvider());

// ---- Data: Data/organisms.csv (or: dotnet run -- --data path\to\file.csv) ----
int dataAt = Array.IndexOf(args, "--data");
string dataPath = dataAt >= 0 && dataAt + 1 < args.Length
    ? args[dataAt + 1]
    : Path.Combine(AppContext.BaseDirectory, "Data", "organisms.csv");

var notes = new List<string>();
LoadResult loaded = new CsvOrganismRepository(dataPath).Load();
notes.AddRange(loaded.Notes);

foreach (var organism in loaded.Organisms)
{
    try { catalog.Add(organism); }
    catch (ArgumentException ex)    // e.g. duplicate ID
    {
        notes.Add($"{organism.Id} skipped: {ex.Message.Split(" (Parameter")[0]}");
    }
}

if (catalog.Count == 0)
{
    notes.Add("No usable records were loaded, so the built-in sample data is being used.");
    SeedData.Load(catalog);
}
else
{
    notes.Insert(0, $"Loaded {catalog.Count} organisms from {Path.GetFileName(dataPath)}.");
}

// Default: plain console. Optional Terminal.Gui interface (build with -p:EnableTui=true, run with --tui).
IExplorerView view = new ConsoleExplorerView();
#if TUI
if (args.Contains("--tui"))
    view = new TerminalGuiExplorerView();
#endif

var controller = new ExplorerController(catalog, view, notes);

try
{
#if TUI
    if (view is TerminalGuiExplorerView tui)
        tui.RunApplication(controller.Run);   // Terminal.Gui: UI on this thread, controller on a worker thread
    else
        controller.Run();
#else
    controller.Run();
#endif
}
finally
{
    (view as IDisposable)?.Dispose();   // shuts down Terminal.Gui if it was used
}
