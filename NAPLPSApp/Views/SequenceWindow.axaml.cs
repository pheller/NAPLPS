// Copyright (c) 2025 FoxCouncil & Contributors - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSApp.Views;

public partial class SequenceWindow : Window
{
    public SequenceWindow()
    {
        InitializeComponent();
    }

    public SequenceWindow(DrawContext drawContext) : this()
    {
        var vectorPlot = this.Find<AvaPlot>("VectorPlot");

        if (vectorPlot is null)
        {
            return;
        }

        DataContext = new SequenceWindowViewModel(drawContext, vectorPlot);
    }
}
