using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

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
