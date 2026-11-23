namespace KingdomExplorer.Models;

public sealed class Mammal : Animal
{
    public string BodyCovering { get; }

    public override string Group => "Mammal";

    public Mammal(string id, string commonName, string scientificName, string habitat,
                  string distinguishingTrait, string diet, string bodyCovering)
        : base(id, commonName, scientificName, habitat, distinguishingTrait, diet)
    {
        BodyCovering = Require(bodyCovering, nameof(bodyCovering));
    }

    // POLYMORPHISM: mammal-specific description.
    public override string DescribeKeyTrait() =>
        $"Mammal: nurses its young with milk. Body covering: {BodyCovering}. Diet: {Diet}.";
}
