using FFmpeg.AutoGen;
using System;
using Rainbow.Medias;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Rainbow
{
    public class AsciiFrame
    {
        private int _nbLinesForFooter = 0;
        private String _originalFooterText = "";
        private String _footerText = "";

        private MediaFiltered? _mediaFiltered;
        private MediaInput? _mediaInput;

        private readonly String FILTER = "fps=1,format=gray8,scale=WIDTH_CALCULATED:-2,pad=CONSOLE_WIDTH:CONSOLE_HEIGHT:(ow-iw)/2:(oh-ih)/2";
        private readonly StringBuilder _asciiBuilder = new StringBuilder();
        private readonly char[] _asciiPixels = " `'.,-~:;<>\"^=+*!?|\\/(){}[]#&$@".ToCharArray();

        private int _videoWidth = 0;
        private int _videoHeight = 0;

        private int _consoleWidth = 0;
        private int _consoleHeight = 0;
        private Boolean _useConsole;
        private Boolean _filterSetOnce = false;

        public event EventHandler<String> OnAsciiFrame;

        public AsciiFrame(Boolean useConsole, int nbLinesForFooter = 0, MediaInput? mediaInput = null)
        {
            _useConsole = useConsole;
            if (mediaInput != null)
            {
                _mediaFiltered = new MediaFiltered("acsiiFrame", new List<MediaInput>() { mediaInput });
                _mediaFiltered.SetPixelFormat(AVPixelFormat.AV_PIX_FMT_GRAY8);
                //_mediaFiltered.OnImage += MediaFiltered_OnImage;
                _mediaFiltered.OnVideoFrame += MediaFiltered_OnVideoFrame;
            }

            if (nbLinesForFooter >= 0)
                _nbLinesForFooter = nbLinesForFooter;

            _nbLinesForFooter = 0;
        }

        public void SetSize(int width, int height)
        {
            if (!_useConsole)
            {
                _consoleWidth = width;
                _consoleHeight = height;
            }
        }

        public void SetNbLinesForFooter(int nbLinesForFooter)
        {
            if ((nbLinesForFooter >= 0) && (_nbLinesForFooter != nbLinesForFooter))
            {
                _nbLinesForFooter = nbLinesForFooter;
                SetFooterText(_originalFooterText);
            }
            _nbLinesForFooter = 0;
        }

        public void SetFooterText(String footerText)
        {
            // TODO: to ensure the footer text is not too big according the nb of lines of the footer
            _originalFooterText = footerText;
            _footerText = footerText;
        }

        public Boolean SetMediaVideo(MediaInput? newMediaInput)
        {
            Boolean result = false;
            if (newMediaInput != null)
            {
                // Check if it's the same MediaInput than before
                if (newMediaInput.Id == _mediaInput?.Id)
                    return true;

                // Create or Update _mediaFiltered
                if (_mediaFiltered == null)
                {
                    _mediaFiltered = new MediaFiltered("acsiiFrame", new List<MediaInput>() { newMediaInput });
                    _mediaFiltered.SetPixelFormat(AVPixelFormat.AV_PIX_FMT_GRAY8);
                    //_mediaFiltered.OnImage += MediaFiltered_OnImage;
                    _mediaFiltered.OnVideoFrame += MediaFiltered_OnVideoFrame;
                }
                else
                    _mediaFiltered.AddMediaInput(newMediaInput);

                _videoWidth = newMediaInput.Width;
                _videoHeight = newMediaInput.Height;

                (result, Boolean needUpdate) = CheckAndUpdateFilterIfNecessary(newMediaInput, _videoWidth, _videoHeight);

                // Remove previous MediaInput (if any)
                if (_mediaInput != null)
                    _mediaFiltered.RemoveMediaInput(_mediaInput.Id);

                // Store new one
                _mediaInput = newMediaInput;

                if (result)
                    _mediaFiltered.Start();
            }
            return result;
        }

        private void MediaFiltered_OnVideoFrame(string mediaId, FFmpegSharp.MediaFrame mediaFrame)
        {
            (Boolean done, Boolean needUpdate) = CheckAndUpdateFilterIfNecessary(_mediaInput, _mediaInput.Width, _mediaInput.Height);
            if (done && !needUpdate)
            {
                var asciiFrame = CreateAsciiFrame(mediaFrame.Ref);
                DrawAsciiFrame(asciiFrame);
                OnAsciiFrame?.Invoke(this, asciiFrame);

                
            }
        }

        private void MediaFiltered_OnImage(string mediaId, int width, int height, int stride, IntPtr data, AVPixelFormat pixelFormat)
        {

            //if (mediaId == _mediaInput?.Id)
            {
                (Boolean done, Boolean needUpdate) = CheckAndUpdateFilterIfNecessary(_mediaInput, _mediaInput.Width, _mediaInput.Height);
                if (done && !needUpdate)
                {
                    // Update ASCII Frame
                    byte[] array;
                    if (height > 0 && stride > 0)
                    {
                        if (stride > width)
                        {
                            array = new byte[height * width];

                            byte[] line;
                            line = new byte[width];

                            int count = 0;
                            for (var i = 0; i < height; i++)
                            {
                                Marshal.Copy(data, array, count, width);
                                count += width;
                            }
                        }
                        else
                        {
                            int num = height * stride;
                            array = new byte[num];
                            Marshal.Copy(data, array, 0, num);
                        }
                        var asciiFrame = CreateAsciiFrame(array);
                        DrawAsciiFrame(asciiFrame);
                        OnAsciiFrame?.Invoke(this, asciiFrame);

                    }

                }
            }
        }

        private unsafe String CreateAsciiFrame(byte[] rawdata)
        {
            // We don't call Console.Clear() here because it actually adds stutter.
            // Go ahead and try this example in Alacritty to see how smooth it is!
            _asciiBuilder.Clear();
            int length = rawdata.Length;

            // Since we know that the frame has the exact size of the terminal window,
            // we have no need to add any newline characters. Thus we can just go through
            // the entire byte array to build the ASCII converted string.
            for (int i = 0; i < length; i++)
            {
                _asciiBuilder.Append(_asciiPixels[RangeMap(rawdata[i], 0, 255, 0, _asciiPixels.Length - 1)]);
            }
            return _asciiBuilder.ToString();
        }

        private unsafe String CreateAsciiFrame(AVFrame frame)
        {
            // We don't call Console.Clear() here because it actually adds stutter.
            // Go ahead and try this example in Alacritty to see how smooth it is!
            _asciiBuilder.Clear();
            Console.SetCursorPosition(0, 0);
            int length = frame.width * frame.height;

            var RawData = new ReadOnlySpan<byte>(frame.data[0], frame.linesize[0] * frame.height);

            // Since we know that the frame has the exact size of the terminal window,
            // we have no need to add any newline characters. Thus we can just go through
            // the entire byte array to build the ASCII converted string.
            for (int i = 0; i < length; i++)
            {
                _asciiBuilder.Append(_asciiPixels[RangeMap(RawData[i], 0, 255, 0, _asciiPixels.Length - 1)]);
            }
            return _asciiBuilder.ToString();
            
        }

        private unsafe void DrawAsciiFrame(String acsiiFrame)
        {
            if (_useConsole)
            {
                Console.SetCursorPosition(0, 0);
                Console.Write(acsiiFrame.ToString());
                if (_footerText.Length > 0)
                    Console.Write(_footerText);
                Console.Out.Flush();
            }
        }

        private (Boolean done, Boolean needUpdate) CheckAndUpdateFilterIfNecessary(MediaInput? mediaInput, int width, int height)
        {
            Boolean done;
            Boolean needUpdate;

            if ((mediaInput != null) && (_mediaFiltered != null))
            {
                int currentConsoleWidth;
                int currentConsoleHeight;

                if (_useConsole)
                {
                    currentConsoleWidth = Console.WindowWidth;
                    currentConsoleHeight = Console.WindowHeight - _nbLinesForFooter;
                }
                else
                {
                    currentConsoleWidth = _consoleWidth;
                    currentConsoleHeight = _consoleHeight - _nbLinesForFooter;
                }

                if ((_consoleWidth == currentConsoleWidth) && (_consoleHeight == currentConsoleHeight) && _filterSetOnce)
                {
                    done = true;
                    needUpdate = false;
                }
                else
                {
                    _consoleWidth = currentConsoleWidth;
                    _consoleHeight = currentConsoleHeight;

                    // Set filter
                    var filter = GetFilter(_videoWidth, _videoHeight);
                    done = _mediaFiltered.SetVideoFilter(new List<String>() { mediaInput.Id }, filter);
                    if (done)
                        _filterSetOnce = true;
                    needUpdate = true;
                }
            }
            else
            {
                needUpdate = true;
                done = false;
            }

            return (done, needUpdate);
        }

        private int GetVideoWidth(int wSource, int hSource)
        {
            if ((wSource == 0) || (wSource == 0))
                return 0;

            int width = 0;

            double wScale;
            double hScale;


            if (_useConsole)
            {
                wScale = Math.Ceiling(((double)wSource / (double)Console.WindowWidth));
                hScale = Math.Ceiling(((double)hSource / (double)Console.WindowHeight - _nbLinesForFooter));
            }
            else
            {
                wScale = Math.Ceiling(((double)wSource / (double)_consoleWidth));
                hScale = Math.Ceiling(((double)hSource / (double)_consoleHeight - _nbLinesForFooter));
            }

            // Get the greater sacle
            var scale = Math.Max(wScale, hScale);

            width = (int)Math.Floor((double)wSource / (double)scale);

            // We want an even number
            if (width % 2 != 0)
                width -= 1;

            return width;
        }

        private String GetFilter(int width, int height)
        {
            int w = GetVideoWidth(width, height);
            if (_useConsole)
                return FILTER.Replace("WIDTH_CALCULATED", w.ToString())
                        .Replace("CONSOLE_WIDTH", Console.WindowWidth.ToString())
                        .Replace("CONSOLE_HEIGHT", (Console.WindowHeight - _nbLinesForFooter).ToString());

            return FILTER.Replace("WIDTH_CALCULATED", w.ToString())
                        .Replace("CONSOLE_WIDTH", _consoleWidth.ToString())
                        .Replace("CONSOLE_HEIGHT", _consoleHeight.ToString());
        }

        public void GotRawImage(int width, int height, IntPtr sample)
        {
            //if (grayConverter == null
            //    || Console.WindowWidth != grayConverter.DestinationWidth
            //    || Console.WindowHeight - nbLinesForFooter != grayConverter.DestinationHeight)
            //{
            //    // We can't just override converter
            //    // We have to dispose the previous one and instanciate a new one with the new window size.
            //    grayConverter?.Dispose();
            //    grayConverter = new VideoFrameConverter(width, height, AVPixelFormat.AV_PIX_FMT_RGB24, Console.WindowWidth, Console.WindowHeight - nbLinesForFooter, FFmpeg.AutoGen.AVPixelFormat.AV_PIX_FMT_GRAY8);
            //}

            //// Resize the frame to the size of the terminal window, then draw it in ASCII.
            //var frame = grayConverter.Convert(sample);
            //DrawAsciiFrame(frame);
        }

        public void DisplayOnlyFooter()
        {
            if (_useConsole)
            {
                Console.Clear();
                if (_footerText.Length > 0)
                {
                    Console.Write(_footerText);
                    Console.Out.Flush();
                }
            }
        }

        private unsafe void DrawAsciiFrame(AVFrame frame)
        {
            // We don't call Console.Clear() here because it actually adds stutter.
            // Go ahead and try this example in Alacritty to see how smooth it is!
            _asciiBuilder.Clear();
            Console.SetCursorPosition(0, 0);
            int length = frame.width * frame.height;

            var RawData = new ReadOnlySpan<byte>(frame.data[0], frame.linesize[0] * frame.height);

            // Since we know that the frame has the exact size of the terminal window,
            // we have no need to add any newline characters. Thus we can just go through
            // the entire byte array to build the ASCII converted string.
            for (int i = 0; i < length; i++)
            {
                _asciiBuilder.Append(_asciiPixels[RangeMap(RawData[i], 0, 255, 0, _asciiPixels.Length - 1)]);
            }

            Console.Write(_asciiBuilder.ToString());
            //if (_footerText.Length > 0)
            //    Console.Write(_footerText);
            Console.Out.Flush();
        }

        public int RangeMap(int x, int in_min, int in_max, int out_min, int out_max)
        => (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;

        public ConsoleColor ClosestConsoleColor(byte r, byte g, byte b)
        {
            ConsoleColor ret = 0;
            double rr = r, gg = g, bb = b, delta = double.MaxValue;

            foreach (ConsoleColor cc in Enum.GetValues(typeof(ConsoleColor)))
            {
                var n = Enum.GetName(typeof(ConsoleColor), cc);
                var c = System.Drawing.Color.FromName(n == "DarkYellow" ? "Orange" : n); // bug fix
                var t = Math.Pow(c.R - rr, 2.0) + Math.Pow(c.G - gg, 2.0) + Math.Pow(c.B - bb, 2.0);
                if (t == 0.0)
                    return cc;
                if (t < delta)
                {
                    delta = t;
                    ret = cc;
                }
            }
            return ret;
        }
    }
}
