namespace KingdomExplorer.Models;

/// <summary>INHERITANCE: kingdom-level type that adds animal-specific data (diet).</summary>
public abstract class Animal : Organism
{
    public string Diet { get; }

    public override Kingdom Kingdom => Kingdom.Animal;

    protected Animal(string id, string commonName, string scientificName,
                     string habitat, string distinguishingTrait, string diet,
                     IEnumerable<string>? sources = null)
        : base(id, commonName, scientificName, habitat, distinguishingTrait, sources)
    {
        Diet = Require(diet, nameof(diet));
    }
}
