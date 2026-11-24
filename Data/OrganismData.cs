using KingdomExplorer.Models;

namespace KingdomExplorer.Data;

/// <summary>
/// ALL ORGANISM RECORDS LIVE HERE. Each record is one block starting with "() => new ...(".
///
/// TO ADD AN ORGANISM: copy a whole block of the same type, paste it right after that block
/// (before the closing "};"), and change the values. Keep the quotes and commas. The app lists any
/// entry that was skipped (with its number and the reason) when it starts.
///
/// Every type needs:  id (unique), commonName, scientificName, habitat, distinguishingTrait, sources.
/// Extra fields:      Mammal diet, bodyCovering | Bird diet, canFly | FloweringPlant flowerColor |
///                    Fern frondDescription
///
/// SOURCES: replace each "SUGGESTED - verify: ..." text with the real citation after you have
/// opened the source and confirmed the facts.
/// </summary>
public sealed class OrganismData : IOrganismRepository
{
    // Each entry is wrapped in "() =>" so one bad entry can be skipped and reported
    // without stopping the others.
    private static readonly List<Func<Organism>> Records = new()
    {
        // ===================== ANIMALS =====================

        // ----- Mammal -----
        () => new Mammal(
            id: "A001",
            commonName: "Lion",
            scientificName: "Panthera leo",
            habitat: "Savannas and grasslands of sub-Saharan Africa (small population in Gir Forest, India)",
            distinguishingTrait: "The only big cat that lives in social groups called prides.",
            diet: "Carnivore",
            bodyCovering: "Short tawny fur",
            sources: new[] { "SUGGESTED - verify: IUCN Red List: Panthera leo", "SUGGESTED - verify: Encyclopaedia Britannica: lion" }),

        () => new Mammal(
            id: "A002",
            commonName: "Philippine Tarsier",
            scientificName: "Carlito syrichta",
            habitat: "Tropical forests of the southern Philippines, such as on Bohol",
            distinguishingTrait: "Tiny nocturnal primate with very large eyes.",
            diet: "Carnivore (insects and small animals)",
            bodyCovering: "Soft fur",
            sources: new[] { "SUGGESTED - verify: IUCN Red List: Carlito syrichta", "SUGGESTED - verify: Encyclopaedia Britannica: tarsier" }),

        () => new Mammal(
            id: "A003",
            commonName: "Blue Whale",
            scientificName: "Balaenoptera musculus",
            habitat: "Open oceans worldwide",
            distinguishingTrait: "The largest animal known to have ever lived.",
            diet: "Filter feeder (mainly krill)",
            bodyCovering: "Smooth skin with very little hair",
            sources: new[] { "SUGGESTED - verify: IUCN Red List: Balaenoptera musculus", "SUGGESTED - verify: Encyclopaedia Britannica: blue whale" }),

        // ----- Bird -----
        () => new Bird(
            id: "A004",
            commonName: "Philippine Eagle",
            scientificName: "Pithecophaga jefferyi",
            habitat: "Rainforests of Luzon, Samar, Leyte, and Mindanao",
            distinguishingTrait: "One of the world's largest eagles, with a shaggy crest; the national bird of the Philippines.",
            diet: "Carnivore (monkeys, flying lemurs, birds, reptiles)",
            canFly: true,
            sources: new[] { "SUGGESTED - verify: IUCN Red List: Pithecophaga jefferyi", "SUGGESTED - verify: Philippine Eagle Foundation" }),

        () => new Bird(
            id: "A005",
            commonName: "Emperor Penguin",
            scientificName: "Aptenodytes forsteri",
            habitat: "Antarctic sea ice and the surrounding ocean",
            distinguishingTrait: "The largest penguin; males incubate the egg through the Antarctic winter.",
            diet: "Carnivore (fish, squid, and krill)",
            canFly: false,
            sources: new[] { "SUGGESTED - verify: IUCN Red List: Aptenodytes forsteri", "SUGGESTED - verify: Encyclopaedia Britannica: emperor penguin" }),

        () => new Bird(
            id: "A006",
            commonName: "Barn Owl",
            scientificName: "Tyto alba",
            habitat: "Open country, farmland, and grasslands on every continent except Antarctica",
            distinguishingTrait: "Heart-shaped white face and near-silent flight.",
            diet: "Carnivore (mostly small mammals)",
            canFly: true,
            sources: new[] { "SUGGESTED - verify: IUCN Red List: Tyto alba", "SUGGESTED - verify: Encyclopaedia Britannica: barn owl" }),


        // ===================== PLANTS =====================

        // ----- Flowering Plant -----
        () => new FloweringPlant(
            id: "P001",
            commonName: "Sunflower",
            scientificName: "Helianthus annuus",
            habitat: "Open, sunny fields and farmland; native to North America",
            distinguishingTrait: "Young flower heads follow the sun across the sky.",
            flowerColor: "Yellow petals with a brown center",
            sources: new[] { "SUGGESTED - verify: Plants of the World Online (Kew): Helianthus annuus", "SUGGESTED - verify: Encyclopaedia Britannica: sunflower" }),

        () => new FloweringPlant(
            id: "P002",
            commonName: "Sampaguita",
            scientificName: "Jasminum sambac",
            habitat: "Warm tropical lowlands and gardens; widely grown in the Philippines",
            distinguishingTrait: "Very fragrant flowers; the national flower of the Philippines.",
            flowerColor: "White",
            sources: new[] { "SUGGESTED - verify: Plants of the World Online (Kew): Jasminum sambac", "SUGGESTED - verify: Encyclopaedia Britannica: jasmine" }),

        () => new FloweringPlant(
            id: "P003",
            commonName: "Narra",
            scientificName: "Pterocarpus indicus",
            habitat: "Tropical lowland forests of Southeast Asia and the western Pacific",
            distinguishingTrait: "Large tree valued for its hardwood; the national tree of the Philippines.",
            flowerColor: "Yellow",
            sources: new[] { "SUGGESTED - verify: Plants of the World Online (Kew): Pterocarpus indicus", "SUGGESTED - verify: IUCN Red List: Pterocarpus indicus" }),

        // ----- Fern -----
        () => new Fern(
            id: "P004",
            commonName: "Bracken",
            scientificName: "Pteridium aquilinum",
            habitat: "Open woodlands, heaths, and hillsides across much of the world",
            distinguishingTrait: "One of the most widespread ferns on Earth.",
            frondDescription: "Large, triangular, and much-divided",
            sources: new[] { "SUGGESTED - verify: Plants of the World Online (Kew): Pteridium aquilinum", "SUGGESTED - verify: Encyclopaedia Britannica: bracken" }),

        () => new Fern(
            id: "P005",
            commonName: "Bird's-nest Fern",
            scientificName: "Asplenium nidus",
            habitat: "Tropical forests, often growing on tree trunks",
            distinguishingTrait: "Fronds form a nest-like rosette that catches fallen leaves.",
            frondDescription: "Undivided, glossy, and arranged in a rosette",
            sources: new[] { "SUGGESTED - verify: Plants of the World Online (Kew): Asplenium nidus", "SUGGESTED - verify: Encyclopaedia Britannica: fern" }),

        () => new Fern(
            id: "P006",
            commonName: "Maidenhair Fern",
            scientificName: "Adiantum capillus-veneris",
            habitat: "Moist, shaded rocks, cliffs, and damp walls",
            distinguishingTrait: "Thin dark stalks hold delicate fan-shaped leaflets.",
            frondDescription: "Delicate, fan-shaped leaflets",
            sources: new[] { "SUGGESTED - verify: Plants of the World Online (Kew): Adiantum capillus-veneris", "SUGGESTED - verify: Encyclopaedia Britannica: maidenhair fern" }),

    };

    public LoadResult Load()
    {
        var result = new LoadResult { Source = "OrganismData.cs" };

        int entry = 0;
        foreach (Func<Organism> create in Records)
        {
            entry++;
            try
            {
                result.Organisms.Add(create());
            }
            catch (ArgumentException ex)    // thrown by the organism classes' validation
            {
                int p = ex.Message.IndexOf(" (Parameter", StringComparison.Ordinal);
                result.Notes.Add($"Entry #{entry} skipped: {(p >= 0 ? ex.Message[..p] : ex.Message)}");
            }
        }

        result.AddSourceNotes();
        return result;
    }
}
