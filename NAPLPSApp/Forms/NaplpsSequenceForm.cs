using NAPLPS;
using NAPLPS.Commands;
using System.CommandLine;
using System.Windows.Forms;

namespace NAPLPSApp.Forms
{
    public partial class NaplpsSequenceForm : Form
    {
        private List<NaplpsSequence> _sequence;

        public NaplpsSequenceForm(List<NaplpsSequence> sequence)
        {
            _sequence = sequence;

            this.FormClosing += (s, e) =>
            {
                e.Cancel = true;
                this.Hide();
            };

            InitializeComponent();

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

            var (command, state) = _sequence[selectedIndex];

            labelCommandName.ForeColor = Color.Black;
            tableLayoutPanelCommand.SetRowSpan(panelOperandsDisplay, 4);
            panelTextDisplay.Visible = false;

            if (!command.IsValid)
            {
                labelCommandName.ForeColor = Color.Red;
            }
            else if (command is ShiftInCommand textCommand)
            {
                labelCommandName.ForeColor = Color.DarkGoldenrod;

                tableLayoutPanelCommand.SetRowSpan(panelOperandsDisplay, 2);
                labelTextDisplay.Text = textCommand.Text;
                panelTextDisplay.Visible = true;
            }
            else if (command is GeometricDrawingCommandBase)
            {
                labelCommandName.ForeColor = Color.Green;
                tableLayoutPanelCommand.SetRowSpan(panelOperandsDisplay, 2);
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
                else if (sequence.Command is ShiftInCommand textCommand)
                {
                    sequenceDataGridView.Rows[^1].DefaultCellStyle.ForeColor = Color.DarkGoldenrod;
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
