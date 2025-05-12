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
        public override Brush GetBrush()
        {
            Bitmap bitmap = new Bitmap(ImagePath);
            if(WhiteTint != Color.White)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        // Get pixel
                        Color originalColor = bitmap.GetPixel(x, y);
                        if (originalColor == Color.Black) continue; // Black doesn't tint
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
            return brush;
        }
    }
    public static class DrawHelper
    {
        public static FillHelper GetImageBrushOrColor(Rectangle container, string path, Color imageColorTint, Color defaultColor)
        {
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
                    WhiteTint = imageColorTint
                };
            }
            else
            {
                brush = new SolidFillHelper() { FillColor = defaultColor };
            }
            return brush;
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
            foreach (string word in words)
            {
                string[] auxWords = word.Split("\r\n"); // May be multiple new lines here
                for(int i = 0; i< auxWords.Length; i++)
                {
                    WordToDraw theWord = new WordToDraw();
                    if (auxWords[i].StartsWith('*'))
                    {
                        auxWords[i] = auxWords[i].Replace("*","");
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
    }
}
