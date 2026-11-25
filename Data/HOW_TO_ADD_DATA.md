# How to add or edit organisms

All organism records are written in C# in **`Data/OrganismData.cs`**, inside the `Records` list. One entry = one organism.
There is no CSV or other data file.

1. Copy an existing entry of the same group and paste it at the end of the list (before the closing `};`).
2. Change the values. Always fill these: `id` (unique, e.g. A031 or P031), `commonName`, `scientificName`, `habitat`, `distinguishingTrait`.
   Then fill the arguments that group needs:

| Class | Extra arguments |
|-------|-----------------|
| `Mammal` | `diet`, `bodyCovering` |
| `Bird` | `diet`, `canFly` (`true` or `false`) |
| `FloweringPlant` | `flowerColor` |
| `Fern` | `frondDescription` |

   Plants do not take a reproduction type: `Fern` and `FloweringPlant` set it themselves.
3. **`sources`**: list where each fact came from, for example
   `sources: new[] { "IUCN Red List - Panthera leo", "Encyclopaedia Britannica - Lion" }`.
   A source that starts with `SUGGESTED` is a placeholder. Open the source, confirm the facts, then replace the text
   (and remove the word `SUGGESTED`). The app lists unchecked sources in its startup messages.
4. Run the app. The first messages tell you how many organisms loaded and name any entry that was skipped (for example a blank
   required field or a repeated ID).

Before adding a record, search the list for its `scientificName` so the same species is not entered twice
(the catalog only rejects repeated IDs).

Adding a brand-new group (for example Reptile) needs a new class in `Models/` and entries that use it.
