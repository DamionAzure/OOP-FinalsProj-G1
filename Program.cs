using KingdomExplorer.Catalog;
using KingdomExplorer.Data;
using KingdomExplorer.UI;

// Composition root: the ONLY place that chooses concrete implementations.
//
// If your instructor approves third-party packages, swap these lines:
//   search:      new NameSearchStrategy()   ->  new FuzzySearchStrategy()      (FuzzySharp)
//   suggestions: new NoSuggestionProvider() ->  new FuzzySuggestionProvider()  (FuzzySharp)
//   view:        new ConsoleExplorerView()  ->  new TerminalGuiExplorerView()  (Terminal.Gui)

var catalog = new OrganismCatalog(
    new NameSearchStrategy(),
    new NoSuggestionProvider());

SeedData.Load(catalog);

IExplorerView view = new ConsoleExplorerView();
new ExplorerController(catalog, view).Run();
