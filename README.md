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
Data/      OrganismData (the organism records, written in C#), IOrganismRepository + LoadResult
           organisms.csv (development only: the spreadsheet teammates edit)
DevTools/  (development only) CSV loading and the CSV-to-C# converter. Deleted before submission.
Tools/     UmlGenerator (prints the class diagram and inheritance table from the real code)
UI/        IExplorerView, ConsoleExplorerView, ExplorerController
Images/    Organism pictures for the Terminal.Gui interface (optional)
Audio/     Animal sound effects (.wav) for the Terminal.Gui interface (optional)
Sound/     AnimalSoundLibrary (finds + randomizes sounds), AudioPlayer, PlaybackCooldown (2 s cooldown)
Program.cs Composition root (chooses the concrete implementations)
```

## OOP evidence (where to look)
- **Encapsulation:** `OrganismCatalog` (private `_organisms`, `Add` rejects null/duplicate IDs; `GetAll` returns a copy); `Organism` (read-only, validated properties).
- **Abstraction:** `Organism` (shared data + abstract `DescribeKeyTrait()`, `Kingdom`, `Group`).
- **Inheritance:** `Animal`/`Plant` derive from `Organism`; `Mammal`/`Bird` derive from `Animal`; `FloweringPlant`/`Fern` derive from `Plant`.
- **Polymorphism:** the catalog stores `Organism` references; `ConsoleExplorerView` and `Organism.GetDetails()` call `DescribeKeyTrait()`, and the runtime picks the subtype's override.

## Adding or editing organisms (Team 2) - development workflow
While the team is working, organisms are edited in `Data/organisms.csv` (Excel or Google Sheets, one row per organism,
sources in the `sources` column separated by `|`, saved as CSV UTF-8). `dotnet run` loads that file and reports any bad row.
The column names match the constructor parameters of the classes in `Models/`, and `type` is the class name.

## Switching to pure C# before submitting
The submitted project uses no data files. When the data is final:
1. `dotnet run -- --export-cs` writes `Data/OrganismData.cs` (all records as C# code) from the CSV.
2. Delete the `DevTools` folder and `Data/organisms.csv`. The project then builds with the C# data only.
3. `dotnet build` and `dotnet run` once more. The startup line should say "Loaded N organisms from OrganismData.cs".
`MakeSubmission.bat` does steps 2 and 3 on a clean copy and zips it.

## Diagrams for the report (Team 1)
```
dotnet run -- --uml          > diagram.mmd     (Mermaid class diagram: Organism hierarchy + OrganismCatalog)
dotnet run -- --uml full     > full.mmd        (every public class and interface)
dotnet run -- --uml table    > inheritance.md  (what each class inherits and adds)
```
Paste a `.mmd` file's text into https://mermaid.live to export a PNG or SVG. A new group needs only its new class in `Models/` plus a CSV column named after its extra constructor parameter(s).

## Third-party packages
| Package | Version | Used for |
|---------|---------|----------|
| FuzzySharp | 2.0.2 | Typo-tolerant search and "Did you mean...?" (`Catalog/FuzzySearchStrategy.cs`) |
| Terminal.Gui (optional) | 2.6.0 | Optional full-screen interface with pictures (`UI/TerminalGui/`); needs .NET 10 |
| SixLabors.ImageSharp (optional) | 3.1.x | Decodes the organism pictures for the Terminal.Gui interface (Six Labors Split License: free for open-source and non-commercial use) |

Exact mode stays exact, and unrelated text still gives the normal no-match message.
To switch back to plain matching, change the two lines in `Program.cs` to `NameSearchStrategy` and `NoSuggestionProvider`.
Cite FuzzySharp in the report's References section.

## Optional: Terminal.Gui interface (with pictures)
The default build uses the plain console (.NET 8). A Terminal.Gui **v2** (2.6.0) interface is included as an opt-in extra
(`UI/TerminalGui/`). It implements `IExplorerView`, so the controller and catalog are unchanged.
Terminal.Gui v2 targets **.NET 10 only**, so this mode needs the .NET 10 SDK; the project switches itself to `net10.0` for it.
```
dotnet run -p:EnableTui=true -- --tui
```
Plain `dotnet run -- --tui` also works: the normal build starts the TUI build for you (or tells you what is missing, e.g. "the .NET 10 SDK was not found").
Check your SDKs with `dotnet --list-sdks`; one of them must start with `10.`.
- One full-screen layout: menu (left), results (right), input bar and status line (bottom). Arrow keys and Enter, Tab to switch pane, Esc on the menu to exit.
- **Pictures:** the results pane lists the organisms; the highlighted one (or the one you look up by ID) is shown with its picture and full record. Press Tab (Shift+Tab goes back) to move between the menu, list, picture and input bar, then Up/Down in the list. Option 5 (view by ID) shows the picture too.
- Put pictures in `Images/`, named by ID or common name (`Images/A001.png`, `Images/Lion.jpg`). PNG, JPEG, GIF, BMP and WebP work. No file means a short "no picture yet" note. See `Images/README.txt`.
- Pictures use the Kitty or Sixel graphics protocol when the terminal supports it (e.g. Windows Terminal 1.22+, WezTerm, kitty, iTerm2) and fall back to colored cells in any other terminal. F4 forces the colored-cell mode.
- F2 changes the theme, F3 the layout.
- Best in Windows Terminal, macOS Terminal, or a Linux terminal. Some IDE output windows cannot run it.
- Without `--tui` (or without `-p:EnableTui=true`) the program runs the normal console version on .NET 8.
- Cite Terminal.Gui and SixLabors.ImageSharp in the report's References section if you use them.

## Facts and sources
Seed data facts must be verified and cited in the project report's References section (e.g., IUCN Red List, Encyclopaedia Britannica, Kew Plants of the World Online).

## Animal sounds (Terminal.Gui interface)
When an animal is shown, the results pane has a **Play Sound (F5)** button that plays a random sound. Put `.wav` files in `Audio/`
(see `Audio/README.txt`): `A001.wav`, `A001-1.wav`, `A001-2.wav` ... are the ID sounds and `Lion.wav` is the common-name sound.
It picks one ID sound at random, or the common-name sound with a 25% chance (falling back to ID sounds if there is no
common-name file). After each play the button is disabled for 2 seconds with a countdown shown.
