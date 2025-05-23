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
            BlueprintCheckBox = new CheckBox();
            CardIconFolders = new Button();
            DebugCheckBox = new CheckBox();
            CardPicturePathLoadButton = new Button();
            CardPicture = new PictureBox();
            SavePictureButton = new Button();
            SaveJsonButton = new Button();
            LoadJsonButton = new Button();
            flowLayoutPanel1 = new FlowLayoutPanel();
            groupBox1 = new GroupBox();
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
            BlueprintsPanel = new GroupBox();
            label12 = new Label();
            label11 = new Label();
            label10 = new Label();
            MountainBpTextBox = new TextBox();
            ForestBpTextBox = new TextBox();
            PlainsBpTextBox = new TextBox();
            PlayerPanel = new Panel();
            label14 = new Label();
            ActivePowerUpDown = new NumericUpDown();
            StartingGoldUpdown = new NumericUpDown();
            label13 = new Label();
            panel1 = new Panel();
            triginterEffects1 = new TriginterEffects();
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
            BlueprintsPanel.SuspendLayout();
            PlayerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ActivePowerUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)StartingGoldUpdown).BeginInit();
            panel1.SuspendLayout();
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
            ImageSelectorSplit.Panel1.Controls.Add(BlueprintCheckBox);
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
            ImageSelectorSplit.Size = new Size(1700, 960);
            ImageSelectorSplit.SplitterDistance = 300;
            ImageSelectorSplit.TabIndex = 0;
            // 
            // BlueprintCheckBox
            // 
            BlueprintCheckBox.AutoSize = true;
            BlueprintCheckBox.Location = new Point(94, 47);
            BlueprintCheckBox.Name = "BlueprintCheckBox";
            BlueprintCheckBox.Size = new Size(91, 24);
            BlueprintCheckBox.TabIndex = 6;
            BlueprintCheckBox.Text = "Blueprint";
            BlueprintCheckBox.UseVisualStyleBackColor = true;
            BlueprintCheckBox.CheckedChanged += BlueprintCheckBox_CheckedChanged;
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
            CardPicture.Size = new Size(285, 836);
            CardPicture.SizeMode = PictureBoxSizeMode.Zoom;
            CardPicture.TabIndex = 1;
            CardPicture.TabStop = false;
            // 
            // SavePictureButton
            // 
            SavePictureButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            SavePictureButton.Location = new Point(12, 919);
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
            flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.AutoScrollMinSize = new Size(0, 603);
            flowLayoutPanel1.Controls.Add(groupBox1);
            flowLayoutPanel1.Controls.Add(groupBox2);
            flowLayoutPanel1.Controls.Add(LivingEntityPanel);
            flowLayoutPanel1.Controls.Add(UnitPanel);
            flowLayoutPanel1.Controls.Add(BlueprintsPanel);
            flowLayoutPanel1.Controls.Add(PlayerPanel);
            flowLayoutPanel1.Controls.Add(panel1);
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(3, 47);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(1390, 910);
            flowLayoutPanel1.TabIndex = 2;
            flowLayoutPanel1.WrapContents = false;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(EntityTypeLabel);
            groupBox1.Controls.Add(TargetOptionsDropdown);
            groupBox1.Controls.Add(EntityTypeDropdown);
            groupBox1.Controls.Add(TargetOptionsLabel);
            groupBox1.Location = new Point(3, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1364, 91);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "PlayInfo";
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
            groupBox2.Location = new Point(3, 100);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(1364, 171);
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
            CardIdUpdown.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            CardIdUpdown.Minimum = new decimal(new int[] { 999, 0, 0, int.MinValue });
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
            EffectDescriptionBox.Size = new Size(1244, 81);
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
            LivingEntityPanel.Location = new Point(3, 277);
            LivingEntityPanel.Name = "LivingEntityPanel";
            LivingEntityPanel.Size = new Size(1364, 36);
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
            UnitPanel.Location = new Point(3, 319);
            UnitPanel.Name = "UnitPanel";
            UnitPanel.Size = new Size(1364, 68);
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
            // BlueprintsPanel
            // 
            BlueprintsPanel.Controls.Add(label12);
            BlueprintsPanel.Controls.Add(label11);
            BlueprintsPanel.Controls.Add(label10);
            BlueprintsPanel.Controls.Add(MountainBpTextBox);
            BlueprintsPanel.Controls.Add(ForestBpTextBox);
            BlueprintsPanel.Controls.Add(PlainsBpTextBox);
            BlueprintsPanel.Location = new Point(3, 393);
            BlueprintsPanel.Name = "BlueprintsPanel";
            BlueprintsPanel.Size = new Size(1364, 131);
            BlueprintsPanel.TabIndex = 4;
            BlueprintsPanel.TabStop = false;
            BlueprintsPanel.Text = "Building Blueprints";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(9, 95);
            label12.Name = "label12";
            label12.Size = new Size(52, 20);
            label12.TabIndex = 5;
            label12.Text = "Mount";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(9, 62);
            label11.Name = "label11";
            label11.Size = new Size(49, 20);
            label11.TabIndex = 4;
            label11.Text = "Forest";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(9, 29);
            label10.Name = "label10";
            label10.Size = new Size(47, 20);
            label10.TabIndex = 3;
            label10.Text = "Plains";
            // 
            // MountainBpTextBox
            // 
            MountainBpTextBox.Location = new Point(59, 92);
            MountainBpTextBox.Name = "MountainBpTextBox";
            MountainBpTextBox.Size = new Size(480, 27);
            MountainBpTextBox.TabIndex = 2;
            MountainBpTextBox.TextChanged += MountainBpTextBox_TextChanged;
            // 
            // ForestBpTextBox
            // 
            ForestBpTextBox.Location = new Point(59, 59);
            ForestBpTextBox.Name = "ForestBpTextBox";
            ForestBpTextBox.Size = new Size(480, 27);
            ForestBpTextBox.TabIndex = 1;
            ForestBpTextBox.TextChanged += ForestBpTextBox_TextChanged;
            // 
            // PlainsBpTextBox
            // 
            PlainsBpTextBox.Location = new Point(59, 26);
            PlainsBpTextBox.Name = "PlainsBpTextBox";
            PlainsBpTextBox.Size = new Size(480, 27);
            PlainsBpTextBox.TabIndex = 0;
            PlainsBpTextBox.TextChanged += PlainsBpTextBox_TextChanged;
            // 
            // PlayerPanel
            // 
            PlayerPanel.Controls.Add(label14);
            PlayerPanel.Controls.Add(ActivePowerUpDown);
            PlayerPanel.Controls.Add(StartingGoldUpdown);
            PlayerPanel.Controls.Add(label13);
            PlayerPanel.Location = new Point(3, 530);
            PlayerPanel.Name = "PlayerPanel";
            PlayerPanel.Size = new Size(1364, 69);
            PlayerPanel.TabIndex = 5;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(9, 38);
            label14.Name = "label14";
            label14.Size = new Size(49, 20);
            label14.TabIndex = 3;
            label14.Text = "Power";
            // 
            // ActivePowerUpDown
            // 
            ActivePowerUpDown.Location = new Point(59, 36);
            ActivePowerUpDown.Maximum = new decimal(new int[] { 999, 0, 0, 0 });
            ActivePowerUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            ActivePowerUpDown.Name = "ActivePowerUpDown";
            ActivePowerUpDown.Size = new Size(49, 27);
            ActivePowerUpDown.TabIndex = 2;
            ActivePowerUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            ActivePowerUpDown.ValueChanged += ActivePowerUpDown_ValueChanged;
            // 
            // StartingGoldUpdown
            // 
            StartingGoldUpdown.Location = new Point(59, 3);
            StartingGoldUpdown.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            StartingGoldUpdown.Name = "StartingGoldUpdown";
            StartingGoldUpdown.Size = new Size(49, 27);
            StartingGoldUpdown.TabIndex = 1;
            StartingGoldUpdown.ValueChanged += StartingGoldUpdown_ValueChanged;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(9, 5);
            label13.Name = "label13";
            label13.Size = new Size(48, 20);
            label13.TabIndex = 0;
            label13.Text = "I.Gold\r\n";
            // 
            // panel1
            // 
            panel1.AutoScroll = true;
            panel1.Controls.Add(triginterEffects1);
            panel1.Location = new Point(3, 605);
            panel1.Name = "panel1";
            panel1.Size = new Size(1364, 214);
            panel1.TabIndex = 6;
            // 
            // triginterEffects1
            // 
            triginterEffects1.AutoSize = true;
            triginterEffects1.BorderStyle = BorderStyle.FixedSingle;
            triginterEffects1.Location = new Point(21, 13);
            triginterEffects1.MinimumSize = new Size(190, 80);
            triginterEffects1.Name = "triginterEffects1";
            triginterEffects1.Size = new Size(190, 82);
            triginterEffects1.TabIndex = 0;
            // 
            // CardGenerator
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1700, 960);
            Controls.Add(ImageSelectorSplit);
            MinimumSize = new Size(1110, 700);
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
            BlueprintsPanel.ResumeLayout(false);
            BlueprintsPanel.PerformLayout();
            PlayerPanel.ResumeLayout(false);
            PlayerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)ActivePowerUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)StartingGoldUpdown).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer ImageSelectorSplit;
        private Button SavePictureButton;
        private PictureBox CardPicture;
        private ComboBox EntityTypeDropdown;
        private Label EntityTypeLabel;
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
        private GroupBox BlueprintsPanel;
        private Label label12;
        private Label label11;
        private Label label10;
        private TextBox MountainBpTextBox;
        private TextBox ForestBpTextBox;
        private TextBox PlainsBpTextBox;
        private CheckBox BlueprintCheckBox;
        private Panel PlayerPanel;
        private NumericUpDown StartingGoldUpdown;
        private Label label13;
        private NumericUpDown ActivePowerUpDown;
        private Label label14;
        private Panel panel1;
        private TriginterEffects triginterEffects1;
    }
}
