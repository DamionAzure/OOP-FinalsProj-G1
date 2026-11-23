using KingdomExplorer.Catalog;
using KingdomExplorer.Models;

namespace KingdomExplorer.Data;

/// <summary>
/// Curated starter records. IMPORTANT: verify each fact against your cited sources
/// (e.g., IUCN Red List, Encyclopaedia Britannica, Kew Plants of the World Online)
/// and list them in the report's References section.
/// </summary>
public static class SeedData
{
    public static void Load(OrganismCatalog catalog)
    {
        // ----- Animals: Mammals -----
        catalog.Add(new Mammal("A001", "Lion", "Panthera leo",
            "Savannas and grasslands of sub-Saharan Africa (small population in Gir Forest, India)",
            "The only big cat that lives in social groups called prides.",
            "Carnivore", "Short tawny fur"));
        catalog.Add(new Mammal("A002", "Philippine Tarsier", "Carlito syrichta",
            "Tropical forests of the southern Philippines, such as on Bohol",
            "Tiny nocturnal primate with very large eyes.",
            "Carnivore (insects and small animals)", "Soft fur"));
        catalog.Add(new Mammal("A003", "Blue Whale", "Balaenoptera musculus",
            "Open oceans worldwide",
            "The largest animal known to have ever lived.",
            "Filter feeder (mainly krill)", "Smooth skin with very little hair"));

        // ----- Animals: Birds -----
        catalog.Add(new Bird("A004", "Philippine Eagle", "Pithecophaga jefferyi",
            "Rainforests of Luzon, Samar, Leyte, and Mindanao",
            "One of the world's largest eagles, with a shaggy crest; the national bird of the Philippines.",
            "Carnivore (monkeys, flying lemurs, birds, reptiles)", true));
        catalog.Add(new Bird("A005", "Emperor Penguin", "Aptenodytes forsteri",
            "Antarctic sea ice and the surrounding ocean",
            "The largest penguin; males incubate the egg through the Antarctic winter.",
            "Carnivore (fish, squid, and krill)", false));
        catalog.Add(new Bird("A006", "Barn Owl", "Tyto alba",
            "Open country, farmland, and grasslands on every continent except Antarctica",
            "Heart-shaped white face and near-silent flight.",
            "Carnivore (mostly small mammals)", true));

        // ----- Plants: Flowering plants -----
        catalog.Add(new FloweringPlant("P001", "Sunflower", "Helianthus annuus",
            "Open, sunny fields and farmland; native to North America",
            "Young flower heads follow the sun across the sky.",
            "Yellow petals with a brown center"));
        catalog.Add(new FloweringPlant("P002", "Sampaguita", "Jasminum sambac",
            "Warm tropical lowlands and gardens; widely grown in the Philippines",
            "Very fragrant flowers; the national flower of the Philippines.",
            "White"));
        catalog.Add(new FloweringPlant("P003", "Narra", "Pterocarpus indicus",
            "Tropical lowland forests of Southeast Asia and the western Pacific",
            "Large tree valued for its hardwood; the national tree of the Philippines.",
            "Yellow"));

        // ----- Plants: Ferns -----
        catalog.Add(new Fern("P004", "Bracken", "Pteridium aquilinum",
            "Open woodlands, heaths, and hillsides across much of the world",
            "One of the most widespread ferns on Earth.",
            "Large, triangular, and much-divided"));
        catalog.Add(new Fern("P005", "Bird's-nest Fern", "Asplenium nidus",
            "Tropical forests, often growing on tree trunks",
            "Fronds form a nest-like rosette that catches fallen leaves.",
            "Undivided, glossy, and arranged in a rosette"));
        catalog.Add(new Fern("P006", "Maidenhair Fern", "Adiantum capillus-veneris",
            "Moist, shaded rocks, cliffs, and damp walls",
            "Thin dark stalks hold delicate fan-shaped leaflets.",
            "Delicate, fan-shaped leaflets"));
    }
}
