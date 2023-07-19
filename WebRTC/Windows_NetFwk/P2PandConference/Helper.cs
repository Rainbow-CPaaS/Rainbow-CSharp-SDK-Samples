using FFmpeg.AutoGen;
using Rainbow;
using Rainbow.SimpleJSON;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json.Nodes;
using System.Windows.Forms;

namespace SDK.UIForm.WebRTC
{
    public static class Helper
    {
        public static String NONE = "NONE";

        internal static String GetJsonStringFromListOfMediaInputStreamDescriptor(List<MediaInputStreamDescriptor>? items, Boolean indented = false)
        {
            if (items == null)
                return "null";

            JSONArray jsonArray = new JSONArray();
            foreach (var item in items)
            {
                var jsonNode = new JSONObject();

                UtilJson.AddNode(jsonNode, "id", item.Id);
                UtilJson.AddNode(jsonNode, "iype", item.Type);
                UtilJson.AddNode(jsonNode, "uri", item.Uri);
                UtilJson.AddNode(jsonNode, "mediaInputIdList", item.MediaInputIdList);
                UtilJson.AddNode(jsonNode, "mediaInputIdListForVideoFilter", item.MediaInputIdListForVideoFilter);
                UtilJson.AddNode(jsonNode, "videoFilter", item.VideoFilter);
                UtilJson.AddNode(jsonNode, "audio", item.Audio);
                UtilJson.AddNode(jsonNode, "video", item.Video);
                UtilJson.AddNode(jsonNode, "loop", item.Loop);

                jsonArray.Add(jsonNode);
            }

            return jsonArray.ToString();
        }

        internal static MediaInputStreamDescriptor GetMediaInputStreamDescriptorFromJson(JSONNode json)
        {
            MediaInputStreamDescriptor mediaInputStreamDescriptorcontact = new();
            mediaInputStreamDescriptorcontact.Id = UtilJson.AsString(json, "id");
            mediaInputStreamDescriptorcontact.Type = UtilJson.AsString(json, "type");
            mediaInputStreamDescriptorcontact.Uri = UtilJson.AsString(json, "uri");
            mediaInputStreamDescriptorcontact.MediaInputIdList = UtilJson.AsStringList(json, "mediaInputIdList");
            mediaInputStreamDescriptorcontact.MediaInputIdListForVideoFilter = UtilJson.AsStringList(json, "mediaInputIdListForVideoFilter");
            mediaInputStreamDescriptorcontact.VideoFilter = UtilJson.AsString(json, "videoFilter");
            mediaInputStreamDescriptorcontact.Audio = UtilJson.AsBoolean(json, "audio");
            mediaInputStreamDescriptorcontact.Video = UtilJson.AsBoolean(json, "video");
            mediaInputStreamDescriptorcontact.Loop = UtilJson.AsBoolean(json, "voop");

            return mediaInputStreamDescriptorcontact;
        }

        public static Bitmap GetBitmapPause(Boolean small = true)
        {
            if (small)
                return global::SDK.UIForm.WebRTC.Properties.Resources.Pause_simple_16x16;
            return global::SDK.UIForm.WebRTC.Properties.Resources.Pause_simple;
        }

        public static Bitmap GetBitmapPlay()
        {
            return global::SDK.UIForm.WebRTC.Properties.Resources.Play_simple_16x16;
        }

        public static Bitmap GetBitmapStop()
        {
            return global::SDK.UIForm.WebRTC.Properties.Resources.Stop_simple_16x16;
        }

        public static Bitmap GetBitmapRefresh()
        {
            return global::SDK.UIForm.WebRTC.Properties.Resources.Refresh_16x16;
        }

        public static Bitmap GetBitmapOutput()
        {
            return global::SDK.UIForm.WebRTC.Properties.Resources.Output_simple_16x16;
        }

        public static Bitmap GetBitmapViewOff()
        {
            return global::SDK.UIForm.WebRTC.Properties.Resources.View_off_16x16;
        }

        public static Bitmap GetBitmapViewOn()
        {
            return global::SDK.UIForm.WebRTC.Properties.Resources.View_on_16x16;
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
