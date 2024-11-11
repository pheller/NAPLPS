namespace NAPLPSApp.Forms
{
    partial class NewCommandForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewCommandForm));
            comboBoxAddCommands = new ComboBox();
            groupBox1 = new GroupBox();
            button1 = new Button();
            button2 = new Button();
            SuspendLayout();
            // 
            // comboBoxAddCommands
            // 
            comboBoxAddCommands.FormattingEnabled = true;
            comboBoxAddCommands.Location = new Point(12, 12);
            comboBoxAddCommands.Name = "comboBoxAddCommands";
            comboBoxAddCommands.Size = new Size(652, 23);
            comboBoxAddCommands.TabIndex = 0;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Location = new Point(12, 41);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(652, 256);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Command Settings";
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button1.Location = new Point(589, 303);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 2;
            button1.Text = "&Cancel";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button2.Enabled = false;
            button2.Location = new Point(508, 303);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 3;
            button2.Text = "&Add";
            button2.UseVisualStyleBackColor = true;
            // 
            // NewCommandForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(676, 338);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(groupBox1);
            Controls.Add(comboBoxAddCommands);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "NewCommandForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "New Command";
            Load += NewCommandForm_Load;
            ResumeLayout(false);
        }

        #endregion

        private ComboBox comboBoxAddCommands;
        private GroupBox groupBox1;
        private Button button1;
        private Button button2;
    }
}