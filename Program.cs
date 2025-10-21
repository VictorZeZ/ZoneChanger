using System.Diagnostics;
using System.Security.Principal;

class TimeZoneManager
{
    // Directory to store previous timezone info
    private static readonly string StoreDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TzGameLauncher"
    );

    // File to store the previously active timezone
    private static readonly string PrevFile = Path.Combine(StoreDir, "prev_tz.txt");

    // List of popular timezones for quick selection
    private static readonly (string Id, string Description)[] PopularTimeZones = new (string, string)[]
    {
        ("GMT Standard Time", "London"),
        ("W. Europe Standard Time", "Berlin/Paris"),
        ("Eastern Standard Time", "New York"),
        ("Pacific Standard Time", "Los Angeles"),
        ("Iran Standard Time", "Tehran"),
        ("India Standard Time", "Delhi"),
        ("China Standard Time", "Beijing")
    };

    static void Main(string[] args)
    {
        try
        {
            // Set console window title
            Console.Title = "TimeZoneManager - .NET 9 Edition";

            // Ensure program is run as Administrator
            if (!IsAdministrator())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: This program must be run as Administrator!");
                Console.ResetColor();

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);

                // Immediately terminate process
                Process.GetCurrentProcess().Kill();
            }

            // Create storage directory if it doesn't exist
            Directory.CreateDirectory(StoreDir);

            // Handle command-line arguments
            if (args.Length == 0)
            {
                Console.WriteLine("No arguments detected. Launching interactive TimeZone selector...");
                RunStart();
            }
            else
            {
                var cmd = args[0].ToLowerInvariant();
                switch (cmd)
                {
                    case "start":
                        RunStart();   // Launch interactive menu
                        break;
                    case "reset":
                        RestorePrevious();  // Restore previously saved timezone
                        break;
                    case "help":
                    default:
                        ShowHelp();   // Display help menu
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Unhandled exception: " + ex);
            Console.ResetColor();
        }
        finally
        {
            WaitForExit();  // Wait for user before exiting
        }
    }

    /// <summary>
    /// Waits for a key press before exiting the application.
    /// </summary>
    private static void WaitForExit()
    {
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey(true);
    }

    /// <summary>
    /// Checks if the current process is running as Administrator.
    /// </summary>
    private static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    /// <summary>
    /// Displays help information and usage instructions.
    /// </summary>
    private static void ShowHelp()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=== TimeZoneManager - Console Utility (.NET 9) ===");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  start   -> Launch interactive TimeZone selector");
        Console.WriteLine("  reset   -> Restore previous TimeZone");
        Console.WriteLine("  help    -> Show this help menu");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  TimeZoneManager start");
        Console.WriteLine("  TimeZoneManager reset");
        Console.WriteLine();
        Console.WriteLine("Interactive mode: Use UP/DOWN arrows to select a TimeZone, press ENTER to apply.");
        Console.WriteLine("After finishing, you can use 'reset' command to restore previous TimeZone.");
        Console.WriteLine();
    }

    /// <summary>
    /// Launches the interactive timezone selection menu.
    /// Adds an extra RESET option to restore the initial system timezone.
    /// </summary>
    private static void RunStart()
    {
        // Save the current system timezone if not already saved
        if (!File.Exists(PrevFile))
        {
            var current = RunTzUtil("/g").Trim();
            File.WriteAllText(PrevFile, current);
            Console.WriteLine($"Saved current TimeZone: {current}");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        Console.CursorVisible = false;
        int index = 0;
        ConsoleKey key;

        // Menu length includes extra RESET option
        int menuLength = PopularTimeZones.Length + 1;

        do
        {
            Console.Clear();
            Console.WriteLine("=== Select a TimeZone ===");

            // Render all timezone options
            for (int i = 0; i < menuLength; i++)
            {
                bool isSelected = i == index;

                if (i < PopularTimeZones.Length)
                {
                    // Highlight currently selected timezone
                    if (isSelected) Console.ForegroundColor = ConsoleColor.Green;
                    else Console.ResetColor();

                    Console.WriteLine($"{(isSelected ? "->" : "  ")} {PopularTimeZones[i].Id} ({PopularTimeZones[i].Description})");
                }
                else
                {
                    // Display RESET option with distinct colors
                    if (isSelected) Console.ForegroundColor = ConsoleColor.Magenta;
                    else Console.ForegroundColor = ConsoleColor.Yellow;

                    Console.WriteLine($"{(isSelected ? "->" : "  ")} RESET to initial TimeZone");
                }
            }

            Console.ResetColor();
            Console.WriteLine("\nUse UP/DOWN arrows to select, ENTER to apply, ESC to exit.");

            // Read user input key
            key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.UpArrow)
            {
                index = (index - 1 + menuLength) % menuLength;  // Navigate up
            }
            else if (key == ConsoleKey.DownArrow)
            {
                index = (index + 1) % menuLength;              // Navigate down
            }
            else if (key == ConsoleKey.Enter)
            {
                if (index < PopularTimeZones.Length)
                {
                    // Apply selected timezone
                    var selected = PopularTimeZones[index];
                    Console.WriteLine($"\nApplying TimeZone: {selected.Id} ({selected.Description})");
                    RunTzUtil($"/s \"{selected.Id}\"");
                    Console.WriteLine("Done. Press any key to continue...");
                    Console.ReadKey(true);
                }
                else
                {
                    // Apply initial system timezone (RESET)
                    var initialTz = File.ReadAllText(PrevFile).Trim();
                    if (!string.IsNullOrEmpty(initialTz))
                    {
                        Console.WriteLine($"\nRestoring TimeZone to initial: {initialTz}");
                        RunTzUtil($"/s \"{initialTz}\"");
                        Console.WriteLine("TimeZone reset successfully. Press any key to continue...");
                        Console.ReadKey(true);
                    }
                }
            }

        } while (key != ConsoleKey.Escape);

        Console.Clear();
        Console.WriteLine("Interactive mode exited. Use 'TimeZoneManager reset' to restore previous TimeZone.");
    }

    /// <summary>
    /// Restores the previously saved timezone and deletes the stored record.
    /// </summary>
    private static void RestorePrevious()
    {
        if (!File.Exists(PrevFile))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No saved previous TimeZone found.");
            Console.ResetColor();
            return;
        }

        var prev = File.ReadAllText(PrevFile).Trim();
        if (!string.IsNullOrEmpty(prev))
        {
            Console.WriteLine($"Restoring TimeZone to: {prev}");
            RunTzUtil($"/s \"{prev}\"");
            Console.WriteLine("TimeZone restored successfully.");
        }

        File.Delete(PrevFile);
    }

    /// <summary>
    /// Executes tzutil.exe with specified arguments.
    /// Captures standard output and error streams.
    /// </summary>
    /// <param name="args">Arguments to pass to tzutil.exe</param>
    /// <returns>Standard output string from tzutil</returns>
    private static string RunTzUtil(string args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "tzutil.exe",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var p = Process.Start(psi);
            string outp = p.StandardOutput.ReadToEnd();
            string err = p.StandardError.ReadToEnd();
            p.WaitForExit();

            if (!string.IsNullOrEmpty(err))
                Console.WriteLine("tzutil stderr: " + err.Trim());

            return outp;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error executing tzutil: " + ex.Message);
            Console.ResetColor();
            return "";
        }
    }
}
