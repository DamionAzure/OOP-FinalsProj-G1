using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using KingdomExplorer.Models;
using TgColor = Terminal.Gui.Drawing.Color;

namespace KingdomExplorer.UI.TerminalGui;

/// <summary>
/// Finds and decodes the picture for an organism.
///
/// CONVENTION (no change to the Organism classes or the CSV needed): a picture is a file in an
/// "Images" folder named after the organism's ID or its common name, in any common format, e.g.
///     Images/A001.png      (by ID)
///     Images/Lion.jpg      (by common name; "Philippine Tarsier" also matches Philippine_Tarsier / Philippine-Tarsier)
///
/// Folders searched, in order: ./Images (the folder you run from, so new pictures show up without
/// rebuilding), then Images next to the program (copied there by the build).
///
/// Decoding uses SixLabors.ImageSharp. Terminal.Gui's ImageView has no decoder of its own;
/// it just displays a Color[,] pixel array, which is what this class produces.
/// </summary>
internal sealed class OrganismImageLoader
{
    private static readonly string[] Extensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".webp" };

    /// <summary>Pictures are shrunk to this many pixels on the long side. Plenty for a terminal.</summary>
    private const int MaxPixels = 600;

    private readonly Dictionary<string, (TgColor[,]? Pixels, string Note)> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The folders searched for pictures, in order.</summary>
    public static IReadOnlyList<string> SearchFolders { get; } = new[]
    {
        Path.Combine(Directory.GetCurrentDirectory(), "Images"),
        Path.Combine(AppContext.BaseDirectory, "Images")
    };

    /// <summary>
    /// Loads the organism's picture (cached). Returns false when there is no usable picture;
    /// <paramref name="note"/> then says why (file missing, or the file could not be read).
    /// </summary>
    public bool TryLoad(Organism organism, out TgColor[,]? pixels, out string note)
    {
        if (!_cache.TryGetValue(organism.Id, out var entry))
        {
            entry = LoadUncached(organism);
            _cache[organism.Id] = entry;
        }

        pixels = entry.Pixels;
        note = entry.Note;
        return pixels is not null;
    }

    /// <summary>Forgets cached pictures so files added or replaced while running are picked up.</summary>
    public void ClearCache() => _cache.Clear();

    private static (TgColor[,]? Pixels, string Note) LoadUncached(Organism organism)
    {
        string? path = FindFile(organism);
        if (path is null)
            return (null, $"No picture yet. Add Images/{organism.Id}.png or Images/{organism.CommonName}.png");

        try
        {
            return (Decode(path), Path.GetFileName(path));
        }
        catch (Exception ex)   // corrupt or unsupported file: show a note, never crash the UI
        {
            return (null, $"Could not read {Path.GetFileName(path)}: {ex.Message}");
        }
    }

    private static string? FindFile(Organism organism)
    {
        var wanted = new List<string>
        {
            organism.Id,
            organism.CommonName,
            organism.CommonName.Replace(' ', '_'),
            organism.CommonName.Replace(' ', '-'),
            organism.CommonName.Replace(" ", "")
        };

        foreach (string folder in SearchFolders)
        {
            if (!Directory.Exists(folder))
                continue;

            string[] files = Directory.GetFiles(folder);
            foreach (string name in wanted)
            {
                string? match = files.FirstOrDefault(f =>
                    string.Equals(Path.GetFileNameWithoutExtension(f), name, StringComparison.OrdinalIgnoreCase) &&
                    Extensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));
                if (match is not null)
                    return match;
            }
        }

        return null;
    }

    /// <summary>Decodes a file to the [x, y] Color array that ImageView expects.</summary>
    private static TgColor[,] Decode(string path)
    {
        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(path);

        if (image.Width > MaxPixels || image.Height > MaxPixels)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new SixLabors.ImageSharp.Size(MaxPixels, MaxPixels),
                Mode = ResizeMode.Max
            }));
        }

        int width = image.Width, height = image.Height;
        var pixels = new TgColor[width, height];

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (int x = 0; x < width; x++)
                {
                    Rgba32 p = row[x];
                    // Terminals have no transparency: blend transparent areas over black.
                    pixels[x, y] = new TgColor(p.R * p.A / 255, p.G * p.A / 255, p.B * p.A / 255);
                }
            }
        });

        return pixels;
    }
}
