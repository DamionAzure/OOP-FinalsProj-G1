using KingdomExplorer.Models;

namespace KingdomExplorer.UI;

/// <summary>Plain System.Console implementation of IExplorerView.</summary>
public sealed class ConsoleExplorerView : IExplorerView
{
    public void ShowWelcome()
    {
        Console.WriteLine("=====================================");
        Console.WriteLine("  Animal and Plant Kingdom Explorer");
        Console.WriteLine("=====================================");
    }

    public string? ShowMainMenu()
    {
        Console.WriteLine();
        Console.WriteLine("--- MAIN MENU ---");
        Console.WriteLine("1. List all organisms");
        Console.WriteLine("2. Search by common or scientific name");
        Console.WriteLine("3. Filter by kingdom");
        Console.WriteLine("4. Filter by group");
        Console.WriteLine("5. View an organism record");
        Console.WriteLine("6. Exit");
        return Prompt("Choose an option (1-6): ");
    }

    public string? Prompt(string message)
    {
        Console.Write(message);
        return Console.ReadLine();
    }

    public void ShowMessage(string message) => Console.WriteLine(message);

    public void ShowError(string message) => Console.WriteLine($"! {message}");

    public void ShowOrganismList(string title, IReadOnlyList<Organism> organisms)
    {
        Console.WriteLine();
        Console.WriteLine($"{title} ({organisms.Count} found)");
        Console.WriteLine(new string('-', 60));
        foreach (Organism o in organisms)
        {
            Console.WriteLine($"[{o.Id}] {o.CommonName} ({o.ScientificName})");
            Console.WriteLine($"     Kingdom: {o.Kingdom} | Group: {o.Group}");
            Console.WriteLine($"     Habitat: {o.Habitat}");
            Console.WriteLine($"     Trait:   {o.DistinguishingTrait}");
            // Polymorphic call through the Organism reference:
            Console.WriteLine($"     Details: {o.DescribeKeyTrait()}");
            Console.WriteLine();
        }
    }

    public void ShowOrganismDetails(Organism organism)
    {
        Console.WriteLine();
        Console.WriteLine(organism.GetDetails());
    }
}
