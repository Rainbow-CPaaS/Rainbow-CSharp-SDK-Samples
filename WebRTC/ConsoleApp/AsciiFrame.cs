using FFmpeg.AutoGen;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.FFmpeg;
using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.ConsoleApp.WebRTC
{
    public class AsciiFrame
    {
        private VideoFrameConverter? grayConverter = null;
        private int nbLinesForFooter = 0;
        private String footerText = "";
        
        readonly StringBuilder asciiBuilder = new StringBuilder();
        readonly char[] asciiPixels = " `'.,-~:;<>\"^=+*!?|\\/(){}[]#&$@".ToCharArray();

        public AsciiFrame(int nbLinesForFooter = 0)
        {
            this.nbLinesForFooter = nbLinesForFooter;
        }

        public void SetFooterText(String footerText)
        {
            this.footerText = footerText;
        }

        public void GotRawImage(int width, int height, IntPtr sample)
        {
            if (grayConverter == null
                || Console.WindowWidth != grayConverter.DestinationWidth
                || Console.WindowHeight - nbLinesForFooter != grayConverter.DestinationHeight)
            {
                // We can't just override converter
                // We have to dispose the previous one and instanciate a new one with the new window size.
                grayConverter?.Dispose();
                grayConverter = new VideoFrameConverter(width, height, AVPixelFormat.AV_PIX_FMT_RGB24, Console.WindowWidth, Console.WindowHeight - nbLinesForFooter, FFmpeg.AutoGen.AVPixelFormat.AV_PIX_FMT_GRAY8);
            }

            // Resize the frame to the size of the terminal window, then draw it in ASCII.
            var frame = grayConverter.Convert(sample);
            DrawAsciiFrame(frame);
        }

        public void GotRawImage(ref RawImage rawImage)
        {
            GotRawImage(rawImage.Width, rawImage.Height, rawImage.Sample);
        }

        public void DisplayFooter()
        {
            if (footerText.Length > 0)
            {
                Console.Clear();
                Console.Write(footerText);
                Console.Out.Flush();
            }
        }

        private unsafe void DrawAsciiFrame(AVFrame frame)
        {
            // We don't call Console.Clear() here because it actually adds stutter.
            // Go ahead and try this example in Alacritty to see how smooth it is!
            asciiBuilder.Clear();
            Console.SetCursorPosition(0, 0);
            int length = frame.width * frame.height;

            var RawData = new ReadOnlySpan<byte>(frame.data[0], frame.linesize[0] * frame.height);

            // Since we know that the frame has the exact size of the terminal window,
            // we have no need to add any newline characters. Thus we can just go through
            // the entire byte array to build the ASCII converted string.
            for (int i = 0; i < length; i++)
            {
                asciiBuilder.Append(asciiPixels[RangeMap(RawData[i], 0, 255, 0, asciiPixels.Length - 1)]);
            }

            Console.Write(asciiBuilder.ToString());
            if(footerText.Length > 0)
                Console.Write(footerText);
            Console.Out.Flush();
        }

        public int RangeMap(int x, int in_min, int in_max, int out_min, int out_max)
        => (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;

    }
}
