using ODLGameEngine;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using static CardGenerationHelper.CardGenerator;

namespace CardGenerationHelper
{
    public abstract class FillHelper
    {
        public abstract Brush GetBrush();
    }
    public class SolidFillHelper : FillHelper
    {
        public Color FillColor = Color.White;
        public override Brush GetBrush()
        {
            return new SolidBrush(FillColor);
        }
    }
    public class ImageFillHelper : FillHelper
    {
        public string ImagePath = "";
        public int Width = 0;
        public int Height = 0;
        public int StartX = 0;
        public int StartY = 0;
        public Color WhiteTint = Color.White;
        public int BlackLighten = 0;
        TextureBrush _brush = null;
        public override Brush GetBrush()
        {
            if (_brush != null) { return _brush; }
            Bitmap bitmap = new Bitmap(ImagePath);
            if(WhiteTint != Color.White)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        // Get pixel
                        Color originalColor = bitmap.GetPixel(x, y);
                        if (originalColor.R == 0 && originalColor.G == 0 && originalColor.B == 0) // I black, special case
                        {
                            if (BlackLighten != 0)
                            {
                                originalColor = Color.FromArgb(BlackLighten, BlackLighten, BlackLighten);
                            }
                            else // Black stays black
                            {
                                continue;
                            }
                        }
                        // Tint it
                        Color newColor = Color.FromArgb(WhiteTint.R * originalColor.R/255, WhiteTint.G * originalColor.G / 255, WhiteTint.B * originalColor.B / 255);
                        // Re-apply
                        bitmap.SetPixel(x, y, newColor);
                    }
                }
            }
            float scaleX = ((float)Width) / bitmap.Width;
            float scaleY = ((float)Height) / bitmap.Height;
            TextureBrush brush = new TextureBrush(bitmap);
            brush.TranslateTransform(StartX, StartY);
            brush.ScaleTransform(scaleX, scaleY);
            _brush = brush;
            return brush;
        }
    }
    public static class DrawHelper
    {
        public static Dictionary<string, FillHelper> _savedBrushes = new Dictionary<string, FillHelper>();
        public static FillHelper GetImageBrushOrColor(Rectangle container, string path, Color imageColorTint, Color defaultColor, int blackLighten = 0)
        {
            if(_savedBrushes.ContainsKey(path)) return _savedBrushes[path];
            FillHelper brush;
            if (Path.Exists(path)) // Check if image exists
            {
                brush = new ImageFillHelper()
                {
                    ImagePath = path,
                    Height = container.Height,
                    Width = container.Width,
                    StartX = container.X,
                    StartY = container.Y,
                    WhiteTint = imageColorTint,
                    BlackLighten = blackLighten
                };
                _savedBrushes.Add(path, brush);
            }
            else
            {
                brush = new SolidFillHelper() { FillColor = defaultColor };
            }
            return brush;
        }
        public static void DrawRectangleFixedBorder(Graphics g, Rectangle bounds, Color borderColor, int border, FillHelper filler)
        {
            int relevantSide = Math.Min(bounds.Width, bounds.Height);
            GraphicsPath path = new GraphicsPath();
            path.AddRectangle(bounds);
            g.FillPath(filler.GetBrush(), path);
            Pen pen = new Pen(borderColor, border);
            pen.Alignment = PenAlignment.Inset;
            g.DrawPath(pen, path);
        }
        public static void DrawRoundedRectangle(Graphics g, Rectangle bounds, float radiusPercentage, Color borderColor, float borderWidthPercentage, FillHelper filler)
        {
            int relevantSide = Math.Min(bounds.Width, bounds.Height);
            int radius = (int)(relevantSide * radiusPercentage);
            int border = (int)(relevantSide * borderWidthPercentage);
            using (GraphicsPath path = new GraphicsPath())
            {
                // Create the rounded corners
                path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
                path.AddArc(bounds.Right - radius, bounds.Y, radius, radius, 270, 90);
                path.AddArc(bounds.Right - radius, bounds.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(bounds.X, bounds.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();

                // Fill the rectangle
                g.FillPath(filler.GetBrush(), path);
                
                // Draw the border
                using (Pen pen = new Pen(borderColor, border))
                {
                    pen.Alignment = PenAlignment.Inset;
                    g.DrawPath(pen, path);
                }
            }
        }
        /// <summary>
        /// Like AutoFit but fixed text size and no weird line break thing
        /// </summary>
        /// <param name="g"></param>
        /// <param name="text"></param>
        /// <param name="textBox"></param>
        /// <param name="font"></param>
        /// <param name="textColor"></param>
        /// <param name="borderColor"></param>
        /// <param name="borderWidth"></param>
        /// <param name="hAlignment"></param>
        /// <param name="vAlignment"></param>
        /// <param name="alignmentSpace"></param>
        /// <param name="debug"></param>
        /// <exception cref="Exception"></exception>
        public static void DrawFixedText(Graphics g, string text, Rectangle textBox, Font font, Color textColor, Color borderColor, float borderWidth, StringAlignment hAlignment, StringAlignment vAlignment, int alignmentSpace = 0, bool debug = false)
        {
            SizeF textSize = g.MeasureString(text, font);
            // Formatted text, either one single line or multiple centered lines
            GraphicsPath path = new GraphicsPath(); // Create path to draw
            float textX = hAlignment switch
            {
                StringAlignment.Center => textBox.X + (textBox.Width - textSize.Width) / 2,
                StringAlignment.Near => textBox.X + alignmentSpace,
                StringAlignment.Far => textBox.X + (textBox.Width - textSize.Width - alignmentSpace),
                _ => throw new Exception("Incorrect alignment")
            };
            float textY = vAlignment switch // If multi strings, this is done in chunks
            {
                StringAlignment.Center => textBox.Y + (textBox.Height - textSize.Height) / 2,
                StringAlignment.Near => textBox.Y + alignmentSpace,
                StringAlignment.Far => textBox.Y + (textBox.Height - textSize.Height - alignmentSpace),
                _ => throw new Exception("Incorrect alignment")
            };
            // Creates the string in correct pixels
            path.AddString(text, font.FontFamily, (int)font.Style, g.DpiY * font.SizeInPoints / 72, new PointF(textX, textY), StringFormat.GenericDefault);
            // Obtained path
            // Draw border as outset
            borderWidth *= font.Size;
            using (Pen pen = new Pen(borderColor, borderWidth))
            {
                pen.Alignment = PenAlignment.Outset;
                g.DrawPath(pen, path);
            }
            // Then fill text
            using (Brush brush = new SolidBrush(textColor))
            {
                g.FillPath(brush, path);
            }
            if (debug)
            {
                // Draw textbox (debug)
                using (Pen pen = new Pen(textColor, 5))
                {
                    pen.Alignment = PenAlignment.Inset;
                    g.DrawRectangle(pen, textBox);
                }
            }
        }
        /// <summary>
        /// Draws text that auto fits on a text box, also allows borders and alignments (for most standalone floating words)
        /// </summary>
        /// <param name="g"></param>
        /// <param name="text"></param>
        /// <param name="textBox"></param>
        /// <param name="font"></param>
        /// <param name="textColor"></param>
        /// <param name="borderColor"></param>
        /// <param name="borderWidth"></param>
        /// <param name="hAlignment"></param>
        /// <param name="vAlignment"></param>
        /// <param name="alignmentSpace"></param>
        /// /// <param name="debug"></param>
        /// <exception cref="Exception"></exception>
        public static void DrawAutoFitText(Graphics g, string text, Rectangle textBox, Font font, Color textColor, Color borderColor, float borderWidth, StringAlignment hAlignment, StringAlignment vAlignment, int alignmentSpace = 0, bool debug = false)
        {
            string[] textToPrint = [];
            SizeF[] textSizes = [];

            Font autoFont = font;
            textToPrint = text.Split("\r\n"); // Get all lines
            textSizes = new SizeF[textToPrint.Length];
            // Now, find a size so that every line fits in a chunk of the bounding box
            float maxX = 0, maxY = 0;
            for (int i = 0; i < textToPrint.Length; i++)
            {
                textSizes[i] = g.MeasureString(textToPrint[i], autoFont);
                maxX = Math.Max(maxX, textSizes[i].Width);
                maxY = Math.Max(maxY, textSizes[i].Height);
            }
            float fontSize = autoFont.Size;
            // Reduce font size until text fits into all bounding boxes
            while (maxX > textBox.Width || maxY > textBox.Height / textToPrint.Length)
            {
                fontSize -= 1; // Reduce size step-by-step
                autoFont = new Font(autoFont.FontFamily, fontSize, autoFont.Style);
                maxX = 0; maxY = 0;
                for (int i = 0; i < textToPrint.Length; i++)
                {
                    textSizes[i] = g.MeasureString(textToPrint[i], autoFont);
                    maxX = Math.Max(maxX, textSizes[i].Width);
                    maxY = Math.Max(maxY, textSizes[i].Height);
                }
            }
            // Formatted text, either one single line or multiple centered lines
            GraphicsPath path = new GraphicsPath(); // Create path to draw
            for (int i = 0; i < textToPrint.Length; i++)
            {
                float textX = hAlignment switch
                {
                    StringAlignment.Center => textBox.X + (textBox.Width - textSizes[i].Width) / 2,
                    StringAlignment.Near => textBox.X + alignmentSpace,
                    StringAlignment.Far => textBox.X + (textBox.Width - textSizes[i].Width - alignmentSpace),
                    _ => throw new Exception("Incorrect alignment")
                };
                float textY = vAlignment switch // If multi strings, this is done in chunks
                {
                    StringAlignment.Center => textBox.Y + (i * textBox.Height / textToPrint.Length) + ((textBox.Height / textToPrint.Length) - textSizes[i].Height) / 2,
                    StringAlignment.Near => textBox.Y + (i * textBox.Height / textToPrint.Length) + alignmentSpace,
                    StringAlignment.Far => textBox.Y + (i * textBox.Height / textToPrint.Length) + (textBox.Height - textSizes[i].Height - alignmentSpace),
                    _ => throw new Exception("Incorrect alignment")
                };
                // Creates the string in correct pixels
                path.AddString(textToPrint[i], autoFont.FontFamily, (int)autoFont.Style, g.DpiY * autoFont.SizeInPoints / 72, new PointF(textX, textY), StringFormat.GenericDefault);
            } // Obtained path
            // Draw border as outset
            borderWidth *= fontSize;
            using (Pen pen = new Pen(borderColor, borderWidth))
            {
                pen.Alignment = PenAlignment.Outset;
                g.DrawPath(pen, path);
            }
            // Then fill text
            using (Brush brush = new SolidBrush(textColor))
            {
                g.FillPath(brush, path);
            }
            if (debug)
            {
                // Draw textbox (debug)
                using (Pen pen = new Pen(textColor, 5))
                {
                    pen.Alignment = PenAlignment.Inset;
                    for (int i = 0; i < textToPrint.Length; i++)
                    {
                        Rectangle bound = new Rectangle(textBox.X, textBox.Y + (i * textBox.Height / textToPrint.Length), textBox.Width, textBox.Height / textToPrint.Length);
                        g.DrawRectangle(pen, bound);
                    }
                }
            }
        }
        public class WordToDraw
        {
            public float X = 0f;
            public float Y = 0f;
            public string Word = "";
            public Font Font = null;
        }
        /// <summary>
        /// Draws a text box, doesnt handle centering or auto-size but handles some rich markup
        /// E.g. if you use * it allows bold. More to be used later if needed
        /// </summary>
        /// <param name="g"></param>
        /// <param name="text"></param>
        /// <param name="bounds"></param>
        /// <param name="font"></param>
        /// <param name="textColor"></param>
        /// <param name="hAlignmentSpace"></param>
        /// <param name="vAlignmentSpace"></param>
        /// <exception cref="Exception"></exception>
        public static void DrawRichTextBox(Graphics g, string text, Rectangle bounds, Font font, Color textColor, int hAlignmentSpace, int vAlignmentSpace, bool debug)
        {
            // Get sizes
            Font regularFont = new Font(font, FontStyle.Regular);
            Font boldFont = new Font(font, FontStyle.Bold);
            float lineSize = Math.Max(regularFont.GetHeight(), boldFont.GetHeight());
            // Find out where every word is placed
            List<WordToDraw> wordsToDraw = new List<WordToDraw>();
            string[] words = text.Split(' '); // Split text into all words
            int currentLine = 0;
            float currentLineWidth = 0;
            bool nextWordToggleBold = false;
            char? currentBoldChar = null;
            foreach (string word in words)
            {
                string[] auxWords = word.Split("\r\n"); // May be multiple new lines here
                for(int i = 0; i< auxWords.Length; i++)
                {
                    WordToDraw theWord = new WordToDraw();
                    if(currentBoldChar == null)
                    {   
                        if (auxWords[i].StartsWith('*'))
                        {
                            currentBoldChar = '*';
                        }
                        else if (auxWords[i].StartsWith('#'))
                        {
                            currentBoldChar = '#';
                        }
                        if(currentBoldChar != null) // Theres a bold char, remove it
                        {
                            auxWords[i] = auxWords[i].Remove(0,1);
                        }
                    }
                    if (currentBoldChar != null)
                    {
                        if(nextWordToggleBold) // Ok its stopped
                        {
                            nextWordToggleBold = false;
                            currentBoldChar = null;
                        }
                        else if (auxWords[i].EndsWith(currentBoldChar.Value))
                        {
                            auxWords[i] = auxWords[i].Replace(currentBoldChar.Value.ToString(), "");
                            nextWordToggleBold = true;
                        }
                    }
                    if (currentBoldChar != null) // Currently bolding...
                    {
                        theWord.Font = boldFont;
                    }
                    else
                    {
                        theWord.Font = regularFont;
                    }
                    SizeF wordSize = g.MeasureString(auxWords[i], theWord.Font);
                    // If adding this word would exceed maxWidth, start a new line
                    // Also this may have been result of a newline so need to make sure to do so if split more than once
                    if ((i > 0) || ((currentLineWidth + wordSize.Width) > (bounds.Width - 2 * hAlignmentSpace))) // 2* alignment because words escape slightly
                    {
                        currentLineWidth = 0;  // Reset width tracking
                        currentLine++;
                    }
                    theWord.Word += auxWords[i]; // Add word
                    theWord.X = currentLineWidth; // Add X location
                    theWord.Y = currentLine * lineSize; // Add Y location
                    wordsToDraw.Add(theWord);
                    currentLineWidth += wordSize.Width;
                }
            }

            // Formatted text, print word by word in right place
            float textXStart = bounds.X + hAlignmentSpace;
            float textYStart = bounds.Y + vAlignmentSpace;
            GraphicsPath path = new GraphicsPath(); // Create path to draw
            foreach (WordToDraw wordToDraw in wordsToDraw)
            {
                // Creates the string in correct pixels
                path.AddString(wordToDraw.Word, wordToDraw.Font.FontFamily, (int)wordToDraw.Font.Style, g.DpiY * wordToDraw.Font.SizeInPoints / 72, new PointF(textXStart + wordToDraw.X, textYStart + wordToDraw.Y), StringFormat.GenericDefault);
            } // Obtained path
            // Fill texts
            using (Brush brush = new SolidBrush(textColor))
            {
                g.FillPath(brush, path);
            }
            // Finally draw textbox (debug)
            if (debug)
            {
                using (Pen pen = new Pen(textColor, 5))
                {
                    pen.Alignment = PenAlignment.Inset;
                    g.DrawRectangle(pen, bounds);
                }
            }
        }
        /// <summary>
        /// Contains all constants for how a card looks
        /// </summary>
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
        /// <summary>
        /// Draws a complete card and returns a bitmap
        /// </summary>
        /// <param name="cardInfo">The info containing all print info needed for the card</param>
        /// <param name="resourcesPath">Folder where to find icons and shit</param>
        /// <param name="debug">Whether to draw debug borders n stuff</param>
        /// <returns>The new bitmap</returns>
        public static Bitmap DrawCard(CardIllustrationInfo cardInfo, string resourcesPath, bool debug = false)
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
            DrawRoundedRectangle(g, bounds, DrawConstants.CardRoundedPercentage, Color.Black, DrawConstants.CardBorder, new SolidFillHelper() { FillColor = Color.LightGray });

            // Then, all non-invalid cards have picture, cost, name, textbox, rarity, etc
            if (cardInfo.EntityType != EntityType.NONE)
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
                string imagePath = Path.Combine(resourcesPath, "CardImagesRaw", cardInfo.Id.ToString() + ".png");
                Rectangle imageBox = new Rectangle((int)currentDrawPointerX, (int)currentDrawPointerY, (int)imageBoxSize, (int)imageBoxSize);
                brush = GetImageBrushOrColor(imageBox, imagePath, Color.White, Color.White);
                DrawRoundedRectangle(g, imageBox, DrawConstants.BoxRoundedPercentage, Color.Black, DrawConstants.ImageBorder, brush);
                // Now Draw all Stats
                // Gold
                float statXpointer = currentDrawPointerX + imageBoxSize + horizontalMargin;
                float statYpointer = currentDrawPointerY;
                Rectangle statBox = new Rectangle((int)statXpointer, (int)statYpointer, (int)statWidth, (int)statWidth);
                imagePath = Path.Combine(resourcesPath, "CardLayoutElements", "gold.png");
                brush = GetImageBrushOrColor(statBox, imagePath, Color.Gold, Color.Gold, 85);
                DrawRoundedRectangle(g, statBox, DrawConstants.StatRoundedPercentage, Color.Black, DrawConstants.ImageBorder, brush);
                float statFontSize = statWidth / 1.333f; // Fixed size to fit stat box in consistent way. 1.333 is empirical
                Font statFont = new Font("Coolvetica Heavy Comp", statFontSize, FontStyle.Bold);
                DrawFixedText(g, cardInfo.Cost, statBox, statFont, Color.White, Color.Black, DrawConstants.StatFontBorderPercentage, StringAlignment.Center, StringAlignment.Center, 0, debug);
                statYpointer += statWidth + verticalMargin;
                // Rest of stats require a specific card
                if (cardInfo.EntityType == EntityType.UNIT || cardInfo.EntityType == EntityType.BUILDING || cardInfo.EntityType == EntityType.PLAYER) // Entities with HP
                {
                    // Then, HP
                    statBox = new Rectangle((int)statXpointer, (int)statYpointer, (int)statWidth, (int)statWidth);
                    imagePath = Path.Combine(resourcesPath, "CardLayoutElements", "hp.png");
                    brush = GetImageBrushOrColor(statBox, imagePath, Color.Red, Color.Red, 85);
                    DrawRoundedRectangle(g, statBox, DrawConstants.StatRoundedPercentage, Color.Black, DrawConstants.ImageBorder, brush);
                    DrawFixedText(g, cardInfo.Hp, statBox, statFont, Color.White, Color.Black, DrawConstants.StatFontBorderPercentage, StringAlignment.Center, StringAlignment.Center, 0, debug);
                    statYpointer += statWidth + verticalMargin;
                    if (cardInfo.EntityType == EntityType.UNIT) // Units will also have attack and mvt
                    {
                        statBox = new Rectangle((int)statXpointer, (int)statYpointer, (int)statWidth, (int)statWidth);
                        imagePath = Path.Combine(resourcesPath, "CardLayoutElements", "attack.png");
                        brush = GetImageBrushOrColor(statBox, imagePath, Color.Silver, Color.Silver, 85);
                        DrawRoundedRectangle(g, statBox, DrawConstants.StatRoundedPercentage, Color.Black, DrawConstants.ImageBorder, brush);
                        DrawFixedText(g, cardInfo.Attack, statBox, statFont, Color.White, Color.Black, DrawConstants.StatFontBorderPercentage, StringAlignment.Center, StringAlignment.Center, 0, debug);
                        statYpointer += statWidth + verticalMargin;
                        statBox = new Rectangle((int)statXpointer, (int)statYpointer, (int)statWidth, (int)statWidth);
                        imagePath = Path.Combine(resourcesPath, "CardLayoutElements", "movement.png");
                        brush = GetImageBrushOrColor(statBox, imagePath, Color.BurlyWood, Color.BurlyWood, 85);
                        DrawRoundedRectangle(g, statBox, DrawConstants.StatRoundedPercentage, Color.Black, DrawConstants.ImageBorder, brush);
                        DrawFixedText(g, cardInfo.Movement, statBox, statFont, Color.White, Color.Black, DrawConstants.StatFontBorderPercentage, StringAlignment.Center, StringAlignment.Center, 0, debug);
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
                DrawAutoFitText(g, cardInfo.Name, nameBox, titleFont, Color.Black, Color.White, DrawConstants.StatFontBorderPercentage, StringAlignment.Center, StringAlignment.Center, 0, debug);
                currentDrawPointerY += titleAreaHeight; // Move down to the next part
                // Effect:
                Rectangle textBox = new Rectangle((int)currentDrawPointerX, (int)currentDrawPointerY, (int)drawableWidth, (int)textAreaHeight);
                brush = new SolidFillHelper() { FillColor = Color.FromArgb((int)(255 * DrawConstants.TextBoxOpacity), Color.White) }; // Semi transparent white box
                DrawRoundedRectangle(g, textBox, DrawConstants.BoxRoundedPercentage, Color.Black, DrawConstants.TextBoxBorder, brush);
                float textFontSize = textAreaHeight / (DrawConstants.TextSizeDivider * 1.33333f); // 1.333 because text is in pixels and I need pt
                Font textFont = new Font("Georgia", textFontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                int minTextBoxSize = Math.Min((int)drawableWidth, (int)textAreaHeight);
                DrawRichTextBox(g, cardInfo.Text, textBox, textFont, Color.Black, (int)(minTextBoxSize * DrawConstants.TextBoxMargin), (int)(minTextBoxSize * DrawConstants.TextBoxMargin), debug);
                currentDrawPointerY += textAreaHeight; // Move down to the next part
                // Extras:
                Rectangle extrasBox = new Rectangle((int)currentDrawPointerX, (int)currentDrawPointerY, (int)drawableWidth, (int)extraAreaHeight);
                float extraFontSize = extraAreaHeight;
                Font extraFont = new Font("Georgia", extraFontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                DrawAutoFitText(g, $"#{cardInfo.Id}", extrasBox, textFont, Color.Black, Color.White, DrawConstants.StatFontBorderPercentage, StringAlignment.Far, StringAlignment.Center, (int)(drawableWidth * DrawConstants.ExtraBoxMargins), debug);
                string rarityString = new string('\u2605', cardInfo.Rarity);
                DrawAutoFitText(g, rarityString, extrasBox, textFont, Color.Black, Color.White, DrawConstants.StatFontBorderPercentage, StringAlignment.Near, StringAlignment.Center, (int)(drawableWidth * DrawConstants.ExtraBoxMargins), debug);
            }
            return bitmap;
        }
        /// <summary>
        /// Draws building's blueprint. Unfortunately it needs extra draw info from the actual building itself
        /// </summary>
        /// <param name="printInfo">Print info of building</param>
        /// <param name="buildingInfo">Building data for the BPs</param>
        /// <param name="resourcesPath">Where the graphic resources are</param>
        /// <param name="debug">Debug flag</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Bitmap DrawBlueprint(CardIllustrationInfo printInfo, Building buildingInfo, string resourcesPath, bool debug = false)
        {
            Bitmap bitmap = null;
            string imagePath = Path.Combine(resourcesPath, "CardLayoutElements", "blueprint.png");
            if (File.Exists(imagePath)) // Blueprint base found
            {
                bitmap = new Bitmap(imagePath);
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
                DrawRectangleFixedBorder(g, rotuloBox, Color.White, DrawConstants.RotuloBorderSize, transparentBrush);
                Rectangle rotuloTitle = new Rectangle(xRotulo, yRotulo, (int)(widthRotulo * (1 - DrawConstants.RotuloRightSize)), heightRotulo);
                Font rotuloFont = new Font("Consolas", heightRotulo);
                DrawAutoFitText(g, printInfo.Name, rotuloTitle, rotuloFont, Color.White, Color.White, 0, StringAlignment.Center, StringAlignment.Center, 0, debug);
                xRotulo += (int)(widthRotulo * (1 - DrawConstants.RotuloRightSize));
                widthRotulo = (int)(widthRotulo * DrawConstants.RotuloRightSize);
                rotuloBox = new Rectangle(xRotulo, yRotulo, widthRotulo, heightRotulo);
                DrawRectangleFixedBorder(g, rotuloBox, Color.White, DrawConstants.RotuloBorderSize, transparentBrush);
                rotuloFont = new Font("Consolas", (int)(heightRotulo * (1 - DrawConstants.RotuloRightSizeBottom)));
                Rectangle rotuloRightTextBox = new Rectangle(xRotulo, yRotulo, widthRotulo, (int)(heightRotulo * (1 - DrawConstants.RotuloRightSizeBottom)));
                DrawAutoFitText(g, "#" + printInfo.Id, rotuloRightTextBox, rotuloFont, Color.White, Color.White, 0, StringAlignment.Center, StringAlignment.Center, 0, debug);
                yRotulo += (int)(heightRotulo * (1 - DrawConstants.RotuloRightSizeBottom));
                heightRotulo = (int)(heightRotulo * DrawConstants.RotuloRightSizeBottom);
                rotuloBox = new Rectangle(xRotulo, yRotulo, widthRotulo, heightRotulo);
                DrawRectangleFixedBorder(g, rotuloBox, Color.White, DrawConstants.RotuloBorderSize, transparentBrush);
                rotuloFont = new Font("Georgia", heightRotulo, FontStyle.Regular, GraphicsUnit.Pixel);
                string rarityString = new string('\u2605', printInfo.Rarity);
                DrawAutoFitText(g, rarityString, rotuloBox, rotuloFont, Color.White, Color.White, 0, StringAlignment.Center, StringAlignment.Center, 0, debug);
                // End of rotulo now the actual matrix
                int yMap = (int)(height * DrawConstants.mapHStart);
                int heightMap = (int)(height * (DrawConstants.mapHEnd - DrawConstants.mapHStart));
                int widthMap = (int)(width * DrawConstants.mapWidth);
                int xMap = (width - widthMap) / 2;
                if (debug)
                {
                    Rectangle mapDebugBox = new Rectangle(xMap, yMap, widthMap, heightMap);
                    DrawRectangleFixedBorder(g, mapDebugBox, Color.White, DrawConstants.RotuloBorderSize, transparentBrush);
                }
                Rectangle getCoordinateRectangle(int row, int column) // Gives me the desired tile
                {
                    // First, calculate coord
                    int nrows = GameConstants.BOARD_NUMBER_OF_LANES;
                    int nCols = new[] { GameConstants.PLAINS_NUMBER_OF_TILES, GameConstants.FOREST_NUMBER_OF_TILES, GameConstants.MOUNTAIN_NUMBER_OF_TILES }.Max();
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
                    int maxCol = new[] { GameConstants.PLAINS_NUMBER_OF_TILES, GameConstants.FOREST_NUMBER_OF_TILES, GameConstants.MOUNTAIN_NUMBER_OF_TILES }.Max();
                    int laneSize = lane switch
                    {
                        LaneID.PLAINS => GameConstants.PLAINS_NUMBER_OF_TILES,
                        LaneID.FOREST => GameConstants.FOREST_NUMBER_OF_TILES,
                        LaneID.MOUNTAIN => GameConstants.MOUNTAIN_NUMBER_OF_TILES,
                        _ => throw new Exception("Not a lane")
                    };
                    int offset = (maxCol - laneSize) / 2;
                    return row + offset;
                }
                // Ok now I need to plot stuff, plot lane by lane
                for (int i = 0; i < GameConstants.PLAINS_NUMBER_OF_TILES; i++)
                {
                    Rectangle rect = getCoordinateRectangle(0, getAdaptedColumn(i, LaneID.PLAINS));
                    DrawRoundedRectangle(g, rect, DrawConstants.tileRounded, Color.White, DrawConstants.tileBorder, transparentBrush);
                }
                for (int i = 0; i < GameConstants.FOREST_NUMBER_OF_TILES; i++)
                {
                    Rectangle rect = getCoordinateRectangle(1, getAdaptedColumn(i, LaneID.FOREST));
                    DrawRoundedRectangle(g, rect, DrawConstants.tileRounded, Color.White, DrawConstants.tileBorder, transparentBrush);
                }
                for (int i = 0; i < GameConstants.MOUNTAIN_NUMBER_OF_TILES; i++)
                {
                    Rectangle rect = getCoordinateRectangle(2, getAdaptedColumn(i, LaneID.MOUNTAIN));
                    DrawRoundedRectangle(g, rect, DrawConstants.tileRounded, Color.White, DrawConstants.tileBorder, transparentBrush);
                }
                // And now the actual BP
                int[] bp = buildingInfo.PlainsBp;
                if (bp != null)
                {
                    for (int i = 0; i < bp.Length; i++)
                    {
                        Rectangle rect = getCoordinateRectangle(0, getAdaptedColumn(bp[i], LaneID.PLAINS));
                        DrawRoundedRectangle(g, rect, DrawConstants.tileRounded, Color.White, DrawConstants.tileBorder, new SolidFillHelper() { FillColor = Color.White });
                        float bpFontSize = rect.Height / 1.333f; // Fixed size to fit BP tile in consistent way. 1.333 is empirical
                        Font bpFont = new Font("Consolas", bpFontSize, FontStyle.Bold);
                        DrawAutoFitText(g, (i + 1).ToString(), rect, bpFont, Color.FromArgb(69, 134, 202), Color.Black, 0, StringAlignment.Center, StringAlignment.Center, 0, debug);
                    }
                }
                bp = buildingInfo.ForestBp;
                if (bp != null)
                {
                    for (int i = 0; i < bp.Length; i++)
                    {
                        Rectangle rect = getCoordinateRectangle(1, getAdaptedColumn(bp[i], LaneID.FOREST));
                        DrawRoundedRectangle(g, rect, DrawConstants.tileRounded, Color.White, DrawConstants.tileBorder, new SolidFillHelper() { FillColor = Color.White });
                        float bpFontSize = rect.Height / 1.333f; // Fixed size to fit BP tile in consistent way. 1.333 is empirical
                        Font bpFont = new Font("Consolas", bpFontSize, FontStyle.Bold);
                        DrawAutoFitText(g, (i + 1).ToString(), rect, bpFont, Color.FromArgb(69, 134, 202), Color.Black, 0, StringAlignment.Center, StringAlignment.Center, 0, debug);
                    }
                }
                bp = buildingInfo.MountainBp;
                if (bp != null)
                {
                    for (int i = 0; i < bp.Length; i++)
                    {
                        Rectangle rect = getCoordinateRectangle(2, getAdaptedColumn(bp[i], LaneID.MOUNTAIN));
                        DrawRoundedRectangle(g, rect, DrawConstants.tileRounded, Color.White, DrawConstants.tileBorder, new SolidFillHelper() { FillColor = Color.White });
                        float bpFontSize = rect.Height / 1.333f; // Fixed size to fit BP tile in consistent way. 1.333 is empirical
                        Font bpFont = new Font("Consolas", bpFontSize, FontStyle.Bold);
                        DrawAutoFitText(g, (i + 1).ToString(), rect, bpFont, Color.FromArgb(69, 134, 202), Color.Black, 0, StringAlignment.Center, StringAlignment.Center, 0, debug);
                    }
                }
                // Draw line now
                Pen dashedPen = new Pen(Color.White, DrawConstants.dashedLineSize);
                dashedPen.DashStyle = DashStyle.Dash;
                g.DrawLine(dashedPen, new Point(xMap + (widthMap / 2), yMap), new Point(xMap + (widthMap / 2), yMap + heightMap));    
            }
            return bitmap;
        }
    }
}
