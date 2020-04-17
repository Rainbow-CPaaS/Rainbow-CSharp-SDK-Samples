using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;

namespace InstantMessaging.Helpers
{
    public class ImageManagement : IImageManagement
    {
        /// <summary>
        /// Draw an source image on destiantion image in (x, y)
        /// </summary>
        /// <param name="msSrc"><see cref="Stream"/>Image Source in Stream format</param>
        /// <param name="msDst"><see cref="Stream"/>Image Destination in Stream format</param>
        /// <param name="xDest"><see cref="int"/>x value</param>
        /// <param name="yDest"><see cref="int"/>y value</param>
        /// <returns><see cref="Stream"/>Image created in Stream format</returns>
        public Stream DrawImage(Stream msSrc, Stream msDst, float xDest, float yDest)
        {
            Stream result;

            using (Bitmap source = new Bitmap(msSrc))
            {
                using (Bitmap destination = new Bitmap(msDst))
                {
                    using (Graphics graphics = Graphics.FromImage(destination))
                        graphics.DrawImage(source, xDest, yDest);

                    result = GetStreamFromBitmap(destination);
                    destination.Dispose();
                }
            }

            // Ensure position of streams provided
            msSrc.Position = 0;
            msDst.Position = 0;

            return result;
        }

        /// <summary>
        /// From image source, get an image part using a arc. 
        /// </summary>
        /// <param name="ms"><see cref="Stream"/>Image Source in Stream format</param>
        /// <param name="partNumber"><see cref="int"/>Number of the part to get</param>
        /// <param name="nbParts"><see cref="int"/>Max. number of parts to have a circle if we add several part</param>
        /// <returns><see cref="Stream"/>Image created in Stream format</returns>
        public Stream GetArcPartFromSquareImage(Stream ms, int partNumber, int nbParts)
        {
            Stream result = null;
            Bitmap avatar = new Bitmap(ms);

            if (avatar != null)
            {
                int diameter = avatar.Width;

                float arcAngle = 360 / nbParts;
                float arcStart = 90 + (partNumber - 1) * arcAngle;
                if (nbParts > 2)
                    arcStart += (180 / nbParts);

                float deltaX = (float)(diameter / 2 - ((diameter * Math.Cos(DegreeToRadian(arcStart + arcAngle / 2)) + diameter) / 2)) / 2;
                float deltaY = (float)(diameter / 2 - ((diameter * Math.Sin(DegreeToRadian(arcStart + arcAngle / 2)) + diameter) / 2)) / 2;

                float startX = (float)((diameter * Math.Cos(DegreeToRadian(arcStart)) + diameter) / 2);
                float startY = (float)((diameter * Math.Sin(DegreeToRadian(arcStart)) + diameter) / 2);

                float endX = (float)((diameter * Math.Cos(DegreeToRadian(arcStart + arcAngle)) + diameter) / 2);
                float endY = (float)((diameter * Math.Sin(DegreeToRadian(arcStart + arcAngle)) + diameter) / 2);

                // First we get the form (we translate using deltaX/Y to have the center of the avatar)
                Bitmap temp = new Bitmap(diameter, diameter);
                using (Graphics graphics = Graphics.FromImage(temp))
                {
                    // To have anti aliasing, we need to use a Path. So we need to use a Texture Brush
                    TextureBrush textureBrush = new TextureBrush(avatar);
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    using (GraphicsPath gp = new GraphicsPath())
                    {
                        gp.AddLine(diameter / 2 + deltaX, diameter / 2 + deltaY, startX + deltaX, startY + deltaY);

                        gp.AddArc(deltaX, deltaY, diameter, diameter, arcStart, arcAngle);

                        gp.AddLine(endX + deltaX, endY + deltaY, diameter / 2 + deltaX, diameter / 2 + deltaY);

                        graphics.FillPath(textureBrush, gp);
                    }

                    avatar.Dispose();
                }

                //// Now we translate back to have correct layout
                result = GetStreamFromBitmap(temp);
                result = GetTranslated(result, -deltaX, -deltaY);

                temp.Dispose();

                // Ensure position of stream provided
                ms.Position = 0;
            }
            return result;
        }

        /// <summary>
        /// Get a value representing the screen density
        /// </summary>
        /// <returns><see cref="double"/>Screen density</returns>
        public double GetDensity()
        {
            // Cf. https://stackoverflow.com/questions/5977445/how-to-get-windows-display-settings 
            if (density == 0)
            {
                density = 1;
                // For Windows 10, version 1607 and more  
                try
                {
                    IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
                    density = GetDpiForWindow(handle) / 96f; // Work for Windows 10, version 1607 and more 
                }
                catch
                {
                    // For older version
                    try
                    {
                        /// Logical pixels inch in X
                        int LOGPIXELSX = 88;

                        IntPtr screen = GetDC(IntPtr.Zero);
                        density = GetDeviceCaps(screen, LOGPIXELSX) / 96;
                        ReleaseDC(IntPtr.Zero, screen);
                    }
                    catch
                    {
                    }
                }
            }
            return density; 
        }

        /// <summary>
        /// Create a filled circle with a centered text
        /// </summary>
        /// <param name="imgSize"><see cref="int"/>Size of the image to create</param>
        /// <param name="rgbCircleColor"><see cref="String"/>Color of the circle in RGB syntax (like "#00FF00")</param>
        /// <param name="txt"><see cref="String"/>Text</param>
        /// <param name="rgbTextColor"><see cref="String"/>Color of the text in RGB syntax (like "#00FF00")</param>
        /// <param name="fontFamilyName"><see cref="String"/>Font family</param>
        /// <param name="fontSize"><see cref="String"/>Font size</param>
        /// <returns><see cref="Stream"/>Image created in Stream format</returns>
        public Stream GetFilledCircleWithCenteredText(int imgSize, string rgbCircleColor, string txt, string rgbTextColor, string fontFamilyName, int fontSize)
        {
            Stream result;
            Bitmap bitmap = new Bitmap(imgSize, imgSize);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                // First create filled rectangle
                SolidBrush transparentBrush = new SolidBrush(ColorTranslator.FromHtml(rgbCircleColor));
                graphics.FillRectangle(transparentBrush, 0, 0, imgSize, imgSize);

                // Draw Initials centered
                Font font = new Font(fontFamilyName, fontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Point);
                Rectangle rectString = new Rectangle(0, 3, imgSize, imgSize); // Delta used --- Could be different according font size ?

                SolidBrush textBrush = new SolidBrush(ColorTranslator.FromHtml(rgbTextColor));
                StringFormat stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                string text;
                if (String.IsNullOrEmpty(txt))
                    text = "?";
                else
                    text = txt.ToUpper();
                graphics.DrawString(text, font, textBrush, rectString, stringFormat);

                textBrush.Dispose();
                font.Dispose();
            }

            result = GetStreamFromBitmap(bitmap);
            bitmap.Dispose();

            return result;
        }

        /// <summary>
        /// Create a rounded image from the square image provided
        /// </summary>
        /// <param name="ms"><see cref="Stream"/>Image Source in Stream format</param>
        /// <returns><see cref="Stream"/>Image created in Stream format</returns>
        public Stream GetRoundedFromSquareImage(Stream stream)
        {
            Stream result = null;
            Bitmap bitmap = new Bitmap(stream);
            if (bitmap != null)
            {
                Bitmap temp = new Bitmap(bitmap.Width, bitmap.Height);

                using (Graphics graphics = Graphics.FromImage(temp))
                {
                    // To have anti aliasing, we need to use a Path. So we need to use a Texture Brush
                    TextureBrush textureBrush = new TextureBrush(bitmap);
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    using (GraphicsPath gp = new GraphicsPath())
                    {
                        gp.AddEllipse(0, 0, bitmap.Width, bitmap.Height);
                        graphics.FillPath(textureBrush, gp);
                    }
                    textureBrush.Dispose();
                }

                result = GetStreamFromBitmap(temp);
                temp.Dispose();

                // Ensure position of stream provided
                stream.Position = 0;
            }

            return result;
        }

        /// <summary>
        /// From image source, get a square and scaled image of the size specified
        /// </summary>
        /// <param name="ms"><see cref="Stream"/>Image Source in Stream format</param>
        /// <param name="imgSize"><see cref="Stream"/>Size of the image to create</param>
        /// <returns><see cref="Stream"/>Image created in Stream format</returns>
        public Stream GetSquareAndScaled(Stream ms, int imgSize)
        {
            return GetScaled(ms, imgSize, imgSize);
        }

        /// <summary>
        /// Resize the image source using width and height
        /// </summary>
        /// <param name="ms"><see cref="Stream"/>Image Source in Stream format</param>
        /// <param name="width"><see cref="int"/>Width</param>
        /// <param name="height"><see cref="int"/>height</param>
        /// <returns><see cref="Stream"/>Image created in Stream format</returns>
        public Stream GetScaled(Stream stream, int imgWidth, int imgHeight)
        {
            Stream result;
            Bitmap avatarOriginal = new Bitmap(stream);
            Bitmap avatar = new Bitmap(imgWidth, imgHeight);
            using (Graphics graphics = Graphics.FromImage(avatar))
            {
                if ((avatarOriginal.Width > imgWidth)
                    || (avatarOriginal.Height > imgHeight))
                {
                    float scale = 1;
                    float x = 0;
                    float y = 0;
                    float width = 0;
                    float height = 0;

                    scale = Math.Min((float) ((float)imgWidth / (float)avatarOriginal.Width), (float) ((float)imgHeight / (float)avatarOriginal.Height) );

                    x = ((float)imgWidth - (avatarOriginal.Width * scale)) / 2;
                    y = ((float)imgHeight - (avatarOriginal.Height * scale)) / 2;
                    width = avatarOriginal.Width * scale;
                    height = avatarOriginal.Height * scale;
                    graphics.DrawImage(avatarOriginal, x, y, width, height);
                }
                else
                {
                    float x = ((float)imgWidth - avatarOriginal.Width) / 2;
                    float y = ((float)imgHeight - avatarOriginal.Height) / 2;
                    graphics.DrawImage(avatarOriginal, x, y);
                }
            }
            // Dispose original avatar
            avatarOriginal.Dispose();

            result = GetStreamFromBitmap(avatar);
            avatar.Dispose();

            // Ensure position of stream provided
            stream.Position = 0;

            // Here we have an avatar in STD size
            return result;
        }

        public System.Drawing.Size GetSize(Stream stream)
        {
            Size result = new Size();
            using (Bitmap avatarOriginal = new Bitmap(stream))
            {
                result = avatarOriginal.Size;
            }
            // Ensure position of stream provided
            stream.Position = 0;

            return result;
        }

        /// <summary>
        /// Get a new bitmap using a translation (x,y) from the image provided
        /// </summary>
        /// <param name="ms"><see cref="Stream"/>Image Source in Stream format</param>
        /// <param name="x"><see cref="float"/>Translation value on X axis</param>
        /// <param name="y"><see cref="float"/>Translation value on Y axis</param>
        /// <returns><see cref="Stream"/>Image created in Stream format</returns>
        public Stream GetTranslated(Stream stream, float x, float y)
        {
            Stream result;
            using (Bitmap source = new Bitmap(stream))
            {
                Bitmap bitmapResult = new Bitmap(source.Width, source.Height);
                using (Graphics graphics = Graphics.FromImage(bitmapResult))
                {
                    graphics.DrawImage(source, x, y);
                }

                result = GetStreamFromBitmap(bitmapResult);
                bitmapResult.Dispose();
            }

            // Ensure position of stream provided
            stream.Position = 0;

            return result;
        }

    #region PRIVATE METHODS

        private float density = 0;

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        private static extern int GetDpiForWindow(IntPtr hWnd);

        private static Stream GetStreamFromBitmap(Bitmap bitmap)
        {
            Stream result = new MemoryStream();

            // Get Stream from bitmap
            bitmap.Save(result, System.Drawing.Imaging.ImageFormat.Png);

            // Return to the start of the stream
            result.Position = 0;

            return result;
        }

        private static float DegreeToRadian(float angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }



    #endregion PRIVATE METHODS

    }
}
