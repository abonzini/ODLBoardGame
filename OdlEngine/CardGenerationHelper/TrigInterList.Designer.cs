namespace CardGenerationHelper
{
    partial class TrigInterList
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
            TrigInterLabel = new Label();
            TriginterEffectsPanel = new FlowLayoutPanel();
            AddButton = new Button();
            TriginterEffectsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // TrigInterLabel
            // 
            TrigInterLabel.AutoSize = true;
            TrigInterLabel.Location = new Point(0, 0);
            TrigInterLabel.Name = "TrigInterLabel";
            TrigInterLabel.Size = new Size(50, 20);
            TrigInterLabel.TabIndex = 0;
            TrigInterLabel.Text = "label1";
            // 
            // TriginterEffectsPanel
            // 
            TriginterEffectsPanel.AutoSize = true;
            TriginterEffectsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            TriginterEffectsPanel.Controls.Add(AddButton);
            TriginterEffectsPanel.FlowDirection = FlowDirection.TopDown;
            TriginterEffectsPanel.Location = new Point(3, 23);
            TriginterEffectsPanel.Name = "TriginterEffectsPanel";
            TriginterEffectsPanel.Size = new Size(37, 37);
            TriginterEffectsPanel.TabIndex = 1;
            // 
            // AddButton
            // 
            AddButton.Location = new Point(3, 3);
            AddButton.Name = "AddButton";
            AddButton.Size = new Size(31, 31);
            AddButton.TabIndex = 0;
            AddButton.Text = "+";
            AddButton.UseVisualStyleBackColor = true;
            AddButton.Click += AddButton_Click;
            // 
            // TrigInterList
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(TriginterEffectsPanel);
            Controls.Add(TrigInterLabel);
            Name = "TrigInterList";
            Size = new Size(53, 63);
            TriginterEffectsPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label TrigInterLabel;
        private FlowLayoutPanel TriginterEffectsPanel;
        private Button AddButton;
    }
}
