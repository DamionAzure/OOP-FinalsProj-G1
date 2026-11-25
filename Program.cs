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

#if !TUI
if (args.Contains("--tui"))
{
    Environment.ExitCode = TuiLauncher.Run(args);
    return;
}
#endif

// Composition root: hardcoded C# data only (CSV removed).
var catalog = new OrganismCatalog(
    new FuzzySearchStrategy(),
    new FuzzySuggestionProvider());

IOrganismRepository repository = new OrganismData();
LoadResult loaded = repository.Load();
var notes = new List<string>(loaded.Notes);

foreach (var organism in loaded.Organisms)
{
    try
    {
        catalog.Add(organism);
    }
    catch (ArgumentException ex)
    {
        notes.Add($"{organism.Id} skipped: {ex.Message.Split(" (Parameter")[0]}");
    }
}

notes.Insert(0, $"Loaded {catalog.Count} organisms from {loaded.Source}.");

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
        tui.RunApplication(controller.Run);
    else
        controller.Run();
#else
    controller.Run();
#endif
}
finally
{
    (view as IDisposable)?.Dispose();
}
