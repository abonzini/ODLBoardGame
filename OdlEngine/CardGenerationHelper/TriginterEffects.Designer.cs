namespace CardGenerationHelper
{
    partial class TriginterEffects
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            EventTypeComboBox = new ComboBox();
            DeleteButton = new Button();
            EffectsPanel = new FlowLayoutPanel();
            AddButton = new Button();
            panel1 = new Panel();
            TriggerLocationComboBox = new ComboBox();
            EffectsPanel.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // EventTypeComboBox
            // 
            EventTypeComboBox.FormattingEnabled = true;
            EventTypeComboBox.Location = new Point(3, 3);
            EventTypeComboBox.Name = "EventTypeComboBox";
            EventTypeComboBox.Size = new Size(120, 28);
            EventTypeComboBox.TabIndex = 0;
            // 
            // DeleteButton
            // 
            DeleteButton.BackColor = Color.Red;
            DeleteButton.Dock = DockStyle.Right;
            DeleteButton.Font = new Font("Segoe UI", 9F);
            DeleteButton.ForeColor = SystemColors.ButtonHighlight;
            DeleteButton.Location = new Point(254, 0);
            DeleteButton.Name = "DeleteButton";
            DeleteButton.Size = new Size(30, 34);
            DeleteButton.TabIndex = 5;
            DeleteButton.Text = "X";
            DeleteButton.UseVisualStyleBackColor = false;
            DeleteButton.Click += DeleteButton_Click;
            // 
            // EffectsPanel
            // 
            EffectsPanel.AutoSize = true;
            EffectsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            EffectsPanel.Controls.Add(AddButton);
            EffectsPanel.Location = new Point(3, 40);
            EffectsPanel.Name = "EffectsPanel";
            EffectsPanel.Size = new Size(37, 35);
            EffectsPanel.TabIndex = 6;
            EffectsPanel.WrapContents = false;
            // 
            // AddButton
            // 
            AddButton.Location = new Point(3, 3);
            AddButton.Name = "AddButton";
            AddButton.Size = new Size(31, 29);
            AddButton.TabIndex = 0;
            AddButton.Text = "+";
            AddButton.UseVisualStyleBackColor = true;
            AddButton.Click += AddButton_Click;
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel1.Controls.Add(TriggerLocationComboBox);
            panel1.Controls.Add(DeleteButton);
            panel1.Controls.Add(EventTypeComboBox);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(284, 34);
            panel1.TabIndex = 7;
            // 
            // TriggerLocationComboBox
            // 
            TriggerLocationComboBox.FormattingEnabled = true;
            TriggerLocationComboBox.Location = new Point(129, 3);
            TriggerLocationComboBox.Name = "TriggerLocationComboBox";
            TriggerLocationComboBox.Size = new Size(120, 28);
            TriggerLocationComboBox.TabIndex = 8;
            // 
            // TriginterEffects
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            BackColor = SystemColors.ControlDark;
            BorderStyle = BorderStyle.Fixed3D;
            Controls.Add(panel1);
            Controls.Add(EffectsPanel);
            MinimumSize = new Size(284, 81);
            Name = "TriginterEffects";
            Size = new Size(284, 81);
            EffectsPanel.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox EventTypeComboBox;
        private Button DeleteButton;
        private FlowLayoutPanel EffectsPanel;
        private Button AddButton;
        private Panel panel1;
        private ComboBox TriggerLocationComboBox;
    }
}
