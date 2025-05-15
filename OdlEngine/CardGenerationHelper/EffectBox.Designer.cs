namespace CardGenerationHelper
{
    partial class EffectBox
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
            EffectTypeComboBox = new ComboBox();
            TempValueUpDown = new Label();
            ValueUpDown = new NumericUpDown();
            button1 = new Button();
            ParamsPanel = new TableLayoutPanel();
            TargetLocationLabel = new Label();
            TargetLocationBox = new ComboBox();
            TargetPlayerLabel = new Label();
            SearchCriterionLabel = new Label();
            TargetTypeLabel = new Label();
            ModifierOperationLabel = new Label();
            ModifierTargetLabel = new Label();
            InputRegisterLabel = new Label();
            OutputRegisterLabel = new Label();
            TargetPlayerBox = new ComboBox();
            SearchCriterionBox = new ComboBox();
            TargetTypeBox = new ComboBox();
            ModifierOperationBox = new ComboBox();
            ModifierTargetBox = new ComboBox();
            InputRegisterBox = new ComboBox();
            OutputRegisterBox = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)ValueUpDown).BeginInit();
            ParamsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // EffectTypeComboBox
            // 
            EffectTypeComboBox.Font = new Font("Segoe UI", 9F);
            EffectTypeComboBox.FormattingEnabled = true;
            EffectTypeComboBox.Location = new Point(3, 3);
            EffectTypeComboBox.Name = "EffectTypeComboBox";
            EffectTypeComboBox.Size = new Size(144, 28);
            EffectTypeComboBox.TabIndex = 0;
            EffectTypeComboBox.SelectedIndexChanged += EffectTypeComboBox_SelectedIndexChanged;
            // 
            // TempValueUpDown
            // 
            TempValueUpDown.AutoSize = true;
            TempValueUpDown.Font = new Font("Segoe UI", 9F);
            TempValueUpDown.Location = new Point(149, 8);
            TempValueUpDown.Name = "TempValueUpDown";
            TempValueUpDown.Size = new Size(45, 20);
            TempValueUpDown.TabIndex = 1;
            TempValueUpDown.Text = "Value";
            // 
            // ValueUpDown
            // 
            ValueUpDown.Font = new Font("Segoe UI", 9F);
            ValueUpDown.Location = new Point(200, 6);
            ValueUpDown.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            ValueUpDown.Minimum = new decimal(new int[] { 999, 0, 0, int.MinValue });
            ValueUpDown.Name = "ValueUpDown";
            ValueUpDown.Size = new Size(69, 27);
            ValueUpDown.TabIndex = 2;
            // 
            // button1
            // 
            button1.BackColor = Color.Red;
            button1.Font = new Font("Segoe UI", 9F);
            button1.ForeColor = SystemColors.ButtonHighlight;
            button1.Location = new Point(275, 3);
            button1.Name = "button1";
            button1.Size = new Size(30, 30);
            button1.TabIndex = 4;
            button1.Text = "X";
            button1.UseVisualStyleBackColor = false;
            // 
            // ParamsPanel
            // 
            ParamsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ParamsPanel.AutoSize = true;
            ParamsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ParamsPanel.ColumnCount = 2;
            ParamsPanel.ColumnStyles.Add(new ColumnStyle());
            ParamsPanel.ColumnStyles.Add(new ColumnStyle());
            ParamsPanel.Controls.Add(TargetLocationLabel, 0, 0);
            ParamsPanel.Controls.Add(TargetLocationBox, 1, 0);
            ParamsPanel.Controls.Add(TargetPlayerLabel, 0, 1);
            ParamsPanel.Controls.Add(SearchCriterionLabel, 0, 2);
            ParamsPanel.Controls.Add(TargetTypeLabel, 0, 3);
            ParamsPanel.Controls.Add(OutputRegisterBox, 1, 7);
            ParamsPanel.Controls.Add(ModifierOperationLabel, 0, 4);
            ParamsPanel.Controls.Add(ModifierTargetLabel, 0, 5);
            ParamsPanel.Controls.Add(InputRegisterLabel, 0, 6);
            ParamsPanel.Controls.Add(OutputRegisterLabel, 0, 7);
            ParamsPanel.Controls.Add(TargetPlayerBox, 1, 1);
            ParamsPanel.Controls.Add(SearchCriterionBox, 1, 2);
            ParamsPanel.Controls.Add(TargetTypeBox, 1, 3);
            ParamsPanel.Controls.Add(ModifierOperationBox, 1, 4);
            ParamsPanel.Controls.Add(ModifierTargetBox, 1, 5);
            ParamsPanel.Controls.Add(InputRegisterBox, 1, 6);
            ParamsPanel.Location = new Point(3, 37);
            ParamsPanel.Name = "ParamsPanel";
            ParamsPanel.RowCount = 8;
            ParamsPanel.RowStyles.Add(new RowStyle());
            ParamsPanel.RowStyles.Add(new RowStyle());
            ParamsPanel.RowStyles.Add(new RowStyle());
            ParamsPanel.RowStyles.Add(new RowStyle());
            ParamsPanel.RowStyles.Add(new RowStyle());
            ParamsPanel.RowStyles.Add(new RowStyle());
            ParamsPanel.RowStyles.Add(new RowStyle());
            ParamsPanel.RowStyles.Add(new RowStyle());
            ParamsPanel.Size = new Size(300, 272);
            ParamsPanel.TabIndex = 5;
            // 
            // TargetLocationLabel
            // 
            TargetLocationLabel.AutoSize = true;
            TargetLocationLabel.Font = new Font("Segoe UI", 9F);
            TargetLocationLabel.Location = new Point(3, 0);
            TargetLocationLabel.Name = "TargetLocationLabel";
            TargetLocationLabel.Size = new Size(111, 20);
            TargetLocationLabel.TabIndex = 0;
            TargetLocationLabel.Text = "Target Location";
            // 
            // TargetLocationBox
            // 
            TargetLocationBox.Font = new Font("Segoe UI", 9F);
            TargetLocationBox.FormattingEnabled = true;
            TargetLocationBox.Location = new Point(146, 3);
            TargetLocationBox.Name = "TargetLocationBox";
            TargetLocationBox.Size = new Size(151, 28);
            TargetLocationBox.TabIndex = 1;
            // 
            // TargetPlayerLabel
            // 
            TargetPlayerLabel.AutoSize = true;
            TargetPlayerLabel.Font = new Font("Segoe UI", 9F);
            TargetPlayerLabel.Location = new Point(3, 34);
            TargetPlayerLabel.Name = "TargetPlayerLabel";
            TargetPlayerLabel.Size = new Size(94, 20);
            TargetPlayerLabel.TabIndex = 2;
            TargetPlayerLabel.Text = "Target Player";
            // 
            // SearchCriterionLabel
            // 
            SearchCriterionLabel.AutoSize = true;
            SearchCriterionLabel.Font = new Font("Segoe UI", 9F);
            SearchCriterionLabel.Location = new Point(3, 68);
            SearchCriterionLabel.Name = "SearchCriterionLabel";
            SearchCriterionLabel.Size = new Size(114, 20);
            SearchCriterionLabel.TabIndex = 3;
            SearchCriterionLabel.Text = "Search Criterion";
            // 
            // TargetTypeLabel
            // 
            TargetTypeLabel.AutoSize = true;
            TargetTypeLabel.Font = new Font("Segoe UI", 9F);
            TargetTypeLabel.Location = new Point(3, 102);
            TargetTypeLabel.Name = "TargetTypeLabel";
            TargetTypeLabel.Size = new Size(85, 20);
            TargetTypeLabel.TabIndex = 4;
            TargetTypeLabel.Text = "Target Type";
            // 
            // ModifierOperationLabel
            // 
            ModifierOperationLabel.AutoSize = true;
            ModifierOperationLabel.Font = new Font("Segoe UI", 9F);
            ModifierOperationLabel.Location = new Point(3, 136);
            ModifierOperationLabel.Name = "ModifierOperationLabel";
            ModifierOperationLabel.Size = new Size(137, 20);
            ModifierOperationLabel.TabIndex = 5;
            ModifierOperationLabel.Text = "Modifier Operation";
            // 
            // ModifierTargetLabel
            // 
            ModifierTargetLabel.AutoSize = true;
            ModifierTargetLabel.Font = new Font("Segoe UI", 9F);
            ModifierTargetLabel.Location = new Point(3, 170);
            ModifierTargetLabel.Name = "ModifierTargetLabel";
            ModifierTargetLabel.Size = new Size(111, 20);
            ModifierTargetLabel.TabIndex = 6;
            ModifierTargetLabel.Text = "Modifier Target";
            // 
            // InputRegisterLabel
            // 
            InputRegisterLabel.AutoSize = true;
            InputRegisterLabel.Font = new Font("Segoe UI", 9F);
            InputRegisterLabel.Location = new Point(3, 204);
            InputRegisterLabel.Name = "InputRegisterLabel";
            InputRegisterLabel.Size = new Size(101, 20);
            InputRegisterLabel.TabIndex = 7;
            InputRegisterLabel.Text = "Input Register";
            // 
            // OutputRegisterLabel
            // 
            OutputRegisterLabel.AutoSize = true;
            OutputRegisterLabel.Font = new Font("Segoe UI", 9F);
            OutputRegisterLabel.Location = new Point(3, 238);
            OutputRegisterLabel.Name = "OutputRegisterLabel";
            OutputRegisterLabel.Size = new Size(113, 20);
            OutputRegisterLabel.TabIndex = 8;
            OutputRegisterLabel.Text = "Output Register";
            // 
            // TargetPlayerBox
            // 
            TargetPlayerBox.Font = new Font("Segoe UI", 9F);
            TargetPlayerBox.FormattingEnabled = true;
            TargetPlayerBox.Location = new Point(146, 37);
            TargetPlayerBox.Name = "TargetPlayerBox";
            TargetPlayerBox.Size = new Size(151, 28);
            TargetPlayerBox.TabIndex = 9;
            // 
            // SearchCriterionBox
            // 
            SearchCriterionBox.Font = new Font("Segoe UI", 9F);
            SearchCriterionBox.FormattingEnabled = true;
            SearchCriterionBox.Location = new Point(146, 71);
            SearchCriterionBox.Name = "SearchCriterionBox";
            SearchCriterionBox.Size = new Size(151, 28);
            SearchCriterionBox.TabIndex = 10;
            // 
            // TargetTypeBox
            // 
            TargetTypeBox.Font = new Font("Segoe UI", 9F);
            TargetTypeBox.FormattingEnabled = true;
            TargetTypeBox.Location = new Point(146, 105);
            TargetTypeBox.Name = "TargetTypeBox";
            TargetTypeBox.Size = new Size(151, 28);
            TargetTypeBox.TabIndex = 11;
            // 
            // ModifierOperationBox
            // 
            ModifierOperationBox.Font = new Font("Segoe UI", 9F);
            ModifierOperationBox.FormattingEnabled = true;
            ModifierOperationBox.Location = new Point(146, 139);
            ModifierOperationBox.Name = "ModifierOperationBox";
            ModifierOperationBox.Size = new Size(151, 28);
            ModifierOperationBox.TabIndex = 12;
            // 
            // ModifierTargetBox
            // 
            ModifierTargetBox.Font = new Font("Segoe UI", 9F);
            ModifierTargetBox.FormattingEnabled = true;
            ModifierTargetBox.Location = new Point(146, 173);
            ModifierTargetBox.Name = "ModifierTargetBox";
            ModifierTargetBox.Size = new Size(151, 28);
            ModifierTargetBox.TabIndex = 13;
            // 
            // InputRegisterBox
            // 
            InputRegisterBox.Font = new Font("Segoe UI", 9F);
            InputRegisterBox.FormattingEnabled = true;
            InputRegisterBox.Location = new Point(146, 207);
            InputRegisterBox.Name = "InputRegisterBox";
            InputRegisterBox.Size = new Size(151, 28);
            InputRegisterBox.TabIndex = 14;
            // 
            // OutputRegisterBox
            // 
            OutputRegisterBox.Font = new Font("Segoe UI", 9F);
            OutputRegisterBox.FormattingEnabled = true;
            OutputRegisterBox.Location = new Point(146, 241);
            OutputRegisterBox.Name = "OutputRegisterBox";
            OutputRegisterBox.Size = new Size(151, 28);
            OutputRegisterBox.TabIndex = 15;
            // 
            // EffectBox
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Controls.Add(ParamsPanel);
            Controls.Add(button1);
            Controls.Add(ValueUpDown);
            Controls.Add(TempValueUpDown);
            Controls.Add(EffectTypeComboBox);
            Name = "EffectBox";
            Size = new Size(308, 313);
            Load += EffectBox_Load;
            ((System.ComponentModel.ISupportInitialize)ValueUpDown).EndInit();
            ParamsPanel.ResumeLayout(false);
            ParamsPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox EffectTypeComboBox;
        private Label TempValueUpDown;
        private NumericUpDown ValueUpDown;
        private Button button1;
        private TableLayoutPanel ParamsPanel;
        private Label TargetLocationLabel;
        private ComboBox TargetLocationBox;
        private Label TargetPlayerLabel;
        private Label SearchCriterionLabel;
        private Label TargetTypeLabel;
        private Label ModifierOperationLabel;
        private Label ModifierTargetLabel;
        private Label InputRegisterLabel;
        private Label OutputRegisterLabel;
        private ComboBox TargetPlayerBox;
        private ComboBox SearchCriterionBox;
        private ComboBox TargetTypeBox;
        private ComboBox ModifierOperationBox;
        private ComboBox ModifierTargetBox;
        private ComboBox InputRegisterBox;
        private ComboBox OutputRegisterBox;
    }
}
