using System.Text;
using KingdomExplorer.Models;
using Terminal.Gui;

namespace KingdomExplorer.UI.TerminalGui;

/// <summary>
/// Terminal.Gui (v1.x) implementation of IExplorerView.
///
/// DESIGN: ONE persistent full-screen window (menu on the left, results on the right, input
/// bar and status line at the bottom). Nothing is ever opened or closed, so nothing can be
/// left behind on screen.
///
/// The controller still uses blocking calls (ShowMainMenu, Prompt), so it runs on a worker
/// thread. Each blocking call updates the UI, then waits until the user answers.
/// Esc on the menu exits. Esc in the input bar cancels that input.
/// </summary>
public sealed class TerminalGuiExplorerView : IExplorerView, IDisposable
{
    private static readonly string[] MenuItems =
    {
        "1. List all organisms",
        "2. Search by name",
        "3. Filter by kingdom",
        "4. Filter by group",
        "5. View an organism record",
        "6. Exit"
    };

    private readonly StringBuilder _output = new();
    private readonly Window _window;
    private readonly ListView _menu;
    private readonly TextView _results;
    private readonly Label _promptLabel;
    private readonly TextField _input;
    private readonly Label _status;

    // Answers the worker thread is waiting for (completed by the UI thread).
    private volatile TaskCompletionSource<string?>? _menuWaiting;
    private volatile TaskCompletionSource<string?>? _promptWaiting;
    private volatile bool _closing;
    private bool _initialized;

    public TerminalGuiExplorerView()
    {
        Application.Init();
        _initialized = true;

        _window = new Window("Animal and Plant Kingdom Explorer")
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };

        // ---- left: menu ----
        var menuFrame = new FrameView("Menu")
        {
            X = 0, Y = 0, Width = 34, Height = Dim.Fill(5)
        };
        _menu = new ListView(MenuItems)
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        menuFrame.Add(_menu);

        // ---- right: results ----
        var resultsFrame = new FrameView("Results")
        {
            X = Pos.Right(menuFrame), Y = 0, Width = Dim.Fill(), Height = Dim.Fill(5)
        };
        _results = new TextView
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(),
            ReadOnly = true, WordWrap = true
        };
        resultsFrame.Add(_results);

        // ---- bottom: input bar and status line ----
        var inputFrame = new FrameView("Input")
        {
            X = 0, Y = Pos.Bottom(menuFrame), Width = Dim.Fill(), Height = 4
        };
        _promptLabel = new Label("") { X = 0, Y = 0, Width = Dim.Fill() };
        _input = new TextField("") { X = 0, Y = 1, Width = Dim.Fill() };
        inputFrame.Add(_promptLabel, _input);

        _status = new Label("") { X = 1, Y = Pos.Bottom(inputFrame), Width = Dim.Fill() };

        _window.Add(menuFrame, resultsFrame, inputFrame, _status);
        Application.Top.Add(_window);

        WireEvents();
    }

    // ---------------- running ----------------

    /// <summary>Runs 'body' (the controller loop) on a worker thread while the UI runs here.</summary>
    public void RunApplication(Action body)
    {
        var worker = new Thread(() =>
        {
            try
            {
                body();
            }
            catch (Exception ex)
            {
                Ui(() => AppendOutput("! Unexpected error: " + ex.Message));
            }
            finally
            {
                Ui(() => Application.RequestStop());   // controller finished, close the app
            }
        })
        { IsBackground = true };

        Application.MainLoop.Invoke(() => worker.Start());   // start once the UI loop is running
        Application.Run();

        // UI closed (e.g. Ctrl+Q): release anything the worker is still waiting for.
        _closing = true;
        _menuWaiting?.TrySetResult(null);
        _promptWaiting?.TrySetResult(null);
    }

    public void Dispose()
    {
        if (_initialized)
        {
            Application.Shutdown();
            _initialized = false;
        }
    }

    // ---------------- IExplorerView ----------------

    public void ShowWelcome() =>
        Ui(() =>
        {
            AppendOutput("Welcome! Pick an option from the menu.");
            SetStatus("Up/Down + Enter: choose  |  Tab: switch pane  |  Esc: exit");
        });

    public string? ShowMainMenu()
    {
        if (_closing) return null;

        var waiting = NewWaiter();
        _menuWaiting = waiting;
        Ui(() =>
        {
            SetStatus("Up/Down + Enter: choose  |  Tab: switch pane  |  Esc: exit");
            _menu.SetFocus();
        });
        return waiting.Task.GetAwaiter().GetResult();   // null (Esc) makes the controller exit
    }

    public string? Prompt(string message)
    {
        if (_closing) return null;

        var waiting = NewWaiter();
        _promptWaiting = waiting;
        Ui(() =>
        {
            _promptLabel.Text = message.Trim();
            _input.Text = "";
            SetStatus("Type your answer and press Enter. Esc cancels.");
            _input.SetFocus();
        });
        return waiting.Task.GetAwaiter().GetResult();
    }

    public void ShowMessage(string message) => Ui(() => AppendOutput(message));

    public void ShowError(string message) => Ui(() => AppendOutput("! " + message));

    public void ShowOrganismList(string title, IReadOnlyList<Organism> organisms)
    {
        var text = new StringBuilder();
        text.AppendLine($"{title} ({organisms.Count} found)");
        text.AppendLine(new string('-', 50));
        foreach (Organism o in organisms)
        {
            text.AppendLine($"[{o.Id}] {o.CommonName} ({o.ScientificName})");
            text.AppendLine($"   Kingdom: {o.Kingdom} | Group: {o.Group}");
            text.AppendLine($"   Habitat: {o.Habitat}");
            text.AppendLine($"   Trait:   {o.DistinguishingTrait}");
            // Polymorphic call through the Organism reference:
            text.AppendLine($"   Details: {o.DescribeKeyTrait()}");
            text.AppendLine();
        }
        Ui(() => SetOutput(text.ToString()));
    }

    public void ShowOrganismDetails(Organism organism) =>
        Ui(() => SetOutput(organism.GetDetails()));

    // ---------------- UI-thread helpers ----------------

    private void WireEvents()
    {
        // Enter on a menu item: clear the results pane, then give the answer to the controller.
        _menu.OpenSelectedItem += args =>
        {
            var waiting = _menuWaiting;
            if (waiting is null) return;
            _menuWaiting = null;
            SetOutput("");
            waiting.TrySetResult((args.Item + 1).ToString());
        };

        _menu.KeyPress += e =>
        {
            if (e.KeyEvent.Key == Key.Esc && _menuWaiting is { } waiting)
            {
                _menuWaiting = null;
                waiting.TrySetResult(null);
                e.Handled = true;
            }
        };

        // Enter submits the input bar. Esc cancels it (an empty answer).
        _input.KeyPress += e =>
        {
            if (e.KeyEvent.Key != Key.Enter && e.KeyEvent.Key != Key.Esc) return;

            var waiting = _promptWaiting;
            if (waiting is null) return;
            _promptWaiting = null;

            string answer = e.KeyEvent.Key == Key.Enter ? _input.Text.ToString() ?? "" : "";
            _input.Text = "";
            _promptLabel.Text = "";
            e.Handled = true;
            waiting.TrySetResult(answer);
        };
    }

    private static TaskCompletionSource<string?> NewWaiter() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>Runs UI work on Terminal.Gui's main (UI) thread.</summary>
    private static void Ui(Action action) => Application.MainLoop.Invoke(action);

    private void SetOutput(string text)
    {
        _output.Clear();
        _output.Append(text);
        ShowOutput();
    }

    private void AppendOutput(string line)
    {
        _output.AppendLine(line);
        ShowOutput();
    }

    private void ShowOutput()
    {
        _results.Text = _output.ToString();
        _results.CursorPosition = default;   // scroll back to the top
        _results.SetNeedsDisplay();
    }

    private void SetStatus(string text)
    {
        _status.Text = text;
        _status.SetNeedsDisplay();
    }
}
