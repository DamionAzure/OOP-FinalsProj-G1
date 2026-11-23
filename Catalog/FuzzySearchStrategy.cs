using FuzzySharp;
using KingdomExplorer.Models;

namespace KingdomExplorer.Catalog;

/// <summary>
/// Shared FuzzySharp scoring used by the fuzzy search and the suggestion provider.
/// Compares the query with each full name AND each word of a name, so a misspelled
/// "tarseer" can still match "Philippine Tarsier".
/// </summary>
internal static class FuzzyScorer
{
    public static int BestScore(Organism o, string query)
    {
        string q = query.Trim().ToLowerInvariant();
        int best = 0;
        foreach (string name in new[] { o.CommonName, o.ScientificName })
        {
            string lower = name.ToLowerInvariant();
            best = Math.Max(best, Fuzz.Ratio(q, lower));
            foreach (string word in lower.Split(' ', '-', '\''))
                if (word.Length > 0)
                    best = Math.Max(best, Fuzz.Ratio(q, word));
        }
        return best;
    }
}

/// <summary>
/// Search that keeps the required behavior (case-insensitive partial matching) and
/// adds typo tolerance: a record also matches when its fuzzy score is at least MinScore.
/// Exact mode stays exact. Unrelated text (e.g. "xyz") still returns no results.
/// </summary>
public sealed class FuzzySearchStrategy : ISearchStrategy
{
    private readonly NameSearchStrategy _plain = new();
    public int MinScore { get; }

    public FuzzySearchStrategy(int minScore = 75) => MinScore = minScore;

    public IReadOnlyList<Organism> Search(IEnumerable<Organism> organisms, string query, bool exactMatch)
    {
        if (exactMatch || string.IsNullOrWhiteSpace(query))
            return _plain.Search(organisms, query, exactMatch);

        var all = organisms.ToList();
        var substring = _plain.Search(all, query, false);

        var fuzzy = all
            .Except(substring)
            .Select(o => (Organism: o, Score: FuzzyScorer.BestScore(o, query)))
            .Where(x => x.Score >= MinScore)
            .OrderByDescending(x => x.Score)
            .Select(x => x.Organism);

        return substring.Concat(fuzzy).ToList();   // exact/partial hits first, then close matches
    }
}

/// <summary>"Did you mean...?" suggestions: up to 3 common names scoring at least MinScore.</summary>
public sealed class FuzzySuggestionProvider : ISuggestionProvider
{
    public int MinScore { get; }

    public FuzzySuggestionProvider(int minScore = 60) => MinScore = minScore;

    public IReadOnlyList<string> Suggest(IEnumerable<Organism> organisms, string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<string>();

        return organisms
            .Select(o => (o.CommonName, Score: FuzzyScorer.BestScore(o, query)))
            .Where(x => x.Score >= MinScore)
            .OrderByDescending(x => x.Score)
            .Take(3)
            .Select(x => x.CommonName)
            .ToList();
    }
}
