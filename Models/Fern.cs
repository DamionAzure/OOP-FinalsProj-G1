namespace KingdomExplorer.Models;

public sealed class Fern : Plant
{
    public string FrondDescription { get; }

    public override string Group => "Fern";

    public Fern(string id, string commonName, string scientificName, string habitat,
                string distinguishingTrait, string frondDescription)
        : base(id, commonName, scientificName, habitat, distinguishingTrait, "Spores")
    {
        FrondDescription = Require(frondDescription, nameof(frondDescription));
    }

    // POLYMORPHISM: fern-specific description.
    public override string DescribeKeyTrait() =>
        $"Fern: seedless, reproduces by {ReproductionType.ToLowerInvariant()}. Fronds: {FrondDescription}.";
}
