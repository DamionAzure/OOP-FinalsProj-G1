using System.Diagnostics;

namespace KingdomExplorer.Sound;

/// <summary>
/// One shared cooldown for every playback button. The first accepted request starts it; until it
/// runs out, further requests are refused. It belongs to the view, not to an organism, so
/// selecting a different animal does not reset it.
/// </summary>
public sealed class PlaybackCooldown
{
    /// <summary>How long playback is blocked after a request.</summary>
    public static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(2);

    private readonly object _lock = new();
    private readonly TimeSpan _duration;
    private readonly Func<long> _nowMs;       // milliseconds from any steady clock (replaceable in tests)
    private long _endsAtMs;                   // 0 = never started. (long.MinValue would overflow when subtracted from.)

    public PlaybackCooldown(TimeSpan? duration = null, Func<long>? nowMs = null)
    {
        _duration = duration ?? DefaultDuration;
        _nowMs = nowMs ?? (() => Stopwatch.GetTimestamp() * 1000 / Stopwatch.Frequency);
    }

    /// <summary>True while requests are being refused.</summary>
    public bool IsActive => Remaining > TimeSpan.Zero;

    /// <summary>Time left before the next request is accepted (zero when ready).</summary>
    public TimeSpan Remaining
    {
        get
        {
            lock (_lock)
            {
                long left = _endsAtMs - _nowMs();
                return left > 0 ? TimeSpan.FromMilliseconds(left) : TimeSpan.Zero;
            }
        }
    }

    /// <summary>
    /// Accepts a request and starts the cooldown, or returns false (and changes nothing) if it is still running.
    /// </summary>
    public bool TryStart()
    {
        lock (_lock)
        {
            long now = _nowMs();
            if (_endsAtMs - now > 0)
                return false;

            _endsAtMs = now + (long)_duration.TotalMilliseconds;
            return true;
        }
    }
}
