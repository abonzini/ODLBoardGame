namespace CardGenerationHelper
{
    partial class FlagEnumCheckbox
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
            CheckboxesPanel = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // CheckboxesPanel
            // 
            CheckboxesPanel.AutoSize = true;
            CheckboxesPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            CheckboxesPanel.Location = new Point(3, 3);
            CheckboxesPanel.Name = "CheckboxesPanel";
            CheckboxesPanel.Size = new Size(0, 0);
            CheckboxesPanel.TabIndex = 0;
            CheckboxesPanel.WrapContents = false;
            // 
            // FlagEnumCheckbox
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(CheckboxesPanel);
            Name = "FlagEnumCheckbox";
            Size = new Size(6, 6);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private FlowLayoutPanel CheckboxesPanel;
    }
}
