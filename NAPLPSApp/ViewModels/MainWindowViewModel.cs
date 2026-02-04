// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using Avalonia.Platform.Storage;

using MsBox.Avalonia;

namespace NAPLPSApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private const string DEFAULT_APP_NAME = "NAPLPS Toolbox";

    private const string DEFAULT_NEW_FILE_NAME = "[Untitled]";

    private const string DEFAULT_NO_FILE_NAME = "Idle";

    [ObservableProperty]
    private Stretch imageStretch = Stretch.None;

    [ObservableProperty]
    private Bitmap? canvasImage;

    [ObservableProperty]
    private bool isAnimated;

    [ObservableProperty]
    private uint baudRate = 2400;

    [ObservableProperty]
    private int totalFrames;

    [ObservableProperty]
    private int currentFrame;

    [ObservableProperty]
    private string canvasSize = "1024x768";

    [ObservableProperty]
    private string titleBar = DEFAULT_APP_NAME + " [" + Program.Version + "]";

    [ObservableProperty]
    private string fileName = DEFAULT_NO_FILE_NAME;

    [ObservableProperty]
    private string bitWidth = "7-Bit";

    [ObservableProperty]
    private bool isFileLoaded;

    [ObservableProperty]
    private bool debugTextDrawing = Drawable.Options.DebugTextDrawing;

    private string loadedFilePath = string.Empty;

    private NaplpsFormat? loadedFile;

    private DrawContext? drawContext;

    private CancellationTokenSource? renderCancellationToken;

    private Window? sequenceWindow;

    public MainWindowViewModel()
    {
    }

    #region File Menu Handlers

    [RelayCommand]
    private async Task New()
    {
        await FileNew();

        await UpdateCanvas();
    }

    [RelayCommand]
    private async Task Open()
    {
        var storageProvider = App.MainWindow?.StorageProvider;

        if (storageProvider == null)
        {
            return;
        }

        var fileTypes = new FilePickerFileType[]
        {
            new("NAPLPS Files") { Patterns = ["*.nap", "*.naplps"] },
            new("All Files") { Patterns = ["*.*"] }
        };

        var options = new FilePickerOpenOptions
        {
            Title = "Open NAPLPS File",
            FileTypeFilter = fileTypes,
            AllowMultiple = false // TODO: Billboards
        };

        var result = await storageProvider.OpenFilePickerAsync(options);

        if (result.Count > 0)
        {
            await FileLoad(result[0].Path.LocalPath);
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (loadedFile == null || App.MainWindow?.StorageProvider == null)
        {
            return;
        }

        var saveFilePath = loadedFilePath;

        if (saveFilePath == DEFAULT_NEW_FILE_NAME)
        {
            var fileTypes = new FilePickerFileType[]
            {
            new("NAPLPS Files")
            {
                Patterns = ["*.nap"]
            }
            };

            var options = new FilePickerSaveOptions
            {
                Title = "Save NAPLPS File",
                FileTypeChoices = fileTypes,
                DefaultExtension = "nap",
                SuggestedFileName = IOPath.GetFileName(loadedFilePath)
            };

            var result = await App.MainWindow.StorageProvider.SaveFilePickerAsync(options);

            if (result != null)
            {
                saveFilePath = result.Path.LocalPath;
            }
        }

        loadedFile.Save(saveFilePath);
    }

    [RelayCommand]
    private void Close()
    {
        FileClose();
    }

    [RelayCommand]
    private async Task Export()
    {
        if (App.MainWindow == null || drawContext == null)
        {
            return;
        }

        var options = new FilePickerSaveOptions
        {
            Title = "Export NAPLPS File to PNG",
            DefaultExtension = "png",
        };

        var result = await App.MainWindow.StorageProvider.SaveFilePickerAsync(options);

        if (result != null)
        {
            drawContext.SaveAsPng(result.Path.LocalPath);
        }
    }

    [RelayCommand]
    private void Quit()
    {
        FileClose();

        // Save-on-close feature?

        App.MainWindow?.Close();
    }

    #endregion

    #region Render Menu Handlers

    [RelayCommand]
    private async Task Rerender()
    {
        await UpdateCanvas();
    }

    [RelayCommand]
    private async Task ToggleAnimate()
    {
        IsAnimated = !IsAnimated;

        await UpdateCanvas();
    }

    [RelayCommand]
    private async Task SetBaudRate(string rate)
    {
        if (uint.TryParse(rate, out uint newRate))
        {
            BaudRate = newRate;

            await UpdateCanvas();
        }
    }

    [RelayCommand]
    private async Task ToggleLayers()
    {
        await MessageBoxManager.GetMessageBoxStandard("The Future", "This would've triggered the layers window/pane?").ShowAsync();
    }

    [RelayCommand]
    private async Task ToggleDebugTextDrawing()
    {
        DebugTextDrawing = !DebugTextDrawing;
        Drawable.Options.DebugTextDrawing = DebugTextDrawing;

        await RenderToFrame(CurrentFrame - 1);
    }

    #endregion

    #region View Menu Handlers

    [RelayCommand]
    private void ToggleSequence()
    {
        if (loadedFile == null || drawContext == null)
        {
            return;
        }

        if (sequenceWindow == null)
        {
            sequenceWindow = new SequenceWindow(drawContext);

            if (sequenceWindow.DataContext == null)
            {
                return;
            }

            ((SequenceWindowViewModel)sequenceWindow.DataContext).FrameChanged += async (sender, index) =>
            {
                await RenderToFrame(index);
            };

            sequenceWindow.Closed += (s, e) => sequenceWindow = null;
            sequenceWindow.Show();
        }
        else
        {
            sequenceWindow.Close();
            sequenceWindow = null;
        }
    }

    [RelayCommand]
    private async Task SetCanvasSize(string size)
    {
        CanvasSize = size;

        if (loadedFile != null)
        {

            BuildDrawContext();

            await UpdateCanvas();
        }
    }

    [RelayCommand]
    private void SetStretchMode(string mode)
    {
        // Parse the string to Stretch enum
        if (Enum.TryParse<Stretch>(mode, out var stretch))
        {
            ImageStretch = stretch;
        }
    }

    #endregion

    #region Help Menu Handlers

    [RelayCommand]
    private async Task Help()
    {
        if (App.MainWindow == null || App.MainWindow.Launcher == null)
        {
            return;
        }

        await App.MainWindow.Launcher.LaunchUriAsync(new Uri("https://github.com/FoxCouncil/NAPLPS/issues"));
    }

    [RelayCommand]
    private async Task GitHub()
    {
        if (App.MainWindow == null || App.MainWindow.Launcher == null)
        {
            return;
        }

        await App.MainWindow.Launcher.LaunchUriAsync(new Uri("https://github.com/FoxCouncil/NAPLPS"));
    }

    [RelayCommand]
    private async Task About()
    {
        await Program.ShowAboutBox();
    }

    #endregion

    #region Frame Navigation Handlers

    [RelayCommand]
    private async Task NextFrame()
    {
        if (drawContext == null || !IsFileLoaded)
        {
            return;
        }

        if (CurrentFrame < TotalFrames)
        {
            await RenderToFrame(CurrentFrame); // CurrentFrame is 1-based, so this renders the next frame
        }
    }

    [RelayCommand]
    private async Task PreviousFrame()
    {
        if (drawContext == null || !IsFileLoaded)
        {
            return;
        }

        if (CurrentFrame > 1)
        {
            await RenderToFrame(CurrentFrame - 2); // Go back one frame (CurrentFrame is 1-based)
        }
    }

    [RelayCommand]
    private async Task FirstFrame()
    {
        if (drawContext == null || !IsFileLoaded)
        {
            return;
        }

        await RenderToFrame(0);
    }

    [RelayCommand]
    private async Task LastFrame()
    {
        if (drawContext == null || !IsFileLoaded)
        {
            return;
        }

        await RenderToFrame((int)drawContext.TotalFrames);
    }

    private async Task RenderToFrame(int frameIndex)
    {
        if (drawContext == null)
        {
            return;
        }

        renderCancellationToken?.Cancel();
        renderCancellationToken = new CancellationTokenSource();

        try
        {
            await Task.Run(() => drawContext.Render((uint)frameIndex), renderCancellationToken.Token);
        }
        catch (OperationCanceledException)
        {
            // Rendering was cancelled, ignore
        }
        catch (Exception ex)
        {
            if (App.MainWindow != null)
            {
                await MessageBoxManager.GetMessageBoxStandard("Error", $"Failed to render frame: {ex.Message}").ShowAsync();
            }
        }
    }

    #endregion

    private async Task FileNew()
    {
        FileClose();

        loadedFile = NaplpsFormat.New();

        IsFileLoaded = true;

        loadedFilePath = DEFAULT_NEW_FILE_NAME;

        BuildDrawContext();

        IsFileLoaded = true;

        FileName = DEFAULT_NEW_FILE_NAME;

        TitleBar = $"{FileName} - {DEFAULT_APP_NAME} [{Program.Version}]";

        BitWidth = loadedFile.Is7Bit ? "7-Bit" : "8-Bit";

        await UpdateCanvas();
    }

    private void FileClose()
    {
        drawContext?.Dispose();
        drawContext = null;

        CanvasImage = null;

        IsFileLoaded = false;

        sequenceWindow?.Close();
        sequenceWindow = null;

        loadedFile = null;
        loadedFilePath = string.Empty;

        FileName = DEFAULT_NO_FILE_NAME;

        TitleBar = $"{DEFAULT_APP_NAME} [{Program.Version}]";
    }

    private async Task FileLoad(string filePath)
    {
        FileClose();

        try
        {
            loadedFile = NaplpsFormat.FromFile(filePath);

            loadedFilePath = filePath;

            BuildDrawContext();

            IsFileLoaded = true;

            FileName = IOPath.GetFileName(filePath);

            TitleBar = $"{FileName} - {DEFAULT_APP_NAME} [{Program.Version}]";

            BitWidth = loadedFile.Is7Bit ? "7-Bit" : "8-Bit";

            await UpdateCanvas();
        }
        catch (Exception ex)
        {
            if (App.MainWindow != null)
            {
                await MessageBoxManager.GetMessageBoxStandard("Error", $"Failed to load file: {ex.Message}").ShowAsync();
            }
        }
    }

    private void BuildDrawContext()
    {
        if (loadedFile == null)
        {
            return;
        }

        var (width, height) = GetSize();

        drawContext?.Dispose();

        drawContext = new DrawContext(loadedFile, new SixLabors.ImageSharp.Size(width, height));

        drawContext.OnImageUpdated += () =>
        {
            CurrentFrame = (int)drawContext.CurrentIndex + 1;
            CanvasImage = drawContext.ToBitmap();
        };

        TotalFrames = (int)drawContext.TotalFrames + 1;

        CurrentFrame = (int)drawContext.CurrentIndex + 1;
    }

    private void FileSave(string filePath)
    {
        loadedFile?.Save(filePath);
    }

    private async Task UpdateCanvas()
    {
        if (loadedFile == null) return;
        if (drawContext == null) return;

        renderCancellationToken?.Cancel();
        renderCancellationToken = new CancellationTokenSource();

        try
        {
            if (IsAnimated)
            {
                uint delayInMilliseconds = (uint)(BaudRate == 0 ? 0 : (loadedFile.Commands.Count * 8 * 1000.0 / BaudRate));

                if (delayInMilliseconds == 0)
                {
                    delayInMilliseconds = 16;
                }

                await drawContext.RenderAsync(renderCancellationToken.Token, delayInMilliseconds);
            }
            else
            {
                drawContext.Render();
            }
        }
        catch (OperationCanceledException)
        {
            // Rendering was cancelled, ignore
        }
        catch (Exception ex)
        {
            if (App.MainWindow != null)
            {
                var messageBox = MessageBoxManager.GetMessageBoxStandard("Error", $"Failed to update canvas: {ex.Message}");

                await messageBox.ShowAsync();
            }
        }
    }

    private (int, int) GetSize()
    {
        var parts = CanvasSize.Split('x');

        if (parts.Length != 2)
        {
            throw new ApplicationException("Malformed CanvasSize!");
        }

        if (!int.TryParse(parts[0], out int width) || !int.TryParse(parts[1], out int height))
        {
            throw new ApplicationException("Malformed CanvasSize!");
        }

        return (width, height);
    }
}
