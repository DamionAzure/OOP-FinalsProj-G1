namespace KingdomExplorer.Models;

public sealed class Bird : Animal
{
    public bool CanFly { get; }

    public override string Group => "Bird";

    public Bird(string id, string commonName, string scientificName, string habitat,
                string distinguishingTrait, string diet, bool canFly,
                IEnumerable<string>? sources = null)
        : base(id, commonName, scientificName, habitat, distinguishingTrait, diet, sources)
    {
        CanFly = canFly;
    }

    // POLYMORPHISM: bird-specific description.
    public override string DescribeKeyTrait() =>
        $"Bird: feathered and lays eggs. {(CanFly ? "Capable of flight" : "Flightless")}. Diet: {Diet}.";
}
