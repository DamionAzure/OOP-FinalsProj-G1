# Animal and Plant Kingdom Explorer

C# console application for the OOP Final Project (menu-driven explorer of Animal and Plant Kingdom organisms).

## Requirements
- .NET 8 SDK (https://dotnet.microsoft.com/download)
- Internet access on the first build (`dotnet build` restores the FuzzySharp 2.0.2 NuGet package automatically)

## Build and run
```
cd KingdomExplorerProject
dotnet build
dotnet run
```

## Menu
1. List all organisms
2. Search by common or scientific name (case-insensitive; partial or exact; partial mode also tolerates small typos using FuzzySharp, and shows "Did you mean...?" when nothing matches)
3. Filter by kingdom (Animal / Plant)
4. Filter by group (Bird, Fern, Flowering Plant, Mammal)
5. View one organism record by ID (e.g., A001, P004)
6. Exit

Empty input and invalid options show a message and return to the menu.

## Project structure
```
Models/    Organism (abstract) -> Animal / Plant (abstract) -> Mammal, Bird / FloweringPlant, Fern
Catalog/   OrganismCatalog (private list, add/list/search/filter), ISearchStrategy, ISuggestionProvider,
           NameSearchStrategy (plain), FuzzySearchStrategy + FuzzySuggestionProvider (FuzzySharp)
Data/      organisms.csv (the data), CsvOrganismRepository + OrganismFactory (loading), SeedData (built-in fallback)
Tools/     UmlGenerator (prints the class diagram and inheritance table from the real code)
UI/        IExplorerView, ConsoleExplorerView, ExplorerController
Program.cs Composition root (chooses the concrete implementations)
```

## OOP evidence (where to look)
- **Encapsulation:** `OrganismCatalog` (private `_organisms`, `Add` rejects null/duplicate IDs; `GetAll` returns a copy); `Organism` (read-only, validated properties).
- **Abstraction:** `Organism` (shared data + abstract `DescribeKeyTrait()`, `Kingdom`, `Group`).
- **Inheritance:** `Animal`/`Plant` derive from `Organism`; `Mammal`/`Bird` derive from `Animal`; `FloweringPlant`/`Fern` derive from `Plant`.
- **Polymorphism:** the catalog stores `Organism` references; `ConsoleExplorerView` and `Organism.GetDetails()` call `DescribeKeyTrait()`, and the runtime picks the subtype's override.

## Adding or editing organisms (Team 2)
Records live in `Data/organisms.csv`, one row per organism, editable in Excel or Google Sheets. No C# needed.
Each row has a `sources` column (separate several with `|`). The app lists any skipped row, with its row number and reason,
when it starts. Full instructions: `Data/HOW_TO_ADD_DATA.md`. To try another file: `dotnet run -- --data path\to\file.csv`.

## Diagrams for the report (Team 1)
```
dotnet run -- --uml          > diagram.mmd     (Mermaid class diagram: Organism hierarchy + OrganismCatalog)
dotnet run -- --uml full     > full.mmd        (every public class and interface)
dotnet run -- --uml table    > inheritance.md  (what each class inherits and adds)
```
Paste a `.mmd` file's text into https://mermaid.live to export a PNG or SVG. A new group class needs one line in `Data/OrganismFactory.cs`.

## Third-party packages
| Package | Version | Used for |
|---------|---------|----------|
| FuzzySharp | 2.0.2 | Typo-tolerant search and "Did you mean...?" (`Catalog/FuzzySearchStrategy.cs`) |
| Terminal.Gui (optional) | 1.17.1 | Optional full-screen interface (`UI/TerminalGui/`) |

Exact mode stays exact, and unrelated text still gives the normal no-match message.
To switch back to plain matching, change the two lines in `Program.cs` to `NameSearchStrategy` and `NoSuggestionProvider`.
Cite FuzzySharp in the report's References section.

## Optional: Terminal.Gui interface
The default build uses the plain console. A Terminal.Gui (v1.17.1) interface is included as an opt-in extra
(`UI/TerminalGui/TerminalGuiExplorerView.cs`). It implements `IExplorerView`, so the controller and catalog are unchanged.
```
dotnet run -p:EnableTui=true -- --tui
```
- One full-screen layout: menu (left), results (right), input bar and status line (bottom). Use the arrow keys and Enter, Tab to switch panes, Esc on the menu to exit.
- Best in Windows Terminal, macOS Terminal, or a Linux terminal. Some IDE output windows cannot run it.
- Without `--tui` (or without `-p:EnableTui=true`) the program runs the normal console version.
- Cite Terminal.Gui in the report's References section if you use it.

## Facts and sources
Seed data facts must be verified and cited in the project report's References section (e.g., IUCN Red List, Encyclopaedia Britannica, Kew Plants of the World Online).
