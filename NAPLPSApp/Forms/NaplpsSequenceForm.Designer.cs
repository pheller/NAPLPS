// Copyright (c) 2024 FoxCouncil - https://github.com/FoxCouncil/NAPLPS

namespace NAPLPSApp.Forms
{
    partial class NaplpsSequenceForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NaplpsSequenceForm));
            toolStrip = new ToolStrip();
            iconToolStripButtonPrevious = new FontAwesome.Sharp.IconToolStripButton();
            iconToolStripButtonNext = new FontAwesome.Sharp.IconToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripLabelCounter = new ToolStripLabel();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStripLabelBits = new ToolStripLabel();
            toolStripSeparator4 = new ToolStripSeparator();
            sequenceDataGridView = new DataGridView();
            splitContainerVertical = new SplitContainer();
            splitContainerHorizontal = new SplitContainer();
            tableLayoutPanelCommand = new TableLayoutPanel();
            labelBGColorText = new Label();
            labelFGColorText = new Label();
            labelBGColor = new Label();
            labelFGColor = new Label();
            panelTextDisplay = new Panel();
            labelTextDisplay = new Label();
            labelCommandName = new LinkLabel();
            labelOperandCount = new Label();
            labelOpcode = new Label();
            panelOperandsDisplay = new Panel();
            labelOperandsDisplay = new Label();
            propertyGridState = new PropertyGrid();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)sequenceDataGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainerVertical).BeginInit();
            splitContainerVertical.Panel1.SuspendLayout();
            splitContainerVertical.Panel2.SuspendLayout();
            splitContainerVertical.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerHorizontal).BeginInit();
            splitContainerHorizontal.Panel1.SuspendLayout();
            splitContainerHorizontal.Panel2.SuspendLayout();
            splitContainerHorizontal.SuspendLayout();
            tableLayoutPanelCommand.SuspendLayout();
            panelTextDisplay.SuspendLayout();
            panelOperandsDisplay.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip
            // 
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Items.AddRange(new ToolStripItem[] { iconToolStripButtonPrevious, iconToolStripButtonNext, toolStripSeparator1, toolStripLabelCounter, toolStripSeparator3, toolStripLabelBits, toolStripSeparator4 });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(784, 25);
            toolStrip.TabIndex = 0;
            toolStrip.Text = "toolStrip";
            // 
            // iconToolStripButtonPrevious
            // 
            iconToolStripButtonPrevious.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iconToolStripButtonPrevious.IconChar = FontAwesome.Sharp.IconChar.ArrowLeft;
            iconToolStripButtonPrevious.IconColor = Color.Black;
            iconToolStripButtonPrevious.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconToolStripButtonPrevious.ImageTransparentColor = Color.Magenta;
            iconToolStripButtonPrevious.Name = "iconToolStripButtonPrevious";
            iconToolStripButtonPrevious.Size = new Size(23, 22);
            iconToolStripButtonPrevious.Text = "Previous";
            // 
            // iconToolStripButtonNext
            // 
            iconToolStripButtonNext.DisplayStyle = ToolStripItemDisplayStyle.Image;
            iconToolStripButtonNext.IconChar = FontAwesome.Sharp.IconChar.ArrowRight;
            iconToolStripButtonNext.IconColor = Color.Black;
            iconToolStripButtonNext.IconFont = FontAwesome.Sharp.IconFont.Auto;
            iconToolStripButtonNext.ImageTransparentColor = Color.Magenta;
            iconToolStripButtonNext.Name = "iconToolStripButtonNext";
            iconToolStripButtonNext.Size = new Size(23, 22);
            iconToolStripButtonNext.Text = "Next";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // toolStripLabelCounter
            // 
            toolStripLabelCounter.Name = "toolStripLabelCounter";
            toolStripLabelCounter.Size = new Size(60, 22);
            toolStripLabelCounter.Text = "0000/0000";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 25);
            // 
            // toolStripLabelBits
            // 
            toolStripLabelBits.Name = "toolStripLabelBits";
            toolStripLabelBits.Size = new Size(32, 22);
            toolStripLabelBits.Text = "7-Bit";
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(6, 25);
            // 
            // sequenceDataGridView
            // 
            sequenceDataGridView.AllowUserToAddRows = false;
            sequenceDataGridView.AllowUserToDeleteRows = false;
            sequenceDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            sequenceDataGridView.Dock = DockStyle.Fill;
            sequenceDataGridView.Location = new Point(0, 0);
            sequenceDataGridView.Name = "sequenceDataGridView";
            sequenceDataGridView.ReadOnly = true;
            sequenceDataGridView.Size = new Size(500, 736);
            sequenceDataGridView.TabIndex = 1;
            // 
            // splitContainerVertical
            // 
            splitContainerVertical.Dock = DockStyle.Fill;
            splitContainerVertical.FixedPanel = FixedPanel.Panel2;
            splitContainerVertical.IsSplitterFixed = true;
            splitContainerVertical.Location = new Point(0, 25);
            splitContainerVertical.Name = "splitContainerVertical";
            // 
            // splitContainerVertical.Panel1
            // 
            splitContainerVertical.Panel1.Controls.Add(sequenceDataGridView);
            splitContainerVertical.Panel1MinSize = 20;
            // 
            // splitContainerVertical.Panel2
            // 
            splitContainerVertical.Panel2.Controls.Add(splitContainerHorizontal);
            splitContainerVertical.Panel2MinSize = 180;
            splitContainerVertical.Size = new Size(784, 736);
            splitContainerVertical.SplitterDistance = 500;
            splitContainerVertical.TabIndex = 2;
            // 
            // splitContainerHorizontal
            // 
            splitContainerHorizontal.BorderStyle = BorderStyle.Fixed3D;
            splitContainerHorizontal.Dock = DockStyle.Fill;
            splitContainerHorizontal.FixedPanel = FixedPanel.Panel1;
            splitContainerHorizontal.IsSplitterFixed = true;
            splitContainerHorizontal.Location = new Point(0, 0);
            splitContainerHorizontal.Name = "splitContainerHorizontal";
            splitContainerHorizontal.Orientation = Orientation.Horizontal;
            // 
            // splitContainerHorizontal.Panel1
            // 
            splitContainerHorizontal.Panel1.Controls.Add(tableLayoutPanelCommand);
            // 
            // splitContainerHorizontal.Panel2
            // 
            splitContainerHorizontal.Panel2.Controls.Add(propertyGridState);
            splitContainerHorizontal.Size = new Size(280, 736);
            splitContainerHorizontal.SplitterDistance = 300;
            splitContainerHorizontal.TabIndex = 1;
            // 
            // tableLayoutPanelCommand
            // 
            tableLayoutPanelCommand.ColumnCount = 2;
            tableLayoutPanelCommand.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 79.3478241F));
            tableLayoutPanelCommand.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20.652174F));
            tableLayoutPanelCommand.Controls.Add(labelBGColorText, 0, 10);
            tableLayoutPanelCommand.Controls.Add(labelFGColorText, 0, 8);
            tableLayoutPanelCommand.Controls.Add(labelBGColor, 1, 10);
            tableLayoutPanelCommand.Controls.Add(labelFGColor, 1, 8);
            tableLayoutPanelCommand.Controls.Add(panelTextDisplay, 0, 3);
            tableLayoutPanelCommand.Controls.Add(labelCommandName, 0, 0);
            tableLayoutPanelCommand.Controls.Add(labelOperandCount, 0, 1);
            tableLayoutPanelCommand.Controls.Add(labelOpcode, 1, 0);
            tableLayoutPanelCommand.Controls.Add(panelOperandsDisplay, 0, 2);
            tableLayoutPanelCommand.Dock = DockStyle.Fill;
            tableLayoutPanelCommand.Location = new Point(0, 0);
            tableLayoutPanelCommand.Name = "tableLayoutPanelCommand";
            tableLayoutPanelCommand.RowCount = 14;
            tableLayoutPanelCommand.RowStyles.Add(new RowStyle());
            tableLayoutPanelCommand.RowStyles.Add(new RowStyle());
            tableLayoutPanelCommand.RowStyles.Add(new RowStyle(SizeType.Absolute, 19F));
            tableLayoutPanelCommand.RowStyles.Add(new RowStyle(SizeType.Absolute, 21F));
            tableLayoutPanelCommand.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanelCommand.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanelCommand.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanelCommand.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanelCommand.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanelCommand.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanelCommand.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanelCommand.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanelCommand.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanelCommand.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanelCommand.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanelCommand.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanelCommand.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanelCommand.Size = new Size(276, 296);
            tableLayoutPanelCommand.TabIndex = 3;
            // 
            // labelBGColorText
            // 
            labelBGColorText.BackColor = Color.Black;
            labelBGColorText.BorderStyle = BorderStyle.Fixed3D;
            labelBGColorText.Dock = DockStyle.Fill;
            labelBGColorText.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelBGColorText.ForeColor = Color.White;
            labelBGColorText.Location = new Point(3, 192);
            labelBGColorText.Name = "labelBGColorText";
            tableLayoutPanelCommand.SetRowSpan(labelBGColorText, 2);
            labelBGColorText.Size = new Size(213, 40);
            labelBGColorText.TabIndex = 9;
            labelBGColorText.Text = "BG";
            labelBGColorText.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelFGColorText
            // 
            labelFGColorText.BackColor = Color.Black;
            labelFGColorText.BorderStyle = BorderStyle.Fixed3D;
            labelFGColorText.Dock = DockStyle.Fill;
            labelFGColorText.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelFGColorText.ForeColor = Color.White;
            labelFGColorText.Location = new Point(3, 152);
            labelFGColorText.Name = "labelFGColorText";
            tableLayoutPanelCommand.SetRowSpan(labelFGColorText, 2);
            labelFGColorText.Size = new Size(213, 40);
            labelFGColorText.TabIndex = 8;
            labelFGColorText.Text = "FG";
            labelFGColorText.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelBGColor
            // 
            labelBGColor.BorderStyle = BorderStyle.Fixed3D;
            labelBGColor.Dock = DockStyle.Fill;
            labelBGColor.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelBGColor.Location = new Point(222, 192);
            labelBGColor.Name = "labelBGColor";
            labelBGColor.RightToLeft = RightToLeft.No;
            tableLayoutPanelCommand.SetRowSpan(labelBGColor, 2);
            labelBGColor.Size = new Size(51, 40);
            labelBGColor.TabIndex = 7;
            labelBGColor.Text = "BG";
            labelBGColor.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelFGColor
            // 
            labelFGColor.BorderStyle = BorderStyle.Fixed3D;
            labelFGColor.Dock = DockStyle.Fill;
            labelFGColor.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelFGColor.Location = new Point(222, 152);
            labelFGColor.Name = "labelFGColor";
            tableLayoutPanelCommand.SetRowSpan(labelFGColor, 2);
            labelFGColor.Size = new Size(51, 40);
            labelFGColor.TabIndex = 6;
            labelFGColor.Text = "FG";
            labelFGColor.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelTextDisplay
            // 
            panelTextDisplay.AutoScroll = true;
            panelTextDisplay.BackColor = Color.Black;
            panelTextDisplay.BorderStyle = BorderStyle.Fixed3D;
            tableLayoutPanelCommand.SetColumnSpan(panelTextDisplay, 2);
            panelTextDisplay.Controls.Add(labelTextDisplay);
            panelTextDisplay.Dock = DockStyle.Fill;
            panelTextDisplay.ForeColor = Color.White;
            panelTextDisplay.Location = new Point(3, 75);
            panelTextDisplay.Name = "panelTextDisplay";
            tableLayoutPanelCommand.SetRowSpan(panelTextDisplay, 4);
            panelTextDisplay.Size = new Size(270, 74);
            panelTextDisplay.TabIndex = 5;
            // 
            // labelTextDisplay
            // 
            labelTextDisplay.AutoSize = true;
            labelTextDisplay.BackColor = Color.Black;
            labelTextDisplay.Dock = DockStyle.Top;
            labelTextDisplay.Font = new Font("Consolas", 10F, FontStyle.Bold);
            labelTextDisplay.ForeColor = Color.White;
            labelTextDisplay.Location = new Point(0, 0);
            labelTextDisplay.MaximumSize = new Size(270, 0);
            labelTextDisplay.Name = "labelTextDisplay";
            labelTextDisplay.Size = new Size(0, 17);
            labelTextDisplay.TabIndex = 5;
            // 
            // labelCommandName
            // 
            labelCommandName.AutoSize = true;
            labelCommandName.Dock = DockStyle.Fill;
            labelCommandName.Font = new Font("Consolas", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelCommandName.Location = new Point(3, 0);
            labelCommandName.Name = "labelCommandName";
            labelCommandName.Size = new Size(213, 17);
            labelCommandName.TabIndex = 1;
            labelCommandName.TabStop = true;
            labelCommandName.Text = "GoesHereCommand";
            labelCommandName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelOperandCount
            // 
            labelOperandCount.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelOperandCount.AutoSize = true;
            labelOperandCount.Location = new Point(3, 17);
            labelOperandCount.Name = "labelOperandCount";
            labelOperandCount.Size = new Size(213, 15);
            labelOperandCount.TabIndex = 2;
            labelOperandCount.Text = "Operand(s): 0";
            labelOperandCount.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelOpcode
            // 
            labelOpcode.BorderStyle = BorderStyle.Fixed3D;
            labelOpcode.Dock = DockStyle.Fill;
            labelOpcode.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelOpcode.Location = new Point(222, 0);
            labelOpcode.Name = "labelOpcode";
            tableLayoutPanelCommand.SetRowSpan(labelOpcode, 2);
            labelOpcode.Size = new Size(51, 32);
            labelOpcode.TabIndex = 3;
            labelOpcode.Text = "0x00";
            labelOpcode.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelOperandsDisplay
            // 
            panelOperandsDisplay.AutoScroll = true;
            panelOperandsDisplay.BackColor = Color.Black;
            panelOperandsDisplay.BorderStyle = BorderStyle.Fixed3D;
            tableLayoutPanelCommand.SetColumnSpan(panelOperandsDisplay, 2);
            panelOperandsDisplay.Controls.Add(labelOperandsDisplay);
            panelOperandsDisplay.Dock = DockStyle.Fill;
            panelOperandsDisplay.ForeColor = Color.White;
            panelOperandsDisplay.Location = new Point(3, 35);
            panelOperandsDisplay.Name = "panelOperandsDisplay";
            tableLayoutPanelCommand.SetRowSpan(panelOperandsDisplay, 2);
            panelOperandsDisplay.Size = new Size(270, 34);
            panelOperandsDisplay.TabIndex = 4;
            // 
            // labelOperandsDisplay
            // 
            labelOperandsDisplay.AutoSize = true;
            labelOperandsDisplay.BackColor = Color.Black;
            labelOperandsDisplay.Dock = DockStyle.Top;
            labelOperandsDisplay.Font = new Font("Consolas", 10F, FontStyle.Bold);
            labelOperandsDisplay.ForeColor = Color.White;
            labelOperandsDisplay.Location = new Point(0, 0);
            labelOperandsDisplay.MaximumSize = new Size(270, 0);
            labelOperandsDisplay.Name = "labelOperandsDisplay";
            labelOperandsDisplay.Size = new Size(0, 17);
            labelOperandsDisplay.TabIndex = 5;
            // 
            // propertyGridState
            // 
            propertyGridState.Dock = DockStyle.Fill;
            propertyGridState.Location = new Point(0, 0);
            propertyGridState.Name = "propertyGridState";
            propertyGridState.Size = new Size(276, 428);
            propertyGridState.TabIndex = 1;
            // 
            // NaplpsSequenceForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 761);
            Controls.Add(splitContainerVertical);
            Controls.Add(toolStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(800, 800);
            Name = "NaplpsSequenceForm";
            Text = "Sequence View";
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)sequenceDataGridView).EndInit();
            splitContainerVertical.Panel1.ResumeLayout(false);
            splitContainerVertical.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerVertical).EndInit();
            splitContainerVertical.ResumeLayout(false);
            splitContainerHorizontal.Panel1.ResumeLayout(false);
            splitContainerHorizontal.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerHorizontal).EndInit();
            splitContainerHorizontal.ResumeLayout(false);
            tableLayoutPanelCommand.ResumeLayout(false);
            tableLayoutPanelCommand.PerformLayout();
            panelTextDisplay.ResumeLayout(false);
            panelTextDisplay.PerformLayout();
            panelOperandsDisplay.ResumeLayout(false);
            panelOperandsDisplay.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButtonPrevious;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButtonNext;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripLabel toolStripLabelCounter;
        private ToolStripSeparator toolStripSeparator3;
        private SplitContainer splitContainerVertical;
        private SplitContainer splitContainerHorizontal;
        private PropertyGrid propertyGridState;
        private TableLayoutPanel tableLayoutPanelCommand;
        private LinkLabel labelCommandName;
        private Label labelOperandCount;
        private Label labelOpcode;
        private Panel panelOperandsDisplay;
        private Label labelOperandsDisplay;
        private Panel panelTextDisplay;
        private Label labelTextDisplay;
        private DataGridView sequenceDataGridView;
        private Label labelFGColor;
        private Label labelBGColor;
        private Label labelFGColorText;
        private Label labelBGColorText;
        private ToolStripLabel toolStripLabelBits;
        private ToolStripSeparator toolStripSeparator4;
    }
}