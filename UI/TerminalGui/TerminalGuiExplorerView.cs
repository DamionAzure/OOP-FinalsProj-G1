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
    private readonly FrameView _menuFrame;
    private readonly FrameView _resultsFrame;
    private readonly FrameView _inputFrame;
    private readonly ListView _menu;
    private readonly TextView _results;
    private readonly Label _promptLabel;
    private readonly TextField _input;
    private readonly Label _status;

    // Answers the worker thread is waiting for (completed by the UI thread).
    private volatile TaskCompletionSource<string?>? _menuWaiting;
    private volatile TaskCompletionSource<string?>? _promptWaiting;
    private volatile bool _closing;
    private int _themeIndex;      // F2 cycles themes
    private int _layoutIndex;     // F3 cycles layouts
    private string _lastStatus = "";
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
        _menuFrame = new FrameView("Menu")
        {
            X = 0, Y = 0, Width = 34, Height = Dim.Fill(5)
        };
        _menu = new ListView(MenuItems)
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        _menuFrame.Add(_menu);

        // ---- right: results ----
        _resultsFrame = new FrameView("Results")
        {
            X = Pos.Right(_menuFrame), Y = 0, Width = Dim.Fill(), Height = Dim.Fill(5)
        };
        _results = new TextView
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(),
            ReadOnly = true, WordWrap = true
        };
        _resultsFrame.Add(_results);

        // ---- bottom: input bar and status line ----
        _inputFrame = new FrameView("Input")
        {
            X = 0, Y = Pos.Bottom(_menuFrame), Width = Dim.Fill(), Height = 4
        };
        _promptLabel = new Label("") { X = 0, Y = 0, Width = Dim.Fill() };
        _input = new TextField("") { X = 0, Y = 1, Width = Dim.Fill() };
        _inputFrame.Add(_promptLabel, _input);

        _status = new Label("") { X = 1, Y = Pos.Bottom(_inputFrame), Width = Dim.Fill() };

        _window.Add(_menuFrame, _resultsFrame, _inputFrame, _status);
        Application.Top.Add(_window);
        ApplyTheme();
        ApplyLayout();
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
        // F2 = next theme, F3 = next layout. Attached to the window and to each focusable pane,
        // so the keys work wherever the focus is.
        _window.KeyPress += e => { if (HandleDesignKey(e.KeyEvent.Key)) e.Handled = true; };
        _menu.KeyPress += e => { if (HandleDesignKey(e.KeyEvent.Key)) e.Handled = true; };
        _results.KeyPress += e => { if (HandleDesignKey(e.KeyEvent.Key)) e.Handled = true; };
        _input.KeyPress += e => { if (HandleDesignKey(e.KeyEvent.Key)) e.Handled = true; };

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

        _results.KeyPress += e =>
        {
            if (e.KeyEvent.Key == Key.Esc)
            {
                if (_promptWaiting is not null)
                    _input.SetFocus();
                else
                    _menu.SetFocus();

                e.Handled = true;
            }
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
        _lastStatus = text;
        _status.Text = $"{text}  |  F2 theme: {Themes[_themeIndex].Name}  F3 layout: {LayoutNames[_layoutIndex]}";
        _status.SetNeedsDisplay();
    }

    // ---------------- designs: themes (F2) and layouts (F3) ----------------

    /// <summary>Returns true when the key was F2 or F3 (and was handled).</summary>
    private bool HandleDesignKey(Key key)
    {
        if (key == Key.F2)
        {
            _themeIndex = (_themeIndex + 1) % Themes.Length;
            ApplyTheme();
            RedrawChrome();
            return true;
        }

        if (key == Key.F3)
        {
            _layoutIndex = (_layoutIndex + 1) % LayoutNames.Length;
            ApplyLayout();
            RedrawChrome();
            return true;
        }

        return false;
    }

    private void RedrawChrome()
    {
        SetStatus(_lastStatus);          // refreshes the theme/layout names in the status line
        _window.LayoutSubviews();
        _window.SetNeedsDisplay();
        Application.Refresh();
    }

    private sealed record Theme(
        string Name,
        Color Bg, Color Text, Color Muted,
        Color Accent, Color AccentText,
        Color InputBg, Color InputText, Color InputFocus,
        Color Status);

    private static readonly Theme[] Themes =
    {
        //            name           bg            text           muted             accent             accentText   inputBg          inputText         inputFocus         status
        new Theme("Forest",       Color.Black,  Color.White,   Color.Gray,       Color.BrightGreen,   Color.Black, Color.DarkGray,  Color.White,      Color.BrightYellow, Color.BrightCyan),
        new Theme("Classic Blue", Color.Blue,   Color.White,   Color.BrightCyan, Color.Cyan,          Color.Black, Color.Black,     Color.White,      Color.BrightYellow, Color.BrightYellow),
        new Theme("Amber",        Color.Black,  Color.BrightYellow, Color.Brown, Color.BrightYellow,  Color.Black, Color.Brown,     Color.BrightYellow, Color.White,      Color.Brown),
        new Theme("Ocean",        Color.Black,  Color.White,   Color.Cyan,       Color.BrightCyan,    Color.Black, Color.Blue,      Color.White,      Color.BrightCyan,   Color.Cyan),
        new Theme("Sunset",       Color.Black,  Color.White,   Color.Gray,       Color.BrightMagenta, Color.Black, Color.DarkGray,  Color.White,      Color.BrightYellow, Color.BrightMagenta),
        new Theme("Paper",        Color.White,  Color.Black,   Color.DarkGray,   Color.Blue,          Color.White, Color.Gray,      Color.Black,      Color.Blue,         Color.Blue),
    };

    private void ApplyTheme()
    {
        Theme t = Themes[_themeIndex];
        static Terminal.Gui.Attribute A(Color fg, Color bg) => Application.Driver.MakeAttribute(fg, bg);

        // Window, borders, menu
        var baseScheme = new ColorScheme
        {
            Normal    = A(t.Text, t.Bg),
            Focus     = A(t.AccentText, t.Accent),
            HotNormal = A(t.Accent, t.Bg),
            HotFocus  = A(t.AccentText, t.Accent),
            Disabled  = A(t.Muted, t.Bg)
        };

        // Results TextView (ReadOnly uses Normal/Disabled in v1.x)
        var resultsScheme = new ColorScheme
        {
            Normal    = A(t.Muted, t.Bg),
            Focus     = A(t.Text, t.Bg),
            HotNormal = A(t.Accent, t.Bg),
            HotFocus  = A(t.Accent, t.Bg),
            Disabled  = A(t.Muted, t.Bg)
        };

        // Bottom input field
        var inputScheme = new ColorScheme
        {
            Normal    = A(t.InputText, t.InputBg),
            Focus     = A(t.InputFocus, t.InputBg),
            HotNormal = A(t.InputFocus, t.InputBg),
            HotFocus  = A(t.InputFocus, t.InputBg),
            Disabled  = A(t.Muted, t.Bg)
        };

        Application.Top.ColorScheme = baseScheme;
        _window.ColorScheme = baseScheme;
        _menu.ColorScheme = baseScheme;
        _results.ColorScheme = resultsScheme;
        _promptLabel.ColorScheme = baseScheme;
        _input.ColorScheme = inputScheme;
        _status.ColorScheme = new ColorScheme
        {
            Normal = A(t.Status, t.Bg)
        };
    }

    private static readonly string[] LayoutNames = { "Side by side", "Stacked", "Menu on right" };

    private void ApplyLayout()
    {
        switch (_layoutIndex)
        {
            case 1: // Stacked: menu across the top, results below
                _menuFrame.X = 0; _menuFrame.Y = 0;
                _menuFrame.Width = Dim.Fill(); _menuFrame.Height = MenuItems.Length + 2;
                _resultsFrame.X = 0; _resultsFrame.Y = Pos.Bottom(_menuFrame);
                _resultsFrame.Width = Dim.Fill(); _resultsFrame.Height = Dim.Fill(5);
                _inputFrame.Y = Pos.Bottom(_resultsFrame);
                break;

            case 2: // Menu on the right, wide results on the left
                _resultsFrame.X = 0; _resultsFrame.Y = 0;
                _resultsFrame.Width = Dim.Fill(34); _resultsFrame.Height = Dim.Fill(5);
                _menuFrame.X = Pos.Right(_resultsFrame); _menuFrame.Y = 0;
                _menuFrame.Width = Dim.Fill(); _menuFrame.Height = Dim.Fill(5);
                _inputFrame.Y = Pos.Bottom(_resultsFrame);
                break;

            default: // Side by side: menu left, results right (original)
                _menuFrame.X = 0; _menuFrame.Y = 0;
                _menuFrame.Width = 34; _menuFrame.Height = Dim.Fill(5);
                _resultsFrame.X = Pos.Right(_menuFrame); _resultsFrame.Y = 0;
                _resultsFrame.Width = Dim.Fill(); _resultsFrame.Height = Dim.Fill(5);
                _inputFrame.Y = Pos.Bottom(_menuFrame);
                break;
        }
    }
}
