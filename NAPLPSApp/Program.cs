// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS


using NAPLPS;

namespace NAPLPSApp;

internal static class Program
{
    /// <summary>NAPLPS App - A toolbox and GUI system for modern NAPLPS rendering and creation</summary>
    [STAThread]
    static void Main(FileInfo input)
    {
        var naplpsFile = NaplpsFormat.FromFile("../../../../examples/beer.nap");

        FileFunctions.Convert(naplpsFile, "autumn");

        if (input != null)
        {
            ProccessInput(input);

            return;
        }

        Console.WriteLine("Starting GUI...");

        ApplicationConfiguration.Initialize();
        Application.Run(new MainNaplpsForm());
    }

    private static void ProccessInput(FileInfo input)
    {
        if (!input.Exists)
        {
            Console.WriteLine("NaplpsApp: Error, [input] file does not exist, check your path and try again.");

            return;
        }

        var naplpsFile = NaplpsFormat.FromFile(input.FullName);

        if (!naplpsFile.IsValid)
        {
            Console.WriteLine("NaplpsApp: Error, [input] file does not seem to be a valid NAPLPS formatted file");

            return;
        }

        var filename = input.Name;

        if (!FileFunctions.Convert(naplpsFile, filename))
        {
            Console.WriteLine("NaplpsApp: Error, [input] failed to convert.");

            return;
        }

        Console.WriteLine("NaplpsApp: [input] converted successfully.");
    }
}