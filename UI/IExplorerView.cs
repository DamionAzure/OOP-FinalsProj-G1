using KingdomExplorer.Models;

namespace KingdomExplorer.UI;

/// <summary>
/// EXTENSION POINT: everything the controller needs from a user interface.
/// ConsoleExplorerView is the default. TerminalGuiExplorerView (optional, UI/TerminalGui) is the full-screen alternative.
/// All input methods return null when input ends (e.g., Ctrl+Z / Ctrl+D).
/// </summary>
public interface IExplorerView
{
    void ShowWelcome();
    string? ShowMainMenu();
    string? Prompt(string message);
    void ShowMessage(string message);
    void ShowError(string message);
    void ShowOrganismList(string title, IReadOnlyList<Organism> organisms);
    void ShowOrganismDetails(Organism organism);
}
