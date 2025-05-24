using ODLGameEngine;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CardGenerationHelper
{
    public partial class CardGenerator : Form
    {
        EntityBase _currentEntity = new EntityBase();
        CardIllustrationInfo _currentIllustrationInfo = new CardIllustrationInfo();

        string _cardImagePath = Properties.Settings.Default.ImagePath;
        string _cardIconsPath = Properties.Settings.Default.LayoutPath;
        bool _debug = Properties.Settings.Default.Debug;

        System.Windows.Forms.Timer _drawUpdateTimer = new System.Windows.Forms.Timer();
        private void DrawTimeout(object sender, EventArgs e)
        {
            _drawUpdateTimer.Stop();
            if (BlueprintCheckBox.Checked)
            {
                DrawBlueprint();
            }
            else
            {
                DrawCard();
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

            // Load last setting
            DebugCheckBox.Checked = _debug;

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
            public const float CardRoundedPercentage = 0.1f;
            public const float CardBorder = 0.03f;
            // Then the locations for the rest of things, proportions
            public const float HorizontalMarginProportion = 0.03f;
            public const float VerticalMarginProportion = 0.015f;
            public const float BoxRoundedPercentage = 0.1f; // For square boxes, how much is it rounded
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
            public const float StatRoundedPercentage = 0.20f; // For square boxes, how much is it rounded
            // Blueprint Rotulo
            public const float RotuloWStart = 0.55f;
            public const float RotuloWEnd = 0.92f;
            public const float RotuloHStart = 0.75f;
            public const float RotuloHEnd = 0.9f;
            public const float RotuloRightSize = 0.25f;
            public const float RotuloRightSizeBottom = 0.33f;
            public const int RotuloBorderSize = 10;
            public const int RotuloTextAlignmentSpace = 150;
            // Blueprint
            public const float mapHStart = 0.15f; // Non-centered
            public const float mapHEnd = 0.72f;
            public const float mapWidth = 0.9f; // Centered
            public const float mapHSpaces = 0.15f; // How much % is horizontal spacing
            public const float mapVSpaces = 0.4f; // How much % is vertical spacing
            public const float tileRounded = 0.25f;
            public const float tileBorder = 0.03f;
            public const float dashedLineSize = 10;
        }
        private void DrawCard()
        {
            int width = DrawConstants.CardWidth;
            int height = DrawConstants.CardHeight;
            int proportionalCardReference = Math.Min(width, height);
            float cardBorder = proportionalCardReference * DrawConstants.CardBorder;

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
                float horizontalMargin = width * DrawConstants.HorizontalMarginProportion; // For reference
                float verticalMargin = height * DrawConstants.VerticalMarginProportion;
                float drawableWidth = width - 2 * cardBorder - 2 * horizontalMargin; // This is the actual horizontal space for drawing stuff
                float drawableHeight = height - 2 * cardBorder - verticalMargin; // Same, for vertical
                float currentDrawPointerX = cardBorder + horizontalMargin; // Where drawing pointer is
                float currentDrawPointerY = cardBorder + verticalMargin; // Data box need vertical margin
                // First the data box
                float dataBoxWidth = drawableWidth - horizontalMargin; // This is the area where image + stats fit (everyhting is a square)
                // This is solved by linear eq, every box a square
                // H = imgW
                // H = N * statW + (N-1) vSpace
                // statW + imgW = W
                // => with imgW = X, statW = Y
                // X-nY = (n-1) vSp
                // X+Y  = W
                // Substitution: (n+1)Y = W - (n-1)vsp
                int n = DrawConstants.NumberOfStats;
                float statWidth = (dataBoxWidth - ((n - 1) * verticalMargin)) / (n + 1);
                float imageBoxSize = dataBoxWidth - statWidth;
                FillHelper brush;
                string imagePath = Path.Combine(_cardImagePath, _currentIllustrationInfo.Id.ToString() + ".png");
                Rectangle imageBox = new Rectangle((int)currentDrawPointerX, (int)currentDrawPointerY, (int)imageBoxSize, (int)imageBoxSize);
                brush = DrawHelper.GetImageBrushOrColor(imageBox, imagePath, Color.White, Color.White);
                DrawHelper.DrawRoundedRectangle(g, imageBox, DrawConstants.BoxRoundedPercentage, Color.Black, DrawConstants.ImageBorder, brush);
                // Now Draw all Stats
                // Gold
                float statXpointer = currentDrawPointerX + imageBoxSize + horizontalMargin;
                float statYpointer = currentDrawPointerY;
                Rectangle statBox = new Rectangle((int)statXpointer, (int)statYpointer, (int)statWidth, (int)statWidth);
                imagePath = Path.Combine(_cardIconsPath, "gold.png");
                brush = DrawHelper.GetImageBrushOrColor(statBox, imagePath, Color.Gold, Color.Gold, 85);
                DrawHelper.DrawRoundedRectangle(g, statBox, DrawConstants.StatRoundedPercentage, Color.Black, DrawConstants.ImageBorder, brush);
                float statFontSize = statWidth / 1.333f; // Fixed size to fit stat box in consistent way. 1.333 is empirical
                Font statFont = new Font("Coolvetica Heavy Comp", statFontSize, FontStyle.Bold);
                DrawHelper.DrawFixedText(g, _currentIllustrationInfo.Cost, statBox, statFont, Color.White, Color.Black, DrawConstants.StatFontBorderPercentage, StringAlignment.Center, StringAlignment.Center, 0, _debug);
                statYpointer += statWidth + verticalMargin;
                // Rest of stats require a specific card
                if (typeof(LivingEntity).IsAssignableFrom(_currentEntity.GetType())) // Entities with HP
                {
                    // Then, HP
                    statBox = new Rectangle((int)statXpointer, (int)statYpointer, (int)statWidth, (int)statWidth);
                    imagePath = Path.Combine(_cardIconsPath, "hp.png");
                    brush = DrawHelper.GetImageBrushOrColor(statBox, imagePath, Color.Red, Color.Red, 85);
                    DrawHelper.DrawRoundedRectangle(g, statBox, DrawConstants.StatRoundedPercentage, Color.Black, DrawConstants.ImageBorder, brush);
                    DrawHelper.DrawFixedText(g, _currentIllustrationInfo.Hp, statBox, statFont, Color.White, Color.Black, DrawConstants.StatFontBorderPercentage, StringAlignment.Center, StringAlignment.Center, 0, _debug);
                    statYpointer += statWidth + verticalMargin;
                    if (typeof(Unit).IsAssignableFrom(_currentEntity.GetType())) // Units will also have attack and mvt
                    {
                        statBox = new Rectangle((int)statXpointer, (int)statYpointer, (int)statWidth, (int)statWidth);
                        imagePath = Path.Combine(_cardIconsPath, "attack.png");
                        brush = DrawHelper.GetImageBrushOrColor(statBox, imagePath, Color.Silver, Color.Silver, 85);
                        DrawHelper.DrawRoundedRectangle(g, statBox, DrawConstants.StatRoundedPercentage, Color.Black, DrawConstants.ImageBorder, brush);
                        DrawHelper.DrawFixedText(g, _currentIllustrationInfo.Attack, statBox, statFont, Color.White, Color.Black, DrawConstants.StatFontBorderPercentage, StringAlignment.Center, StringAlignment.Center, 0, _debug);
                        statYpointer += statWidth + verticalMargin;
                        statBox = new Rectangle((int)statXpointer, (int)statYpointer, (int)statWidth, (int)statWidth);
                        imagePath = Path.Combine(_cardIconsPath, "movement.png");
                        brush = DrawHelper.GetImageBrushOrColor(statBox, imagePath, Color.BurlyWood, Color.BurlyWood, 85);
                        DrawHelper.DrawRoundedRectangle(g, statBox, DrawConstants.StatRoundedPercentage, Color.Black, DrawConstants.ImageBorder, brush);
                        DrawHelper.DrawFixedText(g, _currentIllustrationInfo.Movement, statBox, statFont, Color.White, Color.Black, DrawConstants.StatFontBorderPercentage, StringAlignment.Center, StringAlignment.Center, 0, _debug);
                    }
                }
                currentDrawPointerY += imageBoxSize; // Move down to the next part
                drawableHeight -= imageBoxSize; // Remaining is the space excluding image box and the remaining separators
                // Ok now the rest:
                // Name:
                float titleAreaHeight = drawableHeight * DrawConstants.TextBoxProportion;
                float textAreaHeight = drawableHeight * DrawConstants.EffectBoxProportion;
                float extraAreaHeight = drawableHeight * DrawConstants.ExtraBoxProportion;
                float titleFontSize = titleAreaHeight;
                Font titleFont = new Font("Georgia", titleFontSize, FontStyle.Bold, GraphicsUnit.Pixel);
                Rectangle nameBox = new Rectangle((int)currentDrawPointerX, (int)currentDrawPointerY, (int)drawableWidth, (int)titleAreaHeight);
                DrawHelper.DrawAutoFitText(g, _currentIllustrationInfo.Name, nameBox, titleFont, Color.Black, Color.White, DrawConstants.StatFontBorderPercentage, StringAlignment.Center, StringAlignment.Center, 0, _debug);
                currentDrawPointerY += titleAreaHeight; // Move down to the next part
                // Effect:
                Rectangle textBox = new Rectangle((int)currentDrawPointerX, (int)currentDrawPointerY, (int)drawableWidth, (int)textAreaHeight);
                brush = new SolidFillHelper() { FillColor = Color.FromArgb((int)(255 * DrawConstants.TextBoxOpacity), Color.White) }; // Semi transparent white box
                DrawHelper.DrawRoundedRectangle(g, textBox, DrawConstants.BoxRoundedPercentage, Color.Black, DrawConstants.TextBoxBorder, brush);
                float textFontSize = textAreaHeight / (DrawConstants.TextSizeDivider * 1.33333f); // 1.333 because text is in pixels and I need pt
                Font textFont = new Font("Georgia", textFontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                int minTextBoxSize = Math.Min((int)drawableWidth, (int)textAreaHeight);
                DrawHelper.DrawRichTextBox(g, _currentIllustrationInfo.Text, textBox, textFont, Color.Black, (int)(minTextBoxSize * DrawConstants.TextBoxMargin), (int)(minTextBoxSize * DrawConstants.TextBoxMargin), _debug);
                currentDrawPointerY += textAreaHeight; // Move down to the next part
                // Extras:
                Rectangle extrasBox = new Rectangle((int)currentDrawPointerX, (int)currentDrawPointerY, (int)drawableWidth, (int)extraAreaHeight);
                float extraFontSize = extraAreaHeight;
                Font extraFont = new Font("Georgia", extraFontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                DrawHelper.DrawAutoFitText(g, $"#{_currentIllustrationInfo.Id}", extrasBox, textFont, Color.Black, Color.White, DrawConstants.StatFontBorderPercentage, StringAlignment.Far, StringAlignment.Center, (int)(drawableWidth * DrawConstants.ExtraBoxMargins), _debug);
                string rarityString = new string('\u2605', _currentIllustrationInfo.Rarity);
                DrawHelper.DrawAutoFitText(g, rarityString, extrasBox, textFont, Color.Black, Color.White, DrawConstants.StatFontBorderPercentage, StringAlignment.Near, StringAlignment.Center, (int)(drawableWidth * DrawConstants.ExtraBoxMargins), _debug);
            }
            CardPicture.Image = bitmap;
            if (_debug)
            {
                bitmap.Save("debug.png");
            }
        }
        private void DrawBlueprint()
        {
            string imagePath = Path.Combine(_cardIconsPath, "blueprint.png");
            if (File.Exists(imagePath)) // Blueprint base found
            {
                Bitmap bitmap = new Bitmap(imagePath);
                Graphics g = Graphics.FromImage(bitmap);
                int width = bitmap.Width; // Get dims
                int height = bitmap.Height;
                // Draw rotulo
                SolidFillHelper transparentBrush = new SolidFillHelper() { FillColor = Color.Transparent }; // No fill
                int xRotulo = (int)(width * DrawConstants.RotuloWStart);
                int yRotulo = (int)(height * DrawConstants.RotuloHStart);
                int widthRotulo = (int)(width * (DrawConstants.RotuloWEnd - DrawConstants.RotuloWStart));
                int heightRotulo = (int)(height * (DrawConstants.RotuloHEnd - DrawConstants.RotuloHStart));
                Rectangle rotuloBox = new Rectangle(xRotulo, yRotulo, widthRotulo, heightRotulo);
                DrawHelper.DrawRectangleFixedBorder(g, rotuloBox, Color.White, DrawConstants.RotuloBorderSize, transparentBrush);
                Rectangle rotuloTitle = new Rectangle(xRotulo, yRotulo, (int)(widthRotulo * (1 - DrawConstants.RotuloRightSize)), heightRotulo);
                Font rotuloFont = new Font("Consolas", heightRotulo);
                DrawHelper.DrawAutoFitText(g, _currentIllustrationInfo.Name, rotuloTitle, rotuloFont, Color.White, Color.White, 0, StringAlignment.Center, StringAlignment.Center, 0, _debug);
                xRotulo += (int)(widthRotulo * (1 - DrawConstants.RotuloRightSize));
                widthRotulo = (int)(widthRotulo * DrawConstants.RotuloRightSize);
                rotuloBox = new Rectangle(xRotulo, yRotulo, widthRotulo, heightRotulo);
                DrawHelper.DrawRectangleFixedBorder(g, rotuloBox, Color.White, DrawConstants.RotuloBorderSize, transparentBrush);
                rotuloFont = new Font("Consolas", (int)(heightRotulo * (1 - DrawConstants.RotuloRightSizeBottom)));
                Rectangle rotuloRightTextBox = new Rectangle(xRotulo, yRotulo, widthRotulo, (int)(heightRotulo * (1 - DrawConstants.RotuloRightSizeBottom)));
                DrawHelper.DrawAutoFitText(g, "#" + _currentIllustrationInfo.Id, rotuloRightTextBox, rotuloFont, Color.White, Color.White, 0, StringAlignment.Center, StringAlignment.Center, 0, _debug);
                yRotulo += (int)(heightRotulo * (1 - DrawConstants.RotuloRightSizeBottom));
                heightRotulo = (int)(heightRotulo * DrawConstants.RotuloRightSizeBottom);
                rotuloBox = new Rectangle(xRotulo, yRotulo, widthRotulo, heightRotulo);
                DrawHelper.DrawRectangleFixedBorder(g, rotuloBox, Color.White, DrawConstants.RotuloBorderSize, transparentBrush);
                rotuloFont = new Font("Georgia", heightRotulo, FontStyle.Regular, GraphicsUnit.Pixel);
                string rarityString = new string('\u2605', _currentIllustrationInfo.Rarity);
                DrawHelper.DrawAutoFitText(g, rarityString, rotuloBox, rotuloFont, Color.White, Color.White, 0, StringAlignment.Center, StringAlignment.Center, 0, _debug);
                // End of rotulo now the actual matrix
                int yMap = (int)(height * DrawConstants.mapHStart);
                int heightMap = (int)(height * (DrawConstants.mapHEnd - DrawConstants.mapHStart));
                int widthMap = (int)(width * DrawConstants.mapWidth);
                int xMap = (width - widthMap) / 2;
                if (_debug)
                {
                    Rectangle mapDebugBox = new Rectangle(xMap, yMap, widthMap, heightMap);
                    DrawHelper.DrawRectangleFixedBorder(g, mapDebugBox, Color.White, DrawConstants.RotuloBorderSize, transparentBrush);
                }
                Rectangle getCoordinateRectangle(int row, int column) // Gives me the desired tile
                {
                    // First, calculate coord
                    int nrows = GameConstants.BOARD_LANES_NUMBER;
                    int nCols = new[] { GameConstants.PLAINS_TILES_NUMBER, GameConstants.FOREST_TILES_NUMBER, GameConstants.MOUNTAIN_TILES_NUMBER }.Max();
                    float hSpace = widthMap * DrawConstants.mapHSpaces;
                    float hSep = hSpace / (nCols + 1);
                    float x = (widthMap - hSpace) / nCols;
                    float vSpace = heightMap * DrawConstants.mapVSpaces;
                    float vSep = vSpace / (nrows + 1);
                    float y = (heightMap - vSpace) / nrows;
                    // Now rectangle
                    Rectangle tile = new Rectangle(
                        (int)(xMap + hSep + column * (hSep + x)),
                        (int)(yMap + vSep + row * (vSep + y)),
                        (int)x, (int)y);
                    return tile;
                }
                int getAdaptedColumn(int row, LaneID lane)
                {
                    int maxCol = new[] { GameConstants.PLAINS_TILES_NUMBER, GameConstants.FOREST_TILES_NUMBER, GameConstants.MOUNTAIN_TILES_NUMBER }.Max();
                    int laneSize = lane switch
                    {
                        LaneID.PLAINS => GameConstants.PLAINS_TILES_NUMBER,
                        LaneID.FOREST => GameConstants.FOREST_TILES_NUMBER,
                        LaneID.MOUNTAIN => GameConstants.MOUNTAIN_TILES_NUMBER,
                        _ => throw new Exception("Not a lane")
                    };
                    int offset = (maxCol - laneSize) / 2;
                    return row + offset;
                }
                // Ok now I need to plot stuff, plot lane by lane
                for (int i = 0; i < GameConstants.PLAINS_TILES_NUMBER; i++)
                {
                    Rectangle rect = getCoordinateRectangle(0, getAdaptedColumn(i, LaneID.PLAINS));
                    DrawHelper.DrawRoundedRectangle(g, rect, DrawConstants.tileRounded, Color.White, DrawConstants.tileBorder, transparentBrush);
                }
                for (int i = 0; i < GameConstants.FOREST_TILES_NUMBER; i++)
                {
                    Rectangle rect = getCoordinateRectangle(1, getAdaptedColumn(i, LaneID.FOREST));
                    DrawHelper.DrawRoundedRectangle(g, rect, DrawConstants.tileRounded, Color.White, DrawConstants.tileBorder, transparentBrush);
                }
                for (int i = 0; i < GameConstants.MOUNTAIN_TILES_NUMBER; i++)
                {
                    Rectangle rect = getCoordinateRectangle(2, getAdaptedColumn(i, LaneID.MOUNTAIN));
                    DrawHelper.DrawRoundedRectangle(g, rect, DrawConstants.tileRounded, Color.White, DrawConstants.tileBorder, transparentBrush);
                }
                // And now the actual BP
                int[] bp = ((Building)_currentEntity).PlainsBp;
                if (bp != null)
                {
                    for (int i = 0; i < bp.Length; i++)
                    {
                        Rectangle rect = getCoordinateRectangle(0, getAdaptedColumn(bp[i], LaneID.PLAINS));
                        DrawHelper.DrawRoundedRectangle(g, rect, DrawConstants.tileRounded, Color.White, DrawConstants.tileBorder, new SolidFillHelper() { FillColor = Color.White });
                        float bpFontSize = rect.Height / 1.333f; // Fixed size to fit BP tile in consistent way. 1.333 is empirical
                        Font bpFont = new Font("Consolas", bpFontSize, FontStyle.Bold);
                        DrawHelper.DrawAutoFitText(g, (i + 1).ToString(), rect, bpFont, Color.FromArgb(69, 134, 202), Color.Black, 0, StringAlignment.Center, StringAlignment.Center, 0, _debug);
                    }
                }
                bp = ((Building)_currentEntity).ForestBp;
                if (bp != null)
                {
                    for (int i = 0; i < bp.Length; i++)
                    {
                        Rectangle rect = getCoordinateRectangle(1, getAdaptedColumn(bp[i], LaneID.FOREST));
                        DrawHelper.DrawRoundedRectangle(g, rect, DrawConstants.tileRounded, Color.White, DrawConstants.tileBorder, new SolidFillHelper() { FillColor = Color.White });
                        float bpFontSize = rect.Height / 1.333f; // Fixed size to fit BP tile in consistent way. 1.333 is empirical
                        Font bpFont = new Font("Consolas", bpFontSize, FontStyle.Bold);
                        DrawHelper.DrawAutoFitText(g, (i + 1).ToString(), rect, bpFont, Color.FromArgb(69, 134, 202), Color.Black, 0, StringAlignment.Center, StringAlignment.Center, 0, _debug);
                    }
                }
                bp = ((Building)_currentEntity).MountainBp;
                if (bp != null)
                {
                    for (int i = 0; i < bp.Length; i++)
                    {
                        Rectangle rect = getCoordinateRectangle(2, getAdaptedColumn(bp[i], LaneID.MOUNTAIN));
                        DrawHelper.DrawRoundedRectangle(g, rect, DrawConstants.tileRounded, Color.White, DrawConstants.tileBorder, new SolidFillHelper() { FillColor = Color.White });
                        float bpFontSize = rect.Height / 1.333f; // Fixed size to fit BP tile in consistent way. 1.333 is empirical
                        Font bpFont = new Font("Consolas", bpFontSize, FontStyle.Bold);
                        DrawHelper.DrawAutoFitText(g, (i + 1).ToString(), rect, bpFont, Color.FromArgb(69, 134, 202), Color.Black, 0, StringAlignment.Center, StringAlignment.Center, 0, _debug);
                    }
                }
                // Draw line now
                Pen dashedPen = new Pen(Color.White, DrawConstants.dashedLineSize);
                dashedPen.DashStyle = DashStyle.Dash;
                g.DrawLine(dashedPen, new Point(xMap + (widthMap / 2), yMap), new Point(xMap + (widthMap / 2), yMap + heightMap));
                // Visualize
                CardPicture.Image = bitmap;
                if (_debug)
                {
                    bitmap.Save("debug.png");
                }
            }
        }
        private void CardGenerator_Load(object sender, EventArgs e)
        {
            EntityTypeDropdown.Items.AddRange(Enum.GetValues(typeof(EntityType)).Cast<object>().ToArray());
            EntityTypeDropdown.SelectedIndex = 0;
            TargetOptionsDropdown.Items.AddRange(Enum.GetValues(typeof(TargetLocation)).Cast<object>().ToArray());
            TargetOptionsDropdown.SelectedIndex = 0;
            ExpansionDropdown.Items.AddRange(Enum.GetValues(typeof(ExpansionId)).Cast<object>().ToArray());
            ExpansionDropdown.SelectedIndex = 0;
            ClassDropdown.Items.AddRange(Enum.GetValues(typeof(PlayerClassType)).Cast<object>().ToArray());
            ClassDropdown.SelectedIndex = 0;

            RedrawUi();
        }
        
        private void EntityTypeDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentIllustrationInfo.EntityType = (EntityType)EntityTypeDropdown.SelectedItem;
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
                BlueprintsPanel.Show();
                BlueprintCheckBox.Show();
            }
            else
            {
                BlueprintCheckBox.Hide();
                BlueprintsPanel.Hide();
            }
            if (typeof(Player).IsAssignableFrom(_currentEntity.GetType()))
            {
                PlayerPanel.Show();
            }
            else
            {
                PlayerPanel.Hide();
            }
        }

        private void TargetOptionsDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentEntity.TargetOptions = (TargetLocation)TargetOptionsDropdown.SelectedItem;
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
            _currentIllustrationInfo.ClassType = (PlayerClassType)ClassDropdown.SelectedItem;
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

        private void CostUpDown_ValueChanged(object sender, EventArgs e)
        {
            _currentIllustrationInfo.Cost = CostUpDown.Value.ToString();
            _currentEntity.Cost = Convert.ToInt32(CostUpDown.Value);
            RefreshDrawTimer();
        }

        private void SavePictureButton_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";
                saveFileDialog.Title = "Save card Image";
                saveFileDialog.DefaultExt = "png"; // Default file type

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;

                    // Example: Assuming you have a PictureBox named pictureBox1
                    CardPicture.Image.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                }
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
        private void MovementOrDenominatorUpdown_ValueChanged(object sender, EventArgs e)
        {
            ((Unit)_currentEntity).Movement.BaseValue = Convert.ToInt32(MovementUpdown.Value);
            ((Unit)_currentEntity).MovementDenominator.BaseValue = Convert.ToInt32(DenominatorUpDown.Value);
            string MovString = MovementUpdown.Value.ToString();
            if (DenominatorUpDown.Value != 1)
            {
                MovString += "/" + DenominatorUpDown.Value.ToString();
            }
            _currentIllustrationInfo.Movement = MovString;
            RefreshDrawTimer();
        }
        private void ChangeBlueprint(TargetLocation lane, string bpText)
        {
            int[] bpElements;
            if (bpText == "")
            {
                bpElements = null;
            }
            else
            {
                string[] choices = bpText.Split(','); // Get all inputs
                int maxTiles = lane switch
                {
                    TargetLocation.PLAINS => GameConstants.PLAINS_TILES_NUMBER,
                    TargetLocation.FOREST => GameConstants.FOREST_TILES_NUMBER,
                    TargetLocation.MOUNTAIN => GameConstants.MOUNTAIN_TILES_NUMBER,
                    _ => throw new Exception("Invalid lane BP")
                };
                bpElements = new int[Math.Min(choices.Length, maxTiles)]; // Bp limited by tile amount and also by how many fields
                // Now i parse
                for (int i = 0; i < bpElements.Length; i++)
                {
                    if (int.TryParse(choices[i], out int result))
                    {
                        bpElements[i] = result;
                    }
                }
            }
            // Finally load
            Building bldg = (Building)_currentEntity;
            switch (lane) // Load in the correct BP
            {
                case TargetLocation.PLAINS:
                    bldg.PlainsBp = bpElements; break;
                case TargetLocation.FOREST:
                    bldg.ForestBp = bpElements; break;
                case TargetLocation.MOUNTAIN:
                    bldg.MountainBp = bpElements; break;
                default:
                    throw new Exception("Invalid lane BP");
            }
            RefreshDrawTimer(); // May need to redraw
        }
        private void PlainsBpTextBox_TextChanged(object sender, EventArgs e)
        {
            ChangeBlueprint(TargetLocation.PLAINS, PlainsBpTextBox.Text);
        }

        private void ForestBpTextBox_TextChanged(object sender, EventArgs e)
        {
            ChangeBlueprint(TargetLocation.FOREST, ForestBpTextBox.Text);
        }

        private void MountainBpTextBox_TextChanged(object sender, EventArgs e)
        {
            ChangeBlueprint(TargetLocation.MOUNTAIN, MountainBpTextBox.Text);
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
    }
}
