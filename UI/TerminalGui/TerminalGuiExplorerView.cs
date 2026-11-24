using System.Collections.ObjectModel;
using System.Text;
using KingdomExplorer.Models;
using KingdomExplorer.Sound;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using TgAttribute = Terminal.Gui.Drawing.Attribute;
using TgColor = Terminal.Gui.Drawing.Color;

namespace KingdomExplorer.UI.TerminalGui;

/// <summary>
/// Terminal.Gui v2 implementation of IExplorerView.
///
/// DESIGN: ONE persistent full-screen window (menu on the left, results on the right, input
/// bar and status line at the bottom). Nothing is ever opened or closed, so nothing can be
/// left behind on screen.
///
/// RESULTS PANE: a list of organisms next to a detail area. Whichever organism is highlighted in
/// the list (or the one you looked up) is shown with its PICTURE and its full record. Pictures come
/// from the Images folder (see OrganismImageLoader). ImageView draws them with the Kitty or Sixel
/// protocol when the terminal supports it, and as colored cells otherwise.
///
/// The controller still uses blocking calls (ShowMainMenu, Prompt), so it runs on a worker
/// thread. Each blocking call updates the UI, then waits until the user answers.
/// Esc on the menu exits. Esc in the input bar cancels that input.
///
/// v2 notes: the app is an instance (IApplication), the window is the runnable that is passed to
/// Run, and work from other threads goes through IApplication.Invoke.
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

    private const string MenuStatus = "Enter: choose | Tab: switch pane | Esc: exit";

    private readonly IApplication _app;
    private readonly OrganismImageLoader _images = new();
    private readonly AnimalSoundLibrary _sounds = new();     // finds A001.wav, A001-1.wav, Lion.wav ...
    private readonly AudioPlayer _audio = new();
    private readonly PlaybackCooldown _cooldown = new();     // one cooldown for the button, shared by every animal
    private readonly StringBuilder _output = new();
    private readonly Window _window;
    private readonly FrameView _menuFrame;
    private readonly FrameView _resultsFrame;
    private readonly FrameView _inputFrame;
    private readonly ListView _menu;
    private readonly ListView _list;
    private readonly ImageView _picture;                // Sixel / Kitty terminals
    private readonly HalfBlockPictureView _halfBlock;    // everything else (cmd, classic console)
    private readonly Label _detailText;
    private readonly Button _playButton;       // plays a random sound (ID variants, 25% common name)
    private readonly Label _audioNote;         // "Ready", the file that played, or the cooldown countdown
    private readonly Label _promptLabel;
    private readonly TextField _input;
    private readonly Label _status;
    private SnakeView? _snake;     // EASTER EGG: created the first time someone types "snake"
    private string _titleBeforeSnake = "Results";

    // Answers the worker thread is waiting for (completed by the UI thread).
    private volatile TaskCompletionSource<string?>? _menuWaiting;
    private volatile TaskCompletionSource<string?>? _promptWaiting;
    private volatile bool _closing;
    private int _themeIndex;      // F2 cycles themes
    private int _layoutIndex;     // F3 cycles layouts
    private string _lastStatus = "";
    private bool _workerStarted;
    private bool _disposed;
    private bool _cooldownTimerRunning;
    private bool _keepResultsFocus;       // results were just shown: ShowMainMenu must not pull focus back to the menu
    private bool _shownHasSound;          // does the shown animal have any sound file?
    private string _playNote = "";
    private string? _playNoteFor;         // ID of the organism _playNote belongs to        // what the last playback did; cleared when another organism is shown

    // What the results pane is currently showing (UI thread only).
    private IReadOnlyList<Organism> _listed = Array.Empty<Organism>();
    private Organism? _shown;
    private bool _listMode;

    public TerminalGuiExplorerView()
    {
        _app = Application.Create().Init();

        _window = new Window
        {
            Title = "Animal and Plant Kingdom Explorer",
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };

        // ---- left: menu ----
        _menuFrame = new FrameView
        {
            Title = "Menu",
            X = 0, Y = 0, Width = 34, Height = Dim.Fill(5)
        };
        _menu = new ListView
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()
        };
        _menu.SetSource(new ObservableCollection<string>(MenuItems));
        _menu.SelectedItem = 0;
        _menuFrame.Add(_menu);

        // ---- right: results = organism list + picture + details ----
        _resultsFrame = new FrameView
        {
            Title = "Results",
            X = Pos.Right(_menuFrame), Y = 0, Width = Dim.Fill(), Height = Dim.Fill(5)
        };
        _list = new ListView
        {
            X = 0, Y = 0, Width = Dim.Percent(38), Height = Dim.Fill(),
            Visible = false
        };
        _picture = new ImageView
        {
            X = 0, Y = 0, Width = Dim.Absolute(1), Height = Dim.Absolute(1),
            Visible = false
        };
        _detailText = new Label
        {
            X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(),   // text wraps by default
            CanFocus = true                                           // so the keyboard can rest on a plant's record
        };
        _halfBlock = new HalfBlockPictureView
        {
            X = 0, Y = 0, Width = Dim.Absolute(1), Height = Dim.Absolute(1),
            Visible = false
        };

        // ---- audio row (animals only): bottom of the results pane ----
        _playButton = new Button { Text = "Play Sound (F5)", X = 0, Y = Pos.AnchorEnd(1), Visible = false };
        _audioNote = new Label { X = Pos.Right(_playButton) + 2, Y = Pos.AnchorEnd(1), Width = Dim.Fill(), Visible = false };
        _resultsFrame.Add(_list, _picture, _halfBlock, _detailText, _playButton, _audioNote);

        // ---- bottom: input bar and status line ----
        _inputFrame = new FrameView
        {
            Title = "Input",
            X = 0, Y = Pos.Bottom(_menuFrame), Width = Dim.Fill(), Height = 4
        };
        _promptLabel = new Label { X = 0, Y = 0, Width = Dim.Fill() };
        _input = new TextField { X = 0, Y = 1, Width = Dim.Fill() };
        _inputFrame.Add(_promptLabel, _input);

        _status = new Label { X = 1, Y = Pos.Bottom(_inputFrame), Width = Dim.Fill() };

        _window.Add(_menuFrame, _resultsFrame, _inputFrame, _status);
        ApplyTheme();
        ApplyLayout();
        WireEvents();
    }

    // ---------------- running ----------------

    /// <summary>Runs 'body' (the controller loop) on a worker thread while the UI runs here.</summary>
    public void RunApplication(Action body)
    {
        // Start the worker once the UI loop is actually running.
        _window.IsRunningChanged += (_, e) =>
        {
            if (e.Value && !_workerStarted)
            {
                _workerStarted = true;
                StartWorker(body);
            }
        };

        _app.Run(_window);   // blocks until RequestStop

        // UI closed (e.g. the window was stopped): release anything the worker is still waiting for.
        _closing = true;
        _menuWaiting?.TrySetResult(null);
        _promptWaiting?.TrySetResult(null);
    }

    private void StartWorker(Action body)
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
                Ui(() => _app.RequestStop());   // controller finished, close the app
            }
        })
        { IsBackground = true };

        worker.Start();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _audio.Dispose();
        _window.Dispose();
        _app.Dispose();
    }

    // ---------------- IExplorerView ----------------

    public void ShowWelcome() =>
        Ui(() =>
        {
            AppendOutput("Welcome! Pick an option from the menu.");
            SetStatus(MenuStatus);
        });

    public string? ShowMainMenu()
    {
        if (_closing) return null;

        var waiting = NewWaiter();
        _menuWaiting = waiting;
        Ui(() =>
        {
            if (_keepResultsFocus)
            {
                _keepResultsFocus = false;   // stay on the results; Esc (or Tab) goes back to the menu
                return;
            }
            SetStatus(MenuStatus);
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
            SetStatus("Type your answer, Enter: submit | Esc: cancel");
            _input.SetFocus();
        });
        return waiting.Task.GetAwaiter().GetResult();
    }

    public void ShowMessage(string message) => Ui(() => AppendOutput(message));

    public void ShowError(string message) => Ui(() => AppendOutput("! " + message));

    public void ShowOrganismList(string title, IReadOnlyList<Organism> organisms)
    {
        var snapshot = organisms.ToList();   // the pane may outlive the caller's list
        Ui(() =>
        {
            _listed = snapshot;
            _resultsFrame.Title = $"{title} ({snapshot.Count} found)";
            _list.SetSource(new ObservableCollection<string>(snapshot.Select(o => $"[{o.Id}] {o.CommonName}")));
            if (snapshot.Count > 0)
                _list.SelectedItem = 0;

            SetListMode(snapshot.Count > 0);
            ShowOrganism(snapshot.Count > 0 ? snapshot[0] : null);
            SetStatus("Tab: switch pane | Up/Down in list: pictures | Esc: menu");
            FocusResults();
        });
    }

    public void ShowOrganismDetails(Organism organism) =>
        Ui(() =>
        {
            _listed = new[] { organism };
            _resultsFrame.Title = $"[{organism.Id}] {organism.CommonName}";
            SetListMode(false);
            ShowOrganism(organism);
            SetStatus("Tab: switch pane | Esc: menu");
            FocusResults();
        });

    /// <summary>Moves the keyboard to the info (results) pane: the list if shown, else the sound button, else the record text.</summary>
    private void FocusResults()
    {
        _keepResultsFocus = true;
        if (_list.Visible) _list.SetFocus();
        else if (_playButton.Visible && _playButton.Enabled) _playButton.SetFocus();
        else _detailText.SetFocus();
    }

    // ---------------- UI-thread helpers ----------------

    private void WireEvents()
    {
        // Window-level keys. Keys the focused view does not handle bubble up to here.
        _window.KeyDown += (_, key) =>
        {
            if (key == Key.Tab || key == Key.Tab.WithShift)
            {
                // Each framed pane is its own tab group in v2 (Tab would stay inside it), so cycle the panes ourselves.
                CycleFocus(key == Key.Tab ? 1 : -1);
                key.Handled = true;
            }
            else if (HandleDesignKey(key) || HandleAudioKey(key))
            {
                key.Handled = true;
            }
            else if (key == Key.Esc)
            {
                // Esc anywhere else: go back to whatever the controller is waiting on.
                if (_promptWaiting is not null) _input.SetFocus(); else _menu.SetFocus();
                key.Handled = true;
            }
        };

        // Enter on a menu item: clear the results pane, then give the answer to the controller.
        _menu.Accepted += (_, _) =>
        {
            var waiting = _menuWaiting;
            if (waiting is null || _menu.SelectedItem is not int index) return;
            _menuWaiting = null;
            SetOutput("");
            waiting.TrySetResult((index + 1).ToString());
        };

        _menu.KeyDown += (_, key) =>
        {
            if (key == Key.Esc && _menuWaiting is { } waiting)
            {
                _menuWaiting = null;
                waiting.TrySetResult(null);
                key.Handled = true;
            }
        };

        // Audio button (mouse click, or Enter / Space while focused).
        _playButton.Accepted += (_, _) => PlayRequested();

        // Moving through the list shows that organism's picture and record.
        _list.ValueChanged += (_, e) =>
        {
            if (e.NewValue is int index && index >= 0 && index < _listed.Count)
                ShowOrganism(_listed[index]);
        };

        // Enter submits the input bar. Esc cancels it (an empty answer).
        _input.KeyDown += (_, key) =>
        {
            if (key != Key.Enter && key != Key.Esc) return;

            var waiting = _promptWaiting;
            if (waiting is null) return;

            if (key == Key.Enter && string.Equals(_input.Text?.Trim(), "snake", StringComparison.OrdinalIgnoreCase))
            {
                _input.Text = "";            // EASTER EGG: the prompt stays open for the real answer afterwards
                StartSnake();
                key.Handled = true;
                return;
            }

            _promptWaiting = null;

            string answer = key == Key.Enter ? _input.Text ?? "" : "";
            _input.Text = "";
            _promptLabel.Text = "";
            key.Handled = true;
            waiting.TrySetResult(answer);
        };
    }

    // ---------------- EASTER EGG: snake ----------------

    private void StartSnake()
    {
        if (_snake is null)
        {
            _snake = new SnakeView(_app, bite => _listed.Count > 0 ? _listed[bite % _listed.Count].CommonName : null)
            {
                X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill(), Visible = false
            };
            _snake.Exited += EndSnake;
            _resultsFrame.Add(_snake);
        }

        _titleBeforeSnake = _resultsFrame.Title;
        _resultsFrame.Title = "Snake";
        _list.Visible = false;
        _picture.Visible = false;
        _halfBlock.Visible = false;
        _detailText.Visible = false;
        _playButton.Visible = _audioNote.Visible = false;
        _snake.Visible = true;
        _snake.SetFocus();
        _snake.Start();
        SetStatus("Snake: arrows | Space: restart | Esc: leave");
    }

    private void EndSnake()
    {
        if (_snake is null) return;
        _snake.Visible = false;
        _detailText.Visible = true;
        _resultsFrame.Title = _titleBeforeSnake;
        SetListMode(_listMode);        // brings the list back if it was showing
        ShowOrganism(_shown);          // and the picture
        _input.SetFocus();
        SetStatus("Type your answer, Enter: submit | Esc: cancel");
    }

    /// <summary>Tab / Shift+Tab: menu -> organism list -> picture -> audio button -> input bar (skipping panes that are hidden or disabled).</summary>
    private void CycleFocus(int step)
    {
        if (_snake?.Visible == true) return;   // the game keeps the keyboard until Esc
        View[] panes = new View[] { _menu, _list, _picture, _playButton, _detailText, _input }
            .Where(v => v.Visible && v.Enabled && v.CanFocus).ToArray();
        if (panes.Length == 0) return;

        int current = Array.FindIndex(panes, v => v.HasFocus);
        int next = current < 0 ? 0 : (current + step + panes.Length) % panes.Length;
        panes[next].SetFocus();
    }

    private static TaskCompletionSource<string?> NewWaiter() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>Runs UI work on Terminal.Gui's main (UI) thread.</summary>
    private void Ui(Action action)
    {
        try
        {
            _app.Invoke(action);
        }
        catch (NotInitializedException)
        {
            // The UI has already shut down (the worker is just finishing); nothing to update.
        }
    }

    // ---------------- results pane ----------------

    private void SetOutput(string text)
    {
        _output.Clear();
        _output.Append(text);
        _resultsFrame.Title = "Results";
        _listed = Array.Empty<Organism>();
        SetListMode(false);
        ShowOrganism(null);
        _detailText.Text = _output.ToString();
    }

    private void AppendOutput(string line)
    {
        // Messages replace any organism that was showing, then accumulate like a log.
        if (_shown is not null || _listMode)
        {
            SetListMode(false);
            ShowOrganism(null);
        }

        _output.AppendLine(line);
        _detailText.Text = _output.ToString();
    }

    /// <summary>List mode: organism list on the left, picture and record on the right. Otherwise full width.</summary>
    private void SetListMode(bool on)
    {
        _listMode = on;
        _list.Visible = on;
        Pos x = on ? Pos.Right(_list) + 1 : 0;
        _picture.X = x;
        _halfBlock.X = x;
        _detailText.X = x;
        _playButton.X = x;     // the note follows the button
    }

    private void HidePictures()
    {
        _picture.Image = null;
        _picture.Visible = false;
        _halfBlock.Image = null;
        _halfBlock.Visible = false;
    }

    /// <summary>Shows one organism's picture and record, or clears them when null.</summary>
    private void ShowOrganism(Organism? organism)
    {
        _shown = organism;

        if (organism is null)
        {
            HidePictures();
            _detailText.Y = 0;
            UpdateAudioControls();
            return;
        }

        var text = new StringBuilder();
        if (_images.TryLoad(organism, out TgColor[,]? pixels, out string note) && pixels is not null)
        {
            (int cols, int rows) = FitInCells(pixels.GetLength(0), pixels.GetLength(1));
            // Sixel/Kitty terminals get the real graphics; anything else gets half-block characters.
            bool graphics = _picture.IsUsingRasterGraphics;
            HidePictures();
            View shown = graphics ? _picture : _halfBlock;
            shown.Width = Dim.Absolute(cols);
            shown.Height = Dim.Absolute(rows);
            if (graphics) _picture.Image = pixels; else _halfBlock.Image = pixels;
            shown.Visible = true;
            _detailText.Y = Pos.Bottom(shown) + 1;
        }
        else
        {
            HidePictures();
            _detailText.Y = 0;
            text.AppendLine($"({note})");
            text.AppendLine();
        }

        text.Append(organism.GetDetails().Replace("\r\n", "\n"));
        _detailText.Text = text.ToString();
        _detailText.SetNeedsDraw();

        _shownHasSound = organism is Animal && _sounds.HasSound(organism);
        if (organism.Id != _playNoteFor)
            _playNote = "";
        UpdateAudioControls();
    }

    /// <summary>
    /// Size in terminal cells for a picture of the given pixel size, keeping its shape.
    /// A cell is about twice as tall as it is wide, so rows = columns * (height / width) / 2.
    /// </summary>
    private (int Cols, int Rows) FitInCells(int pixelWidth, int pixelHeight)
    {
        var inner = _resultsFrame.Viewport;
        int listWidth = _listMode ? _list.Frame.Width + 1 : 0;
        int availableCols = inner.Width > 0 ? inner.Width - listWidth - 1 : 40;
        int availableRows = inner.Height > 0 ? inner.Height : 24;

        // Leave room for roughly nine lines of record text under the picture.
        int maxCols = Math.Clamp(availableCols, 8, 64);
        int maxRows = Math.Clamp(availableRows - 9, 4, 28);

        double shape = (double)pixelHeight / pixelWidth;
        int cols = maxCols;
        int rows = (int)Math.Round(cols * shape / 2.0);
        if (rows > maxRows)
        {
            rows = maxRows;
            cols = (int)Math.Round(rows * 2.0 / shape);
        }

        return (Math.Clamp(cols, 1, maxCols), Math.Max(1, rows));
    }

    private void SetStatus(string text)
    {
        _lastStatus = text;
        string mode = !_picture.UseRasterGraphics ? "off" : _picture.IsUsingRasterGraphics ? "graphics" : "half-block";

        // Use the longest version of the line that fits the window, so nothing is cut off.
        int width = _window.Viewport.Width > 4 ? _window.Viewport.Width - 3 : 78;
        string[] candidates =
        {
            $"{text} | F2 theme: {Themes[_themeIndex].Name} | F3 layout: {LayoutNames[_layoutIndex]} | F4 graphics: {mode}",
            $"{text} | F2 theme | F3 layout | F4 graphics: {mode}",
            $"{text} | F2 F3 F4: look",
            text
        };
        _status.Text = candidates.FirstOrDefault(c => c.Length <= width) ?? text;
    }

    // ---------------- audio: Play Sound (F5) = a random sound for the shown animal ----------------

    private bool HandleAudioKey(Key key)
    {
        if (key != Key.F5) return false;
        PlayRequested();
        return true;
    }

    /// <summary>
    /// The button was pressed (or F5). Plays a random sound, then starts the 2-second cooldown.
    /// The cooldown belongs to the view, not the animal, so selecting another animal does not reset it.
    /// </summary>
    private void PlayRequested()
    {
        if (_snake?.Visible == true || _shown is not Animal animal || !_shownHasSound)
            return;
        if (!_cooldown.TryStart())          // still cooling down (this also guards the F5 key)
        {
            UpdateAudioControls();
            return;
        }

        string? path = _sounds.PickRandom(animal);
        if (path is null)
            _playNote = $"No sound found for {animal.CommonName}";
        else if (_audio.TryPlay(path, out string error))
            _playNote = Path.GetFileName(path);
        else
            _playNote = error;
        _playNoteFor = animal.Id;

        StartCooldownTimer();
        UpdateAudioControls();
    }

    /// <summary>Refreshes the countdown about ten times a second until the cooldown ends.</summary>
    private void StartCooldownTimer()
    {
        if (_cooldownTimerRunning) return;
        _cooldownTimerRunning = true;

        _app.AddTimeout(TimeSpan.FromMilliseconds(100), () =>
        {
            if (!_disposed && !_closing)
                UpdateAudioControls();
            if (!_disposed && !_closing && _cooldown.IsActive)
                return true;                // keep ticking
            _cooldownTimerRunning = false;
            return false;                   // cooldown over: stop
        });
    }

    /// <summary>Shows or hides the audio row and sets the buttons and note from the current state.</summary>
    private void UpdateAudioControls()
    {
        bool animal = _shown is Animal && _snake?.Visible != true;
        _playButton.Visible = _audioNote.Visible = animal;
        _detailText.Height = animal ? Dim.Fill(2) : Dim.Fill();   // keep the record text off the audio row
        if (!animal) return;

        bool cooling = _cooldown.IsActive;
        _playButton.Enabled = _shownHasSound && !cooling;

        if (cooling)
        {
            const int barWidth = 10;
            double seconds = _cooldown.Remaining.TotalSeconds;
            double fraction = Math.Clamp(seconds / PlaybackCooldown.DefaultDuration.TotalSeconds, 0, 1);
            int filled = (int)Math.Ceiling(fraction * barWidth);
            string bar = new string('#', filled) + new string('-', barWidth - filled);
            string played = string.IsNullOrEmpty(_playNote) ? "" : $"  {_playNote}";
            _audioNote.Text = $"Cooldown {seconds:0.0}s [{bar}]{played}";
        }
        else if (!_shownHasSound)
        {
            _audioNote.Text = $"No sound yet. Add Audio/{_shown!.Id}.wav";
        }
        else
        {
            _audioNote.Text = string.IsNullOrEmpty(_playNote) ? "Ready" : $"Played: {_playNote}";
        }
    }

    // ---------------- designs: themes (F2), layouts (F3), picture mode (F4) ----------------

    /// <summary>Returns true when the key was F2, F3 or F4 (and was handled).</summary>
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

        if (key == Key.F4)
        {
            // Some terminals claim Sixel/Kitty support but draw it badly: this forces plain colored cells.
            _picture.UseRasterGraphics = !_picture.UseRasterGraphics;
            RedrawChrome();
            return true;
        }

        return false;
    }

    private void RedrawChrome()
    {
        SetStatus(_lastStatus);          // refreshes the theme/layout names in the status line
        _window.Layout();                // sizes depend on the layout, so measure again...
        if (_shown is { } current)
            ShowOrganism(current);       // ...and refit the picture to the new pane size
        _window.SetNeedsDraw();
    }

    private sealed record Theme(
        string Name,
        ColorName16 Bg, ColorName16 Text, ColorName16 Muted,
        ColorName16 Accent, ColorName16 AccentText,
        ColorName16 InputBg, ColorName16 InputText, ColorName16 InputFocus,
        ColorName16 Status);

    // Color.Black, Color.White ... are the 16 standard color names (v2 has no Color.Brown: it is Color.Yellow).
    private static readonly Theme[] Themes =
    {
        //            name           bg            text           muted             accent             accentText   inputBg          inputText         inputFocus         status
        new Theme("Forest",       Color.Black,  Color.White,   Color.Gray,       Color.BrightGreen,   Color.Black, Color.DarkGray,  Color.White,      Color.BrightYellow, Color.BrightCyan),
        new Theme("Classic Blue", Color.Blue,   Color.White,   Color.BrightCyan, Color.Cyan,          Color.Black, Color.Black,     Color.White,      Color.BrightYellow, Color.BrightYellow),
        new Theme("Amber",        Color.Black,  Color.BrightYellow, Color.Yellow, Color.BrightYellow, Color.Black, Color.Yellow,    Color.BrightYellow, Color.White,      Color.Yellow),
        new Theme("Ocean",        Color.Black,  Color.White,   Color.Cyan,       Color.BrightCyan,    Color.Black, Color.Blue,      Color.White,      Color.BrightCyan,   Color.Cyan),
        new Theme("Sunset",       Color.Black,  Color.White,   Color.Gray,       Color.BrightMagenta, Color.Black, Color.DarkGray,  Color.White,      Color.BrightYellow, Color.BrightMagenta),
        new Theme("Paper",        Color.White,  Color.Black,   Color.DarkGray,   Color.Blue,          Color.White, Color.Gray,      Color.Black,      Color.Blue,         Color.Blue),
    };

    private void ApplyTheme()
    {
        Theme t = Themes[_themeIndex];
        static TgAttribute A(ColorName16 fg, ColorName16 bg) => new(fg, bg);

        // Window, borders, menu, list
        var baseScheme = new Scheme
        {
            Normal    = A(t.Text, t.Bg),
            Focus     = A(t.AccentText, t.Accent),
            HotNormal = A(t.Accent, t.Bg),
            HotFocus  = A(t.AccentText, t.Accent),
            Disabled  = A(t.Muted, t.Bg)
        };

        // Details text
        var detailScheme = new Scheme
        {
            Normal    = A(t.Muted, t.Bg),
            Focus     = A(t.Text, t.Bg),
            HotNormal = A(t.Accent, t.Bg),
            HotFocus  = A(t.Accent, t.Bg),
            Disabled  = A(t.Muted, t.Bg)
        };

        // Bottom input field
        var inputScheme = new Scheme
        {
            Normal    = A(t.InputText, t.InputBg),
            Focus     = A(t.InputFocus, t.InputBg),
            HotNormal = A(t.InputFocus, t.InputBg),
            HotFocus  = A(t.InputFocus, t.InputBg),
            Disabled  = A(t.Muted, t.Bg)
        };

        _window.SetScheme(baseScheme);        // the other views inherit it from the window
        _detailText.SetScheme(detailScheme);
        _input.SetScheme(inputScheme);
        _status.SetScheme(new Scheme(A(t.Status, t.Bg)));
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
