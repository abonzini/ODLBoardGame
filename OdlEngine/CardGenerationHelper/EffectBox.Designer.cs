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
            ParamsPanel = new TableLayoutPanel();
            EffectLocationLabel = new Label();
            EffectLocationBox = new ComboBox();
            TargetPlayerLabel = new Label();
            SearchCriterionLabel = new Label();
            TargetTypeLabel = new Label();
            MultiVariableBox = new ComboBox();
            ModifierOperationLabel = new Label();
            InputLabel = new Label();
            OutputLabel = new Label();
            MultiVariableLabel = new Label();
            SearchCriterionBox = new ComboBox();
            ModifierOperationBox = new ComboBox();
            InputBox = new ComboBox();
            OutputBox = new ComboBox();
            TargetPlayerEnumBox = new FlagEnumCheckbox();
            TargetTypeEnumBox = new FlagEnumCheckbox();
            DeleteButton = new Button();
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
            // ParamsPanel
            // 
            ParamsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ParamsPanel.AutoSize = true;
            ParamsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ParamsPanel.ColumnCount = 2;
            ParamsPanel.ColumnStyles.Add(new ColumnStyle());
            ParamsPanel.ColumnStyles.Add(new ColumnStyle());
            ParamsPanel.Controls.Add(EffectLocationLabel, 0, 0);
            ParamsPanel.Controls.Add(EffectLocationBox, 1, 0);
            ParamsPanel.Controls.Add(TargetPlayerLabel, 0, 1);
            ParamsPanel.Controls.Add(SearchCriterionLabel, 0, 2);
            ParamsPanel.Controls.Add(TargetTypeLabel, 0, 3);
            ParamsPanel.Controls.Add(MultiVariableBox, 1, 7);
            ParamsPanel.Controls.Add(ModifierOperationLabel, 0, 4);
            ParamsPanel.Controls.Add(InputLabel, 0, 5);
            ParamsPanel.Controls.Add(OutputLabel, 0, 6);
            ParamsPanel.Controls.Add(MultiVariableLabel, 0, 7);
            ParamsPanel.Controls.Add(SearchCriterionBox, 1, 2);
            ParamsPanel.Controls.Add(ModifierOperationBox, 1, 4);
            ParamsPanel.Controls.Add(InputBox, 1, 5);
            ParamsPanel.Controls.Add(OutputBox, 1, 6);
            ParamsPanel.Controls.Add(TargetPlayerEnumBox, 1, 1);
            ParamsPanel.Controls.Add(TargetTypeEnumBox, 1, 3);
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
            ParamsPanel.Size = new Size(300, 244);
            ParamsPanel.TabIndex = 5;
            // 
            // EffectLocationLabel
            // 
            EffectLocationLabel.AutoSize = true;
            EffectLocationLabel.Font = new Font("Segoe UI", 9F);
            EffectLocationLabel.Location = new Point(3, 0);
            EffectLocationLabel.Name = "EffectLocationLabel";
            EffectLocationLabel.Size = new Size(108, 20);
            EffectLocationLabel.TabIndex = 0;
            EffectLocationLabel.Text = "Effect Location";
            // 
            // EffectLocationBox
            // 
            EffectLocationBox.Font = new Font("Segoe UI", 9F);
            EffectLocationBox.FormattingEnabled = true;
            EffectLocationBox.Location = new Point(146, 3);
            EffectLocationBox.Name = "EffectLocationBox";
            EffectLocationBox.Size = new Size(151, 28);
            EffectLocationBox.TabIndex = 1;
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
            SearchCriterionLabel.Location = new Point(3, 54);
            SearchCriterionLabel.Name = "SearchCriterionLabel";
            SearchCriterionLabel.Size = new Size(114, 20);
            SearchCriterionLabel.TabIndex = 3;
            SearchCriterionLabel.Text = "Search Criterion";
            // 
            // TargetTypeLabel
            // 
            TargetTypeLabel.AutoSize = true;
            TargetTypeLabel.Font = new Font("Segoe UI", 9F);
            TargetTypeLabel.Location = new Point(3, 88);
            TargetTypeLabel.Name = "TargetTypeLabel";
            TargetTypeLabel.Size = new Size(85, 20);
            TargetTypeLabel.TabIndex = 4;
            TargetTypeLabel.Text = "Target Type";
            // 
            // MultiVariableBox
            // 
            MultiVariableBox.Font = new Font("Segoe UI", 9F);
            MultiVariableBox.FormattingEnabled = true;
            MultiVariableBox.Location = new Point(146, 213);
            MultiVariableBox.Name = "MultiVariableBox";
            MultiVariableBox.Size = new Size(151, 28);
            MultiVariableBox.TabIndex = 15;
            // 
            // ModifierOperationLabel
            // 
            ModifierOperationLabel.AutoSize = true;
            ModifierOperationLabel.Font = new Font("Segoe UI", 9F);
            ModifierOperationLabel.Location = new Point(3, 108);
            ModifierOperationLabel.Name = "ModifierOperationLabel";
            ModifierOperationLabel.Size = new Size(137, 20);
            ModifierOperationLabel.TabIndex = 5;
            ModifierOperationLabel.Text = "Modifier Operation";
            // 
            // InputLabel
            // 
            InputLabel.AutoSize = true;
            InputLabel.Font = new Font("Segoe UI", 9F);
            InputLabel.Location = new Point(3, 142);
            InputLabel.Name = "InputLabel";
            InputLabel.Size = new Size(43, 20);
            InputLabel.TabIndex = 6;
            InputLabel.Text = "Input";
            // 
            // OutputLabel
            // 
            OutputLabel.AutoSize = true;
            OutputLabel.Font = new Font("Segoe UI", 9F);
            OutputLabel.Location = new Point(3, 176);
            OutputLabel.Name = "OutputLabel";
            OutputLabel.Size = new Size(55, 20);
            OutputLabel.TabIndex = 7;
            OutputLabel.Text = "Output";
            // 
            // MultiVariableLabel
            // 
            MultiVariableLabel.AutoSize = true;
            MultiVariableLabel.Font = new Font("Segoe UI", 9F);
            MultiVariableLabel.Location = new Point(3, 210);
            MultiVariableLabel.Name = "MultiVariableLabel";
            MultiVariableLabel.Size = new Size(128, 20);
            MultiVariableLabel.TabIndex = 8;
            MultiVariableLabel.Text = "If Multiple Inputs?";
            // 
            // SearchCriterionBox
            // 
            SearchCriterionBox.Font = new Font("Segoe UI", 9F);
            SearchCriterionBox.FormattingEnabled = true;
            SearchCriterionBox.Location = new Point(146, 57);
            SearchCriterionBox.Name = "SearchCriterionBox";
            SearchCriterionBox.Size = new Size(151, 28);
            SearchCriterionBox.TabIndex = 10;
            // 
            // ModifierOperationBox
            // 
            ModifierOperationBox.Font = new Font("Segoe UI", 9F);
            ModifierOperationBox.FormattingEnabled = true;
            ModifierOperationBox.Location = new Point(146, 111);
            ModifierOperationBox.Name = "ModifierOperationBox";
            ModifierOperationBox.Size = new Size(151, 28);
            ModifierOperationBox.TabIndex = 12;
            // 
            // InputBox
            // 
            InputBox.Font = new Font("Segoe UI", 9F);
            InputBox.FormattingEnabled = true;
            InputBox.Location = new Point(146, 145);
            InputBox.Name = "InputBox";
            InputBox.Size = new Size(151, 28);
            InputBox.TabIndex = 13;
            // 
            // OutputBox
            // 
            OutputBox.Font = new Font("Segoe UI", 9F);
            OutputBox.FormattingEnabled = true;
            OutputBox.Location = new Point(146, 179);
            OutputBox.Name = "OutputBox";
            OutputBox.Size = new Size(151, 28);
            OutputBox.TabIndex = 14;
            // 
            // TargetPlayerEnumBox
            // 
            TargetPlayerEnumBox.AutoSize = true;
            TargetPlayerEnumBox.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            TargetPlayerEnumBox.Location = new Point(146, 37);
            TargetPlayerEnumBox.Name = "TargetPlayerEnumBox";
            TargetPlayerEnumBox.Size = new Size(6, 6);
            TargetPlayerEnumBox.TabIndex = 16;
            // 
            // TargetTypeEnumBox
            // 
            TargetTypeEnumBox.AutoSize = true;
            TargetTypeEnumBox.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            TargetTypeEnumBox.Location = new Point(146, 91);
            TargetTypeEnumBox.Name = "TargetTypeEnumBox";
            TargetTypeEnumBox.Size = new Size(6, 6);
            TargetTypeEnumBox.TabIndex = 17;
            // 
            // DeleteButton
            // 
            DeleteButton.BackColor = Color.Red;
            DeleteButton.Font = new Font("Segoe UI", 9F);
            DeleteButton.ForeColor = SystemColors.ButtonHighlight;
            DeleteButton.Location = new Point(275, 3);
            DeleteButton.Name = "DeleteButton";
            DeleteButton.Size = new Size(30, 30);
            DeleteButton.TabIndex = 4;
            DeleteButton.Text = "X";
            DeleteButton.UseVisualStyleBackColor = false;
            DeleteButton.Click += DeleteButton_Click;
            // 
            // EffectBox
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = SystemColors.Control;
            BorderStyle = BorderStyle.Fixed3D;
            Controls.Add(ParamsPanel);
            Controls.Add(DeleteButton);
            Controls.Add(ValueUpDown);
            Controls.Add(TempValueUpDown);
            Controls.Add(EffectTypeComboBox);
            Name = "EffectBox";
            Size = new Size(308, 285);
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
        private TableLayoutPanel ParamsPanel;
        private Label EffectLocationLabel;
        private ComboBox EffectLocationBox;
        private Label TargetPlayerLabel;
        private Label SearchCriterionLabel;
        private Label TargetTypeLabel;
        private Label ModifierOperationLabel;
        private Label InputLabel;
        private Label OutputLabel;
        private Label MultiVariableLabel;
        private ComboBox SearchCriterionBox;
        private ComboBox ModifierOperationBox;
        private ComboBox InputBox;
        private ComboBox OutputBox;
        private ComboBox MultiVariableBox;
        private Button DeleteButton;
        private FlagEnumCheckbox TargetPlayerEnumBox;
        private FlagEnumCheckbox TargetTypeEnumBox;
    }
}
