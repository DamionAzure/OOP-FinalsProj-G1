using KingdomExplorer.Models;

namespace KingdomExplorer.Data;

/// <summary>
/// Turns a spreadsheet row into the right Organism subtype, chosen by the "type" column.
///
/// TEAM 1: to add a new group (for example "Reptile"), write the new class in Models/
/// and add ONE line to the table below. Nothing else needs to change.
/// The constructors validate every field, so a bad row is reported with a clear message.
/// </summary>
internal static class OrganismFactory
{
    private static readonly Dictionary<string, Func<DataRow, Organism>> Builders =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Mammal"] = r => new Mammal(
                r.Get("id"), r.Get("commonName"), r.Get("scientificName"), r.Get("habitat"),
                r.Get("distinguishingTrait"), r.Get("diet"), r.Get("bodyCovering"), r.Sources),

            ["Bird"] = r => new Bird(
                r.Get("id"), r.Get("commonName"), r.Get("scientificName"), r.Get("habitat"),
                r.Get("distinguishingTrait"), r.Get("diet"), r.GetBool("canFly"), r.Sources),

            ["FloweringPlant"] = r => new FloweringPlant(
                r.Get("id"), r.Get("commonName"), r.Get("scientificName"), r.Get("habitat"),
                r.Get("distinguishingTrait"), r.Get("flowerColor"), r.Sources),

            ["Fern"] = r => new Fern(
                r.Get("id"), r.Get("commonName"), r.Get("scientificName"), r.Get("habitat"),
                r.Get("distinguishingTrait"), r.Get("frondDescription"), r.Sources),
        };

    public static Organism Create(DataRow row)
    {
        string type = row.Get("type");
        if (!Builders.TryGetValue(type, out var build))
            throw new FormatException(
                $"Unknown type '{type}'. Valid types: {string.Join(", ", Builders.Keys)}.");
        return build(row);
    }
}
