# Animal and Plant Kingdom Explorer

C# console application for the OOP Final Project (menu-driven explorer of Animal and Plant Kingdom organisms).

## Requirements
- .NET 8 SDK (https://dotnet.microsoft.com/download) - no third-party packages are used.

## Build and run
```
cd KingdomExplorerProject
dotnet build
dotnet run
```

## Menu
1. List all organisms
2. Search by common or scientific name (case-insensitive; partial or exact)
3. Filter by kingdom (Animal / Plant)
4. Filter by group (Bird, Fern, Flowering Plant, Mammal)
5. View one organism record by ID (e.g., A001, P004)
6. Exit

Empty input and invalid options show a message and return to the menu.

## Project structure
```
Models/    Organism (abstract) -> Animal / Plant (abstract) -> Mammal, Bird / FloweringPlant, Fern
Catalog/   OrganismCatalog (private list, add/list/search/filter), ISearchStrategy, ISuggestionProvider
Data/      SeedData (12 starter records: 6 animals, 6 plants)
UI/        IExplorerView, ConsoleExplorerView, ExplorerController
Program.cs Composition root (chooses the concrete implementations)
```

## OOP evidence (where to look)
- **Encapsulation:** `OrganismCatalog` (private `_organisms`, `Add` rejects null/duplicate IDs; `GetAll` returns a copy); `Organism` (read-only, validated properties).
- **Abstraction:** `Organism` (shared data + abstract `DescribeKeyTrait()`, `Kingdom`, `Group`).
- **Inheritance:** `Animal`/`Plant` derive from `Organism`; `Mammal`/`Bird` derive from `Animal`; `FloweringPlant`/`Fern` derive from `Plant`.
- **Polymorphism:** the catalog stores `Organism` references; `ConsoleExplorerView` and `Organism.GetDetails()` call `DescribeKeyTrait()`, and the runtime picks the subtype's override.

## Future extension points (only if the instructor approves third-party packages)
The design keeps these swappable without touching the OOP core:
| Want | Add a class implementing | Library | Change in Program.cs |
|------|--------------------------|---------|----------------------|
| Fuzzy name search | `ISearchStrategy` | FuzzySharp | pass it to `OrganismCatalog` |
| "Did you mean...?" | `ISuggestionProvider` | FuzzySharp | pass it to `OrganismCatalog` |
| Terminal UI | `IExplorerView` | Terminal.Gui | replace `ConsoleExplorerView` |

If you add a package: `dotnet add package <name>`, then document the name/version here and cite it in the report.

## Facts and sources
Seed data facts must be verified and cited in the project report's References section (e.g., IUCN Red List, Encyclopaedia Britannica, Kew Plants of the World Online).
