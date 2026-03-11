// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using Avalonia.Platform.Storage;

using MsBox.Avalonia;

using NAPLPSApp.Editor;
using NAPLPSApp.Editor.Tools;

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
    private bool isPaletteAnimationMode;

    [ObservableProperty]
    private bool isLooping;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BaudRateDisplay))]
    private uint baudRate = 2400;

    public string BaudRateDisplay => BaudRate switch
    {
        0 => "Fastest",
        >= 1000000 => $"{BaudRate / 1000000.0:0.#}Mbps",
        >= 1000 => $"{BaudRate / 1000.0:0.#}Kbps",
        _ => $"{BaudRate}bps"
    };

    [ObservableProperty]
    private int totalFrames;

    [ObservableProperty]
    private int currentFrame;

    [ObservableProperty]
    private string canvasSize = "1024x768";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TitleBarDisplay))]
    private string titleBar = DEFAULT_APP_NAME + " [" + Program.Version + "]";

    [ObservableProperty]
    private string fileName = DEFAULT_NO_FILE_NAME;

    [ObservableProperty]
    private string bitWidth = "7-Bit";

    [ObservableProperty]
    private string fileSystemType = nameof(NaplpsSystemType.NAPLPS);

    [ObservableProperty]
    private bool isFileLoaded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TitleBarDisplay))]
    private bool isFileDirty;

    [ObservableProperty]
    private bool debugTextDrawing = Drawable.Options.DebugTextDrawing;

    private string loadedFilePath = string.Empty;

    private NaplpsFormat? loadedFile;

    private DrawContext? drawContext;

    private CancellationTokenSource? renderCancellationToken;

    private readonly SemaphoreSlim renderLock = new(1, 1);

    private Window? sequenceWindow;

    // Blink animation timer
    private Avalonia.Threading.DispatcherTimer? blinkTimer;

    // Editor state
    private readonly UndoManager undoManager = new();

    private readonly SelectTool selectTool = new();
    private readonly MovePenTool movePenTool = new();
    private readonly LineTool lineTool = new();
    private readonly RectangleTool rectangleTool = new();
    private readonly PolygonTool polygonTool = new();
    private readonly ArcTool arcTool = new();
    private readonly TextTool textTool = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelectToolActive))]
    [NotifyPropertyChangedFor(nameof(IsMovePenToolActive))]
    [NotifyPropertyChangedFor(nameof(IsLineToolActive))]
    [NotifyPropertyChangedFor(nameof(IsRectangleToolActive))]
    [NotifyPropertyChangedFor(nameof(IsPolygonToolActive))]
    [NotifyPropertyChangedFor(nameof(IsArcToolActive))]
    [NotifyPropertyChangedFor(nameof(IsTextToolActive))]
    [NotifyPropertyChangedFor(nameof(ActiveToolName))]
    private EditorToolBase activeTool;

    [ObservableProperty]
    private bool isEditorMode;

    [ObservableProperty]
    private bool isFilledMode = true;

    [ObservableProperty]
    private byte editorForegroundIndex = 7; // White

    [ObservableProperty]
    private byte editorBackgroundIndex = 0; // Black

    [ObservableProperty]
    private ToolPreview? editorPreview;

    [ObservableProperty]
    private GridSettings gridSettings = new();

    [ObservableProperty]
    private int selectedCommandIndex = -1;

    [ObservableProperty]
    private IBrush foregroundColorBrush = new SolidColorBrush(Avalonia.Media.Colors.White);

    [ObservableProperty]
    private IBrush backgroundColorBrush = new SolidColorBrush(Avalonia.Media.Colors.Black);

    [ObservableProperty]
    private ObservableCollection<PaletteColor> paletteColors = [];

    public string TitleBarDisplay => IsFileDirty ? $"*{TitleBar}" : TitleBar;

    public bool IsSelectToolActive => ActiveTool is SelectTool;
    public bool IsMovePenToolActive => ActiveTool is MovePenTool;
    public bool IsLineToolActive => ActiveTool is LineTool;
    public bool IsRectangleToolActive => ActiveTool is RectangleTool;
    public bool IsPolygonToolActive => ActiveTool is PolygonTool;
    public bool IsArcToolActive => ActiveTool is ArcTool;
    public bool IsTextToolActive => ActiveTool is TextTool;
    public string ActiveToolName => ActiveTool?.Name ?? "Select";

    public MainWindowViewModel()
    {
        activeTool = selectTool;
        InitializePalette();
    }

    #region File Menu Handlers

    [RelayCommand]
    private async Task New()
    {
        if (!await PromptSaveIfDirty()) return;

        await FileNew();

        await UpdateCanvas();
    }

    [RelayCommand]
    private async Task Open()
    {
        if (!await PromptSaveIfDirty()) return;

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

        if (saveFilePath != DEFAULT_NEW_FILE_NAME)
        {
            loadedFilePath = saveFilePath;
            FileName = IOPath.GetFileName(saveFilePath);
            TitleBar = $"{FileName} - {DEFAULT_APP_NAME} [{Program.Version}]";
        }

        IsFileDirty = false;
    }

    [RelayCommand]
    private async Task SaveAs()
    {
        if (loadedFile == null || App.MainWindow?.StorageProvider == null) return;

        var fileTypes = new FilePickerFileType[]
        {
            new("NAPLPS Files") { Patterns = ["*.nap"] }
        };

        var options = new FilePickerSaveOptions
        {
            Title = "Save NAPLPS File As",
            FileTypeChoices = fileTypes,
            DefaultExtension = "nap",
            SuggestedFileName = IOPath.GetFileName(loadedFilePath)
        };

        var result = await App.MainWindow.StorageProvider.SaveFilePickerAsync(options);

        if (result != null)
        {
            var savePath = result.Path.LocalPath;
            loadedFile.Save(savePath);
            loadedFilePath = savePath;
            FileName = IOPath.GetFileName(savePath);
            TitleBar = $"{FileName} - {DEFAULT_APP_NAME} [{Program.Version}]";
            IsFileDirty = false;
        }
    }

    /// <summary>
    /// Returns true if it's safe to proceed (user saved or chose Don't Save).
    /// Returns false if user cancelled.
    /// </summary>
    private async Task<bool> PromptSaveIfDirty()
    {
        if (!IsFileDirty || App.MainWindow == null) return true;

        var result = await Program.ShowQuestionDialogBox(
            App.MainWindow,
            "Unsaved Changes",
            "You have unsaved changes. Do you want to save before continuing?");

        if (result)
        {
            await Save();
        }

        // Always proceed (Yes=saved, No=discard)
        return true;
    }

    [RelayCommand]
    private async Task Close()
    {
        if (!await PromptSaveIfDirty()) return;

        FileClose();
    }

    [RelayCommand]
    private async Task Export()
    {
        if (App.MainWindow == null || drawContext == null)
        {
            return;
        }

        var fileTypes = new FilePickerFileType[]
        {
            new("PNG Image") { Patterns = ["*.png"] }
        };

        var options = new FilePickerSaveOptions
        {
            Title = "Export NAPLPS File to PNG",
            DefaultExtension = "png",
            FileTypeChoices = fileTypes,
            SuggestedFileName = IOPath.GetFileNameWithoutExtension(loadedFilePath)
        };

        var result = await App.MainWindow.StorageProvider.SaveFilePickerAsync(options);

        if (result != null)
        {
            drawContext.SaveAsPng(result.Path.LocalPath);
        }
    }

    [RelayCommand]
    private async Task Quit()
    {
        if (!await PromptSaveIfDirty()) return;

        FileClose();

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

            // Only restart rendering if animation is active
            if (IsAnimated)
            {
                await UpdateCanvas();
            }
        }
    }

    [RelayCommand]
    private async Task TogglePaletteAnimation()
    {
        IsPaletteAnimationMode = !IsPaletteAnimationMode;

        if (drawContext != null)
        {
            drawContext.PaletteAnimationMode = IsPaletteAnimationMode;
        }

        await UpdateCanvas();
    }

    [RelayCommand]
    private async Task ToggleLoop()
    {
        IsLooping = !IsLooping;

        if (!IsLooping)
        {
            // Stop current loop by cancelling render
            renderCancellationToken?.Cancel();
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

    #region Editor Handlers

    [RelayCommand]
    private void ToggleToolbox()
    {
        IsEditorMode = !IsEditorMode;

        if (IsEditorMode)
        {
            ActiveTool = selectTool;
        }
    }

    [RelayCommand]
    private void SetActiveTool(string toolName)
    {
        // Commit any pending text before switching tools
        if (ActiveTool is TextTool tt && tt.HasPendingCommit)
        {
            CommitToolCommands(tt.CommitText());
        }

        ActiveTool = toolName switch
        {
            "MovePen" => movePenTool,
            "Line" => lineTool,
            "Rectangle" => rectangleTool,
            "Polygon" => polygonTool,
            "Arc" => arcTool,
            "Text" => textTool,
            _ => selectTool
        };

        // Pass format to SelectTool for hit testing
        if (ActiveTool is SelectTool st)
        {
            st.Format = loadedFile;
            st.SelectedIndex = -1;
            SelectedCommandIndex = -1;
        }
    }

    [RelayCommand]
    private async Task Undo()
    {
        if (loadedFile == null || !undoManager.CanUndo) return;

        undoManager.Undo(loadedFile);
        IsFileDirty = true;
        BuildDrawContext();
        await UpdateCanvas();
    }

    [RelayCommand]
    private async Task Redo()
    {
        if (loadedFile == null || !undoManager.CanRedo) return;

        undoManager.Redo(loadedFile);
        IsFileDirty = true;
        BuildDrawContext();
        await UpdateCanvas();
    }

    [RelayCommand]
    private void SelectPaletteColor(byte index)
    {
        EditorForegroundIndex = index;
        UpdateColorBrushes();
    }

    [RelayCommand]
    private void ToggleGrid()
    {
        GridSettings.IsVisible = !GridSettings.IsVisible;
    }

    [RelayCommand]
    private void ToggleSnap()
    {
        GridSettings.IsSnapEnabled = !GridSettings.IsSnapEnabled;
    }

    [RelayCommand]
    private void SetGridSpacing(string spacing)
    {
        float value = spacing switch
        {
            "1/10" => 1.0f / 10.0f,
            "1/20" => 1.0f / 20.0f,
            "1/40" => 1.0f / 40.0f,
            "1/80" => 1.0f / 80.0f,
            _ => 1.0f / 40.0f
        };
        GridSettings.SpacingX = value;
        GridSettings.SpacingY = value * 0.75f; // Maintain 4:3 aspect ratio
    }

    partial void OnIsFilledModeChanged(bool value)
    {
        rectangleTool.IsFilled = value;
        polygonTool.IsFilled = value;
        arcTool.IsFilled = value;
    }

    private void InitializePalette()
    {
        var colors = new ObservableCollection<PaletteColor>();

        foreach (var kvp in NaplpsState.ColorMapDefaults)
        {
            var nc = kvp.Value;
            var avaloniaColor = Avalonia.Media.Color.FromRgb(
                (byte)(nc.Red * 255),
                (byte)(nc.Green * 255),
                (byte)(nc.Blue * 255));

            colors.Add(new PaletteColor
            {
                Index = kvp.Key,
                Brush = new SolidColorBrush(avaloniaColor)
            });
        }

        PaletteColors = colors;
    }

    private void UpdateColorBrushes()
    {
        if (NaplpsState.ColorMapDefaults.TryGetValue(EditorForegroundIndex, out var fgColor))
        {
            ForegroundColorBrush = new SolidColorBrush(Avalonia.Media.Color.FromRgb(
                (byte)(fgColor.Red * 255),
                (byte)(fgColor.Green * 255),
                (byte)(fgColor.Blue * 255)));
        }

        if (NaplpsState.ColorMapDefaults.TryGetValue(EditorBackgroundIndex, out var bgColor))
        {
            BackgroundColorBrush = new SolidColorBrush(Avalonia.Media.Color.FromRgb(
                (byte)(bgColor.Red * 255),
                (byte)(bgColor.Green * 255),
                (byte)(bgColor.Blue * 255)));
        }
    }

    [RelayCommand]
    private async Task DeleteSelected()
    {
        if (loadedFile == null || SelectedCommandIndex < 0) return;

        var action = new RemoveCommandsAction(loadedFile, SelectedCommandIndex);
        undoManager.Execute(action, loadedFile);
        IsFileDirty = true;

        SelectedCommandIndex = -1;
        if (ActiveTool is SelectTool st)
        {
            st.SelectedIndex = -1;
        }

        BuildDrawContext();
        await UpdateCanvas();
    }

    // Called from MainWindow code-behind
    public void OnEditorPointerPressed(Avalonia.Point pos, Avalonia.Size controlSize, bool isRightButton)
    {
        if (!IsEditorMode || loadedFile == null) return;

        if (isRightButton)
        {
            // Right-click on palette sets background color (handled by SelectPaletteColor)
            return;
        }

        var canvasSizeObj = GetSizeObj();
        var (normX, normY) = CoordinateMapper.ScreenToNaplps(pos, controlSize, canvasSizeObj, ImageStretch);
        normX = GridSettings.SnapX(normX);
        normY = GridSettings.SnapY(normY);
        ActiveTool.OnPointerPressed(normX, normY, isRightButton);

        // Update selection state from SelectTool
        if (ActiveTool is SelectTool st)
        {
            SelectedCommandIndex = st.SelectedIndex;
            EditorPreview = st.GetPreview();
        }
    }

    public void OnEditorPointerMoved(Avalonia.Point pos, Avalonia.Size controlSize)
    {
        if (!IsEditorMode || loadedFile == null) return;

        var canvasSizeObj = GetSizeObj();
        var (normX, normY) = CoordinateMapper.ScreenToNaplps(pos, controlSize, canvasSizeObj, ImageStretch);
        normX = GridSettings.SnapX(normX);
        normY = GridSettings.SnapY(normY);
        ActiveTool.OnPointerMoved(normX, normY);

        // Update rubber-band preview
        EditorPreview = ActiveTool.GetPreview();
    }

    public async void OnEditorPointerReleased(Avalonia.Point pos, Avalonia.Size controlSize)
    {
        if (!IsEditorMode || loadedFile == null) return;

        var canvasSizeObj = GetSizeObj();
        var (normX, normY) = CoordinateMapper.ScreenToNaplps(pos, controlSize, canvasSizeObj, ImageStretch);
        normX = GridSettings.SnapX(normX);
        normY = GridSettings.SnapY(normY);

        var commands = ActiveTool.OnPointerReleased(normX, normY);

        // Clear preview on release
        EditorPreview = null;

        if (commands.Count == 0) return;

        // Prepend a SelectColor command if we're not using the default color
        if (EditorForegroundIndex != 0)
        {
            var (colorOp, colorOps) = NaplpsCommandBuilder.BuildSelectColor(EditorForegroundIndex);
            commands.Insert(0, (colorOp, colorOps));
        }

        var action = new AddCommandsAction(commands);
        undoManager.Execute(action, loadedFile);
        IsFileDirty = true;

        // Re-render
        BuildDrawContext();
        await UpdateCanvas();
    }

    /// <summary>
    /// Commits a list of NAPLPS commands with optional color prefix and undo support.
    /// Shared by OnEditorPointerReleased and tool-switch text commit.
    /// </summary>
    private async void CommitToolCommands(List<(byte opcode, NaplpsOperands operands)> commands)
    {
        if (loadedFile == null || commands.Count == 0) return;

        // Prepend a SelectColor command if we're not using the default color
        if (EditorForegroundIndex != 0)
        {
            var (colorOp, colorOps) = NaplpsCommandBuilder.BuildSelectColor(EditorForegroundIndex);
            commands.Insert(0, (colorOp, colorOps));
        }

        var action = new AddCommandsAction(commands);
        undoManager.Execute(action, loadedFile);
        IsFileDirty = true;

        BuildDrawContext();
        await UpdateCanvas();
    }

    /// <summary>Called from code-behind when a key is pressed while TextTool is active.</summary>
    public void OnEditorTextInput(char c)
    {
        if (ActiveTool is TextTool tt)
        {
            tt.OnKeyDown(c);
        }
    }

    /// <summary>Called from code-behind when Enter is pressed while TextTool is active.</summary>
    public void OnEditorTextCommit()
    {
        if (ActiveTool is TextTool tt && tt.HasPendingCommit)
        {
            CommitToolCommands(tt.CommitText());
        }
    }

    /// <summary>Called from code-behind when click count > 1 for PolygonTool.</summary>
    public void SetClickCount(int clickCount)
    {
        if (ActiveTool is PolygonTool pt)
        {
            pt.ClickCount = clickCount;
        }
    }

    public SixLabors.ImageSharp.Size GetSizeObj()
    {
        var (w, h) = GetSize();
        return new SixLabors.ImageSharp.Size(w, h);
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

        // Cancel any ongoing render
        renderCancellationToken?.Cancel();

        // Wait for the lock to ensure previous render is complete
        await renderLock.WaitAsync();

        try
        {
            renderCancellationToken = new CancellationTokenSource();
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
        finally
        {
            renderLock.Release();
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
        FileSystemType = loadedFile.SystemType.ToString();

        await UpdateCanvas();
    }

    private void StartBlinkTimer()
    {
        StopBlinkTimer();

        if (drawContext?.BlinkAnimator == null || !drawContext.BlinkAnimator.HasActiveProcesses)
            return;

        blinkTimer = new Avalonia.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16) // ~60Hz
        };

        blinkTimer.Tick += (_, _) =>
        {
            if (drawContext != null && drawContext.TickBlink(16))
            {
                CanvasImage = drawContext.ToBitmap();
            }
        };

        blinkTimer.Start();
    }

    private void StopBlinkTimer()
    {
        blinkTimer?.Stop();
        blinkTimer = null;
    }

    private void FileClose()
    {
        StopBlinkTimer();

        drawContext?.Dispose();
        drawContext = null;

        CanvasImage = null;

        IsFileLoaded = false;
        IsEditorMode = false;
        IsFileDirty = false;

        sequenceWindow?.Close();
        sequenceWindow = null;

        undoManager.Clear();

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
            FileSystemType = loadedFile.SystemType.ToString();

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
        drawContext.PaletteAnimationMode = IsPaletteAnimationMode;

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

        // Stop blink timer before starting a new render to prevent
        // concurrent Render calls during RenderAsync's Task.Delay gaps
        StopBlinkTimer();

        // Cancel any ongoing render
        renderCancellationToken?.Cancel();

        // Wait for the lock to ensure previous render is complete
        await renderLock.WaitAsync();

        try
        {
            renderCancellationToken = new CancellationTokenSource();

            if (IsAnimated)
            {
                uint delayInMilliseconds = (uint)(BaudRate == 0 ? 0 : (loadedFile.Commands.Count * 8 * 1000.0 / BaudRate));

                if (delayInMilliseconds == 0)
                {
                    delayInMilliseconds = 16;
                }

                do
                {
                    await drawContext.RenderAsync(renderCancellationToken.Token, delayInMilliseconds);

                    if (IsLooping && !renderCancellationToken.Token.IsCancellationRequested)
                    {
                        await Task.Delay(500, renderCancellationToken.Token);
                    }
                } while (IsLooping && !renderCancellationToken.Token.IsCancellationRequested);
            }
            else
            {
                drawContext.Render();
            }

            // Initialize blink animation after render completes
            drawContext.InitializeBlinkAnimator();
            if (drawContext.BlinkAnimator?.HasActiveProcesses == true)
            {
                StartBlinkTimer();
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
        finally
        {
            renderLock.Release();
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
