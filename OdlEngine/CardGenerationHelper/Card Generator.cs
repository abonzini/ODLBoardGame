using ODLGameEngine;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CardGenerationHelper
{
    public partial class CardGenerator : Form
    {
        EntityBase currentEntity;
        EntityPlayInfo currentPlayInfo;
        EntityPrintInfo currentPrintInfo;

        EntityBase emptyEntity = new EntityBase();
        Unit unit = new Unit();
        Building building = new Building();
        PlayerState player = new PlayerState();
        Skill skill = new Skill();

        string CardImagePath = Properties.Settings.Default.ImagePath;
        public CardGenerator()
        {
            InitializeComponent();
            currentEntity = emptyEntity;
            currentPlayInfo = emptyEntity.EntityPlayInfo;
            currentPrintInfo = emptyEntity.EntityPrintInfo;
            List<EntityBase> entities = [unit, building, player, skill];
            foreach (EntityBase entity in entities) // Set all to same info and fuck it
            {
                entity.EntityPlayInfo = currentPlayInfo;
                entity.EntityPrintInfo = currentPrintInfo;
            }

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
            public const float TextBoxProportion = 0.25f; // Card name
            public const float ExtraBoxProportion = 0.125f; // ID, rarity, expansion
            public const float EffectBoxProportion = 1 - TextBoxProportion - ExtraBoxProportion; // Rest is for description box
            public const float TextBoxOpacity = 0.5f;
            public const float TextBoxBorder = 0.006f;
            public const float TextSizeDivider = 6; // How many lines approx fit
            public const float TextBoxMargin = 0.05f;
            public const float ExtraBoxMargins = 0.05f;
            // Data Box
            public const float ImageToStatRatio = 2f;
            public const float ImageBorder = 0.006f;
        }
        private void DrawCard()
        {
            int width = DrawConstants.CardWidth;
            int height = DrawConstants.CardHeight;
            int proportionalCardReference = Math.Min(width, height);

            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.PageUnit = GraphicsUnit.Pixel;

                Rectangle bounds = new Rectangle(0, 0, width, height);
                int cardRadius = (int)(proportionalCardReference * DrawConstants.CardRoundedPercentage);
                int cardBorder = (int)(proportionalCardReference * DrawConstants.CardBorder);
                DrawHelper.DrawRoundedRectangle(g, bounds, cardRadius, Color.Black, new SolidFillHelper() { FillColor = Color.LightGray }, cardBorder);

                // Then, all non-invalid cards have picture, cost, name, textbox, rarity, etc
                if (typeof(IngameEntity).IsAssignableFrom(currentEntity.GetType()))
                {
                    // Ok it's a real card so let's draw the basics!
                    int horizontalMargin = (int)(width * DrawConstants.HorizontalMarginProportion); // For reference
                    int verticalMargin = (int)(height * DrawConstants.VerticalMarginProportion);
                    // First the data box
                    int dataBoxSpace = width - 2 * cardBorder - 3 * horizontalMargin; // This is the area where image + stats fit (everyhting is a square)
                    int imageBoxSize = (int)(DrawConstants.ImageToStatRatio * (dataBoxSpace / (DrawConstants.ImageToStatRatio + 1)));
                    int boxesStartX = cardBorder + horizontalMargin;
                    int imageBoxStartY = cardBorder + verticalMargin;
                    FillHelper brush;
                    if (Path.Exists(Path.Combine(CardImagePath, currentPrintInfo.Id.ToString() + ".png"))) // Check if image exists
                    {
                        brush = new ImageFillHelper()
                        {
                            ImagePath = Path.Combine(CardImagePath, currentPrintInfo.Id.ToString() + ".png"),
                            Height = imageBoxSize,
                            Width = imageBoxSize,
                            StartX = boxesStartX,
                            StartY = imageBoxStartY,
                        };
                    }
                    else
                    {
                        brush = new SolidFillHelper() { FillColor = Color.White };
                    }
                    Rectangle imageBox = new Rectangle(boxesStartX, imageBoxStartY, imageBoxSize, imageBoxSize);
                    DrawHelper.DrawRoundedRectangle(g, imageBox, (int)(imageBoxSize * DrawConstants.BoxRoundedPercentage), Color.Black, brush, (int)(imageBoxSize * DrawConstants.ImageBorder));
                    // Stats (later, TODO)
                    // Ok now the rest:
                    // Name:
                    int usedCardHeight = cardBorder + verticalMargin + imageBoxSize;
                    int restOfCardHeight = height - usedCardHeight - cardBorder - verticalMargin;
                    int restOfCardWidth = width - 2 * cardBorder - 2 * horizontalMargin; // This is the actual space for the rest
                    int titleAreaHeight = (int)(restOfCardHeight * DrawConstants.TextBoxProportion);
                    int textAreaHeight = (int)(restOfCardHeight * DrawConstants.EffectBoxProportion);
                    int extraAreaHeight = (int)(restOfCardHeight * DrawConstants.ExtraBoxProportion);
                    float titleFontSize = titleAreaHeight; // 1.333 because titleHeight is in pixels and I need pt
                    Font titleFont = new Font("Georgia", titleFontSize, FontStyle.Bold, GraphicsUnit.Pixel);
                    Rectangle nameBox = new Rectangle(boxesStartX, usedCardHeight + verticalMargin, restOfCardWidth, titleAreaHeight - verticalMargin);
                    DrawHelper.DrawTextBox(g, currentPrintInfo.Title, nameBox, titleFont, Color.Black, Color.Black, 0, StringAlignment.Center, StringAlignment.Center, 0, true);
                    usedCardHeight += titleAreaHeight;
                    // Effect:
                    Rectangle textBox = new Rectangle(boxesStartX, usedCardHeight + verticalMargin, restOfCardWidth, textAreaHeight - verticalMargin);
                    int minTextBoxSize = Math.Min(restOfCardWidth, textAreaHeight - verticalMargin);
                    brush = new SolidFillHelper() { FillColor = Color.FromArgb((int)(255 * DrawConstants.TextBoxOpacity), Color.White) }; // Semi transparent white box
                    DrawHelper.DrawRoundedRectangle(g, textBox, (int)(minTextBoxSize * DrawConstants.BoxRoundedPercentage), Color.Black, brush, (int)(minTextBoxSize * DrawConstants.TextBoxBorder));
                    float textFontSize = textAreaHeight / (DrawConstants.TextSizeDivider * 1.33333f); // 1.333 because text is in pixels and I need pt
                    Font textFont = new Font("Georgia", textFontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                    DrawHelper.DrawTextBox(g, currentPrintInfo.Text, textBox, textFont, Color.Black, Color.Black, 0, StringAlignment.Near, StringAlignment.Near, (int)(minTextBoxSize * DrawConstants.TextBoxMargin), false);
                    usedCardHeight += textAreaHeight;
                    // Extras:
                    Rectangle extrasBox = new Rectangle(boxesStartX, usedCardHeight + verticalMargin, restOfCardWidth, extraAreaHeight - verticalMargin);
                    float extraFontSize = extraAreaHeight;
                    Font extraFont = new Font("Georgia", extraFontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                    DrawHelper.DrawTextBox(g, $"#{currentPrintInfo.Id}", extrasBox, textFont, Color.Black, Color.Black, 0, StringAlignment.Far, StringAlignment.Center, (int)(restOfCardWidth * DrawConstants.ExtraBoxMargins), true);
                    string rarityString = new string('\u2605', currentPrintInfo.Rarity);
                    DrawHelper.DrawTextBox(g, rarityString, extrasBox, textFont, Color.Black, Color.Black, 0, StringAlignment.Near, StringAlignment.Center, (int)(restOfCardWidth * DrawConstants.ExtraBoxMargins), true);
                }
            }
            CardPicture.Image = bitmap;
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
            currentPlayInfo.EntityType = (EntityType)EntityTypeDropdown.SelectedItem;
            currentEntity = currentPlayInfo.EntityType switch
            {
                EntityType.NONE => emptyEntity,
                EntityType.PLAYER => player,
                EntityType.SKILL => skill,
                EntityType.UNIT => unit,
                EntityType.BUILDING => building,
                _ => throw new NotImplementedException("Incorrect entity type selected")
            };
            // TODO: Later force UI redraw of elements
            DrawCard();
        }

        private void TargetOptionsDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPlayInfo.TargetOptions = (TargetLocation)TargetOptionsDropdown.SelectedItem;
        }

        private void TargetConditionDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPlayInfo.TargetConditions = (TargetCondition)TargetConditionDropdown.SelectedItem;
        }

        private void CardIdUpdown_ValueChanged(object sender, EventArgs e)
        {
            currentPrintInfo.Id = Convert.ToInt32(CardIdUpdown.Value);
            DrawCard();
        }

        private void CardNameBox_TextChanged(object sender, EventArgs e)
        {
            currentPrintInfo.Title = CardNameBox.Text.ToUpper();
            DrawCard();
        }

        private void ExpansionDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPrintInfo.Expansion = (ExpansionId)ExpansionDropdown.SelectedItem;
            DrawCard();
        }

        private void ClassDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPrintInfo.ClassType = (PlayerClassType)ClassDropdown.SelectedItem;
            DrawCard();
        }

        private void CardPicturePathLoadButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
                {
                    CardImagePath = folderDialog.SelectedPath;
                    Properties.Settings.Default.ImagePath = CardImagePath;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void EffectDescriptionBox_TextChanged(object sender, EventArgs e)
        {
            currentPrintInfo.Text = EffectDescriptionBox.Text;
            DrawCard();
        }

        private void RarityUpDown_ValueChanged(object sender, EventArgs e)
        {
            currentPrintInfo.Rarity = Convert.ToInt32(RarityUpDown.Value);
            DrawCard();
        }
    }
}
