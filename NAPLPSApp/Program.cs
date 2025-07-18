// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using MsBox.Avalonia;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;

namespace NAPLPSApp;

sealed class Program
{
    public const string Version = "0.1.1";

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
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
}
