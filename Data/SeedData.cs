using KingdomExplorer.Catalog;
using KingdomExplorer.Models;

namespace KingdomExplorer.Data;

/// <summary>
/// Hardcoded starter loader that populates an OrganismCatalog directly from OrganismData.
/// </summary>
public static class SeedData
{
    public static void Load(OrganismCatalog catalog)
    {
        LoadResult loaded = new OrganismData().Load();
        foreach (Organism organism in loaded.Organisms)
        {
            catalog.Add(organism);
        }
    }
}
