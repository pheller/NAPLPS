// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using NAPLPSApp.Editor;
using NAPLPSApp.Editor.Tools;
using NAPLPSApp.Resources;
using NAPLPSApp.ViewModels.Menus;
using NAPLPSApp.Views.Menus;

namespace NAPLPSApp.Views;

public partial class MainWindow : Window
{
    private TextEditor? _telidrawEditor;
    private bool _suppressEditorTextSync;
    private bool _menusBuilt;

    public MainWindow()
    {
        InitializeComponent();
        InitializeTelidrawEditor();

        DataContextChanged += (_, _) => BuildMenusFromViewModel();
        BuildMenusFromViewModel();
    }

    /// <summary>
    /// Builds both the in-window <see cref="Menu"/> and (on macOS) the
    /// <see cref="NativeMenu"/> from the single <see cref="MenuTreeBuilder"/> tree.
    /// Re-runs on DataContext assignment; idempotent thereafter.
    /// </summary>
    private void BuildMenusFromViewModel()
    {
        if (_menusBuilt || DataContext is not MainWindowViewModel vm) { return; }

        var tree = MenuTreeBuilder.Build(vm, PlatformGestureSet.Current, windowForCommandParameter: this);

        if (OperatingSystem.IsMacOS())
        {
            NativeMenu.SetMenu(this, MenuRenderer.BuildNativeMenu(vm, tree));
            if (InWindowMenu is not null)
            {
                InWindowMenu.IsVisible = false;
            }
        }
        else if (InWindowMenu is not null)
        {
            MenuRenderer.PopulateInWindowMenu(InWindowMenu, vm, tree);
        }

        _menusBuilt = true;
    }

    /// <summary>
    /// Wire up AvaloniaEdit for the Telidraw text pane: load the syntax-highlighting
    /// definition from embedded XSHD, hook two-way text binding to ViewModel.TelidrawSource
    /// (with reentrancy guard), and register an F5 recompile keybind.
    /// </summary>
    private void InitializeTelidrawEditor()
    {
        _telidrawEditor = this.FindControl<TextEditor>("TelidrawEditor");
        if (_telidrawEditor == null) { return; }

        // Load Telidraw syntax highlighting from the embedded XSHD resource.
        var asm = typeof(MainWindow).Assembly;
        using (var stream = asm.GetManifestResourceStream("NAPLPSApp.Assets.TelidrawHighlighting.xshd"))
        {
            if (stream != null)
            {
                using var reader = new System.Xml.XmlTextReader(stream);
                _telidrawEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
        }

        // Two-way bridge between editor.Text and VM.TelidrawSource. AvaloniaEdit's
        // TextEditor.Text isn't a normal AvaloniaProperty, so we wire it manually.
        _telidrawEditor.TextChanged += (_, _) =>
        {
            if (_suppressEditorTextSync) { return; }
            if (DataContext is MainWindowViewModel vm)
            {
                vm.TelidrawSource = _telidrawEditor.Text;
            }
        };

        DataContextChanged += (_, _) => HookViewModelForEditor();
        HookViewModelForEditor();
    }

    private void HookViewModelForEditor()
    {
        if (DataContext is not MainWindowViewModel vm || _telidrawEditor == null) { return; }

        // Seed editor with current source.
        _suppressEditorTextSync = true;
        _telidrawEditor.Text = vm.TelidrawSource ?? string.Empty;
        _suppressEditorTextSync = false;

        // Push VM-side changes (e.g. file load → decompile) back into the editor.
        vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainWindowViewModel.TelidrawSource) && _telidrawEditor != null)
            {
                if (_telidrawEditor.Text != vm.TelidrawSource)
                {
                    _suppressEditorTextSync = true;
                    _telidrawEditor.Text = vm.TelidrawSource ?? string.Empty;
                    _suppressEditorTextSync = false;
                }
            }
        };
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (DataContext is MainWindowViewModel vm)
        {
            vm.CloseChildWindows();
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        var overlay = this.FindControl<Canvas>("EditorOverlay");

        if (overlay != null)
        {
            overlay.PointerPressed += OnEditorPointerPressed;
            overlay.PointerMoved += OnEditorPointerMoved;
            overlay.PointerReleased += OnEditorPointerReleased;
        }

        // Listen for grid visibility changes
        if (DataContext is MainWindowViewModel vm)
        {
            vm.GridSettings.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName is nameof(Editor.GridSettings.IsVisible) or nameof(Editor.GridSettings.SpacingX) or nameof(Editor.GridSettings.SpacingY))
                {
                    var ov = this.FindControl<Canvas>("EditorOverlay");

                    if (ov != null)
                    {
                        RenderGrid(vm, ov);
                    }
                }
            };

            // Re-render grid when the canvas is resized so lines track the layout.
            if (overlay != null)
            {
                overlay.SizeChanged += (_, _) => RenderGrid(vm, overlay);
                // Initial paint.
                RenderGrid(vm, overlay);
            }

            // Re-render the selection outline whenever EditorPreview changes — including
            // programmatic changes (e.g. sequence-panel click → SelectedCommandIndex change).
            // Without this, the outline only updates on the pointer-event path.
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(MainWindowViewModel.EditorPreview))
                {
                    var ov = this.FindControl<Canvas>("EditorOverlay");
                    if (ov == null) { return; }
                    if (vm.EditorPreview == null)
                    {
                        ClearPreviewOverlay();
                    }
                    else
                    {
                        UpdatePreviewOverlay(vm, ov);
                    }
                }

                if (args.PropertyName == nameof(MainWindowViewModel.ReferenceImage))
                {
                    HookReferenceImageChanges(vm);
                    UpdateReferenceImagePlacement(vm);
                }
            };

            // Initial reference-image hookup + placement.
            HookReferenceImageChanges(vm);

            // The reference image lives in coordinates relative to the NAPLPS canvas's
            // actual rendered rect; when that rect changes (resize, stretch change, source
            // swap), reposition the overlay.
            var canvasCtrl = this.FindControl<Avalonia.Controls.Image>("CanvasImageControl");
            if (canvasCtrl != null)
            {
                canvasCtrl.LayoutUpdated += (_, _) => UpdateReferenceImagePlacement(vm);
            }
        }
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);

        // Only forward typing to the canvas TextTool when the user ISN'T currently typing
        // into a text control (Telidraw pane, TextBox fields, etc.). Otherwise the same
        // character goes to both places and tool-switching shortcuts can steal keys.
        if (IsFocusOnTextInputControl())
        {
            return;
        }

        if (DataContext is MainWindowViewModel vm && vm.IsEditorMode && e.Text?.Length > 0)
        {
            foreach (var c in e.Text)
            {
                vm.OnEditorTextInput(c);
            }
            e.Handled = true;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // Intercept BEFORE base.OnKeyDown so Window.KeyBindings don't fire while typing.
        // Two scenarios need suppression:
        //   1. Canvas TextTool is in insertion-point mode (types into its buffer).
        //   2. Focus is inside a text-input control (Telidraw AvaloniaEdit, plain TextBox).
        // In both cases, unmodified letter keys would otherwise activate tool shortcuts.
        bool textEntry =
            (DataContext is MainWindowViewModel vm && vm.IsEditorMode
                && vm.ActiveTool is TextTool tt && tt.HasInsertionPoint)
            || IsFocusOnTextInputControl();

        if (textEntry)
        {
            if (e.Key == Key.Enter && DataContext is MainWindowViewModel vmEnter
                && vmEnter.ActiveTool is TextTool tte && tte.HasInsertionPoint)
            {
                vmEnter.OnEditorTextCommit();
                e.Handled = true;
                return;
            }

            // Any unmodified key while typing: consume so KeyBindings don't fire.
            // Printable chars still arrive via OnTextInput (a separate platform event)
            // and are delivered to the focused text control by the framework.
            // Keep Escape free so CancelDrawCommand can abort the canvas insertion point.
            if (e.Key != Key.Escape
                && (e.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Meta)) == 0)
            {
                e.Handled = true;
            }
        }

        base.OnKeyDown(e);
    }

    /// <summary>True when the currently-focused element is a text-input control
    /// (TextBox, AvaloniaEdit TextEditor/TextArea, AutoCompleteBox, etc.). Used to
    /// suppress Window-level tool-shortcut KeyBindings while the user is typing.</summary>
    private bool IsFocusOnTextInputControl()
    {
        var focused = FocusManager?.GetFocusedElement();
        if (focused == null) { return false; }

        // Walk up so that focus on a TextBox's inner TextPresenter still counts.
        object? current = focused;
        while (current != null)
        {
            if (current is TextBox) { return true; }
            if (current is AvaloniaEdit.TextEditor) { return true; }
            if (current is AvaloniaEdit.Editing.TextArea) { return true; }
            if (current is AutoCompleteBox) { return true; }

            current = current is Visual v ? Avalonia.VisualTree.VisualExtensions.GetVisualParent(v) : null;
        }
        return false;
    }

    private void OnEditorPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm && sender is Canvas canvas)
        {
            var pos = e.GetPosition(canvas);
            var controlSize = canvas.Bounds.Size;
            var additive = (e.KeyModifiers & (KeyModifiers.Shift | KeyModifiers.Control)) != 0;
            var ctrlHeld = (e.KeyModifiers & KeyModifiers.Control) != 0;
            vm.SetClickCount(e.ClickCount);
            vm.OnEditorPointerPressed(pos, controlSize, e.GetCurrentPoint(canvas).Properties.IsRightButtonPressed, additive, ctrlHeld);
            UpdatePreviewOverlay(vm, canvas);
        }
    }

    private void OnEditorPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm && sender is Canvas canvas)
        {
            var pos = e.GetPosition(canvas);
            var controlSize = canvas.Bounds.Size;
            vm.OnEditorPointerMoved(pos, controlSize);
            UpdatePreviewOverlay(vm, canvas);
        }
    }

    private void OnEditorPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm && sender is Canvas canvas)
        {
            var pos = e.GetPosition(canvas);
            var controlSize = canvas.Bounds.Size;
            vm.OnEditorPointerReleased(pos, controlSize);

            // SelectTool keeps a preview (selection outline) after release, and multi-click
            // tools like Polygon keep one during the click sequence. Only clear when the VM
            // has confirmed nothing's to show.
            if (vm.EditorPreview == null)
            {
                ClearPreviewOverlay();
            }
            else
            {
                UpdatePreviewOverlay(vm, canvas);
            }
        }
    }

    private void UpdatePreviewOverlay(MainWindowViewModel vm, Canvas canvas)
    {
        var overlay = this.FindControl<Canvas>("EditorOverlay");

        if (overlay == null)
        {
            return;
        }

        var preview = vm.EditorPreview;
        if (preview == null || preview.Shape == PreviewShape.None)
        {
            ClearPreviewOverlay();
            return;
        }

        // Clear previous preview shapes (keep any non-preview children if they exist)
        overlay.Children.Clear();

        var controlSize = canvas.Bounds.Size;
        var canvasSize = vm.GetSizeObj();
        var stretch = vm.ImageStretch;

        var dashStyle = new Avalonia.Media.DashStyle([4, 4], 0);
        var strokeBrush = preview.IsSelection
            ? new SolidColorBrush(Avalonia.Media.Colors.Cyan)
            : new SolidColorBrush(Avalonia.Media.Colors.White);

        switch (preview.Shape)
        {
            case PreviewShape.Line:
            {
                var p1 = CoordinateMapper.NaplpsToScreen(preview.X1, preview.Y1, controlSize, canvasSize, stretch);
                var p2 = CoordinateMapper.NaplpsToScreen(preview.X2, preview.Y2, controlSize, canvasSize, stretch);

                var line = new Avalonia.Controls.Shapes.Line
                {
                    StartPoint = p1,
                    EndPoint = p2,
                    Stroke = strokeBrush,
                    StrokeThickness = 1,
                    StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 4, 4 },
                    Opacity = 0.8
                };
                overlay.Children.Add(line);
                break;
            }

            case PreviewShape.Rectangle:
            {
                var p1 = CoordinateMapper.NaplpsToScreen(preview.X1, preview.Y1, controlSize, canvasSize, stretch);
                var p2 = CoordinateMapper.NaplpsToScreen(preview.X2, preview.Y2, controlSize, canvasSize, stretch);

                var x = Math.Min(p1.X, p2.X);
                var y = Math.Min(p1.Y, p2.Y);
                var w = Math.Abs(p2.X - p1.X);
                var h = Math.Abs(p2.Y - p1.Y);

                var rect = new Avalonia.Controls.Shapes.Rectangle
                {
                    Width = w,
                    Height = h,
                    Stroke = strokeBrush,
                    StrokeThickness = 1,
                    StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 4, 4 },
                    Opacity = 0.8,
                    Fill = preview.IsFilled
                        ? new SolidColorBrush(Avalonia.Media.Colors.White) { Opacity = 0.1 }
                        : null
                };
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                overlay.Children.Add(rect);
                break;
            }

            case PreviewShape.Polygon:
            {
                if (preview.Points.Count < 2) goto paint_handles;

                for (int i = 0; i < preview.Points.Count - 1; i++)
                {
                    var pa = CoordinateMapper.NaplpsToScreen(preview.Points[i].X, preview.Points[i].Y, controlSize, canvasSize, stretch);
                    var pb = CoordinateMapper.NaplpsToScreen(preview.Points[i + 1].X, preview.Points[i + 1].Y, controlSize, canvasSize, stretch);

                    var line = new Avalonia.Controls.Shapes.Line
                    {
                        StartPoint = pa,
                        EndPoint = pb,
                        Stroke = strokeBrush,
                        StrokeThickness = 1,
                        StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 4, 4 },
                        Opacity = 0.8
                    };
                    overlay.Children.Add(line);
                }
                break;
            }
        }

        paint_handles:
        // Vertex handles — small filled cyan squares on top of the selection outline.
        // Draw LAST so they sit on top of the bbox stroke and any preview dashes.
        if (preview.Handles.Count > 0)
        {
            var handleBrush = new SolidColorBrush(Avalonia.Media.Colors.Cyan);
            var handleStroke = new SolidColorBrush(Avalonia.Media.Colors.Black);
            const double handleSize = 8;
            foreach (var (hx, hy) in preview.Handles)
            {
                var hp = CoordinateMapper.NaplpsToScreen(hx, hy, controlSize, canvasSize, stretch);
                var square = new Avalonia.Controls.Shapes.Rectangle
                {
                    Width = handleSize,
                    Height = handleSize,
                    Fill = handleBrush,
                    Stroke = handleStroke,
                    StrokeThickness = 1,
                };
                Canvas.SetLeft(square, hp.X - handleSize / 2);
                Canvas.SetTop(square, hp.Y - handleSize / 2);
                overlay.Children.Add(square);
            }
        }
    }

    private ReferenceImage? _hookedReferenceImage;

    private void HookReferenceImageChanges(MainWindowViewModel vm)
    {
        // Detach from the previous instance so we don't leak handlers on every swap.
        if (_hookedReferenceImage != null)
        {
            _hookedReferenceImage.PropertyChanged -= OnReferenceImagePropertyChanged;
        }

        _hookedReferenceImage = vm.ReferenceImage;

        if (_hookedReferenceImage != null)
        {
            _hookedReferenceImage.PropertyChanged += OnReferenceImagePropertyChanged;
        }
    }

    private void OnReferenceImagePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            UpdateReferenceImagePlacement(vm);
        }
    }

    /// <summary>Translate ReferenceImage.X/Y/W/H (normalized NAPLPS coords — X: 0..1,
    /// Y: 0..0.75) into pixel Margin + Width/Height on ReferenceImageControl, keyed to
    /// the actual rendered bounds of CanvasImageControl. Called on any change to either.</summary>
    private void UpdateReferenceImagePlacement(MainWindowViewModel vm)
    {
        var refCtrl = this.FindControl<Avalonia.Controls.Image>("ReferenceImageControl");
        var canvasCtrl = this.FindControl<Avalonia.Controls.Image>("CanvasImageControl");

        if (refCtrl == null || canvasCtrl == null) { return; }

        var ri = vm.ReferenceImage;
        if (ri == null)
        {
            refCtrl.IsVisible = false;
            return;
        }

        var b = canvasCtrl.Bounds;
        if (b.Width <= 0 || b.Height <= 0) { return; }

        // Y normalized range is 0..0.75 (4:3 aspect). So one unit of Y spans b.Height/0.75 px.
        double yScale = b.Height / 0.75;

        refCtrl.Margin = new Thickness(
            b.X + ri.X * b.Width,
            b.Y + ri.Y * yScale,
            0, 0);
        refCtrl.Width  = ri.Width  * b.Width;
        refCtrl.Height = ri.Height * yScale;
    }

    private void ClearPreviewOverlay()
    {
        var overlay = this.FindControl<Canvas>("EditorOverlay");

        if (overlay == null)
        {
            return;
        }

        // Remove only non-grid children (grid lines have Tag="grid")
        for (int i = overlay.Children.Count - 1; i >= 0; i--)
        {
            if (overlay.Children[i] is not Avalonia.Controls.Control ctrl || ctrl.Tag as string != "grid")
            {
                overlay.Children.RemoveAt(i);
            }
        }
    }

    private void RenderGrid(MainWindowViewModel vm, Canvas overlay)
    {
        // Remove existing grid lines
        for (int i = overlay.Children.Count - 1; i >= 0; i--)
        {
            if (overlay.Children[i] is Avalonia.Controls.Control ctrl && ctrl.Tag as string == "grid")
            {
                overlay.Children.RemoveAt(i);
            }
        }

        if (!vm.GridSettings.IsVisible || overlay.Bounds.Width <= 0 || overlay.Bounds.Height <= 0)
        {
            return;
        }

        var controlSize = overlay.Bounds.Size;
        var canvasSize = vm.GetSizeObj();
        var stretch = vm.ImageStretch;
        var minorBrush = new SolidColorBrush(Avalonia.Media.Colors.Gray) { Opacity = 0.20 };
        var majorBrush = new SolidColorBrush(Avalonia.Media.Colors.Cyan) { Opacity = 0.35 };

        const int MajorEvery = 8;
        int xCount = 0;

        // Vertical lines (every SpacingX), with cyan major every Nth
        for (float x = 0; x <= 1.0f + 1e-5f; x += vm.GridSettings.SpacingX, xCount++)
        {
            var top = CoordinateMapper.NaplpsToScreen(x, 0.75f, controlSize, canvasSize, stretch);
            var bottom = CoordinateMapper.NaplpsToScreen(x, 0f, controlSize, canvasSize, stretch);
            bool isMajor = xCount % MajorEvery == 0;
            var line = new Avalonia.Controls.Shapes.Line
            {
                StartPoint = top,
                EndPoint = bottom,
                Stroke = isMajor ? majorBrush : minorBrush,
                StrokeThickness = isMajor ? 1.0 : 0.5,
                Tag = "grid",
                IsHitTestVisible = false
            };
            overlay.Children.Insert(0, line); // Insert behind preview shapes
        }

        int yCount = 0;

        // Horizontal lines
        for (float y = 0; y <= 0.75f + 1e-5f; y += vm.GridSettings.SpacingY, yCount++)
        {
            var left = CoordinateMapper.NaplpsToScreen(0f, y, controlSize, canvasSize, stretch);
            var right = CoordinateMapper.NaplpsToScreen(1.0f, y, controlSize, canvasSize, stretch);
            bool isMajor = yCount % MajorEvery == 0;
            var line = new Avalonia.Controls.Shapes.Line
            {
                StartPoint = left,
                EndPoint = right,
                Stroke = isMajor ? majorBrush : minorBrush,
                StrokeThickness = isMajor ? 1.0 : 0.5,
                Tag = "grid",
                IsHitTestVisible = false
            };
            overlay.Children.Insert(0, line);
        }
    }
}
