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

        private readonly FormsPlot _plot;

        private NaplpsSequence? _selectedSequence;

        public NaplpsSequenceForm(List<NaplpsSequence> sequence)
        {
            _sequence = sequence;

            FormClosing += (s, e) =>
            {
                e.Cancel = true;
                Hide();
            };

            InitializeComponent();

            _plot = new() { 
                Dock = DockStyle.Fill,
                Visible = false,
            };
            _plot.Plot.Axes.SetLimits(0, 1, 0, 1);

            tableLayoutPanelCommand.Controls.Add(_plot, 0, 4);

            tableLayoutPanelCommand.SetColumnSpan(_plot, 2);
            tableLayoutPanelCommand.SetRowSpan(_plot, 6);

            labelCommandName.Click += (s, e) =>
            {
                if (_selectedSequence != null)
                {
                    Process.Start(new ProcessStartInfo($"https://github.com/FoxCouncil/NAPLPS/blob/main/NAPLPS/Commands/{_selectedSequence.Command}.cs") { UseShellExecute = true });
                }
            };

            panelOperandsDisplay.HorizontalScroll.Enabled = false;
            panelOperandsDisplay.HorizontalScroll.Visible = false;

            panelOperandsDisplay.VerticalScroll.Enabled = true;

            UpdateDataGridUI();

            PopulateData();

            UpdateSelection();
        }

        private void UpdateSelection()
        {
            var selectedIndex = sequenceDataGridView.CurrentRow?.Index ?? 0;

            toolStripLabelCurrentIndex.Text = selectedIndex.ToString().PadLeft(4, '0');

            var (command, state) = _selectedSequence = _sequence[selectedIndex];

            tableLayoutPanelCommand.SetRowSpan(panelOperandsDisplay, 4);
            panelTextDisplay.Visible = false;
            _plot.Visible = false;

            if (command is ShiftInCommand textCommand)
            {
                tableLayoutPanelCommand.SetRowSpan(panelOperandsDisplay, 2);
                labelTextDisplay.Text = textCommand.Text;
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
            else if (command is DomainCommand domainCommand)
            {
                tableLayoutPanelCommand.SetRowSpan(panelOperandsDisplay, 2);

                labelTextDisplay.Text  = $" Multibyte: {domainCommand.State.MultiByteValue}\n";
                labelTextDisplay.Text += $"Singlebyte: {domainCommand.State.SingleByteValue}\n";

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

                _plot.Plot.Clear();

                if (baseDrawCommand is PolygonCommand)
                {
                    _plot.Plot.Add.Polygon([..coords]);
                }
                //else (baseDrawCommand is LineCommand) 
                //{
                //    _plot.Plot.Add.Line()
                //}

                var markers = _plot.Plot.Add.Markers(coords);
                
                markers.MarkerShape = MarkerShape.FilledCircle;
                markers.MarkerSize = 5;

                panelTextDisplay.Visible = true;
                _plot.Visible = true;
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
        }

        private void PopulateData()
        {
            toolStripLabelCount.Text = $"Total: {_sequence.Count}";

            var idx = 0;

            foreach (NaplpsSequence sequence in _sequence)
            {
                var idxName = idx.ToString().PadLeft(4, '0');
                var commandIdName = $"0x{((byte)sequence.Command.OpCode):X2}";
                var commandName = sequence.Command.ToString().Replace("Command", string.Empty);
                var sequenceData = sequence.State.ToString();

                sequenceDataGridView.Rows.Add(new[] { idxName, commandIdName, commandName, sequenceData });

                if (!sequence.Command.IsValid)
                {
                    sequenceDataGridView.Rows[^1].DefaultCellStyle.ForeColor = Color.Red;
                }
                else if (sequence.Command is EscCommand)
                {
                    sequenceDataGridView.Rows[^1].DefaultCellStyle.ForeColor = Color.DarkViolet;
                }
                else if (sequence.Command is ShiftInCommand)
                {
                    sequenceDataGridView.Rows[^1].DefaultCellStyle.ForeColor = Color.DarkGoldenrod;
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
        }
    }
}
