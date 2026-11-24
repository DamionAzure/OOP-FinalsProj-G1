using System.Reflection;
using System.Runtime.ExceptionServices;
using KingdomExplorer.Models;

namespace KingdomExplorer.DevTools;

/// <summary>
/// Builds the right Organism subclass from a spreadsheet row. The "type" column is the class
/// name (Mammal, Bird, FloweringPlant, Fern, ...) and each constructor parameter is read from the
/// column with the same name. A new group class therefore works without changing this file.
/// </summary>
internal static class OrganismFactory
{
    private static readonly Dictionary<string, Type> Types = typeof(Organism).Assembly.GetTypes()
        .Where(t => typeof(Organism).IsAssignableFrom(t) && !t.IsAbstract && t.IsPublic)
        .ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

    public static Organism Create(DataRow row)
    {
        string type = row.Get("type");
        if (!Types.TryGetValue(type, out Type? t))
            throw new FormatException(
                $"Unknown type '{type}'. Valid types: {string.Join(", ", Types.Keys.OrderBy(k => k))}.");

        ConstructorInfo ctor = t.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();
        object?[] args = ctor.GetParameters().Select(p => ArgumentFor(row, p)).ToArray();

        try
        {
            return (Organism)ctor.Invoke(args);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();   // show the real validation message
            throw;
        }
    }

    private static object? ArgumentFor(DataRow row, ParameterInfo p) =>
        p.Name == "sources" ? row.Sources.ToList()
        : p.ParameterType == typeof(bool) ? row.GetBool(p.Name!)
        : row.Get(p.Name!);
}
