using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

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
        public override Brush GetBrush()
        {
            Image img = Image.FromFile(ImagePath);
            float scaleX = ((float)Width) / img.Width;
            float scaleY = ((float)Height) / img.Height;
            TextureBrush brush = new TextureBrush(img);
            brush.TranslateTransform(StartX, StartY);
            brush.ScaleTransform(scaleX, scaleY);
            return brush;
        }
    }
    public static class DrawHelper
    {
        public static void DrawRoundedRectangle(Graphics g, Rectangle bounds, int radius, Color borderColor, FillHelper filler, int borderWidth)
        {
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
                using (Pen pen = new Pen(borderColor, borderWidth))
                {
                    pen.Alignment = PenAlignment.Inset;
                    g.DrawPath(pen, path);
                }
            }
        }
        public static void DrawTextBox(Graphics g, string text, Rectangle bounds, Font font, Color textColor, Color borderColor, int borderWidth, StringAlignment hAlignment, StringAlignment vAlignment, int alignmentSpace, bool autoFontHorizontalFit)
        {
            string[] textToPrint = [];
            SizeF[] textSizes = [];

            Font autoFont = font;
            if (autoFontHorizontalFit)
            {
                textToPrint = text.Split("\r\n"); // Get all lines
                textSizes = new SizeF[textToPrint.Length];
                // Now, find a size so that every line fits in a chunk of the bounding box
                float maxX = 0, maxY = 0;
                for(int i = 0; i < textToPrint.Length; i++)
                {
                    textSizes[i] = g.MeasureString(textToPrint[i], autoFont);
                    maxX = Math.Max(maxX, textSizes[i].Width);
                    maxY = Math.Max(maxY, textSizes[i].Height);
                }                
                float fontSize = autoFont.Size;
                // Reduce font size until text fits into all bounding boxes
                while (maxX > bounds.Width || maxY > bounds.Height / textToPrint.Length)
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
            }
            else // Instead I use the algorithm to auto break lines horizontally to fit within bounds
            {
                string[] words = text.Split(' '); // Split text into words
                StringBuilder formattedText = new StringBuilder();
                float currentWidth = 0;
                float spaceWidth = g.MeasureString(" ", font).Width;

                foreach (string word in words)
                {
                    string[] auxWords = word.Split("\r\n"); // May be multiple new lines here
                    for(int i = 0; i< auxWords.Length; i++)
                    {
                        SizeF wordSize = g.MeasureString(auxWords[i], autoFont);
                        // If adding this word would exceed maxWidth, start a new line
                        // Also this may have been result of a newline so need to make sure to do so if split more than once
                        if ((i > 0) || (currentWidth + wordSize.Width > bounds.Width))
                        {
                            formattedText.Append("\n");  // Insert newline
                            currentWidth = 0;  // Reset width tracking
                        }
                        formattedText.Append(auxWords[i]); // Add word
                        currentWidth += wordSize.Width;
                        if((i + 1) == auxWords.Length) // Means this is the last word of a possible multiline
                        {
                            formattedText.Append(" ");
                            currentWidth += spaceWidth;
                        }
                    }
                }
                textToPrint = [formattedText.ToString().Trim()]; // Returns formatted dingle text with newlines
                textSizes = [g.MeasureString(textToPrint[0], autoFont)]; // One last measure to know the final width/height 
            }
            // Formatted text, either one single line or multiple centered lines
            GraphicsPath path = new GraphicsPath(); // Create path to draw
            for (int i = 0; i < textToPrint.Length; i++)
            {
                float textX = hAlignment switch
                {
                    StringAlignment.Center => bounds.X + (bounds.Width - textSizes[i].Width) / 2,
                    StringAlignment.Near => bounds.X + alignmentSpace,
                    StringAlignment.Far => bounds.X + (bounds.Width - textSizes[i].Width - alignmentSpace),
                    _ => throw new Exception("Incorrect alignment")
                };
                float textY = vAlignment switch // If multi strings, this is done in chunks
                {
                    StringAlignment.Center => bounds.Y + (i * bounds.Height / textToPrint.Length) + ((bounds.Height / textToPrint.Length) - textSizes[i].Height) / 2,
                    StringAlignment.Near => bounds.Y + (i * bounds.Height / textToPrint.Length) + alignmentSpace,
                    StringAlignment.Far => bounds.Y + (i * bounds.Height / textToPrint.Length) + (bounds.Height - textSizes[i].Height - alignmentSpace),
                    _ => throw new Exception("Incorrect alignment")
                };
                // Creates the string in correct pixels
                path.AddString(textToPrint[i], autoFont.FontFamily, (int)autoFont.Style, g.DpiY * autoFont.SizeInPoints / 72, new PointF(textX, textY), StringFormat.GenericDefault);
            } // Obtained path
            // Fill text
            using (Brush brush = new SolidBrush(textColor))
            {
                g.FillPath(brush, path);
            }
            // Draw border on top (because it's inset)
            using (Pen pen = new Pen(borderColor, borderWidth))
            {
                pen.Alignment = PenAlignment.Inset;
                g.DrawPath(pen, path);
            }
            // Draw textbox (debug)
            //using (Pen pen = new Pen(textColor, 5))
            //{
            //    pen.Alignment = PenAlignment.Inset;
            //    for (int i = 0; i < textToPrint.Length; i++)
            //    {
            //        Rectangle bound = new Rectangle(bounds.X, bounds.Y + (i * bounds.Height / textToPrint.Length), bounds.Width, bounds.Height / textToPrint.Length);
            //        g.DrawRectangle(pen, bound);
            //    }
            //}
        }
    }
}
