namespace KingdomExplorer.Models;

/// <summary>INHERITANCE: kingdom-level type that adds plant-specific data (reproduction type).</summary>
public abstract class Plant : Organism
{
    public string ReproductionType { get; }

    public override Kingdom Kingdom => Kingdom.Plant;

    protected Plant(string id, string commonName, string scientificName,
                    string habitat, string distinguishingTrait, string reproductionType,
                    IEnumerable<string>? sources = null)
        : base(id, commonName, scientificName, habitat, distinguishingTrait, sources)
    {
        ReproductionType = Require(reproductionType, nameof(reproductionType));
    }
}
