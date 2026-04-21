// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NAPLPSApp.Resources;

namespace NAPLPSApp;

public partial class App : Application
{
    public static Window? MainWindow { get; private set; }

    public override void Initialize()
    {
        DataContext = new AppViewModel();

        AvaloniaXamlLoader.Load(this);

        PlatformGestures.Register(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };

            desktop.MainWindow = MainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}