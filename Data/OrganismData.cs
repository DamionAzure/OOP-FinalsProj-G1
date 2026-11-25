using KingdomExplorer.Models;

namespace KingdomExplorer.Data;

/// <summary>
/// Hardcoded organism catalog records in pure C# (no CSV dependency).
/// Every record uses one of the concrete subtypes: Mammal, Bird, FloweringPlant, or Fern.
/// </summary>
public sealed class OrganismData : IOrganismRepository
{
    private static readonly List<Func<Organism>> Records = new()
    {
        () => new Mammal(
            id: "A001",
            commonName: "Lion",
            scientificName: "Panthera leo",
            habitat: "Savannas and grasslands of sub-Saharan Africa (small population in Gir Forest, India)",
            distinguishingTrait: "The only big cat that lives in social groups called prides.",
            diet: "Carnivore",
            bodyCovering: "Short tawny fur",
            sources: new[] { "SUGGESTED: IUCN Red List - Panthera leo" }),

        () => new Mammal(
            id: "A002",
            commonName: "Philippine Tarsier",
            scientificName: "Carlito syrichta",
            habitat: "Tropical forests of the southern Philippines, such as on Bohol",
            distinguishingTrait: "Tiny nocturnal primate with very large eyes.",
            diet: "Carnivore (insects and small animals)",
            bodyCovering: "Soft fur",
            sources: new[] { "SUGGESTED: IUCN Red List - Carlito syrichta" }),

        () => new Mammal(
            id: "A003",
            commonName: "Blue Whale",
            scientificName: "Balaenoptera musculus",
            habitat: "Open oceans worldwide",
            distinguishingTrait: "The largest animal known to have ever lived.",
            diet: "Filter feeder (mainly krill)",
            bodyCovering: "Smooth skin with very little hair",
            sources: new[] { "SUGGESTED: IUCN Red List - Balaenoptera musculus" }),

        () => new Bird(
            id: "A004",
            commonName: "Philippine Eagle",
            scientificName: "Pithecophaga jefferyi",
            habitat: "Rainforests of Luzon, Samar, Leyte, and Mindanao",
            distinguishingTrait: "One of the world's largest eagles, with a shaggy crest; the national bird of the Philippines.",
            diet: "Carnivore (monkeys, flying lemurs, birds, reptiles)",
            canFly: true,
            sources: new[] { "SUGGESTED: IUCN Red List - Pithecophaga jefferyi" }),

        () => new Bird(
            id: "A005",
            commonName: "Emperor Penguin",
            scientificName: "Aptenodytes forsteri",
            habitat: "Antarctic sea ice and the surrounding ocean",
            distinguishingTrait: "The largest penguin; males incubate the egg through the Antarctic winter.",
            diet: "Carnivore (fish, squid, and krill)",
            canFly: false,
            sources: new[] { "SUGGESTED: IUCN Red List - Aptenodytes forsteri" }),

        () => new Bird(
            id: "A006",
            commonName: "Barn Owl",
            scientificName: "Tyto alba",
            habitat: "Open country, farmland, and grasslands on every continent except Antarctica",
            distinguishingTrait: "Heart-shaped white face and near-silent flight.",
            diet: "Carnivore (mostly small mammals)",
            canFly: true,
            sources: new[] { "SUGGESTED: IUCN Red List - Tyto alba" }),

        () => new FloweringPlant(
            id: "P001",
            commonName: "Sunflower",
            scientificName: "Helianthus annuus",
            habitat: "Open, sunny fields and farmland; native to North America",
            distinguishingTrait: "Young flower heads follow the sun across the sky.",
            flowerColor: "Yellow petals with a brown center",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Helianthus annuus" }),

        () => new FloweringPlant(
            id: "P002",
            commonName: "Sampaguita",
            scientificName: "Jasminum sambac",
            habitat: "Warm tropical lowlands and gardens; widely grown in the Philippines",
            distinguishingTrait: "Very fragrant flowers; the national flower of the Philippines.",
            flowerColor: "White",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Jasminum sambac" }),

        () => new FloweringPlant(
            id: "P003",
            commonName: "Narra",
            scientificName: "Pterocarpus indicus",
            habitat: "Tropical lowland forests of Southeast Asia and the western Pacific",
            distinguishingTrait: "Large tree valued for its hardwood; the national tree of the Philippines.",
            flowerColor: "Yellow",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Pterocarpus indicus" }),

        () => new Fern(
            id: "P004",
            commonName: "Bracken",
            scientificName: "Pteridium aquilinum",
            habitat: "Open woodlands, heaths, and hillsides across much of the world",
            distinguishingTrait: "One of the most widespread ferns on Earth.",
            frondDescription: "Large, triangular, and much-divided",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Pteridium aquilinum" }),

        () => new Fern(
            id: "P005",
            commonName: "Bird's-nest Fern",
            scientificName: "Asplenium nidus",
            habitat: "Tropical forests, often growing on tree trunks",
            distinguishingTrait: "Fronds form a nest-like rosette that catches fallen leaves.",
            frondDescription: "Undivided, glossy, and arranged in a rosette",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Asplenium nidus" }),

        () => new Fern(
            id: "P006",
            commonName: "Maidenhair Fern",
            scientificName: "Adiantum capillus-veneris",
            habitat: "Moist, shaded rocks, cliffs, and damp walls",
            distinguishingTrait: "Thin dark stalks hold delicate fan-shaped leaflets.",
            frondDescription: "Delicate, fan-shaped leaflets",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Adiantum capillus-veneris" }),

        () => new Mammal(
            id: "A007",
            commonName: "Tiger",
            scientificName: "Panthera tigris",
            habitat: "Tropical forests, grasslands, and mangrove swamps",
            distinguishingTrait: "Orange coat with black stripes; powerful solitary predator",
            diet: "Carnivore (deer, wild pigs, and other large prey)",
            bodyCovering: "Fur",
            sources: new[] { "SUGGESTED: IUCN Red List - Panthera tigris" }),

        () => new Mammal(
            id: "A008",
            commonName: "Red Fox",
            scientificName: "Vulpes vulpes",
            habitat: "Forests, grasslands, mountains, and urban areas",
            distinguishingTrait: "Pointed ears and long bushy tail",
            diet: "Omnivore (small mammals, birds, insects, and fruits)",
            bodyCovering: "Fur",
            sources: new[] { "SUGGESTED: IUCN Red List - Vulpes vulpes" }),

        () => new Mammal(
            id: "A009",
            commonName: "Giant Panda",
            scientificName: "Ailuropoda melanoleuca",
            habitat: "Temperate mountain forests of central China",
            distinguishingTrait: "Black-and-white markings and enlarged wrist bone used to grip bamboo",
            diet: "Herbivore (mostly bamboo)",
            bodyCovering: "Fur",
            sources: new[] { "SUGGESTED: IUCN Red List - Ailuropoda melanoleuca" }),

        () => new Bird(
            id: "A010",
            commonName: "Bald Eagle",
            scientificName: "Haliaeetus leucocephalus",
            habitat: "Coasts, rivers, and large lakes across North America",
            distinguishingTrait: "White head and tail contrasting with a dark brown body; national bird of the United States.",
            diet: "Carnivore (mainly fish, plus waterfowl and small mammals)",
            canFly: true,
            sources: new[] { "SUGGESTED: IUCN Red List - Haliaeetus leucocephalus" }),

        () => new Mammal(
            id: "A011",
            commonName: "Red Kangaroo",
            scientificName: "Osphranter rufus",
            habitat: "Arid and semi-arid grasslands and scrublands of Australia",
            distinguishingTrait: "Strong hind legs and a pouch for carrying young",
            diet: "Herbivore (grasses and shrubs)",
            bodyCovering: "Fur",
            sources: new[] { "SUGGESTED: IUCN Red List - Osphranter rufus" }),

        () => new Bird(
            id: "A012",
            commonName: "Atlantic Puffin",
            scientificName: "Fratercula arctica",
            habitat: "North Atlantic coastal cliffs and open ocean",
            distinguishingTrait: "Brightly colored triangular bill during breeding season and black-and-white plumage.",
            diet: "Carnivore (small marine fish such as sand eels and herring)",
            canFly: true,
            sources: new[] { "SUGGESTED: IUCN Red List - Fratercula arctica" }),

        () => new Bird(
            id: "A013",
            commonName: "Peregrine Falcon",
            scientificName: "Falco peregrinus",
            habitat: "Coastal cliffs, mountains, wetlands, and cities worldwide",
            distinguishingTrait: "Extremely fast diving flight and dark facial markings",
            diet: "Carnivore (mainly birds)",
            canFly: true,
            sources: new[] { "SUGGESTED: IUCN Red List - Falco peregrinus" }),

        () => new Bird(
            id: "A014",
            commonName: "Ruby-throated Hummingbird",
            scientificName: "Archilochus colubris",
            habitat: "Open woodlands, meadows, and gardens in eastern North America; winters in Central America",
            distinguishingTrait: "Iridescent green back, metallic red throat in males, and ability to hover and fly backward.",
            diet: "Omnivore (flower nectar and small insects)",
            canFly: true,
            sources: new[] { "SUGGESTED: IUCN Red List - Archilochus colubris" }),

        () => new Bird(
            id: "A015",
            commonName: "Greater Flamingo",
            scientificName: "Phoenicopterus roseus",
            habitat: "Shallow coastal lagoons, mudflats, and salt lakes across Africa, southern Europe, and Asia",
            distinguishingTrait: "Long legs, S-curved neck, downward-bent bill, and pink plumage from carotenoid-rich food.",
            diet: "Omnivore (brine shrimp, blue-green algae, and small invertebrates)",
            canFly: true,
            sources: new[] { "SUGGESTED: IUCN Red List - Phoenicopterus roseus" }),

        () => new Mammal(
            id: "A016",
            commonName: "Atlantic Bottlenose Dolphin",
            scientificName: "Tursiops truncatus",
            habitat: "Temperate and tropical oceans and coastal waters",
            distinguishingTrait: "Curved mouth and uses echolocation to navigate and hunt",
            diet: "Carnivore (fish, squid, and crustaceans)",
            bodyCovering: "Smooth skin",
            sources: new[] { "SUGGESTED: IUCN Red List - Tursiops truncatus" }),

        () => new Bird(
            id: "A017",
            commonName: "Snowy Owl",
            scientificName: "Bubo scandiacus",
            habitat: "Arctic tundra and open northern landscapes",
            distinguishingTrait: "White plumage with dark markings and yellow eyes",
            diet: "Carnivore (lemmings and other small animals)",
            canFly: true,
            sources: new[] { "SUGGESTED: IUCN Red List - Bubo scandiacus" }),

        () => new Bird(
            id: "A018",
            commonName: "Cassowary",
            scientificName: "Casuarius casuarius",
            habitat: "Tropical rainforests of New Guinea and northeastern Australia",
            distinguishingTrait: "Large flightless bird with a prominent helmet-like casque and dagger-like inner toe claw.",
            diet: "Herbivore (mainly fallen rainforest fruits, occasionally fungi and insects)",
            canFly: false,
            sources: new[] { "SUGGESTED: IUCN Red List - Casuarius casuarius" }),

        () => new Mammal(
            id: "A019",
            commonName: "African Elephant",
            scientificName: "Loxodonta africana",
            habitat: "Savannas, forests, and grasslands of sub-Saharan Africa",
            distinguishingTrait: "Large ears, trunk, and tusks",
            diet: "Herbivore (grasses, leaves, bark, and fruit)",
            bodyCovering: "Sparse hair; thick skin",
            sources: new[] { "SUGGESTED: IUCN Red List - Loxodonta africana" }),

        () => new Mammal(
            id: "A020",
            commonName: "Polar Bear",
            scientificName: "Ursus maritimus",
            habitat: "Arctic sea ice and nearby coastal areas",
            distinguishingTrait: "Thick insulating fur and layer of body fat",
            diet: "Carnivore (mainly seals)",
            bodyCovering: "Fur",
            sources: new[] { "SUGGESTED: IUCN Red List - Ursus maritimus" }),

        () => new Bird(
            id: "A021",
            commonName: "Ostrich",
            scientificName: "Struthio camelus",
            habitat: "African savannas and open dry woodlands",
            distinguishingTrait: "Largest living bird; long legs and cannot fly",
            diet: "Omnivore (plants, seeds, and small animals)",
            canFly: false,
            sources: new[] { "SUGGESTED: IUCN Red List - Struthio camelus" }),

        () => new Bird(
            id: "A022",
            commonName: "Toco Toucan",
            scientificName: "Ramphastos toco",
            habitat: "Semi-open forests, savannas, and woodland edges of central and eastern South America",
            distinguishingTrait: "Oversized bright orange-yellow bill with a black tip used for reaching fruit and regulating heat.",
            diet: "Omnivore (fruits, insects, eggs, and small reptiles)",
            canFly: true,
            sources: new[] { "SUGGESTED: IUCN Red List - Ramphastos toco" }),

        () => new Mammal(
            id: "A023",
            commonName: "Gray Wolf",
            scientificName: "Canis lupus",
            habitat: "Forests, tundra, grasslands, and mountains",
            distinguishingTrait: "Social pack hunter with a strong sense of smell",
            diet: "Carnivore (large and small mammals)",
            bodyCovering: "Fur",
            sources: new[] { "SUGGESTED: IUCN Red List - Canis lupus" }),

        () => new Bird(
            id: "A024",
            commonName: "Wandering Albatross",
            scientificName: "Diomedea exulans",
            habitat: "Southern Ocean surrounding Antarctica",
            distinguishingTrait: "Has the largest wingspan of any living bird, reaching up to 3.5 meters, built for dynamic soaring.",
            diet: "Carnivore (squid, fish, and crustaceans)",
            canFly: true,
            sources: new[] { "SUGGESTED: IUCN Red List - Diomedea exulans" }),

        () => new Bird(
            id: "A025",
            commonName: "Kiwi",
            scientificName: "Apteryx mantelli",
            habitat: "Temperate forests and scrublands of New Zealand",
            distinguishingTrait: "Nocturnal flightless bird with hair-like feathers and nostrils at the tip of its long bill.",
            diet: "Omnivore (earthworms, insects, larvae, and fallen fruit)",
            canFly: false,
            sources: new[] { "SUGGESTED: IUCN Red List - Apteryx mantelli" }),

        () => new Mammal(
            id: "A026",
            commonName: "Giraffe",
            scientificName: "Giraffa camelopardalis",
            habitat: "African savannas and open woodlands",
            distinguishingTrait: "Exceptionally long neck and spotted coat",
            diet: "Herbivore (leaves, especially acacia)",
            bodyCovering: "Short fur",
            sources: new[] { "SUGGESTED: IUCN Red List - Giraffa camelopardalis" }),

        () => new Mammal(
            id: "A027",
            commonName: "Emperor Tamarin",
            scientificName: "Saguinus imperator",
            habitat: "Amazon rainforest canopy",
            distinguishingTrait: "Distinctive long white moustache",
            diet: "Omnivore (fruit, insects, tree sap, and small animals)",
            bodyCovering: "Fur",
            sources: new[] { "SUGGESTED: IUCN Red List - Saguinus imperator" }),

        () => new Bird(
            id: "A028",
            commonName: "Common Kingfisher",
            scientificName: "Alcedo atthis",
            habitat: "Rivers, streams, lakes, and ponds",
            distinguishingTrait: "Bright blue-and-orange plumage and long pointed bill",
            diet: "Carnivore (small fish and aquatic invertebrates)",
            canFly: true,
            sources: new[] { "SUGGESTED: IUCN Red List - Alcedo atthis" }),

        () => new Mammal(
            id: "A029",
            commonName: "Humpback Whale",
            scientificName: "Megaptera novaeangliae",
            habitat: "Oceans worldwide; migrates between feeding and breeding waters",
            distinguishingTrait: "Long pectoral fins and complex songs",
            diet: "Carnivore (krill and small fish)",
            bodyCovering: "Smooth skin with a layer of blubber",
            sources: new[] { "SUGGESTED: IUCN Red List - Megaptera novaeangliae" }),

        () => new Mammal(
            id: "A030",
            commonName: "Meerkat",
            scientificName: "Suricata suricatta",
            habitat: "Dry open country and deserts of southern Africa",
            distinguishingTrait: "Often stands upright as a lookout; lives in groups",
            diet: "Omnivore (insects, small animals, and some plant matter)",
            bodyCovering: "Fur",
            sources: new[] { "SUGGESTED: IUCN Red List - Suricata suricatta" }),

        () => new FloweringPlant(
            id: "P007",
            commonName: "Rose",
            scientificName: "Rosa rubiginosa",
            habitat: "Gardens and temperate regions",
            distinguishingTrait: "Thorny stems and layered petals",
            flowerColor: "Pink",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Rosa rubiginosa" }),

        () => new FloweringPlant(
            id: "P008",
            commonName: "Hibiscus (Gumamela)",
            scientificName: "Hibiscus rosa-sinensis",
            habitat: "Tropical and subtropical gardens and hedges",
            distinguishingTrait: "Large showy trumpet-shaped flowers with a prominent central column",
            flowerColor: "Red, pink, yellow, orange, or white",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Hibiscus rosa-sinensis" }),

        () => new FloweringPlant(
            id: "P009",
            commonName: "Mango Tree",
            scientificName: "Mangifera indica",
            habitat: "Tropical and subtropical lowlands",
            distinguishingTrait: "Produces fleshy, sweet stone fruits",
            flowerColor: "Yellowish-white",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Mangifera indica" }),

        () => new FloweringPlant(
            id: "P010",
            commonName: "Coconut Palm",
            scientificName: "Cocos nucifera",
            habitat: "Tropical coastal areas and islands",
            distinguishingTrait: "Tall unbranched trunk and large fibrous fruits",
            flowerColor: "Pale yellow",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Cocos nucifera" }),

        () => new FloweringPlant(
            id: "P011",
            commonName: "Lotus",
            scientificName: "Nelumbo nucifera",
            habitat: "Ponds, lakes, and slow-moving freshwater",
            distinguishingTrait: "Round leaves and flowers held above the water",
            flowerColor: "Pink or white",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Nelumbo nucifera" }),

        () => new FloweringPlant(
            id: "P012",
            commonName: "Lavender",
            scientificName: "Lavandula angustifolia",
            habitat: "Sunny, well-drained fields and gardens",
            distinguishingTrait: "Fragrant narrow leaves and spikes of small flowers",
            flowerColor: "Purple",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Lavandula angustifolia" }),

        () => new FloweringPlant(
            id: "P013",
            commonName: "Venus Flytrap",
            scientificName: "Dionaea muscipula",
            habitat: "Nutrient-poor wetlands and bogs of the Carolinas, USA",
            distinguishingTrait: "Leaves snap shut to trap small insects",
            flowerColor: "White",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Dionaea muscipula" }),

        () => new FloweringPlant(
            id: "P014",
            commonName: "Bamboo",
            scientificName: "Bambusa vulgaris",
            habitat: "Tropical and subtropical forests and cultivated areas",
            distinguishingTrait: "Fast-growing hollow, jointed stems",
            flowerColor: "Rarely flowers; usually inconspicuous",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Bambusa vulgaris" }),

        () => new FloweringPlant(
            id: "P015",
            commonName: "Red Mangrove",
            scientificName: "Rhizophora mangle",
            habitat: "Tropical coastal shorelines and tidal wetlands",
            distinguishingTrait: "Arching prop roots that support the tree in soft mud",
            flowerColor: "Creamy white",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Rhizophora mangle" }),

        () => new FloweringPlant(
            id: "P016",
            commonName: "Blue Passionflower",
            scientificName: "Passiflora caerulea",
            habitat: "Warm temperate and tropical gardens; climbing in sunny places",
            distinguishingTrait: "Distinctive complex flower with a fringe-like corona",
            flowerColor: "White with blue-purple parts",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Passiflora caerulea" }),

        () => new Fern(
            id: "P017",
            commonName: "Staghorn Fern",
            scientificName: "Platycerium bifurcatum",
            habitat: "Humid tropical forests; often grows on tree trunks",
            distinguishingTrait: "Antler-shaped fertile fronds and round shield fronds",
            frondDescription: "Fertile fronds branch like deer antlers; shield fronds attach to the host",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Platycerium bifurcatum" }),

        () => new FloweringPlant(
            id: "P018",
            commonName: "Sensitive Plant",
            scientificName: "Mimosa pudica",
            habitat: "Tropical grasslands, roadsides, and disturbed areas",
            distinguishingTrait: "Leaflets fold when touched or shaken",
            flowerColor: "Pink to purple",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Mimosa pudica" }),

        () => new FloweringPlant(
            id: "P019",
            commonName: "Golden Shower Tree",
            scientificName: "Cassia fistula",
            habitat: "Tropical and subtropical forests and streetscapes",
            distinguishingTrait: "Long hanging clusters of bright yellow flowers",
            flowerColor: "Golden yellow",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Cassia fistula" }),

        () => new FloweringPlant(
            id: "P020",
            commonName: "Waling-Waling",
            scientificName: "Vanda sanderiana",
            habitat: "Lowland tropical forests of the Philippines; often epiphytic",
            distinguishingTrait: "Large orchid blooms with broad patterned petals",
            flowerColor: "Pink, mauve, or white with markings",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Vanda sanderiana" }),

        () => new FloweringPlant(
            id: "P021",
            commonName: "Anthurium",
            scientificName: "Anthurium andraeanum",
            habitat: "Humid tropical forests; commonly grown indoors",
            distinguishingTrait: "Glossy heart-shaped spathe surrounding a spadix",
            flowerColor: "Red, pink, or white spathes",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Anthurium andraeanum" }),

        () => new FloweringPlant(
            id: "P022",
            commonName: "Bougainvillea",
            scientificName: "Bougainvillea spectabilis",
            habitat: "Warm tropical and subtropical gardens and dry areas",
            distinguishingTrait: "Colorful papery bracts surrounding tiny flowers",
            flowerColor: "Magenta, purple, pink, orange, or white bracts",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Bougainvillea spectabilis" }),

        () => new FloweringPlant(
            id: "P023",
            commonName: "Papaya",
            scientificName: "Carica papaya",
            habitat: "Tropical lowlands and cultivated gardens",
            distinguishingTrait: "Tall soft trunk and large lobed leaves; bears large fruits",
            flowerColor: "Creamy white to yellow",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Carica papaya" }),

        () => new FloweringPlant(
            id: "P025",
            commonName: "Aloe Vera",
            scientificName: "Aloe vera",
            habitat: "Dry regions and sunny, well-drained gardens",
            distinguishingTrait: "Thick fleshy leaves containing clear gel",
            flowerColor: "Yellow or orange",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Aloe vera" }),

        () => new FloweringPlant(
            id: "P026",
            commonName: "Snake Plant",
            scientificName: "Dracaena trifasciata",
            habitat: "Dry tropical regions; commonly grown indoors",
            distinguishingTrait: "Upright sword-shaped leaves with banded patterns",
            flowerColor: "Greenish-white",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Dracaena trifasciata" }),

        () => new FloweringPlant(
            id: "P027",
            commonName: "Bird of Paradise",
            scientificName: "Strelitzia reginae",
            habitat: "Subtropical gardens and coastal areas",
            distinguishingTrait: "Flower resembles a brightly colored bird in flight",
            flowerColor: "Orange and blue",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Strelitzia reginae" }),

        () => new FloweringPlant(
            id: "P028",
            commonName: "Water Hyacinth",
            scientificName: "Pontederia crassipes",
            habitat: "Still or slow-moving freshwater ponds and waterways",
            distinguishingTrait: "Floating plant with swollen, air-filled leaf stalks",
            flowerColor: "Lavender to violet",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Pontederia crassipes" }),

        () => new Fern(
            id: "P029",
            commonName: "Boston Fern",
            scientificName: "Nephrolepis exaltata",
            habitat: "Humid tropical forests and swamps in the Americas; widely grown indoors",
            distinguishingTrait: "Arching, sword-shaped pinnate fronds that reproduce via spores on the underside of leaflets.",
            frondDescription: "Long, arching pinnate fronds with toothed lance-shaped leaflets",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Nephrolepis exaltata" }),

        () => new Fern(
            id: "P030",
            commonName: "Tree Fern",
            scientificName: "Cyathea cooperi",
            habitat: "Moist gullies and rainforests of eastern Australia and tropical Pacific islands",
            distinguishingTrait: "Develops a tall, fibrous trunk-like rhizome topped with a crown of large lacy fronds.",
            frondDescription: "Large, bipinnate, lacy fronds spreading from a central trunk crown",
            sources: new[] { "SUGGESTED: Plants of the World Online (Kew) - Cyathea cooperi" }),
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
            catch (ArgumentException ex)
            {
                int p = ex.Message.IndexOf(" (Parameter", StringComparison.Ordinal);
                result.Notes.Add($"Entry #{entry} skipped: {(p >= 0 ? ex.Message[..p] : ex.Message)}");
            }
        }

        result.AddSourceNotes();
        return result;
    }
}
