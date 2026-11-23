using KingdomExplorer.Catalog;
using KingdomExplorer.Models;

namespace KingdomExplorer.UI;

/// <summary>
/// Handles the menu flow. Depends only on OrganismCatalog and IExplorerView,
/// so the interface (console, Terminal.Gui, ...) can be swapped without changes here.
/// </summary>
public sealed class ExplorerController
{
    private readonly OrganismCatalog _catalog;
    private readonly IExplorerView _view;

    private readonly IReadOnlyList<string> _startupNotes;

    public ExplorerController(OrganismCatalog catalog, IExplorerView view,
                              IEnumerable<string>? startupNotes = null)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _startupNotes = (startupNotes ?? Enumerable.Empty<string>()).ToList();
    }

    public void Run()
    {
        _view.ShowWelcome();
        foreach (string note in _startupNotes)      // e.g. data-file loading messages
            _view.ShowMessage(note);

        while (true)
        {
            string? choice = _view.ShowMainMenu();
            if (choice is null) return; // input stream ended

            switch (choice.Trim())
            {
                case "1": ListAll(); break;
                case "2": SearchByName(); break;
                case "3": FilterByKingdom(); break;
                case "4": FilterByGroup(); break;
                case "5": ViewRecord(); break;
                case "6":
                    _view.ShowMessage("Goodbye!");
                    return;
                case "":
                    _view.ShowError("No input entered. Please choose a number from 1 to 6.");
                    break;
                default:
                    _view.ShowError($"'{choice.Trim()}' is not a valid option. Please choose a number from 1 to 6.");
                    break;
            }
        }
    }

    private void ListAll()
    {
        var all = _catalog.GetAll();
        if (all.Count == 0)
            _view.ShowMessage("The catalog is empty.");
        else
            _view.ShowOrganismList("All organisms", all);
    }

    private void SearchByName()
    {
        string? query = _view.Prompt("Enter a common or scientific name: ");
        if (string.IsNullOrWhiteSpace(query))
        {
            _view.ShowError("Search text cannot be empty.");
            return;
        }

        string? mode = _view.Prompt("Match type - 1) Partial (default)  2) Exact: ");
        bool exact = mode?.Trim() == "2";

        var results = _catalog.Search(query, exact);
        if (results.Count > 0)
        {
            _view.ShowOrganismList($"Search results for \"{query.Trim()}\"", results);
            return;
        }

        _view.ShowMessage($"No organisms matched \"{query.Trim()}\".");
        var suggestions = _catalog.SuggestSimilar(query);
        if (suggestions.Count > 0)
            _view.ShowMessage("Did you mean: " + string.Join(", ", suggestions) + "?");
    }

    private void FilterByKingdom()
    {
        string? input = _view.Prompt("Choose a kingdom - 1) Animal  2) Plant: ");
        Kingdom? kingdom = input?.Trim().ToLowerInvariant() switch
        {
            "1" or "animal" => Kingdom.Animal,
            "2" or "plant" => Kingdom.Plant,
            _ => null
        };

        if (kingdom is null)
        {
            _view.ShowError("Please enter 1 (Animal), 2 (Plant), or the kingdom name.");
            return;
        }

        var results = _catalog.FilterByKingdom(kingdom.Value);
        if (results.Count == 0)
            _view.ShowMessage($"No organisms found in the {kingdom} kingdom.");
        else
            _view.ShowOrganismList($"Kingdom: {kingdom}", results);
    }

    private void FilterByGroup()
    {
        var groups = _catalog.GetGroups();
        _view.ShowMessage("Available groups:");
        for (int i = 0; i < groups.Count; i++)
            _view.ShowMessage($"  {i + 1}. {groups[i]}");

        string? input = _view.Prompt("Enter a group number or name: ");
        if (string.IsNullOrWhiteSpace(input))
        {
            _view.ShowError("Group cannot be empty.");
            return;
        }

        string group = input.Trim();
        if (int.TryParse(group, out int index) && index >= 1 && index <= groups.Count)
            group = groups[index - 1];

        var results = _catalog.FilterByGroup(group);
        if (results.Count == 0)
            _view.ShowMessage($"No organisms found in group \"{group}\". Try one of: {string.Join(", ", groups)}.");
        else
            _view.ShowOrganismList($"Group: {results[0].Group}", results);
    }

    private void ViewRecord()
    {
        string? id = _view.Prompt("Enter an organism ID (e.g., A001): ");
        if (string.IsNullOrWhiteSpace(id))
        {
            _view.ShowError("ID cannot be empty.");
            return;
        }

        Organism? organism = _catalog.FindById(id);
        if (organism is null)
            _view.ShowMessage($"No organism found with ID \"{id.Trim()}\".");
        else
            _view.ShowOrganismDetails(organism);
    }
}
