// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSApp
{
#if NET8_0_WINDOWS
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
            helpToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new FontAwesome.Sharp.IconMenuItem();
            statusStripMain = new StatusStrip();
            iconDropDownButtonCommands = new FontAwesome.Sharp.IconDropDownButton();
            pictureBox = new PictureBox();
            toolStripStatusLabelBitness = new ToolStripStatusLabel();
            menuMain.SuspendLayout();
            statusStripMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            SuspendLayout();
            // 
            // menuMain
            // 
            menuMain.ImageScalingSize = new Size(24, 24);
            menuMain.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, helpToolStripMenuItem });
            menuMain.Location = new Point(0, 0);
            menuMain.Name = "menuMain";
            menuMain.Padding = new Padding(4, 1, 0, 1);
            menuMain.Size = new Size(560, 24);
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
            statusStripMain.Items.AddRange(new ToolStripItem[] { iconDropDownButtonCommands, toolStripStatusLabelBitness });
            statusStripMain.Location = new Point(0, 240);
            statusStripMain.Name = "statusStripMain";
            statusStripMain.Padding = new Padding(1, 0, 10, 0);
            statusStripMain.Size = new Size(560, 30);
            statusStripMain.TabIndex = 1;
            statusStripMain.Text = "statusStrip1";
            // 
            // iconDropDownButtonCommands
            // 
            iconDropDownButtonCommands.Enabled = false;
            iconDropDownButtonCommands.IconChar = FontAwesome.Sharp.IconChar.ListDots;
            iconDropDownButtonCommands.IconColor = Color.Black;
            iconDropDownButtonCommands.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconDropDownButtonCommands.ImageAlign = ContentAlignment.MiddleLeft;
            iconDropDownButtonCommands.ImageTransparentColor = Color.Magenta;
            iconDropDownButtonCommands.Name = "iconDropDownButtonCommands";
            iconDropDownButtonCommands.Size = new Size(118, 28);
            iconDropDownButtonCommands.Text = "Commands: 0";
            iconDropDownButtonCommands.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pictureBox
            // 
            pictureBox.BackColor = SystemColors.Desktop;
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.InitialImage = (Image)resources.GetObject("pictureBox.InitialImage");
            pictureBox.Location = new Point(0, 24);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new Size(560, 216);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.TabIndex = 2;
            pictureBox.TabStop = false;
            // 
            // toolStripStatusLabelBitness
            // 
            toolStripStatusLabelBitness.Name = "toolStripStatusLabelBitness";
            toolStripStatusLabelBitness.Size = new Size(0, 25);
            // 
            // MainNaplpsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(560, 270);
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
        private FontAwesome.Sharp.IconDropDownButton iconDropDownButtonCommands;
        private PictureBox pictureBox;
        private ToolStripStatusLabel toolStripStatusLabelBitness;
    }
#endif
}
