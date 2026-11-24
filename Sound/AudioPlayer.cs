using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KingdomExplorer.Sound;

/// <summary>
/// Plays a .wav file without blocking and without any extra NuGet package.
///   Windows: winmm.dll PlaySound (built into Windows).
///   macOS:   afplay (built in).
///   Linux:   the first of paplay, aplay, ffplay, play (sox) that is installed.
/// Starting a new sound stops the previous one. Output of the helper programs is discarded so it
/// can never draw over the terminal interface.
/// </summary>
public sealed class AudioPlayer : IDisposable
{
    // Linux players tried in order: (program, arguments before the file name).
    private static readonly (string Program, string[] Args)[] LinuxPlayers =
    {
        ("paplay", Array.Empty<string>()),
        ("aplay", new[] { "-q" }),
        ("ffplay", new[] { "-nodisp", "-autoexit", "-loglevel", "quiet" }),
        ("play", new[] { "-q" })
    };

    private readonly object _lock = new();
    private Process? _current;
    private string? _linuxProgram;          // remembered once a player has started successfully
    private bool _disposed;

    /// <summary>Starts playing the file. Returns false (with a short reason) when it cannot.</summary>
    public bool TryPlay(string path, out string error)
    {
        error = "";
        if (!File.Exists(path))
        {
            error = $"Sound file not found: {Path.GetFileName(path)}";
            return false;
        }

        try
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    error = "Audio is shut down.";
                    return false;
                }

                StopCurrent();

                if (OperatingSystem.IsWindows())
                {
                    // SND_FILENAME | SND_ASYNC | SND_NODEFAULT: play from the file, return at once, no beep if it fails.
                    if (!PlaySound(path, IntPtr.Zero, 0x00020000 | 0x0001 | 0x0002))
                    {
                        error = $"Windows could not play {Path.GetFileName(path)} (is it a valid .wav?)";
                        return false;
                    }
                    return true;
                }

                if (OperatingSystem.IsMacOS())
                    return StartProcess("afplay", Array.Empty<string>(), path, out error);

                return StartOnLinux(path, out error);
            }
        }
        catch (Exception ex)    // audio must never crash the explorer
        {
            error = $"Could not play {Path.GetFileName(path)}: {ex.Message}";
            return false;
        }
    }

    /// <summary>Stops whatever is playing.</summary>
    public void Stop()
    {
        lock (_lock)
        {
            try { StopCurrent(); } catch { /* nothing useful to do */ }
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed) return;
            _disposed = true;
            try { StopCurrent(); } catch { /* shutting down */ }
        }
    }

    private bool StartOnLinux(string path, out string error)
    {
        // Use the player that worked last time; otherwise try each one until one is installed.
        var candidates = _linuxProgram is null
            ? LinuxPlayers
            : LinuxPlayers.Where(p => p.Program == _linuxProgram).ToArray();

        foreach (var (program, args) in candidates)
        {
            if (StartProcess(program, args, path, out _, notFoundIsSilent: true))
            {
                _linuxProgram = program;
                error = "";
                return true;
            }
        }

        _linuxProgram = null;
        error = "No audio player found. Install pulseaudio-utils (paplay) or alsa-utils (aplay).";
        return false;
    }

    private bool StartProcess(string program, string[] argsBeforeFile, string path, out string error,
                              bool notFoundIsSilent = false)
    {
        error = "";
        var info = new ProcessStartInfo(program)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true
        };
        foreach (string a in argsBeforeFile) info.ArgumentList.Add(a);
        info.ArgumentList.Add(path);

        try
        {
            var process = Process.Start(info);
            if (process is null)
            {
                error = $"Could not start {program}.";
                return false;
            }

            // Drain and discard output so the helper never blocks and nothing reaches the terminal.
            process.OutputDataReceived += (_, _) => { };
            process.ErrorDataReceived += (_, _) => { };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            _current = process;
            return true;
        }
        catch (System.ComponentModel.Win32Exception)    // program is not installed
        {
            error = notFoundIsSilent ? "" : $"{program} is not available on this computer.";
            return false;
        }
    }

    private void StopCurrent()
    {
        Process? p = _current;
        _current = null;
        if (p is null) return;

        try
        {
            if (!p.HasExited) p.Kill();
        }
        catch (InvalidOperationException) { /* already gone */ }
        finally { p.Dispose(); }

        if (OperatingSystem.IsWindows())
            PlaySound(null, IntPtr.Zero, 0x0040);       // SND_PURGE: stop the Windows sound too
    }

    // winmm.dll is only called when OperatingSystem.IsWindows() is true.
    [DllImport("winmm.dll", EntryPoint = "PlaySoundW", CharSet = CharSet.Unicode)]
    private static extern bool PlaySound(string? sound, IntPtr module, int flags);
}
