// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

using NAPLPS;
using NAPLPSApp.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

namespace NAPLPSApp.Forms
{
    public partial class MainNaplpsForm : Form
    {
        public NaplpsFormat? LoadedFile { get; private set; }

        public string LoadedFilePath { get; private set; } = string.Empty;

        private readonly Size DefaultCanvasSize = new(1024, 768);

        private readonly List<string> resolutions = ["160x120", "320x200", "320x240", "640x480", "800x600", "1024x768", "1280x960", "1600x1200", "2048x1536", "4096x3072"];

        private readonly List<uint> baudrates = [0, 460800, 230400, 115200, 57600, 38400, 33600, 28800, 19200, 14400, 9600, 2400, 1200, 300];

        private readonly List<string> baudrateNames = ["Fastest", "460Kbps", "230Kbps", "115Kbps", "56Kbps", "38.4Kbps", "33.6Kbps", "28.8Kbps", "19.2Kbps", "14.4Kbps", "9.6Kbps", "2.4Kbps", "1.2Kbps", "300bps"];

        private DrawContext? ctx = null;

        private CancellationTokenSource? ctxRenderCancellationToken;

        private NaplpsSequenceForm? sequenceForm = null;

        private string canvasSize;

        private bool shouldAnimate = true;

        private uint baudRate = 2400;

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
                    if (LoadedFile != null) 
                    { 
                        FileRender(LoadedFile, canvasSize.StringSize()); 
                    }
                    else
                    {
                        UpdateUI();
                    }
                };

                if (item.Text == canvasSize) { item.Checked = true; }

                iconMenuItemSizes.DropDownItems.Add(item);
            }

            toolStripMenuItemAnimate.Checked = shouldAnimate;
            toolStripMenuItemAnimate.Click += (s, e) =>
            {
                toolStripMenuItemAnimate.Checked = shouldAnimate = !toolStripMenuItemAnimate.Checked;

                if (LoadedFile != null)
                {
                    FileRender(LoadedFile, canvasSize.StringSize()); // TODO Retrigger rendering
                }
                else
                {
                    UpdateUI();
                }
            };

            // Drawing Speeds
            var idx = 0;
            foreach (var baudrate in baudrates)
            {
                var name = baudrateNames[idx++];
                var item = new ToolStripMenuItem(name) { Tag = baudrate };

                item.Click += (sender, e) => {
                    baudRate = baudrate;
                    foreach (ToolStripMenuItem menuItem in iconMenuItemSpeedControl.DropDownItems) { menuItem.Checked = false; }
                    item.Checked = true;
                    if (LoadedFile != null)
                    {
                        FileRender(LoadedFile, canvasSize.StringSize());
                    }
                    else
                    {
                        UpdateUI();
                    }
                };

                if (item.Tag != null && (uint)item.Tag == baudRate) { item.Checked = true; }

                iconMenuItemSpeedControl.DropDownItems.Add(item);
            }

            UpdateUI();
        }

        public void SetFrame(uint frame)
        {
            if (ctx == null)
            {
                throw new ApplicationException("Opsie");
            }

            if (frame > ctx.TotalFrames)
            {
                return;
            }

            shouldAnimate = false;

            if (ctxRenderCancellationToken != null)
            {
                ctxRenderCancellationToken.Cancel();
            }

            ctx.Render(frame);
        }

        private void New(object sender, EventArgs e)
        {
            Close(sender, e);

            LoadedFile = NaplpsFormat.New();

            UpdateUI();
        }

        private void Open(object sender, EventArgs e)
        {
            var openDialog = new OpenFileDialog { InitialDirectory = Debugger.IsAttached ? "X:\\GitHub\\FoxCouncil\\NAPLPS\\Examples" : AppDomain.CurrentDomain.BaseDirectory };

            if (openDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(openDialog.FileName))
            {
                FileOpen(openDialog.FileName, canvasSize.StringSize());
            }

            UpdateUI();
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

            sequenceForm = new NaplpsSequenceForm(this, LoadedFile.Commands);
        }

        private void FileRender(NaplpsFormat file, Size size)
        {
            if (ctxRenderCancellationToken != null)
            {
                ctxRenderCancellationToken.Cancel();
                ctxRenderCancellationToken = null;
            }

            ctx = new DrawContext(file, size);
            ctx.OnImageUpdated += () =>
            {
                if (pictureBox.InvokeRequired)
                {
                    pictureBox.Invoke((MethodInvoker)(() => {
                        pictureBox.Image = ctx.ToImage();
                        toolStripStatusLabelFrame.Text = $"Frame: {ctx.CurrentIndex}";
                    }));
                }
                else
                {
                    pictureBox.Image = ctx.ToImage();
                    toolStripStatusLabelFrame.Text = $"Frame: {ctx.CurrentIndex}";
                }
            };

            if (shouldAnimate)
            {
                ctxRenderCancellationToken = new CancellationTokenSource();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        uint delayInMilliseconds = (uint)(baudRate == 0 ? 0 : (LoadedFile.Commands.Count * 8 * 1000.0 / baudRate));

                        await ctx.RenderAsync(ctxRenderCancellationToken.Token, delayInMilliseconds);
                    }
                    catch (OperationCanceledException)
                    {
                        // if (Debugger.IsAttached) MessageBox.Show("Render Cancelled");
                    }
                    finally
                    {
                        ctxRenderCancellationToken = null;
                        UpdateUI();
                    }
                });
            }
            else
            {
                ctx.Render();
            }
        }

        private void FileClose()
        {
            if (ctxRenderCancellationToken!= null)
            {
                ctxRenderCancellationToken.Cancel();
            }

            ctxRenderCancellationToken = null;

            LoadedFile = null;

            LoadedFilePath = string.Empty;

            pictureBox.Image = null;
            
            ctx?.Dispose();

            sequenceForm?.Dispose();
            sequenceForm = null;

            UpdateUI();
        }

        private void SequenceFormToggle()
        {
            if (sequenceForm != null)
            {
                if (!sequenceForm.Visible)
                {
                    sequenceForm.Show();
                }
                else
                {
                    sequenceForm.Hide();
                }
            }
        }

        private void UpdateUI()
        {
            var fileLoaded = LoadedFile != null;

            saveToolStripMenuItem.Enabled = saveAsToolStripMenuItem.Enabled = fileLoaded;

            closeToolStripMenuItem.Enabled = fileLoaded;

            iconDropDownButtonSequence.Enabled = fileLoaded;

            iconMenuItemSequence.Enabled = fileLoaded;

            toolStripStatusLabelResolution.Text = $"{canvasSize}";

            if (shouldAnimate)
            {
                toolStripStatusLabelResolution.Text += $" | {baudrateNames[baudrates.IndexOf(baudRate)]} | Animated";
            }
            else
            {
                toolStripStatusLabelResolution.Text += " | Static";
            }

            if (LoadedFile != null)
            {
                iconDropDownButtonSequence.Text = $"Commands: {LoadedFile.Commands.Count}";

                toolStripStatusLabelBitness.Text = $"Mode: {(LoadedFile.Is7Bit ? "7-bit" : "8-bit")}";
                toolStripStatusLabelFrame.Text = $"Frame: {ctx?.CurrentIndex}";

                Text = $"{Path.GetFileName(LoadedFilePath)} - NAPLPS Toolbox";
            }
            else
            {
                if (pictureBox.Image != null)
                {
                    pictureBox.Image.Dispose();
                    pictureBox.Image = null;
                }

                ctx?.Dispose();

                iconDropDownButtonSequence.Text = $"Commands: 0";

                toolStripStatusLabelBitness.Text = string.Empty;
                toolStripStatusLabelFrame.Text = string.Empty;

                Text = "NAPLPS Toolbox";
            }
        }
    }
}