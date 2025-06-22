using Newtonsoft.Json;
using ODLGameEngine;

namespace CardGenerationHelper
{
    public partial class CardGenerator : Form
    {
        EntityBase _currentEntity = new EntityBase();
        CardIllustrationInfo _currentIllustrationInfo = new CardIllustrationInfo();

        string _resourcesPath = Properties.Settings.Default.ResourcesPath;
        bool _debug = Properties.Settings.Default.Debug;

        readonly System.Windows.Forms.Timer _drawUpdateTimer = new System.Windows.Forms.Timer();
        private void DrawTimeout(object sender, EventArgs e)
        {
            _drawUpdateTimer.Stop();
            DrawIllustration();
        }
        private void DrawIllustration()
        {
            Bitmap bitmap;
            if (BlueprintCheckBox.Checked)
            {
                bitmap = DrawHelper.DrawBlueprint(_currentIllustrationInfo, (Building)_currentEntity, _resourcesPath, _debug);
            }
            else
            {
                bitmap = DrawHelper.DrawCard(_currentIllustrationInfo, _resourcesPath, _debug);
            }
            CardPicture.Image = bitmap;
            if (_debug)
            {
                bitmap.Save("debug.png");
            }
        }
        private void RefreshDrawTimer()
        {
            _drawUpdateTimer.Stop();
            _drawUpdateTimer.Start();
        }
        public CardGenerator()
        {
            InitializeComponent();

            // Set trigger/inter properly!
            TriggerList.SetTrigInterType(TrigOrInter.TRIGGER);
            InteractionList.SetTrigInterType(TrigOrInter.INTERACTION);

            TargetLocationGrid.Updated += TargetLocationGrid_Updated;

            // Load last setting
            DebugCheckBox.Checked = _debug;

            // Timer
            _drawUpdateTimer.Tick += DrawTimeout;
            _drawUpdateTimer.Interval = 150; // 150ms
            _drawUpdateTimer.Stop();

            DrawIllustration(); // Draw empty card
        }
        private void CardGenerator_Load(object sender, EventArgs e)
        {
            EntityTypeDropdown.Items.AddRange(Enum.GetValues(typeof(EntityType)).Cast<object>().ToArray());
            EntityTypeDropdown.SelectedIndex = 0;
            TargetTypeComboBox.Items.AddRange(Enum.GetValues(typeof(CardTargetingType)).Cast<object>().ToArray());
            ExpansionDropdown.Items.AddRange(Enum.GetValues(typeof(ExpansionId)).Cast<object>().ToArray());
            ExpansionDropdown.SelectedIndex = 0;
            ClassDropdown.Items.AddRange(Enum.GetValues(typeof(PlayerTribe)).Cast<object>().ToArray());
            ClassDropdown.SelectedIndex = 0;
            SkillOnwerFilterFlagCheckbox.Clear();
            SkillOnwerFilterFlagCheckbox.SetEnum(typeof(EntityOwner));
            RedrawUi();
        }
        bool entityTypeAlreadyLoaded = false;
        private void TargetLocationGrid_Updated(object sender, EventArgs e)
        {
            _currentEntity.TargetOptions = TargetLocationGrid.GetLocations();
            RefreshDrawTimer();
        }
        private void EntityTypeDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (entityTypeAlreadyLoaded)
            {
                entityTypeAlreadyLoaded = false;
                return;
            }
            _currentIllustrationInfo = new CardIllustrationInfo
            {
                EntityType = (EntityType)EntityTypeDropdown.SelectedItem
            };
            _currentEntity = _currentIllustrationInfo.EntityType switch
            {
                EntityType.NONE => new EntityBase(),
                EntityType.PLAYER => new Player(),
                EntityType.SKILL => new Skill(),
                EntityType.UNIT => new Unit(),
                EntityType.BUILDING => new Building(),
                _ => throw new NotImplementedException("Incorrect entity type selected")
            };
            _currentEntity.EntityType = _currentIllustrationInfo.EntityType;
            UpdateFields(false);
            RedrawUi();
            RefreshDrawTimer();
        }
        void RedrawUi()
        {
            if (typeof(LivingEntity).IsAssignableFrom(_currentEntity.GetType()))
            {
                LivingEntityPanel.Show();
                TriggerList.Show();
            }
            else
            {
                LivingEntityPanel.Hide();
                TriggerList.Hide();
            }
            if (typeof(Unit).IsAssignableFrom(_currentEntity.GetType()))
            {
                UnitPanel.Show();
            }
            else
            {
                UnitPanel.Hide();
            }
            if (typeof(Building).IsAssignableFrom(_currentEntity.GetType()))
            {
                BlueprintCheckBox.Show();
            }
            else
            {
                BlueprintCheckBox.Checked = false;
                BlueprintCheckBox.Hide();
            }
            if (typeof(Player).IsAssignableFrom(_currentEntity.GetType()))
            {
                PlayerPanel.Show();
            }
            else
            {
                PlayerPanel.Hide();
            }
            if (typeof(Skill).IsAssignableFrom(_currentEntity.GetType()))
            {
                TargetTypeComboBox.SelectedIndex = 0;
                SkillOnwerFilterFlagCheckbox.Clear();
                SkillInfoPanel.Show();
                TargetLocationGrid.SetMode((CardTargetingType)TargetTypeComboBox.SelectedItem);
            }
            else
            {
                SkillInfoPanel.Hide();
                TargetLocationGrid.SetMode(CardTargetingType.TILE); // So it shows all
            }
        }

        private void CardIdUpdown_ValueChanged(object sender, EventArgs e)
        {
            _currentEntity.Id = Convert.ToInt32(CardIdUpdown.Value);
            _currentIllustrationInfo.Id = Convert.ToInt32(CardIdUpdown.Value);
            RefreshDrawTimer();
        }

        private void CardNameBox_TextChanged(object sender, EventArgs e)
        {
            _currentIllustrationInfo.Name = CardNameBox.Text.ToUpper();
            if (typeof(LivingEntity).IsAssignableFrom(_currentEntity.GetType())) // Living entities also have Name
            {
                ((LivingEntity)_currentEntity).Name = CardNameBox.Text.ToUpper();
            }
            RefreshDrawTimer();
        }

        private void ExpansionDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentIllustrationInfo.Expansion = (ExpansionId)ExpansionDropdown.SelectedItem;
            RefreshDrawTimer();
        }

        private void ClassDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentIllustrationInfo.Tribe = (PlayerTribe)ClassDropdown.SelectedItem;
            RefreshDrawTimer();
        }

        private void CardPicturePathLoadButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            DialogResult result = folderDialog.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
            {
                _resourcesPath = folderDialog.SelectedPath;
                Properties.Settings.Default.ResourcesPath = _resourcesPath;
                Properties.Settings.Default.Save();
            }
        }

        private void EffectDescriptionBox_TextChanged(object sender, EventArgs e)
        {
            _currentIllustrationInfo.Text = EffectDescriptionBox.Text;
            RefreshDrawTimer();
        }

        private void RarityUpDown_ValueChanged(object sender, EventArgs e)
        {
            _currentIllustrationInfo.Rarity = Convert.ToInt32(RarityUpDown.Value);
            RefreshDrawTimer();
        }

        private void DebugCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _debug = DebugCheckBox.Checked;
            Properties.Settings.Default.Debug = _debug;
            Properties.Settings.Default.Save();
            RefreshDrawTimer();
        }

        private void CostUpDown_ValueChanged(object sender, EventArgs e)
        {
            _currentIllustrationInfo.Cost = CostUpDown.Value.ToString();
            _currentEntity.Cost = Convert.ToInt32(CostUpDown.Value);
            RefreshDrawTimer();
        }

        private void SavePictureButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp",
                Title = "Save card Image",
                DefaultExt = "png" // Default file type
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                // Example: Assuming you have a PictureBox named pictureBox1
                CardPicture.Image.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private void HpUpDown_ValueChanged(object sender, EventArgs e)
        {
            _currentIllustrationInfo.Hp = HpUpDown.Value.ToString();
            ((LivingEntity)_currentEntity).Hp.BaseValue = Convert.ToInt32(HpUpDown.Value);
            RefreshDrawTimer();
        }

        private void AttackUpDown_ValueChanged(object sender, EventArgs e)
        {
            _currentIllustrationInfo.Attack = AttackUpDown.Value.ToString();
            ((Unit)_currentEntity).Attack.BaseValue = Convert.ToInt32(AttackUpDown.Value);
            RefreshDrawTimer();
        }
        private void MovementUpdown_ValueChanged(object sender, EventArgs e)
        {
            ((Unit)_currentEntity).Movement.BaseValue = Convert.ToInt32(MovementUpdown.Value);
            string MovString = MovementUpdown.Value.ToString();
            if (DenominatorUpDown.Value != 1)
            {
                MovString += "/" + DenominatorUpDown.Value.ToString();
            }
            _currentIllustrationInfo.Movement = MovString;
            RefreshDrawTimer();
        }
        private void DenominatorUpdown_ValueChanged(object sender, EventArgs e)
        {
            ((Unit)_currentEntity).MovementDenominator.BaseValue = Convert.ToInt32(DenominatorUpDown.Value);
            string MovString = MovementUpdown.Value.ToString();
            if (DenominatorUpDown.Value != 1)
            {
                MovString += "/" + DenominatorUpDown.Value.ToString();
            }
            _currentIllustrationInfo.Movement = MovString;
            RefreshDrawTimer();
        }
        private void BlueprintCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            RefreshDrawTimer(); // Will need to redraw anyway
        }

        private void StartingGoldUpdown_ValueChanged(object sender, EventArgs e)
        {
            ((Player)_currentEntity).CurrentGold = Convert.ToInt32(StartingGoldUpdown.Value);
        }

        private void ActivePowerUpDown_ValueChanged(object sender, EventArgs e)
        {
            ((Player)_currentEntity).ActivePowerId = Convert.ToInt32(ActivePowerUpDown.Value);
        }

        private void SaveJsonButton_Click(object sender, EventArgs e)
        {
            // Current entity has everything already except triginers
            _currentEntity.Interactions = InteractionList.GetInteractionsDict();
            // Living entities also have Triggers
            if (typeof(LivingEntity).IsAssignableFrom(_currentEntity.GetType()))
            {
                ((LivingEntity)_currentEntity).Triggers = TriggerList.GetTriggersDict();
            }
            // Also this because it didn't auto update in card
            if (typeof(Skill).IsAssignableFrom(_currentEntity.GetType()))
            {
                ((Skill)_currentEntity).TargetOwner = (EntityOwner)SkillOnwerFilterFlagCheckbox.GetEnumValue();
            }
            // Card complete, now time to deserialize
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented, // Add indentation for better readability
                DefaultValueHandling = DefaultValueHandling.Ignore // Exclude default values
            };
            // Serialize result
            string cardJson = JsonConvert.SerializeObject(_currentEntity, settings);
            string cardPrintJson = JsonConvert.SerializeObject(_currentIllustrationInfo, settings);
            // Get folder where I save this
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            DialogResult result = folderDialog.ShowDialog();
            string folderPath = folderDialog.SelectedPath;
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
            {
                File.WriteAllText(Path.Combine(folderPath, _currentIllustrationInfo.Id + ".json"), cardJson);
                File.WriteAllText(Path.Combine(folderPath, _currentIllustrationInfo.Id + "-illustration.json"), cardPrintJson);
            }
        }
        private void LoadJsonButton_Click(object sender, EventArgs e)
        {
            string illustrationFile = "";
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                DialogResult result = fileDialog.ShowDialog();
                string file = fileDialog.FileName;
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(file))
                {
                    if (Path.GetExtension(file) != ".json") return; // Ensure only json openable
                    // Json file found. Now, will try to deserialize
                    if (file.Contains("-illustration.json")) // Illustration file
                    {
                        illustrationFile = file;
                    }
                    else // Then I assume its the card backend json 
                    {
                        illustrationFile = file.Replace(".json", "-illustration.json"); // Get the other one
                    }
                }
            }
            // Got illust file now, need to check if exists
            if (!File.Exists(illustrationFile)) { return; } // Finish here if non existing
            // Otherwise I can load illustration data!
            _currentIllustrationInfo = JsonConvert.DeserializeObject<CardIllustrationInfo>(File.ReadAllText(illustrationFile));
            // Obtain card ID and folder
            int cardId = _currentIllustrationInfo.Id;
            // Use card finder technology to fetch the card data from the directory
            CardFinder cardFinder = new CardFinder(Path.GetDirectoryName(illustrationFile));
            _currentEntity = cardFinder.GetCard(cardId);
            // Got all I needed
            RedrawUi(); // Redraws all fields that are needed
            UpdateFields(true); // Update all the field values
            DrawIllustration(); // Finally show the card
        }
        void UpdateFields(bool cardLoaded)
        {
            entityTypeAlreadyLoaded = true; // Notify the system not to reset the info

            EntityTypeDropdown.SelectedItem = _currentEntity.EntityType; // Todo -1 player doesnt have correct entity type
            CardIdUpdown.Value = _currentEntity.Id;
            RarityUpDown.Value = _currentIllustrationInfo.Rarity;
            CostUpDown.Value = _currentEntity.Cost;
            TargetLocationGrid.SetLocations(_currentEntity.TargetOptions);
            CardNameBox.Text = _currentIllustrationInfo.Name;
            EffectDescriptionBox.Text = _currentIllustrationInfo.Text;
            ExpansionDropdown.SelectedItem = _currentIllustrationInfo.Expansion;
            ClassDropdown.SelectedItem = _currentIllustrationInfo.Tribe;
            if (typeof(LivingEntity).IsAssignableFrom(_currentEntity.GetType())) // Entities with HP and triggers
            {
                HpUpDown.Value = ((LivingEntity)_currentEntity).Hp.BaseValue;
                TriggerList.SetTriggerDict(((LivingEntity)_currentEntity).Triggers);
            }
            if (typeof(Unit).IsAssignableFrom(_currentEntity.GetType())) // Entities with HP
            {
                AttackUpDown.Value = ((Unit)_currentEntity).Attack.BaseValue;
                MovementUpdown.Value = ((Unit)_currentEntity).Movement.BaseValue;
                DenominatorUpDown.Value = ((Unit)_currentEntity).MovementDenominator.BaseValue;
            }
            if (typeof(Player).IsAssignableFrom(_currentEntity.GetType())) // Entities with HP
            {
                StartingGoldUpdown.Value = ((Player)_currentEntity).CurrentGold;
                ActivePowerUpDown.Value = ((Player)_currentEntity).ActivePowerId;
            }
            if (typeof(Skill).IsAssignableFrom(_currentEntity.GetType())) // Entities with HP
            {
                TargetTypeComboBox.SelectedItem = ((Skill)_currentEntity).TargetType;
                SkillOnwerFilterFlagCheckbox.SetFlags((int)((Skill)_currentEntity).TargetOwner);
            }
            InteractionList.SetInteractionDict(_currentEntity.Interactions);

            if (!cardLoaded)
            {
                entityTypeAlreadyLoaded = false;
            }
        }

        private void TargetTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ((Skill)_currentEntity).TargetType = (CardTargetingType)TargetTypeComboBox.SelectedItem;
            TargetLocationGrid.SetMode((CardTargetingType)TargetTypeComboBox.SelectedItem);
        }
    }
}
