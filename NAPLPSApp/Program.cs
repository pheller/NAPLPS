// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using System.Reflection;
using System.Text.Json;
using SixLabors.ImageSharp.Formats.Gif;

namespace NAPLPSApp;

sealed class Program
{
    public static string Version { get; } = GetLibraryVersion();

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        // Handle CLI commands before starting the GUI
        if (args.Length > 0)
        {
            var command = args[0].ToLowerInvariant();

            if (command == "info" || command == "--info" || command == "-i")
            {
                return HandleInfoCommand(args);
            }

            if (command == "export" || command == "--export" || command == "-e")
            {
                return HandleExportCommand(args);
            }

            if (command == "help" || command == "--help" || command == "-h" || command == "-?")
            {
                PrintHelp();
                return 0;
            }

            if (command == "--version" || command == "-v")
            {
                Console.WriteLine($"NAPLPS Toolbox v{Version}");
                return 0;
            }
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        return 0;
    }

    private static void PrintHelp()
    {
        Console.WriteLine($"NAPLPS Toolbox v{Version}");
        Console.WriteLine();
        Console.WriteLine("Usage: NAPLPSApp [command] [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  info <file> [--format=text|json]   Display file information");
        Console.WriteLine("  export <file> [output] [options]   Export file to image format");
        Console.WriteLine();
        Console.WriteLine("Export Options:");
        Console.WriteLine("  --format=png|gif      Output format (default: png)");
        Console.WriteLine("  --size=WxH            Canvas size (default: 1024x768)");
        Console.WriteLine("  --stdout, -           Output to stdout instead of file");
        Console.WriteLine();
        Console.WriteLine("GIF Options:");
        Console.WriteLine("  --loop                Loop the GIF animation (default: no loop)");
        Console.WriteLine("  --delay=N             Frame delay in 1/100s of a second (default: 5)");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  NAPLPSApp info myfile.nap");
        Console.WriteLine("  NAPLPSApp info myfile.nap --format=json");
        Console.WriteLine("  NAPLPSApp export myfile.nap output.png");
        Console.WriteLine("  NAPLPSApp export myfile.nap --format=gif output.gif");
        Console.WriteLine("  NAPLPSApp export myfile.nap --format=gif --loop --delay=10 output.gif");
        Console.WriteLine("  NAPLPSApp export myfile.nap --stdout > output.png");
    }

    private static int HandleInfoCommand(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Error: No input file specified.");
            Console.Error.WriteLine("Usage: NAPLPSApp info <file> [--format=text|json]");
            return 1;
        }

        var inputFile = args[1];
        var format = "text";

        for (int i = 2; i < args.Length; i++)
        {
            if (args[i].StartsWith("--format="))
            {
                format = args[i]["--format=".Length..].ToLowerInvariant();
            }
        }

        if (!File.Exists(inputFile))
        {
            Console.Error.WriteLine($"Error: File not found: {inputFile}");
            return 1;
        }

        try
        {
            var naplps = NaplpsFormat.FromFile(inputFile);
            var fileInfo = new FileInfo(inputFile);

            if (format == "json")
            {
                var info = new
                {
                    FileName = fileInfo.Name,
                    FilePath = fileInfo.FullName,
                    FileSize = fileInfo.Length,
                    SystemType = naplps.SystemType.ToString(),
                    BitWidth = naplps.Is7Bit ? "7-Bit" : "8-Bit",
                    CommandCount = naplps.Commands.Count,
                    IsValid = naplps.IsValid,
                    ErrorCount = naplps.Errors.Count,
                    Version
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                Console.WriteLine(JsonSerializer.Serialize(info, options));
            }
            else
            {
                // Tab-aligned text format
                Console.WriteLine($"File Name:\t{fileInfo.Name}");
                Console.WriteLine($"File Path:\t{fileInfo.FullName}");
                Console.WriteLine($"File Size:\t{fileInfo.Length} bytes");
                Console.WriteLine($"System Type:\t{naplps.SystemType}");
                Console.WriteLine($"Bit Width:\t{(naplps.Is7Bit ? "7-Bit" : "8-Bit")}");
                Console.WriteLine($"Commands:\t{naplps.Commands.Count}");
                Console.WriteLine($"Valid:\t\t{naplps.IsValid}");
                Console.WriteLine($"Errors:\t\t{naplps.Errors.Count}");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: Failed to parse file: {ex.Message}");
            return 1;
        }
    }

    private static int HandleExportCommand(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Error: No input file specified.");
            Console.Error.WriteLine("Usage: NAPLPSApp export <file> [output] [--format=png|gif] [--size=WxH] [--stdout]");
            return 1;
        }

        var inputFile = args[1];
        string? outputFile = null;
        var format = "png";
        var size = "1024x768";
        var useStdout = false;
        var loop = false;
        var delay = 5; // Default frame delay in 1/100s of a second

        for (int i = 2; i < args.Length; i++)
        {
            if (args[i] == "--stdout" || args[i] == "-")
            {
                useStdout = true;
            }
            else if (args[i] == "--loop")
            {
                loop = true;
            }
            else if (args[i].StartsWith("--format="))
            {
                format = args[i]["--format=".Length..].ToLowerInvariant();
            }
            else if (args[i].StartsWith("--size="))
            {
                size = args[i]["--size=".Length..];
            }
            else if (args[i].StartsWith("--delay="))
            {
                if (!int.TryParse(args[i]["--delay=".Length..], out delay) || delay < 1)
                {
                    Console.Error.WriteLine($"Error: Invalid delay value. Expected positive integer.");
                    return 1;
                }
            }
            else if (!args[i].StartsWith("--") && !args[i].StartsWith("-"))
            {
                outputFile = args[i];
            }
        }

        if (!useStdout && outputFile == null)
        {
            outputFile = IOPath.ChangeExtension(inputFile, format);
        }

        if (!File.Exists(inputFile))
        {
            Console.Error.WriteLine($"Error: File not found: {inputFile}");
            return 1;
        }

        // Parse size
        var sizeParts = size.Split('x');
        if (sizeParts.Length != 2 || !int.TryParse(sizeParts[0], out var width) || !int.TryParse(sizeParts[1], out var height))
        {
            Console.Error.WriteLine($"Error: Invalid size format: {size}. Expected WxH (e.g., 1024x768)");
            return 1;
        }

        try
        {
            var naplps = NaplpsFormat.FromFile(inputFile);
            using var drawContext = new DrawContext(naplps, new SixLabors.ImageSharp.Size(width, height));

            if (format == "gif")
            {
                return ExportGif(drawContext, outputFile, useStdout, loop, delay);
            }
            else
            {
                return ExportPng(drawContext, outputFile, useStdout);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: Failed to export file: {ex.Message}");
            return 1;
        }
    }

    private static int ExportPng(DrawContext drawContext, string? outputFile, bool useStdout)
    {
        drawContext.Render();

        if (useStdout)
        {
            using var stdout = Console.OpenStandardOutput();
            drawContext.Image.SaveAsPng(stdout);
        }
        else if (outputFile != null)
        {
            drawContext.Image.SaveAsPng(outputFile);
            Console.Error.WriteLine($"Exported to: {outputFile}");
        }

        return 0;
    }

    private static int ExportGif(DrawContext drawContext, string? outputFile, bool useStdout, bool loop, int delay)
    {
        using var gif = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(
            drawContext.Size.Width, drawContext.Size.Height);

        var gifMetaData = gif.Metadata.GetGifMetadata();
        gifMetaData.RepeatCount = loop ? (ushort)0 : (ushort)1; // 0 = loop forever, 1 = play once

        // Render each frame and add to GIF
        for (uint i = 0; i <= drawContext.TotalFrames; i++)
        {
            drawContext.Render(i);

            // Clone the current frame
            var frame = drawContext.Image.Clone();

            // Set frame delay (in hundredths of a second)
            var frameMetadata = frame.Frames.RootFrame.Metadata.GetGifMetadata();
            frameMetadata.FrameDelay = delay;

            if (i == 0)
            {
                // First frame replaces the root frame
                gif.Frames.RootFrame.ProcessPixelRows(frame.Frames.RootFrame, (accessorGif, accessorFrame) =>
                {
                    for (int y = 0; y < accessorGif.Height; y++)
                    {
                        var rowGif = accessorGif.GetRowSpan(y);
                        var rowFrame = accessorFrame.GetRowSpan(y);
                        rowFrame.CopyTo(rowGif);
                    }
                });
                gif.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = delay;
            }
            else
            {
                // Add subsequent frames
                gif.Frames.AddFrame(frame.Frames.RootFrame);
            }

            frame.Dispose();
        }

        if (useStdout)
        {
            using var stdout = Console.OpenStandardOutput();
            gif.SaveAsGif(stdout);
        }
        else if (outputFile != null)
        {
            gif.SaveAsGif(outputFile);
            Console.Error.WriteLine($"Exported GIF with {drawContext.TotalFrames + 1} frames to: {outputFile}");
        }

        return 0;
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        IconProvider.Current
            .Register<FontAwesomeIconProvider>();

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }

    public static async Task ShowAboutBox()
    {
        var iconBitmap = new Bitmap(AssetLoader.Open(new Uri("avares://NAPLPSApp/Assets/naplps.ico")));

        var bigDescription = "The North American Presentation Level Protocol Syntax (NAPLPS) was a pioneering \ngraphic display standard that emerged during the early era of online services. \nAlthough largely confined to videotex and teletext experiments, it represented a \nmeaningful step toward a unified way of depicting graphics across disparate \nterminals. NAPLPS introduced vector-based images, scalable fonts, and a color \npalette that was advanced for its time, influencing subsequent standards.";

        var messageBoxParams = new MessageBoxCustomParams
        {
            ContentHeader = $"Version: {Version}\n\nA modern toolbox to read, save, create, and alter NAPLPS files, new and old!\nAn Open Source Project: https://github.com/FoxCouncil/NAPLPS",
            ContentTitle = "About NAPLPS Toolbox",
            ContentMessage = $"{bigDescription}\n\nCreated by Fox & Contributors!\n\tpheller\n\tportyspice",
            ButtonDefinitions = [new ButtonDefinition { Name = "Cool Beans!", IsDefault = true }],
            WindowIcon = new WindowIcon(iconBitmap), // Set the window icon
            ImageIcon = iconBitmap,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };

        var messageBox = MessageBoxManager.GetMessageBoxCustom(messageBoxParams);

        await messageBox.ShowAsync();
    }

    public static async Task<bool> ShowQuestionDialogBox(Window owner, string title, string question)
    {
        var iconBitmap = new Bitmap(AssetLoader.Open(new Uri("avares://NAPLPSApp/Assets/naplps.ico")));

        var messageBoxParams = new MessageBoxCustomParams
        {
            ContentHeader = title,
            ContentTitle = "Question",
            ContentMessage = question,
            ButtonDefinitions = [new ButtonDefinition { Name = "Yes", IsDefault = true }, new ButtonDefinition { Name = "No" }],
            WindowIcon = new WindowIcon(iconBitmap), // Set the window icon
            ImageIcon = iconBitmap,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };

        var messageBox = MessageBoxManager.GetMessageBoxCustom(messageBoxParams);

        var result = await messageBox.ShowWindowDialogAsync(owner);

        return result == "Yes";
    }

    private static string GetLibraryVersion()
    {
        var assembly = typeof(NaplpsFormat).Assembly;

        var info = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();

        if (!string.IsNullOrWhiteSpace(info?.Version))
        {
            return info.Version.ToString();
        }

        return assembly.GetName().Version?.ToString() ?? "?.?.?";
    }
}
