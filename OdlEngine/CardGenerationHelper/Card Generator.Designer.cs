namespace CardGenerationHelper
{
    partial class CardGenerator
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
            ImageSelectorSplit = new SplitContainer();
            CardIconFolders = new Button();
            DebugCheckBox = new CheckBox();
            CardPicturePathLoadButton = new Button();
            CardPicture = new PictureBox();
            SavePictureButton = new Button();
            SaveJsonButton = new Button();
            LoadJsonButton = new Button();
            flowLayoutPanel1 = new FlowLayoutPanel();
            groupBox1 = new GroupBox();
            TargetConditions = new Label();
            TargetConditionDropdown = new ComboBox();
            EntityTypeLabel = new Label();
            TargetOptionsDropdown = new ComboBox();
            EntityTypeDropdown = new ComboBox();
            TargetOptionsLabel = new Label();
            groupBox2 = new GroupBox();
            Cost = new Label();
            CostUpDown = new NumericUpDown();
            label1 = new Label();
            CardIdUpdown = new NumericUpDown();
            RarityUpDown = new NumericUpDown();
            label2 = new Label();
            label6 = new Label();
            CardNameBox = new TextBox();
            label5 = new Label();
            label3 = new Label();
            EffectDescriptionBox = new TextBox();
            ExpansionDropdown = new ComboBox();
            ClassDropdown = new ComboBox();
            label4 = new Label();
            LivingEntityPanel = new Panel();
            HpUpDown = new NumericUpDown();
            label7 = new Label();
            UnitPanel = new Panel();
            DenominatorUpDown = new NumericUpDown();
            MovementUpdown = new NumericUpDown();
            AttackUpDown = new NumericUpDown();
            label9 = new Label();
            label8 = new Label();
            ((System.ComponentModel.ISupportInitialize)ImageSelectorSplit).BeginInit();
            ImageSelectorSplit.Panel1.SuspendLayout();
            ImageSelectorSplit.Panel2.SuspendLayout();
            ImageSelectorSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)CardPicture).BeginInit();
            flowLayoutPanel1.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)CostUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CardIdUpdown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)RarityUpDown).BeginInit();
            LivingEntityPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)HpUpDown).BeginInit();
            UnitPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)DenominatorUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MovementUpdown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)AttackUpDown).BeginInit();
            SuspendLayout();
            // 
            // ImageSelectorSplit
            // 
            ImageSelectorSplit.Dock = DockStyle.Fill;
            ImageSelectorSplit.FixedPanel = FixedPanel.Panel1;
            ImageSelectorSplit.IsSplitterFixed = true;
            ImageSelectorSplit.Location = new Point(0, 0);
            ImageSelectorSplit.Name = "ImageSelectorSplit";
            // 
            // ImageSelectorSplit.Panel1
            // 
            ImageSelectorSplit.Panel1.Controls.Add(CardIconFolders);
            ImageSelectorSplit.Panel1.Controls.Add(DebugCheckBox);
            ImageSelectorSplit.Panel1.Controls.Add(CardPicturePathLoadButton);
            ImageSelectorSplit.Panel1.Controls.Add(CardPicture);
            ImageSelectorSplit.Panel1.Controls.Add(SavePictureButton);
            // 
            // ImageSelectorSplit.Panel2
            // 
            ImageSelectorSplit.Panel2.Controls.Add(SaveJsonButton);
            ImageSelectorSplit.Panel2.Controls.Add(LoadJsonButton);
            ImageSelectorSplit.Panel2.Controls.Add(flowLayoutPanel1);
            ImageSelectorSplit.Size = new Size(1072, 653);
            ImageSelectorSplit.SplitterDistance = 300;
            ImageSelectorSplit.TabIndex = 0;
            // 
            // CardIconFolders
            // 
            CardIconFolders.Location = new Point(158, 12);
            CardIconFolders.Name = "CardIconFolders";
            CardIconFolders.Size = new Size(139, 29);
            CardIconFolders.TabIndex = 5;
            CardIconFolders.Text = "Layout Folder";
            CardIconFolders.UseVisualStyleBackColor = true;
            CardIconFolders.Click += CardIconFolders_Click;
            // 
            // DebugCheckBox
            // 
            DebugCheckBox.AutoSize = true;
            DebugCheckBox.Location = new Point(12, 47);
            DebugCheckBox.Name = "DebugCheckBox";
            DebugCheckBox.Size = new Size(76, 24);
            DebugCheckBox.TabIndex = 4;
            DebugCheckBox.Text = "Debug";
            DebugCheckBox.UseVisualStyleBackColor = true;
            DebugCheckBox.CheckedChanged += DebugCheckBox_CheckedChanged;
            // 
            // CardPicturePathLoadButton
            // 
            CardPicturePathLoadButton.Location = new Point(12, 12);
            CardPicturePathLoadButton.Name = "CardPicturePathLoadButton";
            CardPicturePathLoadButton.Size = new Size(140, 29);
            CardPicturePathLoadButton.TabIndex = 2;
            CardPicturePathLoadButton.Text = "Picture Folder";
            CardPicturePathLoadButton.UseVisualStyleBackColor = true;
            CardPicturePathLoadButton.Click += CardPicturePathLoadButton_Click;
            // 
            // CardPicture
            // 
            CardPicture.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            CardPicture.Location = new Point(12, 77);
            CardPicture.Name = "CardPicture";
            CardPicture.Size = new Size(285, 529);
            CardPicture.SizeMode = PictureBoxSizeMode.Zoom;
            CardPicture.TabIndex = 1;
            CardPicture.TabStop = false;
            // 
            // SavePictureButton
            // 
            SavePictureButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            SavePictureButton.Location = new Point(12, 612);
            SavePictureButton.Name = "SavePictureButton";
            SavePictureButton.Size = new Size(285, 29);
            SavePictureButton.TabIndex = 0;
            SavePictureButton.Text = "Save Picture";
            SavePictureButton.UseVisualStyleBackColor = true;
            SavePictureButton.Click += SavePictureButton_Click;
            // 
            // SaveJsonButton
            // 
            SaveJsonButton.Location = new Point(103, 12);
            SaveJsonButton.Name = "SaveJsonButton";
            SaveJsonButton.Size = new Size(94, 29);
            SaveJsonButton.TabIndex = 4;
            SaveJsonButton.Text = "Save Card";
            SaveJsonButton.UseVisualStyleBackColor = true;
            // 
            // LoadJsonButton
            // 
            LoadJsonButton.Location = new Point(3, 12);
            LoadJsonButton.Name = "LoadJsonButton";
            LoadJsonButton.Size = new Size(94, 29);
            LoadJsonButton.TabIndex = 3;
            LoadJsonButton.Text = "Load Card";
            LoadJsonButton.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.Controls.Add(groupBox1);
            flowLayoutPanel1.Controls.Add(groupBox2);
            flowLayoutPanel1.Controls.Add(LivingEntityPanel);
            flowLayoutPanel1.Controls.Add(UnitPanel);
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(3, 47);
            flowLayoutPanel1.MaximumSize = new Size(762, 2000);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(762, 603);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(TargetConditions);
            groupBox1.Controls.Add(TargetConditionDropdown);
            groupBox1.Controls.Add(EntityTypeLabel);
            groupBox1.Controls.Add(TargetOptionsDropdown);
            groupBox1.Controls.Add(EntityTypeDropdown);
            groupBox1.Controls.Add(TargetOptionsLabel);
            groupBox1.Location = new Point(3, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(750, 131);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "PlayInfo";
            // 
            // TargetConditions
            // 
            TargetConditions.AutoSize = true;
            TargetConditions.Location = new Point(6, 91);
            TargetConditions.Name = "TargetConditions";
            TargetConditions.Size = new Size(125, 20);
            TargetConditions.TabIndex = 5;
            TargetConditions.Text = "Target Conditions";
            // 
            // TargetConditionDropdown
            // 
            TargetConditionDropdown.FormattingEnabled = true;
            TargetConditionDropdown.Location = new Point(137, 88);
            TargetConditionDropdown.Name = "TargetConditionDropdown";
            TargetConditionDropdown.Size = new Size(154, 28);
            TargetConditionDropdown.TabIndex = 4;
            TargetConditionDropdown.SelectedIndexChanged += TargetConditionDropdown_SelectedIndexChanged;
            // 
            // EntityTypeLabel
            // 
            EntityTypeLabel.AutoSize = true;
            EntityTypeLabel.Location = new Point(6, 23);
            EntityTypeLabel.Name = "EntityTypeLabel";
            EntityTypeLabel.Size = new Size(75, 20);
            EntityTypeLabel.TabIndex = 1;
            EntityTypeLabel.Text = "Card Type";
            // 
            // TargetOptionsDropdown
            // 
            TargetOptionsDropdown.FormattingEnabled = true;
            TargetOptionsDropdown.Location = new Point(137, 54);
            TargetOptionsDropdown.Name = "TargetOptionsDropdown";
            TargetOptionsDropdown.Size = new Size(154, 28);
            TargetOptionsDropdown.TabIndex = 2;
            TargetOptionsDropdown.SelectedIndexChanged += TargetOptionsDropdown_SelectedIndexChanged;
            // 
            // EntityTypeDropdown
            // 
            EntityTypeDropdown.FormattingEnabled = true;
            EntityTypeDropdown.Location = new Point(137, 20);
            EntityTypeDropdown.Name = "EntityTypeDropdown";
            EntityTypeDropdown.Size = new Size(154, 28);
            EntityTypeDropdown.TabIndex = 0;
            EntityTypeDropdown.SelectedIndexChanged += EntityTypeDropdown_SelectedIndexChanged;
            // 
            // TargetOptionsLabel
            // 
            TargetOptionsLabel.AutoSize = true;
            TargetOptionsLabel.Location = new Point(6, 57);
            TargetOptionsLabel.Name = "TargetOptionsLabel";
            TargetOptionsLabel.Size = new Size(106, 20);
            TargetOptionsLabel.TabIndex = 3;
            TargetOptionsLabel.Text = "Target Options";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(Cost);
            groupBox2.Controls.Add(CostUpDown);
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(CardIdUpdown);
            groupBox2.Controls.Add(RarityUpDown);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(CardNameBox);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(EffectDescriptionBox);
            groupBox2.Controls.Add(ExpansionDropdown);
            groupBox2.Controls.Add(ClassDropdown);
            groupBox2.Controls.Add(label4);
            groupBox2.Location = new Point(3, 140);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(753, 171);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "PrintInfo";
            // 
            // Cost
            // 
            Cost.AutoSize = true;
            Cost.Location = new Point(6, 94);
            Cost.Name = "Cost";
            Cost.Size = new Size(38, 20);
            Cost.TabIndex = 12;
            Cost.Text = "Cost";
            // 
            // CostUpDown
            // 
            CostUpDown.Location = new Point(59, 92);
            CostUpDown.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            CostUpDown.Name = "CostUpDown";
            CostUpDown.Size = new Size(49, 27);
            CostUpDown.TabIndex = 13;
            CostUpDown.ValueChanged += CostUpDown_ValueChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 29);
            label1.Name = "label1";
            label1.Size = new Size(24, 20);
            label1.TabIndex = 0;
            label1.Text = "ID";
            // 
            // CardIdUpdown
            // 
            CardIdUpdown.Location = new Point(59, 26);
            CardIdUpdown.Maximum = new decimal(new int[] { -1486618624, 232830643, 0, 0 });
            CardIdUpdown.Name = "CardIdUpdown";
            CardIdUpdown.Size = new Size(49, 27);
            CardIdUpdown.TabIndex = 1;
            CardIdUpdown.ValueChanged += CardIdUpdown_ValueChanged;
            // 
            // RarityUpDown
            // 
            RarityUpDown.Location = new Point(59, 59);
            RarityUpDown.Maximum = new decimal(new int[] { 3, 0, 0, 0 });
            RarityUpDown.Name = "RarityUpDown";
            RarityUpDown.Size = new Size(49, 27);
            RarityUpDown.TabIndex = 11;
            RarityUpDown.ValueChanged += RarityUpDown_ValueChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(114, 28);
            label2.Name = "label2";
            label2.Size = new Size(49, 20);
            label2.TabIndex = 2;
            label2.Text = "Name";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(6, 61);
            label6.Name = "label6";
            label6.Size = new Size(47, 20);
            label6.TabIndex = 10;
            label6.Text = "Rarity";
            // 
            // CardNameBox
            // 
            CardNameBox.Location = new Point(169, 25);
            CardNameBox.Multiline = true;
            CardNameBox.Name = "CardNameBox";
            CardNameBox.Size = new Size(154, 27);
            CardNameBox.TabIndex = 3;
            CardNameBox.TextChanged += CardNameBox_TextChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(114, 61);
            label5.Name = "label5";
            label5.Size = new Size(127, 20);
            label5.TabIndex = 9;
            label5.Text = "Effect Description";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(329, 28);
            label3.Name = "label3";
            label3.Size = new Size(76, 20);
            label3.TabIndex = 4;
            label3.Text = "Expansion";
            // 
            // EffectDescriptionBox
            // 
            EffectDescriptionBox.Location = new Point(114, 84);
            EffectDescriptionBox.Multiline = true;
            EffectDescriptionBox.Name = "EffectDescriptionBox";
            EffectDescriptionBox.Size = new Size(630, 81);
            EffectDescriptionBox.TabIndex = 8;
            EffectDescriptionBox.TextChanged += EffectDescriptionBox_TextChanged;
            // 
            // ExpansionDropdown
            // 
            ExpansionDropdown.FormattingEnabled = true;
            ExpansionDropdown.Location = new Point(411, 26);
            ExpansionDropdown.Name = "ExpansionDropdown";
            ExpansionDropdown.Size = new Size(128, 28);
            ExpansionDropdown.TabIndex = 5;
            ExpansionDropdown.SelectedIndexChanged += ExpansionDropdown_SelectedIndexChanged;
            // 
            // ClassDropdown
            // 
            ClassDropdown.FormattingEnabled = true;
            ClassDropdown.Location = new Point(593, 26);
            ClassDropdown.Name = "ClassDropdown";
            ClassDropdown.Size = new Size(151, 28);
            ClassDropdown.TabIndex = 7;
            ClassDropdown.SelectedIndexChanged += ClassDropdown_SelectedIndexChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(545, 29);
            label4.Name = "label4";
            label4.Size = new Size(42, 20);
            label4.TabIndex = 6;
            label4.Text = "Class";
            // 
            // LivingEntityPanel
            // 
            LivingEntityPanel.Controls.Add(HpUpDown);
            LivingEntityPanel.Controls.Add(label7);
            LivingEntityPanel.Location = new Point(3, 317);
            LivingEntityPanel.Name = "LivingEntityPanel";
            LivingEntityPanel.Size = new Size(753, 36);
            LivingEntityPanel.TabIndex = 2;
            // 
            // HpUpDown
            // 
            HpUpDown.Location = new Point(59, 3);
            HpUpDown.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            HpUpDown.Name = "HpUpDown";
            HpUpDown.Size = new Size(49, 27);
            HpUpDown.TabIndex = 1;
            HpUpDown.ValueChanged += HpUpDown_ValueChanged;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(9, 5);
            label7.Name = "label7";
            label7.Size = new Size(29, 20);
            label7.TabIndex = 0;
            label7.Text = "Hp";
            // 
            // UnitPanel
            // 
            UnitPanel.Controls.Add(DenominatorUpDown);
            UnitPanel.Controls.Add(MovementUpdown);
            UnitPanel.Controls.Add(AttackUpDown);
            UnitPanel.Controls.Add(label9);
            UnitPanel.Controls.Add(label8);
            UnitPanel.Location = new Point(3, 359);
            UnitPanel.Name = "UnitPanel";
            UnitPanel.Size = new Size(753, 68);
            UnitPanel.TabIndex = 3;
            // 
            // DenominatorUpDown
            // 
            DenominatorUpDown.Location = new Point(114, 36);
            DenominatorUpDown.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            DenominatorUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            DenominatorUpDown.Name = "DenominatorUpDown";
            DenominatorUpDown.Size = new Size(49, 27);
            DenominatorUpDown.TabIndex = 5;
            DenominatorUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            DenominatorUpDown.ValueChanged += MovementOrDenominatorUpdown_ValueChanged;
            // 
            // MovementUpdown
            // 
            MovementUpdown.Location = new Point(59, 36);
            MovementUpdown.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            MovementUpdown.Name = "MovementUpdown";
            MovementUpdown.Size = new Size(49, 27);
            MovementUpdown.TabIndex = 4;
            MovementUpdown.ValueChanged += MovementOrDenominatorUpdown_ValueChanged;
            // 
            // AttackUpDown
            // 
            AttackUpDown.Location = new Point(59, 3);
            AttackUpDown.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            AttackUpDown.Name = "AttackUpDown";
            AttackUpDown.Size = new Size(49, 27);
            AttackUpDown.TabIndex = 3;
            AttackUpDown.ValueChanged += AttackUpDown_ValueChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(6, 38);
            label9.Name = "label9";
            label9.Size = new Size(43, 20);
            label9.TabIndex = 1;
            label9.Text = "Movt";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(6, 5);
            label8.Name = "label8";
            label8.Size = new Size(51, 20);
            label8.TabIndex = 0;
            label8.Text = "Attack";
            // 
            // CardGenerator
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1072, 653);
            Controls.Add(ImageSelectorSplit);
            MaximumSize = new Size(1090, 10000000);
            MinimumSize = new Size(1090, 700);
            Name = "CardGenerator";
            Text = "Card Generator";
            Load += CardGenerator_Load;
            ImageSelectorSplit.Panel1.ResumeLayout(false);
            ImageSelectorSplit.Panel1.PerformLayout();
            ImageSelectorSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)ImageSelectorSplit).EndInit();
            ImageSelectorSplit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)CardPicture).EndInit();
            flowLayoutPanel1.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)CostUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)CardIdUpdown).EndInit();
            ((System.ComponentModel.ISupportInitialize)RarityUpDown).EndInit();
            LivingEntityPanel.ResumeLayout(false);
            LivingEntityPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)HpUpDown).EndInit();
            UnitPanel.ResumeLayout(false);
            UnitPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)DenominatorUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)MovementUpdown).EndInit();
            ((System.ComponentModel.ISupportInitialize)AttackUpDown).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer ImageSelectorSplit;
        private Button SavePictureButton;
        private PictureBox CardPicture;
        private ComboBox EntityTypeDropdown;
        private Label EntityTypeLabel;
        private Label TargetConditions;
        private ComboBox TargetConditionDropdown;
        private Label TargetOptionsLabel;
        private ComboBox TargetOptionsDropdown;
        private Label label1;
        private NumericUpDown CardIdUpdown;
        private TextBox CardNameBox;
        private Label label2;
        private ComboBox ExpansionDropdown;
        private Label label3;
        private ComboBox ClassDropdown;
        private Label label4;
        private Button CardPicturePathLoadButton;
        private Label label5;
        private TextBox EffectDescriptionBox;
        private Label label6;
        private NumericUpDown RarityUpDown;
        private CheckBox DebugCheckBox;
        private Button CardIconFolders;
        private NumericUpDown CostUpDown;
        private Label Cost;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button SaveJsonButton;
        private Button LoadJsonButton;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private Panel LivingEntityPanel;
        private Label label7;
        private NumericUpDown HpUpDown;
        private Panel UnitPanel;
        private NumericUpDown AttackUpDown;
        private Label label9;
        private Label label8;
        private NumericUpDown DenominatorUpDown;
        private NumericUpDown MovementUpdown;
    }
}
