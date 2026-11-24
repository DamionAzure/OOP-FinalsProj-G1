using System.Text;
using Terminal.Gui.Drawing;
using Terminal.Gui.ViewBase;
using TgAttribute = Terminal.Gui.Drawing.Attribute;
using TgColor = Terminal.Gui.Drawing.Color;

namespace KingdomExplorer.UI.TerminalGui;

/// <summary>
/// Picture view for terminals without Sixel/Kitty graphics (e.g. cmd / classic console).
/// Every character cell shows TWO pixels: the upper half block "▀" is colored with the top
/// pixel and its background with the bottom pixel. That doubles the vertical detail compared
/// with ImageView's one-color-per-cell fallback. Needs a true-color terminal (Windows 10+).
/// </summary>
internal sealed class HalfBlockPictureView : View
{
    private TgColor[,]? _source;
    private TgColor[,]? _scaled;
    private (int W, int H) _scaledFor;

    public HalfBlockPictureView() => CanFocus = false;

    /// <summary>Pixels as [x, y], same layout as ImageView.Image.</summary>
    public TgColor[,]? Image
    {
        get => _source;
        set
        {
            _source = value;
            _scaled = null;
            SetNeedsDraw();
        }
    }

    protected override bool OnDrawingContent(DrawContext? context)
    {
        if (_source is null) return true;

        int cols = Viewport.Width, rows = Viewport.Height;
        if (cols <= 0 || rows <= 0) return true;

        if (_scaled is null || _scaledFor != (cols, rows * 2))
        {
            _scaled = Scale(_source, cols, rows * 2);
            _scaledFor = (cols, rows * 2);
        }

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                SetAttribute(new TgAttribute(_scaled[x, y * 2], _scaled[x, y * 2 + 1]));
                AddRune(x, y, (Rune)'▀');   // ▀
            }
        }

        return true;
    }

    /// <summary>Box-average scaling (nearest neighbor looks speckled when shrinking a big picture).</summary>
    private static TgColor[,] Scale(TgColor[,] src, int width, int height)
    {
        int sw = src.GetLength(0), sh = src.GetLength(1);
        var dst = new TgColor[width, height];

        for (int y = 0; y < height; y++)
        {
            int y0 = y * sh / height;
            int y1 = Math.Max(y0 + 1, (y + 1) * sh / height);
            for (int x = 0; x < width; x++)
            {
                int x0 = x * sw / width;
                int x1 = Math.Max(x0 + 1, (x + 1) * sw / width);

                long r = 0, g = 0, b = 0;
                int n = 0;
                for (int sy = y0; sy < y1 && sy < sh; sy++)
                    for (int sx = x0; sx < x1 && sx < sw; sx++)
                    {
                        TgColor c = src[sx, sy];
                        r += c.R; g += c.G; b += c.B;
                        n++;
                    }

                dst[x, y] = n == 0 ? src[Math.Min(x0, sw - 1), Math.Min(y0, sh - 1)]
                                   : new TgColor((int)(r / n), (int)(g / n), (int)(b / n));
            }
        }

        return dst;
    }
}
