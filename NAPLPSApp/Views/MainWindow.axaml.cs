// Copyright (c) 2026 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using Avalonia.Input;
using Avalonia.Interactivity;
using NAPLPSApp.Editor;

namespace NAPLPSApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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
        }
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);

        if (DataContext is MainWindowViewModel vm && vm.IsEditorMode && e.Text?.Length > 0)
        {
            foreach (var c in e.Text)
            {
                vm.OnEditorTextInput(c);
            }
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (DataContext is MainWindowViewModel vm && vm.IsEditorMode)
        {
            if (e.Key == Key.Enter)
            {
                vm.OnEditorTextCommit();
                e.Handled = true;
            }
        }
    }

    private void OnEditorPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm && sender is Canvas canvas)
        {
            var pos = e.GetPosition(canvas);
            var controlSize = canvas.Bounds.Size;
            vm.SetClickCount(e.ClickCount);
            vm.OnEditorPointerPressed(pos, controlSize, e.GetCurrentPoint(canvas).Properties.IsRightButtonPressed);
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

            // Only clear preview if tool isn't still in a multi-click operation
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
                if (preview.Points.Count < 2) break;

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

        if (!vm.GridSettings.IsVisible)
        {
            return;
        }

        var controlSize = overlay.Bounds.Size;
        var canvasSize = vm.GetSizeObj();
        var stretch = vm.ImageStretch;
        var gridBrush = new SolidColorBrush(Avalonia.Media.Colors.Gray) { Opacity = 0.3 };

        // Vertical lines
        for (float x = 0; x <= 1.0f; x += vm.GridSettings.SpacingX)
        {
            var top = CoordinateMapper.NaplpsToScreen(x, 0.75f, controlSize, canvasSize, stretch);
            var bottom = CoordinateMapper.NaplpsToScreen(x, 0f, controlSize, canvasSize, stretch);
            var line = new Avalonia.Controls.Shapes.Line
            {
                StartPoint = top,
                EndPoint = bottom,
                Stroke = gridBrush,
                StrokeThickness = 0.5,
                Tag = "grid",
                IsHitTestVisible = false
            };
            overlay.Children.Insert(0, line); // Insert behind preview shapes
        }

        // Horizontal lines
        for (float y = 0; y <= 0.75f; y += vm.GridSettings.SpacingY)
        {
            var left = CoordinateMapper.NaplpsToScreen(0f, y, controlSize, canvasSize, stretch);
            var right = CoordinateMapper.NaplpsToScreen(1.0f, y, controlSize, canvasSize, stretch);
            var line = new Avalonia.Controls.Shapes.Line
            {
                StartPoint = left,
                EndPoint = right,
                Stroke = gridBrush,
                StrokeThickness = 0.5,
                Tag = "grid",
                IsHitTestVisible = false
            };
            overlay.Children.Insert(0, line);
        }
    }
}
