using FFmpeg.AutoGen;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SDK.UIForm.WebRTC
{
    public static class Helper
    {
        public static String NONE = "NONE";

        public static Bitmap GetBitmapPause(Boolean small = true)
        {
            if (small)
                return global::SDK.UIForm.Medias.Properties.Resources.Pause_simple_16x16;
            return global::SDK.UIForm.Medias.Properties.Resources.Pause_simple;
        }

        public static Bitmap GetBitmapPlay()
        {
            return global::SDK.UIForm.Medias.Properties.Resources.Play_simple_16x16;
        }

        public static Bitmap GetBitmapStop()
        {
            return global::SDK.UIForm.Medias.Properties.Resources.Stop_simple_16x16;
        }

        public static Bitmap GetBitmapRefresh()
        {
            return global::SDK.UIForm.Medias.Properties.Resources.Refresh_16x16;
        }

        public static Bitmap GetBitmapOutput()
        {
            return global::SDK.UIForm.Medias.Properties.Resources.Output_simple_16x16;
        }

        public static Bitmap GetBitmapViewOff()
        {
            return global::SDK.UIForm.Medias.Properties.Resources.View_off_16x16;
        }

        public static Bitmap GetBitmapViewOn()
        {
            return global::SDK.UIForm.Medias.Properties.Resources.View_on_16x16;
        }

        public static Bitmap? BitmapFromImageData(int width, int height, int stride, IntPtr data, AVPixelFormat avPixelFormat)
        {
            Bitmap? bmpImage = null;
            try
            {
                PixelFormat pixelFormat;
                if (avPixelFormat == AVPixelFormat.AV_PIX_FMT_BGRA)
                    pixelFormat = PixelFormat.Format32bppArgb;
                else if (avPixelFormat == AVPixelFormat.AV_PIX_FMT_BGR24)
                    pixelFormat = PixelFormat.Format24bppRgb;
                else
                    return null;

                bmpImage = new Bitmap(width, height, stride, pixelFormat, data);
            }
            catch
            {
            }
            return bmpImage;
        }

        public static void UpdatePictureBox(PictureBox pictureBox, Bitmap? bmpImage)
        {
            if (pictureBox?.IsHandleCreated == true)
            {
                pictureBox.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        pictureBox.Image = bmpImage;
                    }
                    catch
                    {
                    }
                }));
            }
        }
    }
}
