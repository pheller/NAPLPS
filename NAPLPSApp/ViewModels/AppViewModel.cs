// Copyright (c) 2024 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSApp.ViewModels;

public partial class AppViewModel : ViewModelBase
{
    public AppViewModel()
    {
    }

    [RelayCommand]
    private async Task About()
    {
        await Program.ShowAboutBox();
    }
}
