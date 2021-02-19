using System;
using System.IO;
using Android.Graphics;
using Android.Text;
using Android.Util;

using Rainbow.Helpers;


namespace Rainbow.Droid.Helpers
{
    public class ImageManagement: IImageManagement 
    {

#region INTERFACE IImageManagement


        public Double GetDensity()
        {
            if(density == 0)
            {
                DisplayMetrics displayMetrics = new DisplayMetrics();
                density = displayMetrics?.Density ?? 0;
                displayMetrics?.Dispose();
                if (density == 0)
                    density = 1;
            }

            return density;
        }

        public Stream DrawImage(Stream msSrc, Stream msDst, float xDest, float yDest)
        {
            Stream result = null;

            using (Bitmap bitmapSource = BitmapFactory.DecodeStream(msSrc))
            {
                using (Bitmap bitmapDestination = BitmapFactory.DecodeStream(msDst))
                {
                    using (Bitmap bitmapResult = Bitmap.CreateBitmap(bitmapDestination.Width, bitmapDestination.Height, Bitmap.Config.Argb8888))
                    {
                        using (Paint paint = new Paint(PaintFlags.AntiAlias))
                        {
                            using (Canvas canvas = new Canvas(bitmapResult))
                            {
                                canvas.DrawBitmap(bitmapDestination, 0, 0, paint);
                                canvas.DrawBitmap(bitmapSource, xDest, yDest, paint);
                                result = GetStreamFromBitmap(bitmapResult);
                            }
                        }
                        bitmapResult.Recycle();
                    }
                }
                bitmapSource.Recycle();
            }
            return result;
        }

        public Stream GetTranslated(Stream ms, float x, float y)
        {
            Stream result = null;
            using (Bitmap temp = BitmapFactory.DecodeStream(ms))
            {
                using (Bitmap bitmapResult = Bitmap.CreateBitmap(temp.Width, temp.Height, Bitmap.Config.Argb8888))
                {
                    using (Paint paint = new Paint(PaintFlags.AntiAlias))
                    {
                        using (Canvas canvas = new Canvas(bitmapResult))
                        {
                            canvas.DrawBitmap(temp, x, y, paint);
                            result = GetStreamFromBitmap(bitmapResult);
                        }
                    }
                    bitmapResult.Recycle();
                }
                temp.Recycle();
            }

            return result;
        }

        public Stream GetArcPartFromSquareImage(Stream ms, int partNumber, int nbParts)
        {
            Stream result = null;
            using (Bitmap bitmap = BitmapFactory.DecodeStream(ms))
            {
                int diameter = bitmap.Width; // (int)(AvatarSize * GetDensity());

                float arcAngle = 360 / nbParts;
                float arcStart = 90 + (partNumber - 1) * arcAngle;
                if (nbParts > 2)
                    arcStart += (180 / nbParts);

                float deltaX = (float)(diameter / 2 - ((diameter * Math.Cos(DegreeToRadian(arcStart + arcAngle / 2)) + diameter) / 2)) / 2;
                float deltaY = (float)(diameter / 2 - ((diameter * Math.Sin(DegreeToRadian(arcStart + arcAngle / 2)) + diameter) / 2)) / 2;

                float startX = (float)((diameter * Math.Cos(DegreeToRadian(arcStart)) + diameter) / 2);
                float startY = (float)((diameter * Math.Sin(DegreeToRadian(arcStart)) + diameter) / 2);

                //float endX = (float)((diameter * Math.Cos(DegreeToRadian(arcStart + arcAngle)) + diameter) / 2);
                //float endY = (float)((diameter * Math.Sin(DegreeToRadian(arcStart + arcAngle)) + diameter) / 2);

                using (Bitmap temp = Bitmap.CreateBitmap(diameter, diameter, Bitmap.Config.Argb8888))
                {
                    // First we get the bitmap part (we translate using deltaX/Y to have the center of the avatar)
                    using (BitmapShader shader = new BitmapShader(bitmap, Shader.TileMode.Clamp, Shader.TileMode.Clamp))
                    {
                        using (Paint paint = new Paint(PaintFlags.AntiAlias))
                        {
                            paint.SetShader(shader);

                            using (Android.Graphics.Path path = new Android.Graphics.Path())
                            {
                                using (Canvas canvas = new Canvas(temp))
                                {
                                    path.MoveTo(diameter / 2 + deltaX, diameter / 2 + deltaY);
                                    path.LineTo(startX + deltaX, startY + deltaY);
                                    path.AddArc(deltaX, deltaY, diameter + deltaX, diameter + deltaY, arcStart, arcAngle);
                                    path.LineTo(diameter / 2 + deltaX, diameter / 2 + deltaY);
                                    canvas.DrawPath(path, paint);

                                    //canvas.DrawArc(deltaX, deltaY, deltaX + diameter, deltaY + diameter, arcStart, arcAngle, true, paint);
                                }
                            }
                        }
                    }

                    // Now we translate back to have correct layout
                    result = GetTranslated(GetStreamFromBitmap(temp), -deltaX, -deltaY);
                    
                    temp.Recycle();
                }
                bitmap.Recycle();
            }
            
            return result;
        }

        public Stream GetSquareAndScaled(Stream ms, int imgSize)
        {
            return GetScaled(ms, imgSize, imgSize);
        }

        public Stream GetScaled(Stream ms, int width, int height)
        {
            Stream result = null;
            using (Bitmap bitmap = BitmapFactory.DecodeStream(ms))
            {
                using (Bitmap bitmapResult = Bitmap.CreateScaledBitmap(bitmap, width, height, true))
                {
                    result = GetStreamFromBitmap(bitmapResult);
                }
            }
            return result;
        }

        public System.Drawing.Size GetSize(Stream ms)
        {
            System.Drawing.Size result = new System.Drawing.Size();
            using (Bitmap bitmap = BitmapFactory.DecodeStream(ms))
            {
                result.Width = bitmap.Width;
                result.Height = bitmap.Height;
            }
            return result;
        }

        public Stream GetFilledCircleWithCenteredText(int imgSize, String rgbCircleColor, String txt, String rgbTextColor, string fontFamilyName, int fontSize)
        {
            Stream result = null;

            using (Bitmap bitmapResult = Bitmap.CreateBitmap(imgSize, imgSize, Bitmap.Config.Argb8888))
            {
                using (Canvas canvas = new Canvas(bitmapResult))
                {
                    using (Paint paintBgd = new Paint(PaintFlags.AntiAlias))
                    {
                        paintBgd.Color = Color.ParseColor(rgbCircleColor);

                        canvas.DrawColor(Color.Transparent);
                        canvas.DrawOval(new RectF(0, 0, imgSize, imgSize), paintBgd);
                    }

                    //TODO: use fontName...
                    using (TextPaint textPaint = new TextPaint(PaintFlags.AntiAlias))
                    {
                        textPaint.Color = Color.ParseColor(rgbTextColor);
                        textPaint.TextAlign = Paint.Align.Center;
                        textPaint.TextSize = (int)(fontSize * GetDensity());

                        float textHeight = textPaint.Descent() - textPaint.Ascent();
                        float textOffset = (textHeight / 2) - textPaint.Descent();

                        using (RectF bounds = new RectF(0, 0, imgSize, imgSize))
                            canvas.DrawText(txt, bounds.CenterX(), bounds.CenterY() + textOffset, textPaint);
                    }
                    result = GetStreamFromBitmap(bitmapResult);
                }
                //bitmapResult.Recycle();
            }

            return result;
        }

        public Stream GetRoundedFromSquareImage(Stream stream)
        {
            Stream result = null;
            using (Bitmap bitmap = BitmapFactory.DecodeStream(stream))
            {
                using (BitmapShader shader = new BitmapShader(bitmap, Shader.TileMode.Clamp, Shader.TileMode.Clamp))
                {
                    using (Paint paint = new Paint(PaintFlags.AntiAlias))
                    {
                        paint.SetShader(shader);
                        RectF rect = new RectF(0.0f, 0.0f, bitmap.Width, bitmap.Height);

                        using (Bitmap bitmapResult = Bitmap.CreateBitmap(bitmap.Width, bitmap.Height, Bitmap.Config.Argb8888))
                        {
                            using (Canvas canvas = new Canvas(bitmapResult))
                            {
                                canvas.DrawColor(Color.Transparent);
                                canvas.DrawRoundRect(rect, bitmap.Width / 2, bitmap.Height / 2, paint);

                                result = GetStreamFromBitmap(bitmapResult);
                            }
                            bitmapResult.Recycle();
                        }
                    }
                }
                bitmap.Recycle();
            }

            return result;
        }

#endregion INTERFACE IImageManagement


#region ANDROID SPECIFIC

        private double density = 0;

        private float DegreeToRadian(float angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }
 
        private Stream GetStreamFromBitmap(Bitmap bitmap)
        {
            MemoryStream result = new MemoryStream();
            bitmap.Compress(Bitmap.CompressFormat.Png, 0, result);
            result.Seek(0, SeekOrigin.Begin);
            return result;
        }

#endregion ANDROID SPECIFIC    

    }
}