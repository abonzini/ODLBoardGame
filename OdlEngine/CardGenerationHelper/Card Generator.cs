using ODLGameEngine;

namespace CardGenerationHelper
{
    public partial class CardGenerator : Form
    {
        EntityBase _currentEntity;
        EntityPlayInfo _currentPlayInfo;
        EntityPrintInfo _currentPrintInfo;

        EntityBase _emptyEntity = new EntityBase();
        Unit _unit = new Unit();
        Building _building = new Building();
        PlayerState _player = new PlayerState();
        Skill _skill = new Skill();

        string _cardImagePath = Properties.Settings.Default.ImagePath;
        string _cardIconsPath = Properties.Settings.Default.LayoutPath;
        bool _debug = Properties.Settings.Default.Debug;

        System.Windows.Forms.Timer _drawUpdateTimer = new System.Windows.Forms.Timer();
        private void DrawTimeout(object sender, EventArgs e)
        {
            _drawUpdateTimer.Stop();
            DrawCard();
        }
        private void RefreshDrawTimer()
        {
            _drawUpdateTimer.Stop();
            _drawUpdateTimer.Start();
        }
        public CardGenerator()
        {
            InitializeComponent();
            _currentEntity = _emptyEntity;
            _currentPlayInfo = _emptyEntity.EntityPlayInfo;
            _currentPrintInfo = _emptyEntity.EntityPrintInfo;
            List<EntityBase> entities = [_unit, _building, _player, _skill];
            foreach (EntityBase entity in entities) // Set all to same info and fuck it
            {
                entity.EntityPlayInfo = _currentPlayInfo;
                entity.EntityPrintInfo = _currentPrintInfo;
            }

            DebugCheckBox.Checked = _debug; // Load last setting

            // Timer
            _drawUpdateTimer.Tick += DrawTimeout;
            _drawUpdateTimer.Interval = 150; // 150ms
            _drawUpdateTimer.Stop();

            DrawCard(); // Draw empty card
        }
        public static class DrawConstants
        {
            public const int CardWidth = 2500; // 2.5x3.5 is resolution of a typical TCG card
            public const int CardHeight = 3500;
            public const float CardRoundedPercentage = 0.2f;
            public const float CardBorder = 0.03f;
            // Then the locations for the rest of things, proportions
            public const float HorizontalMarginProportion = 0.03f;
            public const float VerticalMarginProportion = 0.015f;
            public const float BoxRoundedPercentage = 0.2f; // For square boxes, how much is it rounded
            // For the rest (non data box)
            public const float TextBoxProportion = 0.2f; // Card name
            public const float ExtraBoxProportion = 0.125f; // ID, rarity, expansion
            public const float EffectBoxProportion = 1 - TextBoxProportion - ExtraBoxProportion; // Rest is for description box
            public const float TextBoxOpacity = 0.5f;
            public const float TextBoxBorder = 0.006f;
            public const float TextSizeDivider = 6; // How many lines approx fit
            public const float TextBoxMargin = 0.05f;
            public const float ExtraBoxMargins = 0.00f;
            // Data Box
            public const int NumberOfStats = 4; // How many stats in data box
            public const float ImageBorder = 0.006f;
            // Stats Box
            public const float StatFontBorderPercentage = 0.1f;
            public const float GoldStatSize = 0.2f;
            public static readonly Color GoldColorTint = Color.Gold;
            public const float StatRoundedPercentage = 0.5f; // For square boxes, how much is it rounded
        }
        private void DrawCard()
        {
            int width = DrawConstants.CardWidth;
            int height = DrawConstants.CardHeight;
            int proportionalCardReference = Math.Min(width, height);
            int cardBorder = (int)(proportionalCardReference * DrawConstants.CardBorder);

            Bitmap bitmap = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bitmap);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.PageUnit = GraphicsUnit.Pixel;
            Rectangle bounds = new Rectangle(0, 0, width, height);
            DrawHelper.DrawRoundedRectangle(g, bounds, DrawConstants.CardRoundedPercentage, Color.Black, DrawConstants.CardBorder, new SolidFillHelper() { FillColor = Color.LightGray });

            // Then, all non-invalid cards have picture, cost, name, textbox, rarity, etc
            if (typeof(IngameEntity).IsAssignableFrom(_currentEntity.GetType()))
            {
                // Ok it's a real card so let's draw the basics!
                int horizontalMargin = (int)(width * DrawConstants.HorizontalMarginProportion); // For reference
                int verticalMargin = (int)(height * DrawConstants.VerticalMarginProportion);
                int drawableWidth = width - 2 * cardBorder - 2 * horizontalMargin; // This is the actual horizontal space for drawing stuff
                int drawableHeight = height - 2 * cardBorder - 2 * verticalMargin; // Same, for vertical
                int currentDrawPointerX = cardBorder + horizontalMargin; // Where drawing pointer is
                int currentDrawPointerY = cardBorder + verticalMargin;
                // First the data box
                int dataBoxWidth = drawableWidth - horizontalMargin; // This is the area where image + stats fit (everyhting is a square)
                // This is solved by linear eq, every box a square
                // H = imgW
                // H = N * statW + (N-1) vSpace
                // statW + imgW = W
                // => with imgW = X, statW = Y
                // X-nY = (n-1) vSp
                // X+Y  = W
                // Substitution: (n+1)Y = W - (n-1)vsp
                int n = DrawConstants.NumberOfStats;
                int statWidth = (dataBoxWidth - ((n-1)*horizontalMargin)) / (n+1);
                int imageBoxSize = dataBoxWidth - statWidth;
                FillHelper brush;
                string imagePath = Path.Combine(_cardImagePath, _currentPrintInfo.Id.ToString() + ".png");
                Rectangle imageBox = new Rectangle(currentDrawPointerX, currentDrawPointerY, imageBoxSize, imageBoxSize);
                brush = DrawHelper.GetImageBrushOrColor(imageBox, imagePath, Color.White, Color.White);
                DrawHelper.DrawRoundedRectangle(g, imageBox, DrawConstants.BoxRoundedPercentage, Color.Black, DrawConstants.ImageBorder, brush);
                // Now Draw all Stats
                // Gold
                int statXpointer = currentDrawPointerX + imageBoxSize + horizontalMargin;
                int statYpointer = currentDrawPointerY;
                Rectangle statBox = new Rectangle(statXpointer, statYpointer, statWidth, statWidth);
                imagePath = Path.Combine(_cardIconsPath, "gold.png");
                brush = DrawHelper.GetImageBrushOrColor(statBox, imagePath, Color.Gold, Color.White);
                DrawHelper.DrawRoundedRectangle(g, statBox, DrawConstants.StatRoundedPercentage, Color.Black, DrawConstants.ImageBorder, brush);
                float statFontSize = statWidth / 1.333f; // Fixed size to fit stat box in consistent way. 1.333 is empirical
                Font statFont = new Font("Coolvetica Heavy Comp", statFontSize, FontStyle.Bold);
                DrawHelper.DrawFixedText(g, "2/3", statBox, statFont, Color.White, Color.Black, DrawConstants.StatFontBorderPercentage, StringAlignment.Center, StringAlignment.Center, 0, _debug);
                // Finished stats
                currentDrawPointerY += imageBoxSize + verticalMargin; // Move down to the next part
                drawableHeight -= imageBoxSize + 3 * verticalMargin; // Remaining is the space excluding image box and the remaining separators
                // Ok now the rest:
                // Name:
                int titleAreaHeight = (int)(drawableHeight * DrawConstants.TextBoxProportion);
                int textAreaHeight = (int)(drawableHeight * DrawConstants.EffectBoxProportion);
                int extraAreaHeight = (int)(drawableHeight * DrawConstants.ExtraBoxProportion);
                float titleFontSize = titleAreaHeight; // 1.333 because titleHeight is in pixels and I need pt
                Font titleFont = new Font("Georgia", titleFontSize, FontStyle.Bold, GraphicsUnit.Pixel);
                Rectangle nameBox = new Rectangle(currentDrawPointerX, currentDrawPointerY, drawableWidth, titleAreaHeight);
                DrawHelper.DrawAutoFitText(g, _currentPrintInfo.Title, nameBox, titleFont, Color.Black, Color.Black, 0, StringAlignment.Center, StringAlignment.Center, 0, _debug);
                currentDrawPointerY += titleAreaHeight + verticalMargin; // Move down to the next part
                // Effect:
                Rectangle textBox = new Rectangle(currentDrawPointerX, currentDrawPointerY, drawableWidth, textAreaHeight);
                brush = new SolidFillHelper() { FillColor = Color.FromArgb((int)(255 * DrawConstants.TextBoxOpacity), Color.White) }; // Semi transparent white box
                DrawHelper.DrawRoundedRectangle(g, textBox, DrawConstants.BoxRoundedPercentage, Color.Black, DrawConstants.TextBoxBorder, brush);
                float textFontSize = textAreaHeight / (DrawConstants.TextSizeDivider * 1.33333f); // 1.333 because text is in pixels and I need pt
                Font textFont = new Font("Georgia", textFontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                int minTextBoxSize = Math.Min(drawableWidth, textAreaHeight);
                DrawHelper.DrawRichTextBox(g, _currentPrintInfo.Text, textBox, textFont, Color.Black, (int)(minTextBoxSize * DrawConstants.TextBoxMargin), (int)(minTextBoxSize * DrawConstants.TextBoxMargin), _debug);
                currentDrawPointerY += textAreaHeight + verticalMargin; // Move down to the next part
                // Extras:
                Rectangle extrasBox = new Rectangle(currentDrawPointerX, currentDrawPointerY, drawableWidth, extraAreaHeight);
                float extraFontSize = extraAreaHeight;
                Font extraFont = new Font("Georgia", extraFontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                DrawHelper.DrawAutoFitText(g, $"#{_currentPrintInfo.Id}", extrasBox, textFont, Color.Black, Color.Black, 0, StringAlignment.Far, StringAlignment.Center, (int)(drawableWidth * DrawConstants.ExtraBoxMargins), _debug);
                string rarityString = new string('\u2605', _currentPrintInfo.Rarity);
                DrawHelper.DrawAutoFitText(g, rarityString, extrasBox, textFont, Color.Black, Color.Black, 0, StringAlignment.Near, StringAlignment.Center, (int)(drawableWidth * DrawConstants.ExtraBoxMargins), _debug);
            }
            CardPicture.Image = bitmap;
            bitmap.Save("debug.png");
        }

        private void CardGenerator_Load(object sender, EventArgs e)
        {
            EntityTypeDropdown.Items.AddRange(Enum.GetValues(typeof(EntityType)).Cast<object>().ToArray());
            EntityTypeDropdown.SelectedIndex = 0;
            TargetOptionsDropdown.Items.AddRange(Enum.GetValues(typeof(TargetLocation)).Cast<object>().ToArray());
            TargetOptionsDropdown.SelectedIndex = 0;
            TargetConditionDropdown.Items.AddRange(Enum.GetValues(typeof(TargetCondition)).Cast<object>().ToArray());
            TargetConditionDropdown.SelectedIndex = 0;
            ExpansionDropdown.Items.AddRange(Enum.GetValues(typeof(ExpansionId)).Cast<object>().ToArray());
            ExpansionDropdown.SelectedIndex = 0;
            ClassDropdown.Items.AddRange(Enum.GetValues(typeof(PlayerClassType)).Cast<object>().ToArray());
            ClassDropdown.SelectedIndex = 0;
        }

        private void EntityTypeDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentPlayInfo.EntityType = (EntityType)EntityTypeDropdown.SelectedItem;
            _currentEntity = _currentPlayInfo.EntityType switch
            {
                EntityType.NONE => _emptyEntity,
                EntityType.PLAYER => _player,
                EntityType.SKILL => _skill,
                EntityType.UNIT => _unit,
                EntityType.BUILDING => _building,
                _ => throw new NotImplementedException("Incorrect entity type selected")
            };
            // TODO: Later force UI redraw of elements
            RefreshDrawTimer();
        }

        private void TargetOptionsDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentPlayInfo.TargetOptions = (TargetLocation)TargetOptionsDropdown.SelectedItem;
        }

        private void TargetConditionDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentPlayInfo.TargetConditions = (TargetCondition)TargetConditionDropdown.SelectedItem;
        }

        private void CardIdUpdown_ValueChanged(object sender, EventArgs e)
        {
            _currentPrintInfo.Id = Convert.ToInt32(CardIdUpdown.Value);
            RefreshDrawTimer();
        }

        private void CardNameBox_TextChanged(object sender, EventArgs e)
        {
            _currentPrintInfo.Title = CardNameBox.Text.ToUpper();
            RefreshDrawTimer();
        }

        private void ExpansionDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentPrintInfo.Expansion = (ExpansionId)ExpansionDropdown.SelectedItem;
            RefreshDrawTimer();
        }

        private void ClassDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentPrintInfo.ClassType = (PlayerClassType)ClassDropdown.SelectedItem;
            RefreshDrawTimer();
        }

        private void CardPicturePathLoadButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
                {
                    _cardImagePath = folderDialog.SelectedPath;
                    Properties.Settings.Default.ImagePath = _cardImagePath;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void EffectDescriptionBox_TextChanged(object sender, EventArgs e)
        {
            _currentPrintInfo.Text = EffectDescriptionBox.Text;
            RefreshDrawTimer();
        }

        private void RarityUpDown_ValueChanged(object sender, EventArgs e)
        {
            _currentPrintInfo.Rarity = Convert.ToInt32(RarityUpDown.Value);
            RefreshDrawTimer();
        }

        private void DebugCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _debug = DebugCheckBox.Checked;
            Properties.Settings.Default.Debug = _debug;
            Properties.Settings.Default.Save();
            RefreshDrawTimer();
        }

        private void CardIconFolders_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
                {
                    _cardIconsPath = folderDialog.SelectedPath;
                    Properties.Settings.Default.LayoutPath = _cardIconsPath;
                    Properties.Settings.Default.Save();
                }
            }
        }
    }
}
