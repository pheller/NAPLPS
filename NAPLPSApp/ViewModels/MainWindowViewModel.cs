// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using Avalonia.Platform.Storage;

using MsBox.Avalonia;

using NAPLPSApp.Editor;
using NAPLPSApp.Editor.Tools;

namespace NAPLPSApp.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
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
    private readonly FillTool fillTool = new();
    private readonly IncrementalLineTool incrementalLineTool = new();
    private readonly IncrementalPolygonTool incrementalPolygonTool = new();
    private readonly IncrementalPointTool incrementalPointTool = new();

    /// <summary>
    /// Exposes the TextTool instance to XAML so the Attributes panel can bind directly
    /// to its properties (CharWidth, CharHeight, Rotation, Path, Spacing, Interrow).
    /// </summary>
    public TextTool TextToolInstance => textTool;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelectToolActive))]
    [NotifyPropertyChangedFor(nameof(IsMovePenToolActive))]
    [NotifyPropertyChangedFor(nameof(IsLineToolActive))]
    [NotifyPropertyChangedFor(nameof(IsRectangleToolActive))]
    [NotifyPropertyChangedFor(nameof(IsPolygonToolActive))]
    [NotifyPropertyChangedFor(nameof(IsArcToolActive))]
    [NotifyPropertyChangedFor(nameof(IsTextToolActive))]
    [NotifyPropertyChangedFor(nameof(IsFillToolActive))]
    [NotifyPropertyChangedFor(nameof(IsIncrementalLineToolActive))]
    [NotifyPropertyChangedFor(nameof(IsIncrementalPolygonToolActive))]
    [NotifyPropertyChangedFor(nameof(IsIncrementalPointToolActive))]
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

    /// <summary>
    /// NAPLPS-normalized cursor coordinates updated on canvas pointer-move.
    /// Bind to a status-bar TextBlock to surface the user's current pointer position.
    /// </summary>
    [ObservableProperty]
    private string coordReadout = string.Empty;

    /// <summary>True while the macro recorder is actively capturing tool commits into the macro buffer.</summary>
    [ObservableProperty]
    private bool isMacroRecording;

    /// <summary>
    /// The character slot the current recording will be saved into when StopMacroRecording
    /// fires. Defaults to 'A'; user can change it before starting a new recording.
    /// </summary>
    [ObservableProperty]
    private char macroRecordingSlot = 'A';

    /// <summary>Buffered raw bytes of the in-progress macro recording. Flushed on StopMacroRecording.</summary>
    private readonly List<byte> _macroRecordingBuffer = [];

    /// <summary>
    /// The Telidraw source text for the currently loaded file. Decompiled automatically
    /// from .nap on load; editable in the text pane. Changes trigger a debounced recompile
    /// that updates the canvas. For .td files, this is the source-of-truth.
    /// </summary>
    [ObservableProperty]
    private string telidrawSource = string.Empty;

    /// <summary>Set by the text-pane edit handler; cleared after recompile fires.</summary>
    private bool _telidrawSourceDirty;

    /// <summary>
    /// Set true while programmatically updating <see cref="TelidrawSource"/> (FileLoad,
    /// SyncTelidrawFromFormat, decompile-after-tool). Prevents <see cref="OnTelidrawSourceChanged"/>
    /// from interpreting our own write as a user keystroke and triggering a recompile/feedback loop.
    /// </summary>
    private bool _suppressTelidrawRecompile;

    /// <summary>Whether the Telidraw text pane is visible (toggle via View menu or button).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TelidrawColumnWidth))]
    [NotifyPropertyChangedFor(nameof(TelidrawSplitterWidth))]
    private bool isTelidrawPaneVisible;

    /// <summary>GridLength for the Telidraw column: 400px when visible, 0 when hidden.
    /// Bound by the main layout so the column doesn't reserve space when the pane is off
    /// (otherwise the canvas leaves blackspace between itself and the right edge).</summary>
    public Avalonia.Controls.GridLength TelidrawColumnWidth =>
        IsTelidrawPaneVisible ? new Avalonia.Controls.GridLength(400) : new Avalonia.Controls.GridLength(0);

    /// <summary>GridLength for the splitter column: 4px when Telidraw pane visible, 0 otherwise.</summary>
    public Avalonia.Controls.GridLength TelidrawSplitterWidth =>
        IsTelidrawPaneVisible ? new Avalonia.Controls.GridLength(4) : new Avalonia.Controls.GridLength(0);

    [ObservableProperty]
    private IBrush foregroundColorBrush = new SolidColorBrush(Avalonia.Media.Colors.White);

    [ObservableProperty]
    private IBrush backgroundColorBrush = new SolidColorBrush(Avalonia.Media.Colors.Black);

    [ObservableProperty]
    private ObservableCollection<PaletteColor> paletteColors = [];

    [ObservableProperty]
    private ObservableCollection<DiagnosticItem> diagnosticItems = [];

    [ObservableProperty]
    private bool hasDiagnostics;

    [ObservableProperty]
    private int errorCount;

    [ObservableProperty]
    private int warningCount;

    [ObservableProperty]
    private bool isDiagnosticsPanelVisible;

    public string TitleBarDisplay => IsFileDirty ? $"*{TitleBar}" : TitleBar;

    public bool IsSelectToolActive => ActiveTool is SelectTool;
    public bool IsMovePenToolActive => ActiveTool is MovePenTool;
    public bool IsLineToolActive => ActiveTool is LineTool;
    public bool IsRectangleToolActive => ActiveTool is RectangleTool;
    public bool IsPolygonToolActive => ActiveTool is PolygonTool;
    public bool IsArcToolActive => ActiveTool is ArcTool;
    public bool IsTextToolActive => ActiveTool is TextTool;
    public bool IsFillToolActive => ActiveTool is FillTool;
    public bool IsIncrementalLineToolActive => ActiveTool is IncrementalLineTool;
    public bool IsIncrementalPolygonToolActive => ActiveTool is IncrementalPolygonTool;
    public bool IsIncrementalPointToolActive => ActiveTool is IncrementalPointTool;
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
        if (!await PromptSaveIfDirty())
        {
            return;
        }

        await FileNew();

        // Auto-open the editor toolbox on a fresh document so the user lands directly in
        // authoring mode without having to enable it from the View menu.
        IsEditorMode = true;

        await UpdateCanvas();
    }

    [RelayCommand]
    private async Task Open()
    {
        if (!await PromptSaveIfDirty())
        {
            return;
        }

        var storageProvider = App.MainWindow?.StorageProvider;

        if (storageProvider == null)
        {
            return;
        }

        var fileTypes = new FilePickerFileType[]
        {
            new("NAPLPS Files") { Patterns = ["*.nap", "*.naplps"] },
            new("Telidraw Source") { Patterns = ["*.td"] },
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
        if (loadedFile == null || App.MainWindow?.StorageProvider == null)
        {
            return;
        }

        var fileTypes = new FilePickerFileType[]
        {
            new("NAPLPS Files") { Patterns = ["*.nap"] },
            new("Telidraw Source") { Patterns = ["*.td"] }
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

            if (savePath.EndsWith(".td", StringComparison.OrdinalIgnoreCase))
            {
                // Save as Telidraw source — decompile the current format to text.
                var tdSource = NAPLPS.Telidraw.Decompiler.Decompile(loadedFile);
                await System.IO.File.WriteAllTextAsync(savePath, tdSource);
            }
            else
            {
                loadedFile.Save(savePath);
            }

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
        if (!IsFileDirty || App.MainWindow == null)
        {
            return true;
        }

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
        if (!await PromptSaveIfDirty())
        {
            return;
        }

        FileClose();
    }

    /// <summary>
    /// Import an SVG file as Telidraw source. Parses the SVG path commands into a `.td`
    /// string, then compiles that into a new NaplpsFormat so the canvas renders it.
    /// Subset support (M/L/H/V/Z) — beziers approximate as line-to-endpoint.
    /// </summary>
    [RelayCommand]
    private async Task ImportSvg()
    {
        if (App.MainWindow == null) { return; }

        var picker = await App.MainWindow.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "Import SVG",
            AllowMultiple = false,
            FileTypeFilter = [new Avalonia.Platform.Storage.FilePickerFileType("SVG") { Patterns = ["*.svg"] }]
        });
        if (picker.Count == 0) { return; }

        var svgXml = await System.IO.File.ReadAllTextAsync(picker[0].Path.LocalPath);
        var td = NAPLPS.Import.SvgImporter.ToTelidraw(svgXml);

        await LoadTelidrawSource(td);
    }

    /// <summary>
    /// Import a bitmap (PNG/JPG/BMP/GIF) as a quantized 40x30 cell-grid Telidraw scene.
    /// Resizes and nearest-color-matches against the current palette defaults.
    /// </summary>
    [RelayCommand]
    private async Task ImportBitmap()
    {
        if (App.MainWindow == null) { return; }

        var picker = await App.MainWindow.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "Import Bitmap",
            AllowMultiple = false,
            FileTypeFilter = [new Avalonia.Platform.Storage.FilePickerFileType("Images") { Patterns = ["*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif"] }]
        });
        if (picker.Count == 0) { return; }

        var td = NAPLPS.Import.BitmapImporter.ToTelidraw(picker[0].Path.LocalPath);
        await LoadTelidrawSource(td);
    }

    /// <summary>
    /// Shared helper: compile the given Telidraw source and install it as the loaded
    /// document, entering editor mode so the user can refine the imported scene.
    /// </summary>
    private async Task LoadTelidrawSource(string td)
    {
        try
        {
            var tokens = new NAPLPS.Telidraw.Lexer(td).Tokenize();
            var ast = new NAPLPS.Telidraw.Parser(tokens).Parse();
            var compiler = new NAPLPS.Telidraw.Compiler(ast);
            loadedFile = compiler.Compile();

            _suppressTelidrawRecompile = true;
            try { TelidrawSource = td; } finally { _suppressTelidrawRecompile = false; }

            IsFileLoaded = true;
            IsEditorMode = true;
            IsFileDirty = true;

            BuildDrawContext();
            await UpdateCanvas();
        }
        catch (System.Exception ex)
        {
            NetworkStatus = $"Import failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task Export()
    {
        if (App.MainWindow == null || drawContext == null)
        {
            return;
        }

        // Ask the user for format/scale/quality options BEFORE the file picker so the
        // file-type filter can match the chosen format.
        var vm = await ExportDialog.PromptAsync(App.MainWindow, drawContext.Image.Width, drawContext.Image.Height);
        if (vm == null) { return; }

        var (ext, patterns, label) = vm.Format switch
        {
            ExportFormat.Png  => ("png",  new[] { "*.png"  }, "PNG Image"),
            ExportFormat.Jpeg => ("jpg",  new[] { "*.jpg", "*.jpeg" }, "JPEG Image"),
            ExportFormat.Bmp  => ("bmp",  new[] { "*.bmp"  }, "Bitmap Image"),
            ExportFormat.Gif  => ("gif",  new[] { "*.gif"  }, "GIF Image"),
            ExportFormat.Apng => ("png",  new[] { "*.png"  }, "Animated PNG"),
            _                 => ("png",  new[] { "*.png"  }, "PNG Image"),
        };

        var options = new FilePickerSaveOptions
        {
            Title = $"Export to {label}",
            DefaultExtension = ext,
            FileTypeChoices = new[] { new FilePickerFileType(label) { Patterns = patterns } },
            SuggestedFileName = IOPath.GetFileNameWithoutExtension(loadedFilePath)
        };

        var result = await App.MainWindow.StorageProvider.SaveFilePickerAsync(options);
        if (result == null) { return; }

        var path = result.Path.LocalPath;

        // APNG path is fundamentally different: RenderToApng walks the command sequence and
        // produces a multi-frame Image<Rgba32> with WAIT-driven inter-frame timing baked in.
        // Single-frame formats just snapshot the live canvas.
        if (vm.Format == ExportFormat.Apng)
        {
            int delayHundredths = System.Math.Max(1, vm.ApngFrameDelayMs / 10);
            using var apngImage = drawContext.RenderToApng(delayHundredths, vm.ApngLoop, vm.ApngBlinkCycles);

            // Clip to the user's frame range if they specified one (1-based inclusive).
            if (vm.ApngStartFrame > 0 || vm.ApngEndFrame > 0)
            {
                int start = System.Math.Max(0, vm.ApngStartFrame - 1);
                int endExclusive = vm.ApngEndFrame > 0
                    ? System.Math.Min(apngImage.Frames.Count, vm.ApngEndFrame)
                    : apngImage.Frames.Count;

                // Drop frames outside [start, endExclusive). Iterate from the end so indices stay stable.
                for (int i = apngImage.Frames.Count - 1; i >= endExclusive; i--) { apngImage.Frames.RemoveFrame(i); }
                for (int i = start - 1; i >= 0; i--) { apngImage.Frames.RemoveFrame(i); }
            }

            // Resize each frame if the user picked a non-1x scale.
            if (System.Math.Abs(vm.Scale - 1.0) > 0.001)
            {
                apngImage.Mutate(ctx => ctx.Resize(vm.OutputWidth, vm.OutputHeight));
            }

            await apngImage.SaveAsPngAsync(path);
            return;
        }

        // Single-frame formats: clone the live canvas image so our transforms don't mutate
        // what the editor renders. Resize when the user picked a non-1x scale.
        using var image = drawContext.Image.Clone(ctx =>
        {
            if (System.Math.Abs(vm.Scale - 1.0) > 0.001)
            {
                ctx.Resize(vm.OutputWidth, vm.OutputHeight);
            }
        });

        switch (vm.Format)
        {
            case ExportFormat.Png:
                await image.SaveAsPngAsync(path);
                break;
            case ExportFormat.Jpeg:
                var jpegEncoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = vm.JpegQuality };
                await image.SaveAsJpegAsync(path, jpegEncoder);
                break;
            case ExportFormat.Bmp:
                await image.SaveAsBmpAsync(path);
                break;
            case ExportFormat.Gif:
                await image.SaveAsGifAsync(path);
                break;
        }
    }

    [RelayCommand]
    private async Task Quit()
    {
        if (!await PromptSaveIfDirty())
        {
            return;
        }

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
            sequenceWindow = new SequenceWindow(drawContext, undoManager);

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

    [ObservableProperty]
    private string displayRatio = "0.80";

    [RelayCommand]
    private async Task SetDisplayRatio(string ratio)
    {
        if (float.TryParse(ratio, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float value))
        {
            DisplayRatio = ratio;
            NaplpsUtils.DisplayRatio = value;

            if (drawContext != null)
            {
                await UpdateCanvas();
            }
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
            "Fill" => fillTool,
            "IncrementalLine" => incrementalLineTool,
            "IncrementalPolygon" => incrementalPolygonTool,
            "IncrementalPoint" => incrementalPointTool,
            _ => selectTool
        };

        // Keep FillTool's format reference current so hit-tests are against the active file.
        if (ActiveTool is FillTool ft)
        {
            ft.Format = loadedFile;
        }

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
        if (loadedFile == null || !undoManager.CanUndo)
        {
            return;
        }

        undoManager.Undo(loadedFile);
        IsFileDirty = true;
        BuildDrawContext();
        await UpdateCanvas();
    }

    [RelayCommand]
    private async Task Redo()
    {
        if (loadedFile == null || !undoManager.CanRedo)
        {
            return;
        }

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

    /// <summary>
    /// Set the current drawing background color by palette index. Mirrors the
    /// foreground-selection flow; used by the BG swatch flyout and shift-click on
    /// the palette grid.
    /// </summary>
    [RelayCommand]
    private void SelectBackgroundPaletteColor(byte index)
    {
        EditorBackgroundIndex = index;
        UpdateColorBrushes();
    }

    /// <summary>
    /// Replace the current palette with Prodigy's canonical CLUT. Emits SELECT COLOR +
    /// SET COLOR pairs for each palette slot so the load goes through UndoManager and
    /// becomes part of the drawing's own command history (undoable, visible in .nap).
    /// </summary>
    [RelayCommand]
    private void LoadProdigyPalette()
    {
        ApplyPalettePreset(NaplpsState.ColorMapProdigyDefaults);
    }

    /// <summary>
    /// Replace the current palette with NAPLPS spec defaults (3-bit GRB ramp).
    /// </summary>
    [RelayCommand]
    private void LoadDefaultPalette()
    {
        ApplyPalettePreset(NaplpsState.ColorMapDefaults);
    }

    /// <summary>
    /// Activate the Rectangle tool and set its fill variant. Called from the rectangle
    /// tool's chevron flyout. Also flips the global IsFilledMode so other shape tools
    /// stay consistent with the user's most-recent intent.
    /// </summary>
    /// <summary>
    /// Cancel the in-progress editor draw (e.g. mid-polygon click sequence). Hooked to Esc.
    /// Resets the active tool's accumulated state and clears the preview overlay.
    /// </summary>
    [RelayCommand]
    private void CancelDraw()
    {
        if (ActiveTool != null)
        {
            ActiveTool.Reset();
        }
        EditorPreview = null;
    }

    /// <summary>
    /// Force a re-decompile of the current loadedFile into the Telidraw text pane.
    /// Hooked to F5. Useful when the user wants to discard local edits and reset to source.
    /// </summary>
    [RelayCommand]
    private void RecompileTelidraw()
    {
        if (loadedFile == null)
        {
            return;
        }

        SyncTelidrawFromFormat();
    }

    [RelayCommand]
    private void SetRectangleVariant(string variant)
    {
        IsFilledMode = variant == "Filled";
        SetActiveTool("Rectangle");
    }

    [RelayCommand]
    private void SetPolygonVariant(string variant)
    {
        IsFilledMode = variant == "Filled";
        SetActiveTool("Polygon");
    }

    [RelayCommand]
    private void SetArcVariant(string variant)
    {
        IsFilledMode = variant == "Filled";
        SetActiveTool("Arc");
    }

    /// <summary>
    /// Emit a TEXTURE command for the currently-selected fill pattern + pel size.
    /// Per ANSI X3.110 §5.3.2.7, TEXTURE has operands (linePattern, highlight, fillPattern, maskSize).
    /// We map the 4 UI choices to fillPattern bytes 0..3 (solid / vertical / horizontal / mesh).
    /// </summary>
    [RelayCommand]
    private void ApplyFillPattern()
    {
        if (loadedFile == null)
        {
            return;
        }

        byte fillPattern = (byte)Math.Clamp(FillPatternIndex, 0, 3);
        var maskSize = new Vector3((float)FillPelWidth / 40.0f, (float)FillPelHeight / 40.0f, 0);
        var tx = NaplpsCommandBuilder.BuildTexture(
            linePattern: 0,
            highlight: false,
            fillPattern: fillPattern,
            maskSize: maskSize);

        undoManager.Execute(new AddCommandsAction([tx]), loadedFile);
        IsFileDirty = true;
        BuildDrawContext();
    }

    /// <summary>Current fill pattern index (0=Solid, 1=Vertical, 2=Horizontal, 3=Mesh).</summary>
    [ObservableProperty]
    private int fillPatternIndex = 0;

    /// <summary>Pel-width for custom texture masks (in 1/40ths of screen width).</summary>
    [ObservableProperty]
    private double fillPelWidth = 1;

    /// <summary>Pel-height for custom texture masks (in 1/40ths of screen height).</summary>
    [ObservableProperty]
    private double fillPelHeight = 1;

    // ---- Domain attribute editor state ----------------------------------------------------
    [ObservableProperty] private byte domainSingleByteValue = 1;
    [ObservableProperty] private byte domainMultiByteValue = 3;
    [ObservableProperty] private byte domainDimensionality = 2;
    [ObservableProperty] private double domainLogicalPelX;
    [ObservableProperty] private double domainLogicalPelY;

    /// <summary>Emit a DOMAIN command from the current editor inputs (single/multi/dim + pel).</summary>
    [RelayCommand]
    private void ApplyDomain()
    {
        if (loadedFile == null) { return; }

        var pel = (DomainLogicalPelX > 0 || DomainLogicalPelY > 0)
            ? new Vector3((float)DomainLogicalPelX, (float)DomainLogicalPelY, 0)
            : (Vector3?)null;

        var cmd = NaplpsCommandBuilder.BuildDomain(DomainSingleByteValue, DomainMultiByteValue, DomainDimensionality, pel);
        undoManager.Execute(new AddCommandsAction([cmd]), loadedFile);
        IsFileDirty = true;
        BuildDrawContext();
    }

    // ---- Field attribute editor state -----------------------------------------------------
    [ObservableProperty] private double fieldOriginX;
    [ObservableProperty] private double fieldOriginY;
    [ObservableProperty] private double fieldDimWidth = 1.0;
    [ObservableProperty] private double fieldDimHeight = 0.75;

    [RelayCommand]
    private void ApplyField()
    {
        if (loadedFile == null) { return; }

        var origin = new Vector3((float)FieldOriginX, (float)FieldOriginY, 0);
        var dims = new Vector3((float)FieldDimWidth, (float)FieldDimHeight, 0);
        var cmd = NaplpsCommandBuilder.BuildField(origin, dims);
        undoManager.Execute(new AddCommandsAction([cmd]), loadedFile);
        IsFileDirty = true;
        BuildDrawContext();
    }

    // ---- Line/Texture attribute editor state ---------------------------------------------
    [ObservableProperty] private int lineTextureIndex;       // 0-3 (solid/dashed/dotted/dot-dash)
    [ObservableProperty] private bool lineHighlight;

    [RelayCommand]
    private void ApplyLineAttributes()
    {
        if (loadedFile == null) { return; }

        // TEXTURE command carries (linePattern, highlight, fillPattern). We reuse the current
        // Fill section's pattern index so the commit is a single combined attribute write.
        var cmd = NaplpsCommandBuilder.BuildTexture(
            (byte)Math.Clamp(LineTextureIndex, 0, 3),
            LineHighlight,
            (byte)Math.Clamp(FillPatternIndex, 0, 3));
        undoManager.Execute(new AddCommandsAction([cmd]), loadedFile);
        IsFileDirty = true;
        BuildDrawContext();
    }

    // ---- Color mode editor state ----------------------------------------------------------
    [ObservableProperty] private int colorModeIndex;  // 0 = direct RGB, 1 = palette mode 1, 2 = palette mode 2
    [ObservableProperty] private bool colorTransparent;

    // ---- Networking (TCP send/receive) ---------------------------------------------------
    private readonly NAPLPS.Networking.NaplpsNetworkService _network = new();

    [ObservableProperty] private bool isNetworkListening;
    [ObservableProperty] private int networkListenPort = 5510;
    [ObservableProperty] private string networkRemoteHost = "127.0.0.1";
    [ObservableProperty] private int networkRemotePort = 5510;
    [ObservableProperty] private string networkStatus = "Network: idle";

    [RelayCommand]
    private void StartNetworkListener()
    {
        // Hook events on first start (idempotent per service instance).
        _network.StatusChanged -= HandleNetworkStatus;
        _network.StatusChanged += HandleNetworkStatus;
        _network.BytesReceived -= OnNetworkBytesReceived;
        _network.BytesReceived += OnNetworkBytesReceived;

        _network.ClearReceivedBuffer();
        _network.StartListening(NetworkListenPort);
        IsNetworkListening = true;
    }

    [RelayCommand]
    private void StopNetworkListener()
    {
        _network.StopListening();
        IsNetworkListening = false;
    }

    [RelayCommand]
    private async Task SendDocumentToRemote()
    {
        if (loadedFile == null)
        {
            NetworkStatus = "Network: no document loaded";
            return;
        }

        try
        {
            NetworkStatus = $"Network: sending to {NetworkRemoteHost}:{NetworkRemotePort}...";
            var bytes = loadedFile.ToBytes();
            await NAPLPS.Networking.NaplpsNetworkService.SendAsync(NetworkRemoteHost, NetworkRemotePort, bytes);
            NetworkStatus = $"Network: sent {bytes.Length} bytes to {NetworkRemoteHost}:{NetworkRemotePort}";
        }
        catch (System.Exception ex)
        {
            NetworkStatus = $"Network: send failed — {ex.Message}";
        }
    }

    private void HandleNetworkStatus(string status)
    {
        // Network events fire on worker threads; marshal back to UI thread for VM updates.
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            NetworkStatus = $"Network: {status}";
        });
    }

    private void OnNetworkBytesReceived(byte[] _)
    {
        // Re-parse the entire received buffer and replace the loaded document. Cheap for
        // small streams, would need incremental rendering for large ones — defer that.
        Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
        {
            try
            {
                var bytes = _network.SnapshotReceivedBuffer();
                if (bytes.Length == 0) { return; }

                loadedFile = NaplpsFormat.FromBytes(bytes);
                IsFileLoaded = true;
                IsEditorMode = false;  // network-received content is view-mode by default
                BuildDrawContext();
                await UpdateCanvas();
                NetworkStatus = $"Network: rendered {bytes.Length} received bytes";
            }
            catch (System.Exception ex)
            {
                NetworkStatus = $"Network: parse error — {ex.Message}";
            }
        });
    }

    [RelayCommand]
    private void ApplyColorMode()
    {
        // SELECT COLOR with transparent flag is the cleanest way to commit a transparent
        // background. Mode toggling itself is done via SELECT COLOR with the operand bytes
        // structured for the chosen mode — handled by BuildSelectColor.
        if (loadedFile == null) { return; }

        if (ColorTransparent)
        {
            var cmd = NaplpsCommandBuilder.BuildSetColorTransparent();
            undoManager.Execute(new AddCommandsAction([cmd]), loadedFile);
            IsFileDirty = true;
            BuildDrawContext();
        }
    }

    /// <summary>
    /// Apply a palette preset by updating every PaletteColor's R/G/B. The RgbChanged
    /// event fires per entry, which emits SELECT COLOR + SET COLOR + SELECT COLOR
    /// triples through UndoManager — baking the palette change into the .nap so it
    /// survives save/reload.
    /// </summary>
    private void ApplyPalettePreset(Dictionary<byte, NaplpsColor> preset)
    {
        if (loadedFile == null || PaletteColors == null)
        {
            return;
        }

        foreach (var entry in PaletteColors)
        {
            if (preset.TryGetValue(entry.Index, out var color))
            {
                // Assigning triggers RgbChanged → OnPaletteEntryEdited which emits the
                // SELECT+SET+SELECT command triple for this slot. Do NOT use LoadFromNaplpsColor
                // here (that path bypasses the event, which is only right for initial load).
                entry.Red = color.Red;
                entry.Green = color.Green;
                entry.Blue = color.Blue;
            }
        }

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
            var entry = new PaletteColor { Index = kvp.Key };

            // Seed R/G/B from the 0-255 NaplpsColor, which triggers RefreshBrush internally.
            // LoadFromNaplpsColor bypasses the RgbChanged event so bulk init doesn't emit
            // N SetColor commands.
            entry.LoadFromNaplpsColor(kvp.Value);
            entry.RgbChanged += OnPaletteEntryEdited;

            colors.Add(entry);
        }

        PaletteColors = colors;
    }

    /// <summary>
    /// Handler invoked when the user mutates R/G/B on a palette entry from the Palette
    /// Editor. Emits SELECT COLOR (target entry) + SET COLOR (new RGB) + SELECT COLOR
    /// (restore) so the edited slot is updated without disturbing the user's current
    /// foreground selection. Routes through the shared UndoManager.
    /// </summary>
    private void OnPaletteEntryEdited(object? sender, EventArgs e)
    {
        if (loadedFile == null || sender is not PaletteColor entry)
        {
            return;
        }

        var prevForeground = EditorForegroundIndex;

        var selectEdited = NaplpsCommandBuilder.BuildSelectColor(entry.Index);
        var setColor = NaplpsCommandBuilder.BuildSetColorRgb(entry.Green, entry.Red, entry.Blue);
        var selectRestore = NaplpsCommandBuilder.BuildSelectColor(prevForeground);

        var action = new AddCommandsAction([selectEdited, setColor, selectRestore]);
        undoManager.Execute(action, loadedFile);
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
        if (loadedFile == null || SelectedCommandIndex < 0)
        {
            return;
        }

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
        if (!IsEditorMode || loadedFile == null)
        {
            return;
        }

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
        if (!IsEditorMode || loadedFile == null)
        {
            return;
        }

        var canvasSizeObj = GetSizeObj();
        var (normX, normY) = CoordinateMapper.ScreenToNaplps(pos, controlSize, canvasSizeObj, ImageStretch);
        normX = GridSettings.SnapX(normX);
        normY = GridSettings.SnapY(normY);
        ActiveTool.OnPointerMoved(normX, normY);

        // Status-bar coord readout in NAPLPS-normalized coords (X: 0..1, Y: 0..0.75).
        CoordReadout = $"X={normX:F4}  Y={normY:F4}";

        // Update rubber-band preview
        EditorPreview = ActiveTool.GetPreview();
    }

    public async void OnEditorPointerReleased(Avalonia.Point pos, Avalonia.Size controlSize)
    {
        if (!IsEditorMode || loadedFile == null)
        {
            return;
        }

        var canvasSizeObj = GetSizeObj();
        var (normX, normY) = CoordinateMapper.ScreenToNaplps(pos, controlSize, canvasSizeObj, ImageStretch);
        normX = GridSettings.SnapX(normX);
        normY = GridSettings.SnapY(normY);

        var commands = ActiveTool.OnPointerReleased(normX, normY);

        // Clear preview on release
        EditorPreview = null;

        if (commands.Count == 0)
        {
            return;
        }

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
        if (loadedFile == null || commands.Count == 0)
        {
            return;
        }

        // Prepend a SelectColor command if we're not using the default color
        if (EditorForegroundIndex != 0)
        {
            var (colorOp, colorOps) = NaplpsCommandBuilder.BuildSelectColor(EditorForegroundIndex);
            commands.Insert(0, (colorOp, colorOps));
        }

        // If the macro recorder is running, also copy each command's raw bytes into the
        // recording buffer. The buffer gets wrapped with DefMacro + End when the user
        // clicks Stop, producing a single macro-definition sequence.
        if (IsMacroRecording)
        {
            foreach (var (opcode, operands) in commands)
            {
                _macroRecordingBuffer.Add(opcode);
                _macroRecordingBuffer.AddRange(operands);
            }
        }

        var action = new AddCommandsAction(commands);
        undoManager.Execute(action, loadedFile);
        IsFileDirty = true;

        BuildDrawContext();
        await UpdateCanvas();
        SyncTelidrawFromFormat();
    }

    /// <summary>
    /// Open the DRCS character designer as a modal. When the user commits, emit a
    /// DEF DRCS control command with the slot char + simplified 8×10 bitmap bytes,
    /// followed by END.
    /// </summary>
    [RelayCommand]
    private async Task OpenDrcsDesigner(Window parent)
    {
        if (loadedFile == null)
        {
            return;
        }

        var result = await DrcsDesignerWindow.PromptAsync(parent);

        if (result is not var (slot, bitmap))
        {
            return;
        }

        var operands = new NaplpsOperands { (byte)slot };
        operands.AddRange(bitmap);

        var defDrcs = (NaplpsCommandBuilder.OpDefDrcs, operands);
        var end = (NaplpsCommandBuilder.OpEnd, new NaplpsOperands());

        undoManager.Execute(new AddCommandsAction([defDrcs, end]), loadedFile);
        IsFileDirty = true;

        BuildDrawContext();
        await UpdateCanvas();
    }

    /// <summary>
    /// Open the texture mask designer as a modal. When committed, emit DEF TEXTURE with
    /// the mask id (4/1-4/4) + pattern bytes + mask bytes, followed by END.
    /// </summary>
    [RelayCommand]
    private async Task OpenTextureDesigner(Window parent)
    {
        if (loadedFile == null)
        {
            return;
        }

        var result = await TextureDesignerWindow.PromptAsync(parent);

        if (result is not var (maskId, pattern, mask))
        {
            return;
        }

        // DefTexture operand layout per spec: first byte is 4/1..4/4 selecting mask A..D.
        var operands = new NaplpsOperands { (byte)(0x41 + maskId) };
        operands.AddRange(pattern);
        operands.AddRange(mask);

        var defTexture = (NaplpsCommandBuilder.OpDefTexture, operands);
        var end = (NaplpsCommandBuilder.OpEnd, new NaplpsOperands());

        undoManager.Execute(new AddCommandsAction([defTexture, end]), loadedFile);
        IsFileDirty = true;

        BuildDrawContext();
        await UpdateCanvas();
    }

    /// <summary>
    /// Called by the generated OnTelidrawSourceChanged partial when the text pane content
    /// changes. Recompiles the source and updates the canvas. Uses BareFormat so the
    /// raw commands round-trip byte-identically.
    /// </summary>
    partial void OnTelidrawSourceChanged(string value)
    {
        // Suppress recompile when WE wrote the source (FileLoad, SyncTelidrawFromFormat).
        // Only user keystrokes in the text pane should trigger the lex/parse/compile cycle.
        if (_suppressTelidrawRecompile)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(value) || loadedFile == null)
        {
            return;
        }

        _telidrawSourceDirty = true;

        try
        {
            var tokens = new NAPLPS.Telidraw.Lexer(value).Tokenize();
            var parser = new NAPLPS.Telidraw.Parser(tokens);
            var ast = parser.Parse();

            if (parser.Diagnostics.Count > 0)
            {
                return; // Don't recompile on parse errors — user is mid-edit
            }

            var compiler = new NAPLPS.Telidraw.Compiler(ast) { BareFormat = true };
            var newFormat = compiler.Compile();

            if (compiler.Diagnostics.Count > 0)
            {
                return;
            }

            loadedFile = newFormat;
            _telidrawSourceDirty = false;
            IsFileDirty = true;

            BuildDrawContext();
            _ = UpdateCanvas();
        }
        catch
        {
            // Swallow — user is typing and the source is temporarily invalid.
        }
    }

    /// <summary>
    /// After any tool action that mutates Commands, refresh the Telidraw text pane with the
    /// newly decompiled source. Skipped if the text pane triggered this recompile cycle
    /// (to avoid infinite loops).
    /// </summary>
    private void SyncTelidrawFromFormat()
    {
        if (_telidrawSourceDirty || loadedFile == null || !IsTelidrawPaneVisible)
        {
            return;
        }

        try
        {
            _suppressTelidrawRecompile = true;
            TelidrawSource = NAPLPS.Telidraw.Decompiler.Decompile(loadedFile);
        }
        catch
        {
            // Decompile failure shouldn't crash the editor.
        }
        finally
        {
            _suppressTelidrawRecompile = false;
        }
    }

    [RelayCommand]
    private void ToggleTelidrawPane()
    {
        IsTelidrawPaneVisible = !IsTelidrawPaneVisible;

        if (IsTelidrawPaneVisible && loadedFile != null)
        {
            SyncTelidrawFromFormat();
        }
    }

    /// <summary>Begin capturing subsequent tool commits into the macro recorder buffer.</summary>
    [RelayCommand]
    private void StartMacroRecording()
    {
        _macroRecordingBuffer.Clear();
        IsMacroRecording = true;
    }

    /// <summary>
    /// Finalize the macro recording: emit DEF MACRO (with the slot char) followed by the
    /// buffered body bytes, then END. The parser's buffered mode handles the body bytes on
    /// next load and populates <c>state.Macros[slot]</c>. Routes through UndoManager so the
    /// whole insertion is one undo step.
    /// </summary>
    [RelayCommand]
    private async Task StopMacroRecording()
    {
        if (!IsMacroRecording || loadedFile == null)
        {
            IsMacroRecording = false;
            return;
        }

        IsMacroRecording = false;

        var bodyBytes = _macroRecordingBuffer.ToArray();
        _macroRecordingBuffer.Clear();

        if (bodyBytes.Length == 0)
        {
            return;
        }

        // DEF MACRO C1 code (0x80) with the slot character as the first operand byte,
        // followed by the buffered body. The End sentinel is emitted as a sibling
        // ControlCommand so the stream reads:  DEF MACRO  <body>  END.
        var defOperands = new NaplpsOperands { (byte)MacroRecordingSlot };
        defOperands.AddRange(bodyBytes);

        var defMacroCmd = (NaplpsCommandBuilder.OpDefMacro, defOperands);
        var endCmd = (NaplpsCommandBuilder.OpEnd, new NaplpsOperands());

        var action = new AddCommandsAction([defMacroCmd, endCmd]);
        undoManager.Execute(action, loadedFile);
        IsFileDirty = true;

        BuildDrawContext();
        await UpdateCanvas();
    }

    /// <summary>Cancel the current recording without emitting DefMacro. Useful for rethinking a take.</summary>
    [RelayCommand]
    private void CancelMacroRecording()
    {
        _macroRecordingBuffer.Clear();
        IsMacroRecording = false;
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

        // Restart blink timer when returning to the final frame
        if (drawContext.BlinkAnimator?.HasActiveProcesses == true)
        {
            StartBlinkTimer();
        }
    }

    private async Task RenderToFrame(int frameIndex)
    {
        if (drawContext == null)
        {
            return;
        }

        // Stop blink timer — it calls Render() which would overwrite the navigated frame.
        // Blink restarts when the user returns to the last frame via LastFrame().
        StopBlinkTimer();

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
        {
            return;
        }

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

    public void CloseChildWindows()
    {
        StopBlinkTimer();

        sequenceWindow?.Close();
        sequenceWindow = null;

        propertiesWindow?.Close();
        propertiesWindow = null;
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

        propertiesWindow?.Close();
        propertiesWindow = null;

        undoManager.Clear();

        loadedFile = null;
        loadedFilePath = string.Empty;

        ClearDiagnostics();

        FileName = DEFAULT_NO_FILE_NAME;

        TitleBar = $"{DEFAULT_APP_NAME} [{Program.Version}]";
    }

    private async Task FileLoad(string filePath)
    {
        FileClose();

        try
        {
            // Suppress recompile while we initialize TelidrawSource — these writes are NOT
            // user keystrokes and must not trigger the recompile-and-replace-loadedFile path.
            _suppressTelidrawRecompile = true;
            try
            {
                if (filePath.EndsWith(".td", StringComparison.OrdinalIgnoreCase))
                {
                    // Compile Telidraw source → NaplpsFormat
                    var source = await System.IO.File.ReadAllTextAsync(filePath);
                    var tokens = new NAPLPS.Telidraw.Lexer(source).Tokenize();
                    var parser = new NAPLPS.Telidraw.Parser(tokens);
                    var ast = parser.Parse();
                    var compiler = new NAPLPS.Telidraw.Compiler(ast);
                    loadedFile = compiler.Compile();
                    TelidrawSource = source;
                }
                else
                {
                    loadedFile = NaplpsFormat.FromFile(filePath);
                    TelidrawSource = NAPLPS.Telidraw.Decompiler.Decompile(loadedFile);
                }
            }
            finally
            {
                _suppressTelidrawRecompile = false;
            }

            loadedFilePath = filePath;

            BuildDrawContext();

            IsFileLoaded = true;

            FileName = IOPath.GetFileName(filePath);

            TitleBar = $"{FileName} - {DEFAULT_APP_NAME} [{Program.Version}]";

            BitWidth = loadedFile.Is7Bit ? "7-Bit" : "8-Bit";
            FileSystemType = loadedFile.SystemType.ToString();

            UpdateDiagnostics(loadedFile);

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

    private void UpdateDiagnostics(NaplpsFormat naplps)
    {
        var items = new ObservableCollection<DiagnosticItem>();

        foreach (var error in naplps.Errors)
        {
            var details = "";

            if (error.Opcode.HasValue)
            {
                details += $"Opcode: 0x{error.Opcode.Value:X2}";
            }

            if (error.StreamPosition.HasValue)
            {
                if (details.Length > 0)
                {
                    details += " | ";
                }

                details += $"Position: {error.StreamPosition.Value}";
            }

            items.Add(new DiagnosticItem
            {
                IsError = error.Severity == NaplpsErrorSeverity.Error,
                Severity = error.Severity.ToString(),
                Type = error.Type.ToString(),
                Message = error.Message,
                Details = details
            });
        }

        DiagnosticItems = items;
        ErrorCount = items.Count(i => i.IsError);
        WarningCount = items.Count(i => !i.IsError);
        HasDiagnostics = items.Count > 0;
        IsDiagnosticsPanelVisible = HasDiagnostics;
    }

    private void ClearDiagnostics()
    {
        DiagnosticItems = [];
        ErrorCount = 0;
        WarningCount = 0;
        HasDiagnostics = false;
        IsDiagnosticsPanelVisible = false;
    }

    private Window? propertiesWindow;

    [RelayCommand]
    private void Properties()
    {
        ShowPropertiesWindow(0);
    }

    [RelayCommand]
    private void ShowDiagnostics()
    {
        ShowPropertiesWindow(1);
    }

    private void ShowPropertiesWindow(int startTab)
    {
        if (loadedFile == null || App.MainWindow == null)
        {
            return;
        }

        if (propertiesWindow != null)
        {
            if (propertiesWindow.DataContext is PropertiesWindowViewModel existingVm)
            {
                existingVm.SelectedTabIndex = startTab;
            }

            propertiesWindow.Activate();
            return;
        }

        var vm = PropertiesWindowViewModel.FromFile(loadedFile, loadedFilePath, DiagnosticItems, startTab);

        propertiesWindow = new PropertiesWindow
        {
            DataContext = vm
        };

        propertiesWindow.Closed += (_, _) => propertiesWindow = null;
        propertiesWindow.Show(App.MainWindow);
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

    private async Task UpdateCanvas()
    {
        if (loadedFile == null)
        {
            return;
        }

        if (drawContext == null)
        {
            return;
        }

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
        if (!Program.ParseSize(CanvasSize, out var width, out var height))
        {
            throw new InvalidOperationException("Malformed CanvasSize!");
        }

        return (width, height);
    }

    public void Dispose()
    {
        drawContext?.Dispose();
        renderLock.Dispose();
        blinkTimer?.Stop();
        GC.SuppressFinalize(this);
    }
}
