// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSApp.Forms
{
    partial class MainNaplpsForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainNaplpsForm));
            menuMain = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            newNAPLPSFileToolStripMenuItem = new FontAwesome.Sharp.IconMenuItem();
            openToolStripMenuItem = new FontAwesome.Sharp.IconMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            saveToolStripMenuItem = new FontAwesome.Sharp.IconMenuItem();
            saveAsToolStripMenuItem = new FontAwesome.Sharp.IconMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            closeToolStripMenuItem = new FontAwesome.Sharp.IconMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            quitToolStripMenuItem = new FontAwesome.Sharp.IconMenuItem();
            renderToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItemAnimate = new ToolStripMenuItem();
            iconMenuItemSpeedControl = new FontAwesome.Sharp.IconMenuItem();
            toolStripSeparatorRender = new ToolStripSeparator();
            iconMenuItemDebugLayers = new FontAwesome.Sharp.IconMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            iconMenuItemSequence = new FontAwesome.Sharp.IconMenuItem();
            iconMenuItemSizes = new FontAwesome.Sharp.IconMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new FontAwesome.Sharp.IconMenuItem();
            statusStripMain = new StatusStrip();
            iconDropDownButtonSequence = new FontAwesome.Sharp.IconDropDownButton();
            toolStripStatusLabelFrame = new ToolStripStatusLabel();
            toolStripStatusLabelBitness = new ToolStripStatusLabel();
            toolStripStatusLabelResolution = new ToolStripStatusLabel();
            pictureBox = new PictureBox();
            menuMain.SuspendLayout();
            statusStripMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            SuspendLayout();
            // 
            // menuMain
            // 
            menuMain.ImageScalingSize = new Size(24, 24);
            menuMain.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, renderToolStripMenuItem, viewToolStripMenuItem, helpToolStripMenuItem });
            menuMain.Location = new Point(0, 0);
            menuMain.Name = "menuMain";
            menuMain.Padding = new Padding(4, 1, 0, 1);
            menuMain.Size = new Size(640, 24);
            menuMain.TabIndex = 0;
            menuMain.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newNAPLPSFileToolStripMenuItem, openToolStripMenuItem, toolStripSeparator1, saveToolStripMenuItem, saveAsToolStripMenuItem, toolStripSeparator2, closeToolStripMenuItem, toolStripSeparator3, quitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 22);
            fileToolStripMenuItem.Text = "&File";
            // 
            // newNAPLPSFileToolStripMenuItem
            // 
            newNAPLPSFileToolStripMenuItem.IconChar = FontAwesome.Sharp.IconChar.FileCirclePlus;
            newNAPLPSFileToolStripMenuItem.IconColor = Color.Black;
            newNAPLPSFileToolStripMenuItem.IconFont = FontAwesome.Sharp.IconFont.Auto;
            newNAPLPSFileToolStripMenuItem.Name = "newNAPLPSFileToolStripMenuItem";
            newNAPLPSFileToolStripMenuItem.Size = new Size(165, 22);
            newNAPLPSFileToolStripMenuItem.Text = "&New NAPLPS File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.IconChar = FontAwesome.Sharp.IconChar.FolderOpen;
            openToolStripMenuItem.IconColor = Color.Black;
            openToolStripMenuItem.IconFont = FontAwesome.Sharp.IconFont.Auto;
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(165, 22);
            openToolStripMenuItem.Text = "&Open";
            openToolStripMenuItem.Click += Open;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(162, 6);
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Enabled = false;
            saveToolStripMenuItem.IconChar = FontAwesome.Sharp.IconChar.FileDownload;
            saveToolStripMenuItem.IconColor = Color.Black;
            saveToolStripMenuItem.IconFont = FontAwesome.Sharp.IconFont.Auto;
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new Size(165, 22);
            saveToolStripMenuItem.Text = "&Save";
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Enabled = false;
            saveAsToolStripMenuItem.IconChar = FontAwesome.Sharp.IconChar.FileUpload;
            saveAsToolStripMenuItem.IconColor = Color.Black;
            saveAsToolStripMenuItem.IconFont = FontAwesome.Sharp.IconFont.Auto;
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.Size = new Size(165, 22);
            saveAsToolStripMenuItem.Text = "Sav&e As";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(162, 6);
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.Enabled = false;
            closeToolStripMenuItem.IconChar = FontAwesome.Sharp.IconChar.None;
            closeToolStripMenuItem.IconColor = Color.Black;
            closeToolStripMenuItem.IconFont = FontAwesome.Sharp.IconFont.Auto;
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.Size = new Size(165, 22);
            closeToolStripMenuItem.Text = "&Close";
            closeToolStripMenuItem.Click += Close;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(162, 6);
            // 
            // quitToolStripMenuItem
            // 
            quitToolStripMenuItem.IconChar = FontAwesome.Sharp.IconChar.XmarkCircle;
            quitToolStripMenuItem.IconColor = Color.Black;
            quitToolStripMenuItem.IconFont = FontAwesome.Sharp.IconFont.Auto;
            quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            quitToolStripMenuItem.Size = new Size(165, 22);
            quitToolStripMenuItem.Text = "&Quit";
            quitToolStripMenuItem.Click += Quit;
            // 
            // renderToolStripMenuItem
            // 
            renderToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemAnimate, iconMenuItemSpeedControl, toolStripSeparatorRender, iconMenuItemDebugLayers });
            renderToolStripMenuItem.Name = "renderToolStripMenuItem";
            renderToolStripMenuItem.Size = new Size(56, 22);
            renderToolStripMenuItem.Text = "Render";
            // 
            // toolStripMenuItemAnimate
            // 
            toolStripMenuItemAnimate.Checked = true;
            toolStripMenuItemAnimate.CheckState = CheckState.Checked;
            toolStripMenuItemAnimate.Name = "toolStripMenuItemAnimate";
            toolStripMenuItemAnimate.Size = new Size(119, 22);
            toolStripMenuItemAnimate.Text = "Animate";
            // 
            // iconMenuItemSpeedControl
            // 
            iconMenuItemSpeedControl.IconChar = FontAwesome.Sharp.IconChar.GaugeMed;
            iconMenuItemSpeedControl.IconColor = Color.Black;
            iconMenuItemSpeedControl.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconMenuItemSpeedControl.Name = "iconMenuItemSpeedControl";
            iconMenuItemSpeedControl.Size = new Size(119, 22);
            iconMenuItemSpeedControl.Text = "Speed";
            // 
            // toolStripSeparatorRender
            // 
            toolStripSeparatorRender.Name = "toolStripSeparatorRender";
            toolStripSeparatorRender.Size = new Size(116, 6);
            // 
            // iconMenuItemDebugLayers
            // 
            iconMenuItemDebugLayers.IconChar = FontAwesome.Sharp.IconChar.LayerGroup;
            iconMenuItemDebugLayers.IconColor = Color.Black;
            iconMenuItemDebugLayers.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconMenuItemDebugLayers.Name = "iconMenuItemDebugLayers";
            iconMenuItemDebugLayers.Size = new Size(119, 22);
            iconMenuItemDebugLayers.Text = "Layers";
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { iconMenuItemSequence, iconMenuItemSizes });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(44, 22);
            viewToolStripMenuItem.Text = "&View";
            // 
            // iconMenuItemSequence
            // 
            iconMenuItemSequence.Enabled = false;
            iconMenuItemSequence.IconChar = FontAwesome.Sharp.IconChar.List12;
            iconMenuItemSequence.IconColor = Color.Black;
            iconMenuItemSequence.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconMenuItemSequence.Name = "iconMenuItemSequence";
            iconMenuItemSequence.Size = new Size(134, 22);
            iconMenuItemSequence.Text = "Sequence";
            // 
            // iconMenuItemSizes
            // 
            iconMenuItemSizes.IconChar = FontAwesome.Sharp.IconChar.Expand;
            iconMenuItemSizes.IconColor = Color.Black;
            iconMenuItemSizes.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconMenuItemSizes.Name = "iconMenuItemSizes";
            iconMenuItemSizes.Size = new Size(134, 22);
            iconMenuItemSizes.Text = "Render Size";
            iconMenuItemSizes.TextImageRelation = TextImageRelation.TextBeforeImage;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 22);
            helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.IconChar = FontAwesome.Sharp.IconChar.QuestionCircle;
            aboutToolStripMenuItem.IconColor = Color.Black;
            aboutToolStripMenuItem.IconFont = FontAwesome.Sharp.IconFont.Auto;
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(107, 22);
            aboutToolStripMenuItem.Text = "&About";
            // 
            // statusStripMain
            // 
            statusStripMain.ImageScalingSize = new Size(24, 24);
            statusStripMain.Items.AddRange(new ToolStripItem[] { iconDropDownButtonSequence, toolStripStatusLabelFrame, toolStripStatusLabelBitness, toolStripStatusLabelResolution });
            statusStripMain.Location = new Point(0, 417);
            statusStripMain.Name = "statusStripMain";
            statusStripMain.Padding = new Padding(1, 0, 10, 0);
            statusStripMain.Size = new Size(640, 30);
            statusStripMain.TabIndex = 1;
            statusStripMain.Text = "statusStrip1";
            // 
            // iconDropDownButtonSequence
            // 
            iconDropDownButtonSequence.Enabled = false;
            iconDropDownButtonSequence.IconChar = FontAwesome.Sharp.IconChar.List12;
            iconDropDownButtonSequence.IconColor = Color.Black;
            iconDropDownButtonSequence.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconDropDownButtonSequence.ImageAlign = ContentAlignment.MiddleLeft;
            iconDropDownButtonSequence.ImageTransparentColor = Color.Magenta;
            iconDropDownButtonSequence.Name = "iconDropDownButtonSequence";
            iconDropDownButtonSequence.Size = new Size(118, 28);
            iconDropDownButtonSequence.Text = "Commands: 0";
            iconDropDownButtonSequence.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabelFrame
            // 
            toolStripStatusLabelFrame.Name = "toolStripStatusLabelFrame";
            toolStripStatusLabelFrame.Size = new Size(52, 25);
            toolStripStatusLabelFrame.Text = "Frame: 0";
            // 
            // toolStripStatusLabelBitness
            // 
            toolStripStatusLabelBitness.Name = "toolStripStatusLabelBitness";
            toolStripStatusLabelBitness.Size = new Size(38, 25);
            toolStripStatusLabelBitness.Text = "Mode";
            // 
            // toolStripStatusLabelResolution
            // 
            toolStripStatusLabelResolution.Name = "toolStripStatusLabelResolution";
            toolStripStatusLabelResolution.Size = new Size(63, 25);
            toolStripStatusLabelResolution.Text = "Resolution";
            // 
            // pictureBox
            // 
            pictureBox.BackColor = SystemColors.Desktop;
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.InitialImage = (Image)resources.GetObject("pictureBox.InitialImage");
            pictureBox.Location = new Point(0, 24);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new Size(640, 393);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.TabIndex = 2;
            pictureBox.TabStop = false;
            // 
            // MainNaplpsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(640, 447);
            Controls.Add(pictureBox);
            Controls.Add(statusStripMain);
            Controls.Add(menuMain);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuMain;
            Margin = new Padding(2);
            Name = "MainNaplpsForm";
            Text = "NAPLPS Toolbox";
            menuMain.ResumeLayout(false);
            menuMain.PerformLayout();
            statusStripMain.ResumeLayout(false);
            statusStripMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuMain;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem helpToolStripMenuItem;
        private StatusStrip statusStripMain;
        private FontAwesome.Sharp.IconMenuItem newNAPLPSFileToolStripMenuItem;
        private FontAwesome.Sharp.IconMenuItem openToolStripMenuItem;
        private FontAwesome.Sharp.IconMenuItem saveToolStripMenuItem;
        private FontAwesome.Sharp.IconMenuItem saveAsToolStripMenuItem;
        private FontAwesome.Sharp.IconMenuItem closeToolStripMenuItem;
        private FontAwesome.Sharp.IconMenuItem quitToolStripMenuItem;
        private FontAwesome.Sharp.IconMenuItem aboutToolStripMenuItem;
        private FontAwesome.Sharp.IconDropDownButton iconDropDownButtonSequence;
        private PictureBox pictureBox;
        private ToolStripStatusLabel toolStripStatusLabelBitness;
        private ToolStripMenuItem viewToolStripMenuItem;
        private FontAwesome.Sharp.IconMenuItem iconMenuItemSequence;
        private FontAwesome.Sharp.IconMenuItem iconMenuItemSizes;
        private ToolStripMenuItem renderToolStripMenuItem;
        private FontAwesome.Sharp.IconMenuItem iconMenuItemSpeedControl;
        private ToolStripMenuItem toolStripMenuItemAnimate;
        private ToolStripSeparator toolStripSeparatorRender;
        private FontAwesome.Sharp.IconMenuItem iconMenuItemDebugLayers;
        private ToolStripStatusLabel toolStripStatusLabelFrame;
        private ToolStripStatusLabel toolStripStatusLabelResolution;
    }
}