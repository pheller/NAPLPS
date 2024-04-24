// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS;
using NAPLPSApp.Drawing;

namespace NAPLPSApp.Forms
{
    public partial class MainNaplpsForm : Form
    {
        public NaplpsFormat? LoadedFile { get; private set; }

        public string LoadedFilePath { get; private set; } = string.Empty;

        private DrawContext? _context = null;

        private NaplpsSequenceForm? _sequenceForm = null;

        public MainNaplpsForm()
        {
            InitializeComponent();

            iconDropDownButtonCommands.Click += (s, e) =>
            {
                if (_sequenceForm != null)
                {
                    if (!_sequenceForm.Visible)
                    {
                        _sequenceForm.Show();
                    }
                    else
                    {
                        _sequenceForm.Hide();
                    }
                }
            };

            UpdateUI();
        }

        private void New(object sender, EventArgs e)
        {
            Close(sender, e);

            LoadedFile = NaplpsFormat.New();

            UpdateUI();
        }

        private void Open(object sender, EventArgs e)
        {
            var openDialog = new OpenFileDialog { InitialDirectory = AppDomain.CurrentDomain.BaseDirectory };

            if (openDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(openDialog.FileName))
            {
                LoadedFilePath = openDialog.FileName;

                LoadedFile = NaplpsFormat.FromFile(LoadedFilePath);

                _context = new DrawContext(LoadedFile, new Size(1024, 768));

                pictureBox.Image = _context.ToImage();

                _sequenceForm = new NaplpsSequenceForm(LoadedFile.Commands);
            }

            UpdateUI();
        }

        private void Close(object sender, EventArgs e)
        {
            LoadedFile = null;

            LoadedFilePath = string.Empty;

            _sequenceForm?.Dispose();
            _sequenceForm = null;

            UpdateUI();
        }

        private void Quit(object sender, EventArgs e)
        {
            Close();
        }

        private void UpdateUI()
        {
            var fileLoaded = LoadedFile != null;

            saveToolStripMenuItem.Enabled = saveAsToolStripMenuItem.Enabled = fileLoaded;

            closeToolStripMenuItem.Enabled = fileLoaded;

            iconDropDownButtonCommands.Enabled = fileLoaded;

            if (LoadedFile != null)
            {
                iconDropDownButtonCommands.Text = $"Commands: {LoadedFile.Commands.Count}";

                toolStripStatusLabelBitness.Text = LoadedFile.Is7Bit ? "7-bit" : "8-bit";

                Text = $"{Path.GetFileName(LoadedFilePath)} - NAPLPS Toolbox";
            }
            else
            {
                if (pictureBox.Image != null)
                {
                    pictureBox.Image.Dispose();
                    pictureBox.Image = null;
                }

                if (_context != null)
                {
                    _context.Dispose();
                }

                iconDropDownButtonCommands.Text = $"Commands: 0";

                toolStripStatusLabelBitness.Text = string.Empty;

                Text = "NAPLPS Toolbox";
            }
        }
    }
}