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
            CardPicturePathLoadButton = new Button();
            CardPicture = new PictureBox();
            SavePictureButton = new Button();
            PrintInfo = new GroupBox();
            RarityUpDown = new NumericUpDown();
            label6 = new Label();
            label5 = new Label();
            EffectDescriptionBox = new TextBox();
            ClassDropdown = new ComboBox();
            label4 = new Label();
            ExpansionDropdown = new ComboBox();
            label3 = new Label();
            CardNameBox = new TextBox();
            label2 = new Label();
            CardIdUpdown = new NumericUpDown();
            label1 = new Label();
            PlayInfo = new GroupBox();
            TargetConditions = new Label();
            TargetConditionDropdown = new ComboBox();
            TargetOptionsLabel = new Label();
            TargetOptionsDropdown = new ComboBox();
            EntityTypeLabel = new Label();
            EntityTypeDropdown = new ComboBox();
            DebugCheckBox = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)ImageSelectorSplit).BeginInit();
            ImageSelectorSplit.Panel1.SuspendLayout();
            ImageSelectorSplit.Panel2.SuspendLayout();
            ImageSelectorSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)CardPicture).BeginInit();
            PrintInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)RarityUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CardIdUpdown).BeginInit();
            PlayInfo.SuspendLayout();
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
            ImageSelectorSplit.Panel1.Controls.Add(DebugCheckBox);
            ImageSelectorSplit.Panel1.Controls.Add(CardPicturePathLoadButton);
            ImageSelectorSplit.Panel1.Controls.Add(CardPicture);
            ImageSelectorSplit.Panel1.Controls.Add(SavePictureButton);
            // 
            // ImageSelectorSplit.Panel2
            // 
            ImageSelectorSplit.Panel2.Controls.Add(PrintInfo);
            ImageSelectorSplit.Panel2.Controls.Add(PlayInfo);
            ImageSelectorSplit.Size = new Size(1072, 653);
            ImageSelectorSplit.SplitterDistance = 300;
            ImageSelectorSplit.TabIndex = 0;
            // 
            // CardPicturePathLoadButton
            // 
            CardPicturePathLoadButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            CardPicturePathLoadButton.Location = new Point(12, 12);
            CardPicturePathLoadButton.Name = "CardPicturePathLoadButton";
            CardPicturePathLoadButton.Size = new Size(285, 29);
            CardPicturePathLoadButton.TabIndex = 2;
            CardPicturePathLoadButton.Text = "Card Image Folder";
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
            // 
            // PrintInfo
            // 
            PrintInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PrintInfo.Controls.Add(RarityUpDown);
            PrintInfo.Controls.Add(label6);
            PrintInfo.Controls.Add(label5);
            PrintInfo.Controls.Add(EffectDescriptionBox);
            PrintInfo.Controls.Add(ClassDropdown);
            PrintInfo.Controls.Add(label4);
            PrintInfo.Controls.Add(ExpansionDropdown);
            PrintInfo.Controls.Add(label3);
            PrintInfo.Controls.Add(CardNameBox);
            PrintInfo.Controls.Add(label2);
            PrintInfo.Controls.Add(CardIdUpdown);
            PrintInfo.Controls.Add(label1);
            PrintInfo.Location = new Point(3, 143);
            PrintInfo.Name = "PrintInfo";
            PrintInfo.Size = new Size(753, 161);
            PrintInfo.TabIndex = 1;
            PrintInfo.TabStop = false;
            PrintInfo.Text = "PrintInfo";
            // 
            // RarityUpDown
            // 
            RarityUpDown.Location = new Point(59, 79);
            RarityUpDown.Maximum = new decimal(new int[] { 3, 0, 0, 0 });
            RarityUpDown.Name = "RarityUpDown";
            RarityUpDown.Size = new Size(33, 27);
            RarityUpDown.TabIndex = 11;
            RarityUpDown.ValueChanged += RarityUpDown_ValueChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(6, 81);
            label6.Name = "label6";
            label6.Size = new Size(47, 20);
            label6.TabIndex = 10;
            label6.Text = "Rarity";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(4, 54);
            label5.Name = "label5";
            label5.Size = new Size(127, 20);
            label5.TabIndex = 9;
            label5.Text = "Effect Description";
            // 
            // EffectDescriptionBox
            // 
            EffectDescriptionBox.Location = new Point(137, 54);
            EffectDescriptionBox.Multiline = true;
            EffectDescriptionBox.Name = "EffectDescriptionBox";
            EffectDescriptionBox.Size = new Size(598, 96);
            EffectDescriptionBox.TabIndex = 8;
            EffectDescriptionBox.TextChanged += EffectDescriptionBox_TextChanged;
            // 
            // ClassDropdown
            // 
            ClassDropdown.FormattingEnabled = true;
            ClassDropdown.Location = new Point(584, 21);
            ClassDropdown.Name = "ClassDropdown";
            ClassDropdown.Size = new Size(151, 28);
            ClassDropdown.TabIndex = 7;
            ClassDropdown.SelectedIndexChanged += ClassDropdown_SelectedIndexChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(536, 24);
            label4.Name = "label4";
            label4.Size = new Size(42, 20);
            label4.TabIndex = 6;
            label4.Text = "Class";
            // 
            // ExpansionDropdown
            // 
            ExpansionDropdown.FormattingEnabled = true;
            ExpansionDropdown.Location = new Point(379, 21);
            ExpansionDropdown.Name = "ExpansionDropdown";
            ExpansionDropdown.Size = new Size(151, 28);
            ExpansionDropdown.TabIndex = 5;
            ExpansionDropdown.SelectedIndexChanged += ExpansionDropdown_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(297, 23);
            label3.Name = "label3";
            label3.Size = new Size(76, 20);
            label3.TabIndex = 4;
            label3.Text = "Expansion";
            // 
            // CardNameBox
            // 
            CardNameBox.Location = new Point(137, 21);
            CardNameBox.Multiline = true;
            CardNameBox.Name = "CardNameBox";
            CardNameBox.Size = new Size(154, 27);
            CardNameBox.TabIndex = 3;
            CardNameBox.TextChanged += CardNameBox_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(87, 24);
            label2.Name = "label2";
            label2.Size = new Size(49, 20);
            label2.TabIndex = 2;
            label2.Text = "Name";
            // 
            // CardIdUpdown
            // 
            CardIdUpdown.Location = new Point(32, 21);
            CardIdUpdown.Maximum = new decimal(new int[] { -1486618624, 232830643, 0, 0 });
            CardIdUpdown.Name = "CardIdUpdown";
            CardIdUpdown.Size = new Size(49, 27);
            CardIdUpdown.TabIndex = 1;
            CardIdUpdown.ValueChanged += CardIdUpdown_ValueChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 23);
            label1.Name = "label1";
            label1.Size = new Size(24, 20);
            label1.TabIndex = 0;
            label1.Text = "ID";
            // 
            // PlayInfo
            // 
            PlayInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PlayInfo.Controls.Add(TargetConditions);
            PlayInfo.Controls.Add(TargetConditionDropdown);
            PlayInfo.Controls.Add(TargetOptionsLabel);
            PlayInfo.Controls.Add(TargetOptionsDropdown);
            PlayInfo.Controls.Add(EntityTypeLabel);
            PlayInfo.Controls.Add(EntityTypeDropdown);
            PlayInfo.Location = new Point(3, 12);
            PlayInfo.Name = "PlayInfo";
            PlayInfo.Size = new Size(753, 125);
            PlayInfo.TabIndex = 0;
            PlayInfo.TabStop = false;
            PlayInfo.Text = "PlayInfo";
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
            // TargetOptionsLabel
            // 
            TargetOptionsLabel.AutoSize = true;
            TargetOptionsLabel.Location = new Point(6, 57);
            TargetOptionsLabel.Name = "TargetOptionsLabel";
            TargetOptionsLabel.Size = new Size(106, 20);
            TargetOptionsLabel.TabIndex = 3;
            TargetOptionsLabel.Text = "Target Options";
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
            // EntityTypeLabel
            // 
            EntityTypeLabel.AutoSize = true;
            EntityTypeLabel.Location = new Point(6, 23);
            EntityTypeLabel.Name = "EntityTypeLabel";
            EntityTypeLabel.Size = new Size(75, 20);
            EntityTypeLabel.TabIndex = 1;
            EntityTypeLabel.Text = "Card Type";
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
            PrintInfo.ResumeLayout(false);
            PrintInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)RarityUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)CardIdUpdown).EndInit();
            PlayInfo.ResumeLayout(false);
            PlayInfo.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer ImageSelectorSplit;
        private Button SavePictureButton;
        private PictureBox CardPicture;
        private GroupBox PlayInfo;
        private GroupBox PrintInfo;
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
    }
}
