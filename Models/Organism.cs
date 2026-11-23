namespace KingdomExplorer.Models;

/// <summary>
/// ABSTRACTION: shared data and responsibilities for every organism.
/// ENCAPSULATION: properties are read-only and validated once, in the constructor,
/// so an Organism can never exist in an incomplete state.
/// </summary>
public abstract class Organism
{
    public string Id { get; }
    public string CommonName { get; }
    public string ScientificName { get; }
    public string Habitat { get; }
    public string DistinguishingTrait { get; }

    /// <summary>Kingdom is fixed by the subtype (Animal or Plant).</summary>
    public abstract Kingdom Kingdom { get; }

    /// <summary>Group is fixed by the concrete subtype (e.g. Mammal, Fern).</summary>
    public abstract string Group { get; }

    /// <summary>
    /// Abstract behavior: every concrete group must describe its own key trait.
    /// This is the polymorphic method called through an Organism reference.
    /// </summary>
    public abstract string DescribeKeyTrait();

    protected Organism(string id, string commonName, string scientificName,
                       string habitat, string distinguishingTrait)
    {
        Id = Require(id, nameof(id));
        CommonName = Require(commonName, nameof(commonName));
        ScientificName = Require(scientificName, nameof(scientificName));
        Habitat = Require(habitat, nameof(habitat));
        DistinguishingTrait = Require(distinguishingTrait, nameof(distinguishingTrait));
    }

    /// <summary>Full multi-line record. Calls the overridden DescribeKeyTrait().</summary>
    public string GetDetails() =>
        $"ID:               {Id}{Environment.NewLine}" +
        $"Common name:      {CommonName}{Environment.NewLine}" +
        $"Scientific name:  {ScientificName}{Environment.NewLine}" +
        $"Kingdom:          {Kingdom}{Environment.NewLine}" +
        $"Group:            {Group}{Environment.NewLine}" +
        $"Habitat:          {Habitat}{Environment.NewLine}" +
        $"Distinguishing:   {DistinguishingTrait}{Environment.NewLine}" +
        $"Key trait:        {DescribeKeyTrait()}";

    /// <summary>Shared validation helper for subclasses and this class.</summary>
    protected static string Require(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"'{paramName}' is required and cannot be empty.", paramName);
        return value.Trim();
    }
}
