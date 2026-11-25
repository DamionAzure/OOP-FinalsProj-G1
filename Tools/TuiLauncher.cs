using System.Diagnostics;
using System.Text.RegularExpressions;

namespace KingdomExplorer.Tools;

/// <summary>
/// Used only by the normal (.NET 8) build, which does not contain the Terminal.Gui interface.
/// When someone passes --tui to it, this builds and starts the TUI version of the project
/// (dotnet run -p:EnableTui=true) instead of silently falling back to the console menu.
/// If that is not possible (no .NET 10 SDK, or the project folder cannot be found), it says why.
/// </summary>
internal static class TuiLauncher
{
    private const string ProjectFile = "KingdomExplorer.csproj";

    /// <summary>Returns the exit code to use for this program.</summary>
    public static int Run(string[] args)
    {
        string? project = FindProject();
        if (project is null)
        {
            Explain($"Could not find {ProjectFile} to build the Terminal.Gui interface from.",
                    "Run this from the project folder:  dotnet run -p:EnableTui=true -- --tui");
            return 1;
        }

        if (!HasNet10Sdk())
        {
            Explain("The Terminal.Gui interface needs the .NET 10 SDK, and it was not found (dotnet --list-sdks).",
                    "Install it from https://dotnet.microsoft.com/download/dotnet/10.0 then run:  dotnet run -p:EnableTui=true -- --tui",
                    "Until then, the program keeps working as the console version (run it without --tui).");
            return 1;
        }

        Console.WriteLine("Starting the Terminal.Gui interface (building it first; the first time needs internet to restore packages)...");

        var info = new ProcessStartInfo("dotnet") { UseShellExecute = false };   // inherits this console
        foreach (string a in new[] { "run", "--project", project, "-p:EnableTui=true", "--" })
            info.ArgumentList.Add(a);
        foreach (string a in args)
            info.ArgumentList.Add(a);

        try
        {
            using Process? process = Process.Start(info);
            if (process is null)
            {
                Explain("Could not start the dotnet command.");
                return 1;
            }
            process.WaitForExit();
            return process.ExitCode;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            Explain("The 'dotnet' command was not found on this computer.");
            return 1;
        }
    }

    private static void Explain(params string[] lines)
    {
        Console.WriteLine();
        Console.WriteLine("! The --tui option was not started.");
        foreach (string line in lines)
            Console.WriteLine("  " + line);
        Console.WriteLine();
    }

    /// <summary>Looks for the project file in the current folder, then upwards, then upwards from the program's own folder.</summary>
    private static string? FindProject()
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (var dir = new DirectoryInfo(start); dir is not null; dir = dir.Parent)
            {
                string candidate = Path.Combine(dir.FullName, ProjectFile);
                if (File.Exists(candidate))
                    return candidate;
            }
        }
        return null;
    }

    private static bool HasNet10Sdk()
    {
        try
        {
            var info = new ProcessStartInfo("dotnet", "--list-sdks")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using Process? process = Process.Start(info);
            if (process is null) return false;

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return Regex.IsMatch(output, @"^\s*(1[0-9]|[2-9][0-9])\.", RegexOptions.Multiline);   // 10.x or newer
        }
        catch (Exception)    // dotnet missing or not runnable
        {
            return false;
        }
    }
}
