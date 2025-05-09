using System.Drawing;
using System.Drawing.Drawing2D;

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
        public override Brush GetBrush()
        {
            Image img = Image.FromFile(ImagePath);
            return new TextureBrush(img);
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
    }
}
