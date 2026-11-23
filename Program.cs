using KingdomExplorer.Catalog;
using KingdomExplorer.Data;
using KingdomExplorer.UI;
#if TUI
using KingdomExplorer.UI.TerminalGui;
#endif

// Composition root: the ONLY place that chooses concrete implementations.
// FuzzySharp is plugged in below. For plain matching, use NameSearchStrategy
// and NoSuggestionProvider instead.

var catalog = new OrganismCatalog(
    new FuzzySearchStrategy(),
    new FuzzySuggestionProvider());

SeedData.Load(catalog);

// Default: plain console. Optional Terminal.Gui interface (build with -p:EnableTui=true, run with --tui).
IExplorerView view = new ConsoleExplorerView();
#if TUI
if (args.Contains("--tui"))
    view = new TerminalGuiExplorerView();
#endif

var controller = new ExplorerController(catalog, view);

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
