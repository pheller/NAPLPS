// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

using Avalonia.Media;

namespace NAPLPSApp.Editor;

public class PaletteColor
{
    public byte Index { get; set; }
    public IBrush Brush { get; set; } = Brushes.Black;
}
