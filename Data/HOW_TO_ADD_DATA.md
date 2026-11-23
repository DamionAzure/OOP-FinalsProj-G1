# How to add or edit organisms (Team 2)

All organism records live in **`Data/organisms.csv`**. You do not need to touch any C# code.

1. Open `organisms.csv` in Excel or Google Sheets. One row = one organism.
2. Fill the columns that apply to the organism's `type`:

| type | extra columns to fill |
|------|-----------------------|
| `Mammal` | `diet`, `bodyCovering` |
| `Bird` | `diet`, `canFly` (true or false) |
| `FloweringPlant` | `flowerColor` |
| `Fern` | `frondDescription` |

   Always fill: `type`, `id` (unique, e.g. A007 or P007), `commonName`, `scientificName`, `habitat`, `distinguishingTrait`.
   Leave the columns that do not apply to that type empty.
3. **`sources`**: put where each fact came from. For several sources, separate them with a pipe: `IUCN Red List: Panthera leo | Britannica: lion`.
   Records whose source still starts with `SUGGESTED` are placeholders. Open the source, confirm the facts, then replace the text.
4. Save as **CSV UTF-8** (Excel: File > Save As > "CSV UTF-8"; Google Sheets: File > Download > CSV).
5. Run the app. The first messages tell you what loaded and list any row that was skipped, with its row number and the problem.

To check a different file without replacing this one: `dotnet run -- --data path\to\file.csv`

Adding a brand-new group (for example Reptile) needs a new class in `Models/` and one line in `Data/OrganismFactory.cs`. That part belongs to Team 1.
