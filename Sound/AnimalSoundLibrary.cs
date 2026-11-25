using System.Text.RegularExpressions;
using KingdomExplorer.Models;

namespace KingdomExplorer.Sound;

/// <summary>
/// Finds the sound effects for an organism and picks one at random.
///
/// CONVENTION (same idea as the pictures; no change to the Organism classes needed):
/// a sound is a .wav file in an "Audio" folder, named after the organism's ID or its common name.
/// For ID A001 / common name Lion:
///     Audio/A001.wav      ID sound
///     Audio/A001-1.wav    ID sound (numbered variant; any number, gaps are fine: -1, -2, -7 ...)
///     Audio/A001-2.wav    ID sound (numbered variant)
///     Audio/Lion.wav      common-name sound ("Philippine Tarsier" also matches Philippine_Tarsier / Philippine-Tarsier)
///
/// RANDOM PICK (<see cref="PickRandom"/>):
///   * All ID sounds are found automatically; each is equally likely.
///   * The common-name sound has a 25% chance (<see cref="CommonNameChance"/>); otherwise an ID sound is used.
///   * If there is no common-name sound, an ID sound is always used.
///   * If there are no ID sounds at all, the common-name sound is used (so the button still works).
///   * Only files that were actually found on disk are ever returned. No files at all gives null.
///
/// Folders searched, in order: ./Audio (the folder you run from, so new sounds show up without
/// rebuilding), then Audio next to the program (copied there by the build). If the same file name
/// exists in both, the first folder wins. Files are looked up on every call, so nothing is cached.
/// </summary>
public sealed class AnimalSoundLibrary
{
    /// <summary>Chance that the common-name sound (e.g. Lion.wav) is chosen when it exists.</summary>
    public const double CommonNameChance = 0.25;

    private const string Extension = ".wav";

    private readonly IReadOnlyList<string> _folders;
    private readonly Random _random;

    /// <summary>The default folders searched for sounds, in order.</summary>
    public static IReadOnlyList<string> DefaultFolders { get; } = new[]
    {
        Path.Combine(Directory.GetCurrentDirectory(), "Audio"),
        Path.Combine(AppContext.BaseDirectory, "Audio")
    };

    /// <param name="folders">Folders to search (defaults to <see cref="DefaultFolders"/>). Mainly for tests.</param>
    /// <param name="random">Random source (defaults to a new one). Pass a seeded one in tests.</param>
    public AnimalSoundLibrary(IEnumerable<string>? folders = null, Random? random = null)
    {
        _folders = (folders ?? DefaultFolders).ToList();
        _random = random ?? new Random();
    }

    /// <summary>
    /// The ID-based sounds (A001.wav, A001-1.wav, ...), in order: the plain ID first, then -1, -2, ...
    /// </summary>
    public IReadOnlyList<string> FindIdSounds(Organism organism)
    {
        var pattern = new Regex($"^{Regex.Escape(organism.Id)}(?:-(?<n>\\d+))?$",
                                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        var found = new Dictionary<string, (int Number, string Path)>(StringComparer.OrdinalIgnoreCase);
        foreach (string file in WavFiles())
        {
            string name = Path.GetFileNameWithoutExtension(file);
            Match m = pattern.Match(name);
            if (!m.Success)
                continue;

            int number = m.Groups["n"].Success && int.TryParse(m.Groups["n"].Value, out int n) ? n : 0;
            found.TryAdd(Path.GetFileName(file), (number, file));   // first folder wins
        }

        return found.Values
            .OrderBy(v => v.Number)
            .ThenBy(v => v.Path, StringComparer.OrdinalIgnoreCase)
            .Select(v => v.Path)
            .ToList();
    }

    /// <summary>The common-name sound (e.g. Lion.wav), or null when there is none.</summary>
    public string? FindCommonNameSound(Organism organism)
    {
        string[] wanted =
        {
            organism.CommonName,
            organism.CommonName.Replace(' ', '_'),
            organism.CommonName.Replace(' ', '-'),
            organism.CommonName.Replace(" ", "")
        };

        var files = WavFiles().ToList();
        foreach (string name in wanted)
        {
            string? match = files.FirstOrDefault(f =>
                string.Equals(Path.GetFileNameWithoutExtension(f), name, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
                return match;
        }

        return null;
    }

    /// <summary>True when the organism has at least one sound of any kind.</summary>
    public bool HasSound(Organism organism) =>
        FindIdSounds(organism).Count > 0 || FindCommonNameSound(organism) is not null;

    /// <summary>Picks a sound at random using the rules in the class summary. Null when there is none.</summary>
    public string? PickRandom(Organism organism)
    {
        IReadOnlyList<string> idSounds = FindIdSounds(organism);
        string? nameSound = FindCommonNameSound(organism);
        return Choose(idSounds, nameSound, _random.NextDouble(), _random.Next);
    }

    /// <summary>
    /// The selection rule on its own, so it can be tested without any files.
    /// <paramref name="roll"/> is a number in [0, 1); <paramref name="nextIndex"/> returns [0, count).
    /// </summary>
    internal static string? Choose(IReadOnlyList<string> idSounds, string? nameSound,
                                   double roll, Func<int, int> nextIndex)
    {
        if (idSounds.Count == 0)
            return nameSound;                       // may be null: nothing to play
        if (nameSound is not null && roll < CommonNameChance)
            return nameSound;
        return idSounds[nextIndex(idSounds.Count)];
    }

    /// <summary>Every .wav file in the search folders (missing folders are skipped).</summary>
    private IEnumerable<string> WavFiles()
    {
        foreach (string folder in _folders)
        {
            if (!Directory.Exists(folder))
                continue;

            string[] files;
            try { files = Directory.GetFiles(folder); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                continue;   // an unreadable folder just has no sounds
            }

            foreach (string file in files)
                if (string.Equals(Path.GetExtension(file), Extension, StringComparison.OrdinalIgnoreCase))
                    yield return file;
        }
    }
}
