namespace KingdomExplorer.Models;

public sealed class FloweringPlant : Plant
{
    public string FlowerColor { get; }

    public override string Group => "Flowering Plant";

    public FloweringPlant(string id, string commonName, string scientificName, string habitat,
                          string distinguishingTrait, string flowerColor)
        : base(id, commonName, scientificName, habitat, distinguishingTrait,
               "Seeds produced in flowers")
    {
        FlowerColor = Require(flowerColor, nameof(flowerColor));
    }

    // POLYMORPHISM: flowering-plant-specific description.
    public override string DescribeKeyTrait() =>
        $"Flowering plant: {ReproductionType.ToLowerInvariant()}. Flower color: {FlowerColor}.";
}
