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
            BlueprintCheckBox = new CheckBox();
            CardIconFolders = new Button();
            DebugCheckBox = new CheckBox();
            CardPicturePathLoadButton = new Button();
            CardPicture = new PictureBox();
            SavePictureButton = new Button();
            CardElementsPanel = new FlowLayoutPanel();
            PlayInfoBox = new GroupBox();
            EntityTypeLabel = new Label();
            TargetOptionsDropdown = new ComboBox();
            EntityTypeDropdown = new ComboBox();
            TargetOptionsLabel = new Label();
            PrintInfoBox = new GroupBox();
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
            TriggerList = new TrigInterList();
            InteractionList = new TrigInterList();
            PictureCheckboxPanel = new Panel();
            PictureButtonsArea = new Panel();
            PictureAreaPanel = new Panel();
            MainPanel = new Panel();
            LoadSaveButtonPanel = new Panel();
            SaveJsonButton = new Button();
            ((System.ComponentModel.ISupportInitialize)CardPicture).BeginInit();
            CardElementsPanel.SuspendLayout();
            PlayInfoBox.SuspendLayout();
            PrintInfoBox.SuspendLayout();
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
            PictureCheckboxPanel.SuspendLayout();
            PictureButtonsArea.SuspendLayout();
            PictureAreaPanel.SuspendLayout();
            MainPanel.SuspendLayout();
            LoadSaveButtonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // BlueprintCheckBox
            // 
            BlueprintCheckBox.AutoSize = true;
            BlueprintCheckBox.Dock = DockStyle.Left;
            BlueprintCheckBox.Location = new Point(76, 0);
            BlueprintCheckBox.Name = "BlueprintCheckBox";
            BlueprintCheckBox.Size = new Size(91, 30);
            BlueprintCheckBox.TabIndex = 6;
            BlueprintCheckBox.Text = "Blueprint";
            BlueprintCheckBox.UseVisualStyleBackColor = true;
            BlueprintCheckBox.CheckedChanged += BlueprintCheckBox_CheckedChanged;
            // 
            // CardIconFolders
            // 
            CardIconFolders.Dock = DockStyle.Left;
            CardIconFolders.Location = new Point(140, 0);
            CardIconFolders.Name = "CardIconFolders";
            CardIconFolders.Size = new Size(139, 35);
            CardIconFolders.TabIndex = 5;
            CardIconFolders.Text = "Layout Folder";
            CardIconFolders.UseVisualStyleBackColor = true;
            CardIconFolders.Click += CardIconFolders_Click;
            // 
            // DebugCheckBox
            // 
            DebugCheckBox.AutoSize = true;
            DebugCheckBox.Dock = DockStyle.Left;
            DebugCheckBox.Location = new Point(0, 0);
            DebugCheckBox.Name = "DebugCheckBox";
            DebugCheckBox.Size = new Size(76, 30);
            DebugCheckBox.TabIndex = 4;
            DebugCheckBox.Text = "Debug";
            DebugCheckBox.UseVisualStyleBackColor = true;
            DebugCheckBox.CheckedChanged += DebugCheckBox_CheckedChanged;
            // 
            // CardPicturePathLoadButton
            // 
            CardPicturePathLoadButton.Dock = DockStyle.Left;
            CardPicturePathLoadButton.Location = new Point(0, 0);
            CardPicturePathLoadButton.Name = "CardPicturePathLoadButton";
            CardPicturePathLoadButton.Size = new Size(140, 35);
            CardPicturePathLoadButton.TabIndex = 2;
            CardPicturePathLoadButton.Text = "Picture Folder";
            CardPicturePathLoadButton.UseVisualStyleBackColor = true;
            CardPicturePathLoadButton.Click += CardPicturePathLoadButton_Click;
            // 
            // CardPicture
            // 
            CardPicture.Dock = DockStyle.Fill;
            CardPicture.Location = new Point(0, 65);
            CardPicture.Name = "CardPicture";
            CardPicture.Size = new Size(283, 414);
            CardPicture.SizeMode = PictureBoxSizeMode.Zoom;
            CardPicture.TabIndex = 1;
            CardPicture.TabStop = false;
            // 
            // SavePictureButton
            // 
            SavePictureButton.Dock = DockStyle.Bottom;
            SavePictureButton.Location = new Point(0, 479);
            SavePictureButton.Name = "SavePictureButton";
            SavePictureButton.Size = new Size(283, 29);
            SavePictureButton.TabIndex = 0;
            SavePictureButton.Text = "Save Picture";
            SavePictureButton.UseVisualStyleBackColor = true;
            SavePictureButton.Click += SavePictureButton_Click;
            // 
            // CardElementsPanel
            // 
            CardElementsPanel.AutoSize = true;
            CardElementsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            CardElementsPanel.Controls.Add(PlayInfoBox);
            CardElementsPanel.Controls.Add(PrintInfoBox);
            CardElementsPanel.Controls.Add(LivingEntityPanel);
            CardElementsPanel.Controls.Add(UnitPanel);
            CardElementsPanel.Controls.Add(BlueprintsPanel);
            CardElementsPanel.Controls.Add(PlayerPanel);
            CardElementsPanel.Controls.Add(TriggerList);
            CardElementsPanel.Controls.Add(InteractionList);
            CardElementsPanel.FlowDirection = FlowDirection.TopDown;
            CardElementsPanel.Location = new Point(6, 52);
            CardElementsPanel.Name = "CardElementsPanel";
            CardElementsPanel.Size = new Size(756, 787);
            CardElementsPanel.TabIndex = 2;
            CardElementsPanel.WrapContents = false;
            // 
            // PlayInfoBox
            // 
            PlayInfoBox.AutoSize = true;
            PlayInfoBox.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            PlayInfoBox.Controls.Add(EntityTypeLabel);
            PlayInfoBox.Controls.Add(TargetOptionsDropdown);
            PlayInfoBox.Controls.Add(EntityTypeDropdown);
            PlayInfoBox.Controls.Add(TargetOptionsLabel);
            PlayInfoBox.Location = new Point(3, 3);
            PlayInfoBox.Name = "PlayInfoBox";
            PlayInfoBox.Size = new Size(297, 108);
            PlayInfoBox.TabIndex = 0;
            PlayInfoBox.TabStop = false;
            PlayInfoBox.Text = "PlayInfo";
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
            // PrintInfoBox
            // 
            PrintInfoBox.AutoSize = true;
            PrintInfoBox.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            PrintInfoBox.Controls.Add(Cost);
            PrintInfoBox.Controls.Add(CostUpDown);
            PrintInfoBox.Controls.Add(label1);
            PrintInfoBox.Controls.Add(CardIdUpdown);
            PrintInfoBox.Controls.Add(RarityUpDown);
            PrintInfoBox.Controls.Add(label2);
            PrintInfoBox.Controls.Add(label6);
            PrintInfoBox.Controls.Add(CardNameBox);
            PrintInfoBox.Controls.Add(label5);
            PrintInfoBox.Controls.Add(label3);
            PrintInfoBox.Controls.Add(EffectDescriptionBox);
            PrintInfoBox.Controls.Add(ExpansionDropdown);
            PrintInfoBox.Controls.Add(ClassDropdown);
            PrintInfoBox.Controls.Add(label4);
            PrintInfoBox.Location = new Point(3, 117);
            PrintInfoBox.Name = "PrintInfoBox";
            PrintInfoBox.Size = new Size(750, 191);
            PrintInfoBox.TabIndex = 1;
            PrintInfoBox.TabStop = false;
            PrintInfoBox.Text = "PrintInfo";
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
            LivingEntityPanel.AutoSize = true;
            LivingEntityPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            LivingEntityPanel.Controls.Add(HpUpDown);
            LivingEntityPanel.Controls.Add(label7);
            LivingEntityPanel.Location = new Point(3, 314);
            LivingEntityPanel.Name = "LivingEntityPanel";
            LivingEntityPanel.Size = new Size(111, 33);
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
            UnitPanel.AutoSize = true;
            UnitPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            UnitPanel.Controls.Add(DenominatorUpDown);
            UnitPanel.Controls.Add(MovementUpdown);
            UnitPanel.Controls.Add(AttackUpDown);
            UnitPanel.Controls.Add(label9);
            UnitPanel.Controls.Add(label8);
            UnitPanel.Location = new Point(3, 353);
            UnitPanel.Name = "UnitPanel";
            UnitPanel.Size = new Size(166, 66);
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
            BlueprintsPanel.AutoSize = true;
            BlueprintsPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BlueprintsPanel.Controls.Add(label12);
            BlueprintsPanel.Controls.Add(label11);
            BlueprintsPanel.Controls.Add(label10);
            BlueprintsPanel.Controls.Add(MountainBpTextBox);
            BlueprintsPanel.Controls.Add(ForestBpTextBox);
            BlueprintsPanel.Controls.Add(PlainsBpTextBox);
            BlueprintsPanel.Location = new Point(3, 425);
            BlueprintsPanel.Name = "BlueprintsPanel";
            BlueprintsPanel.Size = new Size(545, 145);
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
            PlayerPanel.AutoSize = true;
            PlayerPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            PlayerPanel.Controls.Add(label14);
            PlayerPanel.Controls.Add(ActivePowerUpDown);
            PlayerPanel.Controls.Add(StartingGoldUpdown);
            PlayerPanel.Controls.Add(label13);
            PlayerPanel.Location = new Point(3, 576);
            PlayerPanel.Name = "PlayerPanel";
            PlayerPanel.Size = new Size(111, 66);
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
            // TriggerList
            // 
            TriggerList.AutoSize = true;
            TriggerList.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            TriggerList.BorderStyle = BorderStyle.FixedSingle;
            TriggerList.Location = new Point(3, 648);
            TriggerList.Name = "TriggerList";
            TriggerList.Size = new Size(67, 65);
            TriggerList.TabIndex = 7;
            // 
            // InteractionList
            // 
            InteractionList.AutoSize = true;
            InteractionList.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            InteractionList.BorderStyle = BorderStyle.FixedSingle;
            InteractionList.Location = new Point(3, 719);
            InteractionList.Name = "InteractionList";
            InteractionList.Size = new Size(67, 65);
            InteractionList.TabIndex = 6;
            // 
            // PictureCheckboxPanel
            // 
            PictureCheckboxPanel.Controls.Add(BlueprintCheckBox);
            PictureCheckboxPanel.Controls.Add(DebugCheckBox);
            PictureCheckboxPanel.Dock = DockStyle.Top;
            PictureCheckboxPanel.Location = new Point(0, 35);
            PictureCheckboxPanel.Name = "PictureCheckboxPanel";
            PictureCheckboxPanel.Size = new Size(283, 30);
            PictureCheckboxPanel.TabIndex = 1;
            // 
            // PictureButtonsArea
            // 
            PictureButtonsArea.Controls.Add(CardIconFolders);
            PictureButtonsArea.Controls.Add(CardPicturePathLoadButton);
            PictureButtonsArea.Dock = DockStyle.Top;
            PictureButtonsArea.Location = new Point(0, 0);
            PictureButtonsArea.Name = "PictureButtonsArea";
            PictureButtonsArea.Size = new Size(283, 35);
            PictureButtonsArea.TabIndex = 0;
            // 
            // PictureAreaPanel
            // 
            PictureAreaPanel.Controls.Add(CardPicture);
            PictureAreaPanel.Controls.Add(SavePictureButton);
            PictureAreaPanel.Controls.Add(PictureCheckboxPanel);
            PictureAreaPanel.Controls.Add(PictureButtonsArea);
            PictureAreaPanel.Dock = DockStyle.Left;
            PictureAreaPanel.Location = new Point(0, 0);
            PictureAreaPanel.MinimumSize = new Size(283, 0);
            PictureAreaPanel.Name = "PictureAreaPanel";
            PictureAreaPanel.Size = new Size(283, 508);
            PictureAreaPanel.TabIndex = 5;
            // 
            // MainPanel
            // 
            MainPanel.AutoScroll = true;
            MainPanel.Controls.Add(CardElementsPanel);
            MainPanel.Controls.Add(LoadSaveButtonPanel);
            MainPanel.Dock = DockStyle.Fill;
            MainPanel.Location = new Point(283, 0);
            MainPanel.Name = "MainPanel";
            MainPanel.Size = new Size(779, 508);
            MainPanel.TabIndex = 6;
            // 
            // LoadSaveButtonPanel
            // 
            LoadSaveButtonPanel.Controls.Add(SaveJsonButton);
            LoadSaveButtonPanel.Dock = DockStyle.Top;
            LoadSaveButtonPanel.Location = new Point(0, 0);
            LoadSaveButtonPanel.Name = "LoadSaveButtonPanel";
            LoadSaveButtonPanel.Size = new Size(762, 46);
            LoadSaveButtonPanel.TabIndex = 8;
            // 
            // SaveJsonButton
            // 
            SaveJsonButton.Dock = DockStyle.Left;
            SaveJsonButton.Location = new Point(0, 0);
            SaveJsonButton.Name = "SaveJsonButton";
            SaveJsonButton.Size = new Size(94, 46);
            SaveJsonButton.TabIndex = 0;
            SaveJsonButton.Text = "Save Card";
            SaveJsonButton.UseVisualStyleBackColor = true;
            SaveJsonButton.Click += SaveJsonButton_Click;
            // 
            // CardGenerator
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1062, 508);
            Controls.Add(MainPanel);
            Controls.Add(PictureAreaPanel);
            MinimumSize = new Size(1080, 555);
            Name = "CardGenerator";
            Text = "Card Generator";
            Load += CardGenerator_Load;
            ((System.ComponentModel.ISupportInitialize)CardPicture).EndInit();
            CardElementsPanel.ResumeLayout(false);
            CardElementsPanel.PerformLayout();
            PlayInfoBox.ResumeLayout(false);
            PlayInfoBox.PerformLayout();
            PrintInfoBox.ResumeLayout(false);
            PrintInfoBox.PerformLayout();
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
            PictureCheckboxPanel.ResumeLayout(false);
            PictureCheckboxPanel.PerformLayout();
            PictureButtonsArea.ResumeLayout(false);
            PictureAreaPanel.ResumeLayout(false);
            MainPanel.ResumeLayout(false);
            MainPanel.PerformLayout();
            LoadSaveButtonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
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
        private FlowLayoutPanel CardElementsPanel;
        private GroupBox PlayInfoBox;
        private GroupBox PrintInfoBox;
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
        private Panel LivingEntityPanel;
        private NumericUpDown HpUpDown;
        private Label label7;
        private Panel PictureButtonsArea;
        private Panel PictureCheckboxPanel;
        private Panel PictureAreaPanel;
        private Panel MainPanel;
        private TrigInterList InteractionList;
        private TrigInterList TriggerList;
        private Panel LoadSaveButtonPanel;
        private Button SaveJsonButton;
    }
}
