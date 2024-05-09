// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS;
using NAPLPSApp.Drawing;
using System.Diagnostics;

namespace NAPLPSApp.Forms
{
    public partial class MainNaplpsForm : Form
    {
        public NaplpsFormat? LoadedFile { get; private set; }

        public string LoadedFilePath { get; private set; } = string.Empty;

        private readonly Size DefaultCanvasSize = new(1024, 768);

        private readonly List<string> resolutions = ["160x120", "320x200", "320x240", "640x480", "800x600", "1024x768", "1280x960", "1600x1200", "2048x1536", "4096x3072"];

        private DrawContext? _context = null;

        private NaplpsSequenceForm? _sequenceForm = null;

        private string canvasSize;

        public MainNaplpsForm()
        {
            InitializeComponent();

            iconDropDownButtonSequence.Click += (s, e) => SequenceFormToggle();
            iconMenuItemSequence.Click += (s, e) => SequenceFormToggle();

            canvasSize = DefaultCanvasSize.SizeString();

            // UI Sizes
            foreach (var resolution in resolutions)
            {
                var item = new ToolStripMenuItem(resolution);

                item.Click += (sender, e) => { 
                    canvasSize = resolution;
                    foreach (ToolStripMenuItem menuItem in iconMenuItemSizes.DropDownItems) { menuItem.Checked = false; }
                    item.Checked = true;
                    if (LoadedFile != null) { FileRender(LoadedFile, canvasSize.StringSize()); }
                };

                if (item.Text == canvasSize) { item.Checked = true; }

                iconMenuItemSizes.DropDownItems.Add(item);
            }

            UIUpdate();
        }

        private void New(object sender, EventArgs e)
        {
            Close(sender, e);

            LoadedFile = NaplpsFormat.New();

            UIUpdate();
        }

        private void Open(object sender, EventArgs e)
        {
            var openDialog = new OpenFileDialog { InitialDirectory = Debugger.IsAttached ? "X:\\GitHub\\FoxCouncil\\NAPLPS\\Examples" : AppDomain.CurrentDomain.BaseDirectory };

            if (openDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(openDialog.FileName))
            {
                FileOpen(openDialog.FileName, canvasSize.StringSize());
            }

            UIUpdate();
        }

        private void Close(object sender, EventArgs e)
        {
            FileClose();
        }

        private void Quit(object sender, EventArgs e)
        {
            Close();
        }

        private void FileOpen(string file, Size? size = null)
        {
            FileClose();

            LoadedFilePath = file;

            LoadedFile = NaplpsFormat.FromFile(LoadedFilePath);

            if (size == null)
            {
                size = DefaultCanvasSize;
            }

            FileRender(LoadedFile, size.Value);

            _sequenceForm = new NaplpsSequenceForm(this, LoadedFile.Commands);

            int newFormX = Location.X + Width;
            int newFormY = Location.Y;

            if (Screen.PrimaryScreen != null)
            {
                // Check if the child form will be out of the screen at this position
                if (newFormX + _sequenceForm.Width > Screen.PrimaryScreen.WorkingArea.Width)
                {
                    // If it doesn't fit, position it to the left of the parent form instead
                    newFormX = Location.X - _sequenceForm.Width;
                }

                if (newFormY + _sequenceForm.Height > Screen.PrimaryScreen.WorkingArea.Height)
                {
                    // If bottom is out of screen, adjust it upwards
                    newFormY = Screen.PrimaryScreen.WorkingArea.Height - _sequenceForm.Height;
                }
            }

            // Set the location of the child form
            _sequenceForm.Location = new Point(newFormX, newFormY);
        }

        private void FileRender(NaplpsFormat file, Size size)
        {
            _context = new DrawContext(file, size);

            pictureBox.Image = _context.ToImage();
        }

        private void FileClose()
        {
            LoadedFile = null;

            LoadedFilePath = string.Empty;

            _sequenceForm?.Dispose();
            _sequenceForm = null;

            UIUpdate();
        }

        private void SequenceFormToggle()
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
        }

        private void UIUpdate()
        {
            var fileLoaded = LoadedFile != null;

            saveToolStripMenuItem.Enabled = saveAsToolStripMenuItem.Enabled = fileLoaded;

            closeToolStripMenuItem.Enabled = fileLoaded;

            iconDropDownButtonSequence.Enabled = fileLoaded;

            iconMenuItemSequence.Enabled = fileLoaded;

            if (LoadedFile != null)
            {
                iconDropDownButtonSequence.Text = $"Commands: {LoadedFile.Commands.Count}";

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

                _context?.Dispose();

                iconDropDownButtonSequence.Text = $"Commands: 0";

                toolStripStatusLabelBitness.Text = string.Empty;

                Text = "NAPLPS Toolbox";
            }
        }
    }
}