// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS;
using NAPLPS.Commands;
using ScottPlot;
using ScottPlot.WinForms;
using System.Diagnostics;
using Color = System.Drawing.Color;
using FontStyle = System.Drawing.FontStyle;

namespace NAPLPSApp.Forms
{
    public partial class NaplpsSequenceForm : Form
    {
        private readonly List<NaplpsSequence> _sequence;

        private readonly FormsPlot plotter;

        private readonly MainNaplpsForm parentForm;

        private NaplpsSequence? _selectedSequence;

        public NaplpsSequenceForm(MainNaplpsForm parentForm, List<NaplpsSequence> sequence)
        {
            this.parentForm = parentForm;

            _sequence = sequence;

            FormClosing += (s, e) =>
            {
                e.Cancel = true;
                Hide();
            };

            InitializeComponent();

            plotter = new()
            {
                Dock = DockStyle.Fill,
                Visible = false,
            };

            plotter.Plot.Axes.SetLimits(0, 1, 0, 1);

            tableLayoutPanelCommand.Controls.Add(plotter, 0, 4);

            tableLayoutPanelCommand.SetColumnSpan(plotter, 2);
            tableLayoutPanelCommand.SetRowSpan(plotter, 6);

            labelCommandName.Click += (s, e) =>
            {
                if (_selectedSequence != null)
                {
                    Process.Start(new ProcessStartInfo($"https://github.com/FoxCouncil/NAPLPS/blob/main/NAPLPS/Commands/{_selectedSequence.Command}.cs") { UseShellExecute = true });
                }
            };

            iconToolStripButtonNext.Click += (s, e) => DataGridNext();
            iconToolStripButtonPrevious.Click += (s, e) => DataGridPrevious();

            panelOperandsDisplay.HorizontalScroll.Enabled = false;
            panelOperandsDisplay.HorizontalScroll.Visible = false;

            panelOperandsDisplay.VerticalScroll.Enabled = true;

            toolStripLabelCounter.Text = $"0000/{_sequence.Count}";

            toolStripLabelBits.Text = parentForm.LoadedFile != null && parentForm.LoadedFile.Is7Bit ? "7-bit" : "8-bit";

            UpdateDataGridUI();

            PopulateData();

            UpdateSelection();
        }

        private void DataGridNext()
        {
            sequenceDataGridView.Focus();

            if (sequenceDataGridView.CurrentRow != null)
            {
                var currentIndex = sequenceDataGridView.CurrentRow.Index;
                var nextIndex = currentIndex + 1;

                if (currentIndex >= sequenceDataGridView.Rows.Count - 1)
                {
                    nextIndex = 0;
                }

                sequenceDataGridView.CurrentCell = sequenceDataGridView.Rows[nextIndex].Cells[0];
            }

            UpdateSelection();
        }

        private void DataGridPrevious()
        {
            sequenceDataGridView.Focus();

            if (sequenceDataGridView.CurrentRow != null)
            {
                var currentIndex = sequenceDataGridView.CurrentRow.Index;
                var previousIndex = currentIndex - 1;

                if (currentIndex == 0)
                {
                    previousIndex = sequenceDataGridView.Rows.Count - 1;
                }

                sequenceDataGridView.CurrentCell = sequenceDataGridView.Rows[previousIndex].Cells[0];
            }

            UpdateSelection();
        }

        private void UpdateSelection(bool gotoFrame = false)
        {
            if (_sequence == null || _sequence.Count == 0)
            {
                return;
            }

            var selectedIndex = sequenceDataGridView.CurrentRow?.Index ?? 0;

            toolStripLabelCounter.Text = $"{(selectedIndex + 1).ToString().PadLeft(4, '0')}/{_sequence.Count.ToString().PadLeft(4, '0')}";

            var (command, state) = _selectedSequence = _sequence[selectedIndex];

            tableLayoutPanelCommand.SetRowSpan(panelOperandsDisplay, 4);
            panelTextDisplay.Visible = false;

            labelFGColorText.Visible = false;
            labelFGColor.Visible = false;

            labelBGColorText.Visible = false;
            labelBGColor.Visible = false;

            plotter.Visible = false;

            if (command is ShiftInCommand shiftInCommand)
            {
                tableLayoutPanelCommand.SetRowSpan(panelOperandsDisplay, 2);
                labelTextDisplay.Text = shiftInCommand.Text;
                panelTextDisplay.Visible = true;
            }
            else if (command is EscCommand escCommand)
            {
                tableLayoutPanelCommand.SetRowSpan(panelOperandsDisplay, 2);

                var escapeCode = escCommand.Operands.First().ToString("X");

                labelTextDisplay.Text = $"Escape Char: 0x{escapeCode}, {escapeCode[0]}/{escapeCode[1]}\n";

                foreach (var extraChars in escCommand.Operands.Skip(1))
                {
                    var extraCharsStr = extraChars.ToString("X");

                    labelTextDisplay.Text += $"Extra Chars: 0x{extraCharsStr}, {extraCharsStr[0]}/{extraCharsStr[1]}\n";
                }

                panelTextDisplay.Visible = true;
            }
            else if (command is SelectColorCommand selectColorCommand)
            {
                tableLayoutPanelCommand.SetRowSpan(panelOperandsDisplay, 2);

                labelTextDisplay.Text = $" ColorMode: {state.ColorMode}\n";
                labelTextDisplay.Text += $"Foreground: {state.ColorMapForeground}\n";
                labelTextDisplay.Text += $"Background: {state.ColorMapBackground}\n";

                panelTextDisplay.Visible = true;

                labelFGColorText.Text = state.ColorMap[state.ColorMapForeground].ToString();
                labelFGColor.BackColor = state.ColorMap[state.ColorMapForeground].ToColor();
                labelFGColor.ForeColor = GetContrastingColor(labelFGColor.BackColor);

                labelBGColorText.Text = state.ColorMap[state.ColorMapBackground].ToString();
                labelBGColor.BackColor = state.ColorMap[state.ColorMapBackground].ToColor();
                labelBGColor.ForeColor = GetContrastingColor(labelBGColor.BackColor);

                labelFGColorText.Visible = labelBGColorText.Visible = true;
                labelFGColor.Visible = labelBGColor.Visible = true;
            }
            else if (command is TextCommand textCommand)
            {
                tableLayoutPanelCommand.SetRowSpan(panelOperandsDisplay, 2);

                labelTextDisplay.Text = $"Text Field Size: {textCommand.State.TextFieldSize}\n";

                var point = new Coordinates(state.Pen.X, state.Pen.Y);
                var size = new Coordinates(state.TextFieldSize.X, state.TextFieldSize.Y);

                plotter.Plot.Clear();
                plotter.Plot.Add.Rectangle(
                    point.X,
                    point.Y,
                    point.X + size.X,
                    point.Y + size.Y
                );

                plotter.Plot.Add.Marker(point);
                plotter.Plot.Add.Marker(size);

                panelTextDisplay.Visible = true;
                plotter.Visible = true;
            }
            else if (command is DomainCommand domainCommand)
            {
                tableLayoutPanelCommand.SetRowSpan(panelOperandsDisplay, 2);

                labelTextDisplay.Text = $" Multibyte: {state.MultiByteValue}\n";
                labelTextDisplay.Text += $"Singlebyte: {state.SingleByteValue}\n";

                panelTextDisplay.Visible = true;
            }
            else if (command is TextureCommand textureCommand)
            {
                tableLayoutPanelCommand.SetRowSpan(panelOperandsDisplay, 2);

                labelTextDisplay.Text = $"   LINE: {textureCommand.LineTexture}\n";
                labelTextDisplay.Text += $"HIGHLGT: {textureCommand.ShouldHighlight}\n";
                labelTextDisplay.Text += $"TEXTURE: {textureCommand.TexturePattern}\n\n";
                labelTextDisplay.Text += $"MASK SZ: {textureCommand.MaskSize}\n";

                panelTextDisplay.Visible = true;
            }
            else if (command is GeometricDrawingCommandBase baseDrawCommand)
            {
                tableLayoutPanelCommand.SetRowSpan(panelOperandsDisplay, 2);

                labelTextDisplay.Text = "Draw Point(s):\n";

                var coords = new List<Coordinates>();

                foreach (var point in baseDrawCommand.Points)
                {
                    labelTextDisplay.Text += $"{point}\n";

                    coords.Add(new Coordinates(point.X, point.Y));
                }

                labelTextDisplay.Text += "Vertice(s):\n";

                foreach (var vert in baseDrawCommand.Vertices)
                {
                    labelTextDisplay.Text += $"{vert}\n";
                }

                plotter.Plot.Clear();

                if (baseDrawCommand is PolygonCommand)
                {
                    plotter.Plot.Add.Polygon([.. coords]);
                }
                //else (baseDrawCommand is LineCommand) 
                //{
                //    _plot.Plot.Add.Line()
                //}
                else if (baseDrawCommand is RectangleCommand rectangleCmd)
                {
                    var point = new Coordinates(state.Pen.X, state.Pen.Y);
                    var size = new Coordinates(rectangleCmd.Dimensions.X, rectangleCmd.Dimensions.Y);

                    plotter.Plot.Clear();
                    plotter.Plot.Add.Rectangle(
                        point.X,
                        point.Y,
                        point.X + size.X,
                        point.Y + size.Y
                    );

                    plotter.Plot.Add.Marker(point);
                    plotter.Plot.Add.Marker(size);
                }

                var markers = plotter.Plot.Add.Markers(coords);

                markers.MarkerShape = MarkerShape.FilledCircle;
                markers.MarkerSize = 10;

                panelTextDisplay.Visible = true;
                plotter.Visible = true;
            }

            labelCommandName.Text = command.ToString();
            labelOperandCount.Text = $"Operand(s): {command.Operands.Count}";
            labelOpcode.Text = $"0x{command.OpCode:X}";

            labelOperandsDisplay.Text = string.Empty;
            var idx = 0;

            foreach (var operand in command.Operands)
            {
                if (idx != 0 && idx % 10 == 0)
                {
                    labelOperandsDisplay.Text += "\n";
                }

                labelOperandsDisplay.Text += $"{operand:X} ";

                idx++;
            }

            propertyGridState.SelectedObject = state ?? null;

            if (gotoFrame)
            {
                parentForm.SetFrame((uint)selectedIndex);
            }
        }

        private void PopulateData()
        {
            var idx = 0;

            foreach (NaplpsSequence sequence in _sequence)
            {
                var idxName = idx.ToString().PadLeft(4, '0');
                var commandIdName = $"0x{(byte)sequence.Command.OpCode:X2}";
                var commandName = sequence.Command.ToString().Replace("Command", string.Empty);
                var sequenceData = sequence.State.ToString();

                sequenceDataGridView.Rows.Add(new[] { idxName, commandIdName, commandName, sequenceData });

                if (!sequence.Command.IsValid)
                {
                    sequenceDataGridView.Rows[^1].DefaultCellStyle.ForeColor = Color.White;
                    sequenceDataGridView.Rows[^1].DefaultCellStyle.BackColor = Color.Red;
                }
                else if (sequence.Command is EscCommand)
                {
                    sequenceDataGridView.Rows[^1].DefaultCellStyle.ForeColor = Color.DarkViolet;
                }
                else if (sequence.Command is ShiftInCommand)
                {
                    sequenceDataGridView.Rows[^1].DefaultCellStyle.ForeColor = Color.DarkGoldenrod;
                }
                else if (sequence.Command is SelectColorCommand)
                {
                    sequenceDataGridView.Rows[^1].DefaultCellStyle.ForeColor = Color.DeepPink;
                }
                else if (sequence.Command is TextCommand)
                {
                    sequenceDataGridView.Rows[^1].DefaultCellStyle.ForeColor = Color.DarkBlue;
                }
                else if (sequence.Command is DomainCommand)
                {
                    sequenceDataGridView.Rows[^1].DefaultCellStyle.ForeColor = Color.DarkOrange;
                }
                else if (sequence.Command is GeometricDrawingCommandBase)
                {
                    sequenceDataGridView.Rows[^1].DefaultCellStyle.ForeColor = Color.Green;
                }

                idx++;
            }
        }

        private static Color GetContrastingColor(Color backgroundColor)
        {
            // Convert RGB to YIQ (a color space used for broadcast color television).
            double yiq = ((backgroundColor.R * 299) + (backgroundColor.G * 587) + (backgroundColor.B * 114)) / 1000;

            // Determine whether the background color is light or dark.
            // YIQ value greater than 128 means it's a light color, so return dark color (black), otherwise return light color (white).
            return yiq >= 128 ? Color.Black : Color.White;
        }

        private void UpdateDataGridUI()
        {
            sequenceDataGridView.ColumnCount = 4;

            sequenceDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
            sequenceDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            sequenceDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Consolas", 10f, FontStyle.Bold);

            sequenceDataGridView.DefaultCellStyle.Font = new Font("Consolas", 9f, FontStyle.Regular);

            sequenceDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            sequenceDataGridView.MultiSelect = false;
            sequenceDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            sequenceDataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            sequenceDataGridView.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            sequenceDataGridView.GridColor = Color.Black;
            sequenceDataGridView.RowHeadersVisible = false;

            sequenceDataGridView.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            sequenceDataGridView.Columns[0].Name = "Index";
            sequenceDataGridView.Columns[0].Width = 70;
            sequenceDataGridView.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            sequenceDataGridView.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            sequenceDataGridView.Columns[1].Name = "";
            sequenceDataGridView.Columns[1].Width = 60;
            sequenceDataGridView.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            sequenceDataGridView.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
            sequenceDataGridView.Columns[2].Name = "Command";
            sequenceDataGridView.Columns[2].Width = 200;

            sequenceDataGridView.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;
            sequenceDataGridView.Columns[3].Name = "State";
            sequenceDataGridView.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            sequenceDataGridView.SelectionChanged += (s, e) => UpdateSelection();
            sequenceDataGridView.DoubleClick += (s, e) => UpdateSelection(true);
        }

        private void iconToolStripButtonAddCommand_Click(object sender, EventArgs e)
        {
            var addNewCommandForm = new NewCommandForm();

            addNewCommandForm.ShowDialog(this);
        }
    }
}
