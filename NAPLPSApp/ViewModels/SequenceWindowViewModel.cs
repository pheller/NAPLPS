using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAPLPS;
using NAPLPS.Commands;
using NAPLPS.Drawing;
using ScottPlot;
using ScottPlot.Avalonia;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Color = Avalonia.Media.Color;
using Colors = Avalonia.Media.Colors;

namespace NAPLPSApp.ViewModels;

public partial class SequenceWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<CommandInfo> commands = new();

    [ObservableProperty]
    private NaplpsSequence? currentSequence;

    [ObservableProperty]
    private NaplpsState? state;

    [ObservableProperty]
    private CommandInfo? selectedCommand;

    [ObservableProperty]
    private SolidColorBrush? colorBackground;

    [ObservableProperty]
    private SolidColorBrush? colorForeground;

    [ObservableProperty]
    private SolidColorBrush? colorBackgroundText;

    [ObservableProperty]
    private SolidColorBrush? colorForegroundText;

    [ObservableProperty]
    private int selectedIndex;

    [ObservableProperty]
    private int currentFrame;

    [ObservableProperty]
    private int totalFrames;

    [ObservableProperty]
    private bool isDetailPaneVisible = true;

    [ObservableProperty]
    private bool isColorInfoVisible = false;

    [ObservableProperty]
    private bool isPlotviewVisible = true;

    [ObservableProperty]
    private string bitWidth = "7-Bit";

    [ObservableProperty]
    private string operandsDetails = string.Empty;

    [ObservableProperty]
    private string extraDetails = string.Empty;

    private AvaPlot avaPlot;

    private readonly DrawContext context;

    private NaplpsFormat loadedFile => context.NAPLPS;

    public SequenceWindowViewModel(DrawContext context, AvaPlot avaPlot)
    {
        this.context = context;
        this.avaPlot = avaPlot;

        this.avaPlot.Plot.Axes.SetLimits(0, 1, 0, 1);

        TotalFrames = (int)context.TotalFrames + 1;

        bitWidth = loadedFile.Is7Bit ? "7-Bit" : "8-Bit";

        LoadCommands();

        SelectedIndex = (int)context.CurrentIndex;

        SelectionChanged();
    }

    [RelayCommand]
    private void SelectionChanged()
    {
        CurrentFrame = SelectedIndex + 1;

        CurrentSequence = loadedFile.Commands[SelectedIndex];

        var command = CurrentSequence.Command;
        var state = CurrentSequence.State;

        OperandsDetails = command.Operands.Any() ? string.Join(" ", command.Operands.Select(operand => $"{operand:X2}")) + " " : string.Empty;
        ExtraDetails = string.Empty;
        IsColorInfoVisible = false;

        avaPlot.Plot.Clear();

        if (command is ShiftInCommand shiftInCommand)
        {
            ExtraDetails = shiftInCommand.Text;
        }
        else if (command is EscCommand escCommand)
        {
            var escapeCode = escCommand.Operands.First().ToString("X");

            ExtraDetails = $"Escape Char: 0x{escapeCode}, {escapeCode[0]}/{escapeCode[1]}\n";

            foreach (var extraChars in escCommand.Operands.Skip(1))
            {
                var extraCharsStr = extraChars.ToString("X");

                ExtraDetails += $"Extra Chars: 0x{extraCharsStr}, {extraCharsStr[0]}/{extraCharsStr[1]}\n";
            }
        }
        else if (command is SelectColorCommand selectColorCommand)
        {
            IsColorInfoVisible = true;


            var str = $" ColorMode: {state.ColorMode}\n";
            ExtraDetails += $"Foreground: {state.ColorMapForeground} | {state.ColorMap[state.ColorMapForeground]}\n";
            ExtraDetails += $"Background: {state.ColorMapBackground} | {state.ColorMap[state.ColorMapBackground]}\n\n";

            var colorBg = state.ColorMap[state.ColorMapBackground].ToColor();

            ColorBackground = new SolidColorBrush(Color.FromArgb(colorBg.A, colorBg.R, colorBg.G, colorBg.B));
            ColorBackgroundText = new SolidColorBrush(GetContrastingColor(colorBg));

            var colorFg = state.ColorMap[state.ColorMapForeground].ToColor();

            ColorForeground = new SolidColorBrush(Color.FromArgb(colorFg.A, colorFg.R, colorFg.G, colorFg.B));
            ColorForegroundText = new SolidColorBrush(GetContrastingColor(colorFg));
        }
        else if (command is TextCommand textCommand)
        {
            ExtraDetails = $"Text Field Size: {textCommand.State.TextFieldSize}";

            var point = new Coordinates(state.Pen.X, state.Pen.Y);
            var size = new Coordinates(state.TextFieldSize.X, state.TextFieldSize.Y);

            avaPlot.Plot.Add.Rectangle(
                point.X,
                point.Y,
                point.X + size.X,
                point.Y + size.Y
            );

            avaPlot.Plot.Add.Marker(point);
            avaPlot.Plot.Add.Marker(size);
        }
        else if (command is DomainCommand domainCommand)
        {
            ExtraDetails = $" Multibyte: {state.MultiByteValue}\n";
            ExtraDetails += $"Singlebyte: {state.SingleByteValue}";
        }
        else if (command is TextureCommand textureCommand)
        {
            ExtraDetails = $"   LINE: {textureCommand.LineTexture}\n";
            ExtraDetails += $"HIGHLGT: {textureCommand.ShouldHighlight}\n";
            ExtraDetails += $"TEXTURE: {textureCommand.TexturePattern}\n\n";
            ExtraDetails += $"MASK SZ: {textureCommand.MaskSize}";
        }
        else if (command is GeometricDrawingCommandBase baseDrawCommand)
        {
            ExtraDetails = "Draw Point(s):\n";

            var coords = new List<Coordinates>();

            foreach (var point in baseDrawCommand.Points)
            {
                ExtraDetails += $"{point}\n";

                coords.Add(new Coordinates(point.X, point.Y));
            }

            ExtraDetails += "Vertice(s):\n";

            foreach (var vert in baseDrawCommand.Vertices)
            {
                ExtraDetails += $"{vert}\n";
            }

            avaPlot.Plot.Clear();

            if (baseDrawCommand is PolygonCommand)
            {
                avaPlot.Plot.Add.Polygon([.. coords]);
            }
            //else (baseDrawCommand is LineCommand) 
            //{
            //    _plot.Plot.Add.Line()
            //}
            else if (baseDrawCommand is RectangleCommand rectangleCmd)
            {
                var point = new Coordinates(state.Pen.X, state.Pen.Y);
                var size = new Coordinates(rectangleCmd.Dimensions.X, rectangleCmd.Dimensions.Y);

                avaPlot.Plot.Clear();
                avaPlot.Plot.Add.Rectangle(
                    point.X,
                    point.Y,
                    point.X + size.X,
                    point.Y + size.Y
                );

                avaPlot.Plot.Add.Marker(point);
                avaPlot.Plot.Add.Marker(size);
            }

            var markers = avaPlot.Plot.Add.Markers(coords);

            markers.MarkerShape = MarkerShape.FilledCircle;
            markers.MarkerSize = 10;
        }

        IsPlotviewVisible = avaPlot.Plot.PlottableList.Any();
    }

    [RelayCommand]
    private void FramePrevious()
    {
        CurrentFrame--;

        if (CurrentFrame < 1)
        {
            CurrentFrame = TotalFrames;
        }

        SelectedIndex = CurrentFrame - 1;
    }

    [RelayCommand]
    private void FrameNext()
    {
        CurrentFrame++;

        if (CurrentFrame > TotalFrames)
        {
            CurrentFrame = 1;
        }

        SelectedIndex = CurrentFrame - 1;
    }

    [RelayCommand]
    private void ToggleDetailPane()
    {
        IsDetailPaneVisible = !IsDetailPaneVisible;
    }

    private void LoadCommands()
    {
        Commands.Clear();

        var index = 0;

        foreach (var sequence in loadedFile.Commands)
        {
            Commands.Add(new CommandInfo
            {
                Index = ++index,
                OpCode = sequence.Command.OpCode.ToString("X"),
                CommandType = sequence.Command.ToString().Replace("Command", string.Empty),
                State = sequence.State.ToString(),
            });
        }
    }

    private static Color GetContrastingColor(System.Drawing.Color backgroundColor)
    {
        // Convert RGB to YIQ (a color space used for broadcast color television).
        double yiq = ((backgroundColor.R * 299) + (backgroundColor.G * 587) + (backgroundColor.B * 114)) / 1000;

        // Determine whether the background color is light or dark.
        // YIQ value greater than 128 means it's a light color, so return dark color (black), otherwise return light color (white).
        return yiq >= 128 ? Colors.Black : Colors.White;
    }
}

public class CommandInfo
{
    public int Index { get; set; }

    public string OpCode { get; set; } = string.Empty;

    public string CommandType { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;
}
