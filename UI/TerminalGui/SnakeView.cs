using System.Text;
using Terminal.Gui.App;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using TgAttribute = Terminal.Gui.Drawing.Attribute;

namespace KingdomExplorer.UI.TerminalGui;

/// <summary>
/// EASTER EGG: a small snake game. Type "snake" at any input prompt and press Enter.
/// Arrow keys steer, Space restarts, Esc leaves. Each bite shows the name of an organism
/// from the list you were last looking at.
///
/// Self-contained: delete this file (and the few lines in TerminalGuiExplorerView marked EASTER EGG)
/// and nothing else changes.
/// </summary>
internal sealed class SnakeView : View
{
    private const int TickMs = 100;

    private readonly IApplication _app;
    private readonly Func<int, string?> _nameForBite;
    private readonly Random _rng = new();
    private readonly LinkedList<(int X, int Y)> _body = new();

    private (int X, int Y) _dir = (1, 0);
    private (int X, int Y) _nextDir = (1, 0);
    private (int X, int Y) _food;
    private int _cols, _rows, _score;
    private string _banner = "";
    private bool _over, _active;

    /// <summary>Raised when the player presses Esc.</summary>
    public event Action? Exited;

    public SnakeView(IApplication app, Func<int, string?> nameForBite)
    {
        _app = app;
        _nameForBite = nameForBite;
        CanFocus = true;
        KeyDown += OnKey;
    }

    /// <summary>Starts (or restarts) a game sized to the current view.</summary>
    public void Start()
    {
        // A grid cell is two characters wide so it looks square; row 0 is the score line.
        _cols = Viewport.Width / 2;
        _rows = Viewport.Height - 1;
        _score = 0;
        _over = false;
        _banner = "";
        _dir = _nextDir = (1, 0);

        _body.Clear();
        int startX = Math.Max(3, _cols / 2), startY = Math.Max(0, _rows / 2);
        for (int i = 0; i < 3; i++)
            _body.AddFirst((startX - 2 + i, startY));   // head ends up first, heading right

        PlaceFood();

        if (!_active)
        {
            _active = true;
            _app.AddTimeout(TimeSpan.FromMilliseconds(TickMs), Tick);   // returning true keeps it repeating
        }

        SetNeedsDraw();
    }

    /// <summary>Stops the timer. Safe to call more than once.</summary>
    public void Stop() => _active = false;

    private bool Tick()
    {
        if (!_active) return false;
        if (_over || _cols < 4 || _rows < 3) return true;

        _dir = _nextDir;
        var head = _body.First!.Value;
        var next = (X: head.X + _dir.X, Y: head.Y + _dir.Y);

        bool hitWall = next.X < 0 || next.Y < 0 || next.X >= _cols || next.Y >= _rows;
        if (hitWall || _body.Contains(next))
        {
            _over = true;
            _banner = "Game over";
            SetNeedsDraw();
            return true;
        }

        _body.AddFirst(next);
        if (next == _food)
        {
            _score++;
            string? name = _nameForBite(_score - 1);
            _banner = name is null ? "Yum!" : $"Ate: {name}";
            PlaceFood();
        }
        else
        {
            _body.RemoveLast();
        }

        SetNeedsDraw();
        return true;
    }

    private void PlaceFood()
    {
        if (_cols < 1 || _rows < 1) return;
        for (int tries = 0; tries < 500; tries++)
        {
            var spot = (X: _rng.Next(_cols), Y: _rng.Next(_rows));
            if (!_body.Contains(spot))
            {
                _food = spot;
                return;
            }
        }
    }

    private void OnKey(object? sender, Key key)
    {
        if (key == Key.Esc)
        {
            Stop();
            Exited?.Invoke();
        }
        else if (key == Key.Space)
        {
            Start();
        }
        else if (key == Key.CursorUp) Turn(0, -1);
        else if (key == Key.CursorDown) Turn(0, 1);
        else if (key == Key.CursorLeft) Turn(-1, 0);
        else if (key == Key.CursorRight) Turn(1, 0);
        else return;

        key.Handled = true;
    }

    /// <summary>Ignores a turn straight back into the snake's own neck.</summary>
    private void Turn(int dx, int dy)
    {
        if (dx == -_dir.X && dy == -_dir.Y) return;
        _nextDir = (dx, dy);
    }

    protected override bool OnDrawingContent(DrawContext? context)
    {
        var field = new TgAttribute(ColorName16.White, ColorName16.Black);
        var body = new TgAttribute(ColorName16.Black, ColorName16.Green);
        var head = new TgAttribute(ColorName16.Black, ColorName16.BrightGreen);
        var food = new TgAttribute(ColorName16.Black, ColorName16.BrightRed);

        SetAttribute(field);
        for (int y = 0; y < Viewport.Height; y++)
            for (int x = 0; x < Viewport.Width; x++)
                AddRune(x, y, (Rune)' ');

        AddStr(0, 0, $"Snake  score {_score}  {_banner}".PadRight(Viewport.Width));
        if (_cols < 4 || _rows < 3)
        {
            AddStr(0, 1, "Window too small for snake.");
            return true;
        }

        DrawCell(_food, food);
        bool first = true;
        foreach (var part in _body)
        {
            DrawCell(part, first ? head : body);
            first = false;
        }

        SetAttribute(field);
        AddStr(0, Viewport.Height - 1, _over ? "Space: play again   Esc: leave" : "Arrows: steer   Esc: leave");
        return true;
    }

    private void DrawCell((int X, int Y) cell, TgAttribute attribute)
    {
        SetAttribute(attribute);
        AddStr(cell.X * 2, cell.Y + 1, "  ");
    }
}
