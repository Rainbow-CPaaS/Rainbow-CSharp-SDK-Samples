using System;

using Xamarin.Essentials;
using Rainbow.Helpers;
using System.IO;
using Foundation;
using UIKit;
using System.Drawing.Drawing2D;
using CoreGraphics;
using System.Drawing;

namespace InstantMessaging.iOS.Helpers
{
    public class ImageManagement : IImageManagement
    {
        private double density = 0;

        public Stream DrawImage(Stream msSrc, Stream msDst, float xDest, float yDest)
        {
            using (CGImage src = UIImage.LoadFromData(NSData.FromStream(msSrc)).CGImage)
            {
                using (CGImage dst = UIImage.LoadFromData(NSData.FromStream(msDst)).CGImage)
                {
                    UIGraphics.BeginImageContext(new SizeF((float)src.Width, (float)src.Height));

                    using (CGContext context = UIGraphics.GetCurrentContext())
                    {
                        context.DrawImage(new CGRect(0, 0, dst.Width, dst.Height), dst);
                        context.DrawImage(new CGRect(xDest, yDest, src.Width, src.Height), src);
                        var resultImage = UIGraphics.GetImageFromCurrentImageContext();

                        NSData data = resultImage.AsJPEG();
                        msDst = data.AsStream();
                        msDst.Seek(0, SeekOrigin.Begin);
                    }
                    UIGraphics.EndImageContext();
                }
            }

            return msDst;
        }

        private float DegreeToRadian(float angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }

        public Stream GetArcPartFromSquareImage(Stream ms, int partNumber, int nbParts)
        {
            Stream result = null;

            int diameter;
            float arcAngle;
            float arcStart;
            float startX;
            float startY;
            Stream temp;

            using (UIImage image = UIImage.LoadFromData(NSData.FromStream(ms)))
            {
                diameter = (int)image.Size.Width;

                arcAngle = 360 / nbParts;
                arcStart = 90 + (partNumber - 1) * arcAngle;
                if (nbParts > 2)
                    arcStart += (180 / nbParts);

                float deltaX = (float)(diameter / 2 - ((diameter * Math.Cos(DegreeToRadian(arcStart + arcAngle / 2)) + diameter) / 2)) / 2;
                float deltaY = (float)(diameter / 2 + ((diameter * Math.Sin(DegreeToRadian(arcStart + arcAngle / 2)) + diameter) / 2)) / 2;

                startX = (float)((diameter * Math.Cos(DegreeToRadian(arcStart)) + diameter) / 2);
                startY = (float)((diameter * Math.Sin(DegreeToRadian(arcStart)) + diameter) / 2);

                temp = GetTranslated(image.AsJPEG().AsStream(), -deltaX, deltaY / 2);
            }

            using (UIImage image = UIImage.LoadFromData(NSData.FromStream(temp)))
            {
                UIGraphics.BeginImageContext(new SizeF(image.CGImage.Width, image.CGImage.Height));

                using (UIBezierPath path = new UIBezierPath())
                {
                    path.MoveTo(new CGPoint(diameter / 2, diameter / 2));
                    path.AddLineTo(new CGPoint(startX, startY));
                    path.AddArc(new CGPoint(diameter / 2, diameter / 2), diameter / 2, DegreeToRadian(arcStart), DegreeToRadian(arcStart + arcAngle), true);
                    path.AddLineTo(new CGPoint(diameter / 2, diameter / 2));

                    path.AddClip();
                    image.Draw(new Rectangle(0, 0, (int)image.CGImage.Width, (int)image.CGImage.Height));

                }
                var resultImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();

                NSData data = resultImage.AsPNG();
                result = data.AsStream();
            }

            return result;
        }

        public double GetDensity()
        {
            if (density == 0)
                density = DeviceDisplay.MainDisplayInfo.Density;

            return density;
        }

        public Stream GetFilledCircleWithCenteredText(int imgSize, string rgbCircleColor, string txt, string rgbTextColor, string fontFamilyName, int fontSize)
        {
            Stream result = null;

            using (UIImage image = new UIImage())
            {
                UIGraphics.BeginImageContext(new SizeF(imgSize, imgSize));

                CGRect rect = new CGRect(0, 0, imgSize, imgSize);

                image.Draw(rect);

                using (CGContext g = UIGraphics.GetCurrentContext())
                {
                    float x = (float)(rect.X + (rect.Width / 2));
                    float y = (float)(rect.Y + (rect.Height / 2));

                    // Draws the circle
                    UIColor background = FromHexString(rgbCircleColor);
                    g.SetLineWidth(1);
                    g.SetFillColor(background.CGColor);
                    g.SetStrokeColor(background.CGColor);

                    CGPath path = new CGPath();
                    path.AddArc(x, y, NMath.Min(rect.Width, rect.Height) / 2 - 1f, 0, 2.0f * (float)Math.PI, true);
                    g.AddPath(path);
                    g.DrawPath((CGPathDrawingMode.FillStroke));


                    // Draws the text
                    g.SetFillColor(FromHexString(rgbTextColor).CGColor);

                    var attributedString = new NSAttributedString(txt, new CoreText.CTStringAttributes { ForegroundColorFromContext = true, Font = new CoreText.CTFont(fontFamilyName, fontSize * 2) });

                    using (var textLine = new CoreText.CTLine(attributedString))
                    {
                        g.TranslateCTM(x - (textLine.GetBounds(CoreText.CTLineBoundsOptions.UseGlyphPathBounds).Width / 2), y + (textLine.GetBounds(CoreText.CTLineBoundsOptions.UseGlyphPathBounds).Height / 2));
                        g.ScaleCTM(1, -1);
                        textLine.Draw(g);
                    }
                }

                var resultImage = UIGraphics.GetImageFromCurrentImageContext();

                UIGraphics.EndImageContext();

                NSData data = resultImage.AsPNG();

                result = data.AsStream();
            }

            return result;
        }

        public static UIColor FromHexString(string hexValue)
        {
            var colorString = hexValue.Replace("#", "");

            float red, green, blue;

            red = Convert.ToInt32(colorString.Substring(0, 2), 16) / 255f;
            green = Convert.ToInt32(colorString.Substring(2, 2), 16) / 255f;
            blue = Convert.ToInt32(colorString.Substring(4, 2), 16) / 255f;

            return UIColor.FromRGB(red, green, blue);

        }

        public Stream GetRoundedFromSquareImage(Stream stream)
        {
            Stream result = null;

            using (UIImage image = UIImage.LoadFromData(NSData.FromStream(stream)))
            {
                UIGraphics.BeginImageContext(new SizeF(image.CGImage.Width, image.CGImage.Height));

                CGRect rect = new CGRect(0, 0, image.CGImage.Width, image.CGImage.Height);

                UIBezierPath.FromRoundedRect(rect, Math.Max(image.CGImage.Width, image.CGImage.Height) / 2).AddClip();
                image.Draw(new Rectangle(0, 0, (int)image.CGImage.Width, (int)image.CGImage.Height));

                var resultImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();

                NSData data = resultImage.AsPNG();
                result = data.AsStream();
            }
            return result;
        }

        public Stream GetSquareAndScaled(Stream ms, int imgSize)
        {
            Stream result = null;

            using (UIImage image = UIImage.LoadFromData(NSData.FromStream(ms)))
            {
                UIGraphics.BeginImageContext(new SizeF(imgSize, imgSize));
                image.Draw(new RectangleF(0, 0, imgSize, imgSize));

                var resultImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();

                NSData data = resultImage.AsPNG();
                result = data.AsStream();
            }
            return result;
        }

        public Stream GetTranslated(Stream ms, float x, float y)
        {
            Stream result = null;

            using (UIImage image = UIImage.LoadFromData(NSData.FromStream(ms)))
            {
                UIGraphics.BeginImageContext(new SizeF((float)image.Size.Width, (float)image.Size.Height));
                image.Draw(new RectangleF(x, -y, (float)image.Size.Width, (float)image.Size.Height));

                var resultImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();

                NSData data = resultImage.AsPNG();
                result = data.AsStream();
            } 
            return result;
        }

        public Stream GetScaled(Stream stream, int width, int height)
        {
            Stream result;
            using (UIImage image = UIImage.LoadFromData(NSData.FromStream(stream)))
            {
                UIGraphics.BeginImageContext(new SizeF(width, height));
                image.Draw(new RectangleF(0, 0, width, height));

                var resultImage = UIGraphics.GetImageFromCurrentImageContext();
                UIGraphics.EndImageContext();

                NSData data = resultImage.AsPNG();
                result = data.AsStream();
            }
            return result;
        }

        public Size GetSize(Stream ms)
        {
            using (UIImage image = UIImage.LoadFromData(NSData.FromStream(ms)))
            {
                return new Size((int)image.Size.Width, (int)image.Size.Height);
            }
        }
    }
}
