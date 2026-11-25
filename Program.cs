using KingdomExplorer.Catalog;
using KingdomExplorer.Data;
using KingdomExplorer.Tools;
using KingdomExplorer.UI;
#if DEVTOOLS
using KingdomExplorer.DevTools;
#endif
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
// "--tui" needs the Terminal.Gui build. In the normal build, start that build (or explain what is missing)
// instead of quietly showing the console menu.
if (args.Contains("--tui"))
{
    Environment.ExitCode = TuiLauncher.Run(args);
    return;
}
#endif

#if DEVTOOLS
// ---- Development only: dotnet run -- --export-cs  (writes Data/OrganismData.cs from the CSV) ----
if (DevCommands.TryRunExport(args))
    return;
#endif

// Composition root: the ONLY place that chooses concrete implementations.
// FuzzySharp is plugged in below. For plain matching, use NameSearchStrategy
// and NoSuggestionProvider instead.
var catalog = new OrganismCatalog(
    new FuzzySearchStrategy(),
    new FuzzySuggestionProvider());

// ---- Data: records written in Data/OrganismData.cs ----
IOrganismRepository repository = new OrganismData();
#if DEVTOOLS
repository = DevCommands.CreateRepository(args) ?? repository;   // development: use the CSV if present
#endif

LoadResult loaded = repository.Load();
var notes = new List<string>(loaded.Notes);

if (loaded.Organisms.Count == 0 && repository is not OrganismData)
{
    notes.Add("Nothing usable was loaded, so the records in OrganismData.cs are used instead.");
    loaded = new OrganismData().Load();
    notes.AddRange(loaded.Notes);
}

foreach (var organism in loaded.Organisms)
{
    try { catalog.Add(organism); }
    catch (ArgumentException ex)    // e.g. duplicate ID
    {
        notes.Add($"{organism.Id} skipped: {ex.Message.Split(" (Parameter")[0]}");
    }
}

notes.Insert(0, $"Loaded {catalog.Count} organisms from {loaded.Source}.");

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
