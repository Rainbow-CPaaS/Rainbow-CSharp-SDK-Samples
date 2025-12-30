using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rainbow.Example.Common
{
    public static class VideoFilter
    {
        /// <summary>
        /// To create video filter to display several videos in a mosaic.
        /// </summary>
        /// <param name="srcVideoSize"><see cref="T:List<Size>"/>List of video size used as source. The order is important</param>
        /// <param name="vignette"><see cref="Size"/>Size of each vignette in the mosaic</param>
        /// <param name="layout"><see cref="String"/>
        /// Possible values: 
        ///     - "H" horizontal (one or many video can be used)
        ///     - "V" vertical (one or many video can be used)
        ///     - "G" grid display (one or many video can be used)
        ///     - "M+2V" main video on left and 2 verticals on right (exactly 3 video must be used)
        ///     - "M+2H" main video on top and 2 horizontals on bottom (exactly 3 video must be used)
        ///     - "M+2V+3H" main video on top left, 2 verticals on right and 3 horizontals on bottom (exactly 6 video must be used)
        /// </param>
        /// <param name="fps"><see cref="int"/>fps to set for the resulting video</param>
        /// <returns><see cref="String"/> - The video filter or null if parameters used are incompatible</returns>
        static public String? FilterToMosaic(List<Size> srcVideoSize, Size vignette, string layout, int fps = 10)
        {
            List<String> filtersScaledList = [];
            int index = 0;
            Boolean isSimpleCase = (layout == "H") || (layout == "V") || (layout == "G");

            foreach (var ratio in srcVideoSize)
            {
                if ((index == 0) && (!isSimpleCase))
                    filtersScaledList.Add(FilterToScaleSize(ratio, new Size(vignette.Width * 2, vignette.Height * 2), fps));
                else
                    filtersScaledList.Add(FilterToScaleSize(ratio, vignette, fps));

                index++;
            }

            string result = "";
            string subpart = "";
            index = 0;
            foreach (var filerScaled in filtersScaledList)
            {
                result += $"[{index}]setpts=PTS-STARTPTS,{filerScaled}[g{index}];\r\n";

                subpart += $"[g{index}]";
                index++;
            }
            subpart += $"xstack=inputs={index}:";

            switch (layout)
            {
                case "H":
                    subpart += $"grid={index}x1";
                    break;
                case "V":
                    subpart += $"grid=1x{index}";
                    break;
                case "G":
                    var sqrt = (int)Math.Ceiling(Math.Sqrt(srcVideoSize.Count));

                    if (srcVideoSize.Count == sqrt * sqrt)
                    {
                        subpart += $"grid={sqrt}x{sqrt}";
                    }
                    else
                    {
                        subpart += $"layout=";
                        // Example: layout=0_0|0_h0|0_h0+h1|w0_0|w0_h0|w0_h0+h1|w0+w3_0|w0+w3_h0|w0+w3_h0+h1 => 3x3 grid

                        int nb = 0;
                        string h = "";
                        string v = "";

                        for (int v_index = 0; v_index < sqrt; v_index++)
                        {
                            if (v_index == 0)
                                v = "0";
                            else if (v_index == 1)
                                v = "h0";
                            else
                                v += $"+h{v_index - 1}";

                            for (int h_index = 0; h_index < sqrt; h_index++)
                            {
                                if (h_index == 0)
                                    h = "0";
                                else if (h_index == 1)
                                    h = "w0";
                                else
                                    h += $"+w{h_index - 1}";

                                subpart += $"{h}_{v}|";

                                nb++;
                                if (nb >= index)
                                    break;
                            }

                            if (nb >= index)
                                break;
                        }

                        subpart = subpart[..^1];
                        subpart += ":fill=black";
                    }
                    break;
                case "M+2H":
                    if (index != 3)
                        return null;
                    subpart += $"layout=0_0|w0_0|w0_h1";
                    break;

                case "M+2V":
                    if (index != 3)
                        return null;
                    subpart += $"layout=0_0|0_h0|w1_h0";
                    break;

                case "M+2V+3H":
                    if (index != 6)
                        return null;
                    subpart += $"layout=0_0|w0_0|w0_h1|0_h0|w1_h0|w1+w2_h1+h2";
                    break;
            }

            result += subpart;
            result += "[grid];\r\n";
            result += $"[grid]fps={fps}";

            return result;
        }

        /// <summary>
        /// To create a video filter that allows you to overlay a video at a given position onto the main video.
        /// </summary>
        /// <param name="srcMain"><see cref="Size"/>Size of the main video</param>
        /// <param name="srcOverlay"><see cref="Size"/>Size of the video used to overlay the main one</param>
        /// <param name="dst"><see cref="Size"/>Size of the resulting video (in the main window with the overlay)</param>
        /// <param name="dstOverlay"><see cref="Size"/>Size of the overlay to used in the resulting video</param>
        /// <param name="dstOverlayPosition"><see cref="String"/>Where the overlay must be display: "TR" Top right, "TL" Top left, "BR" Bottom right, "BL" Bottom left</param>
        /// <param name="fps"><see cref="int"/>fps to set for the resulting video</param>
        /// <returns><see cref="String"/>The video filter</returns>
        static public String FilterToOverlay(Size srcMain, Size srcOverlay, Size dst, Size dstOverlay, string dstOverlayPosition, int fps = 10)
        {
            string result;

            var mainScaleFilter = FilterToScaleSize(srcMain, dst, fps);
            var overlayScaleFilter = FilterToScaleSize(srcOverlay, dstOverlay, fps);

            string overlayFilter;
            switch (dstOverlayPosition)
            {
                case "TR":
                default:
                    overlayFilter = "overlay=x=W-w:y=0";
                    break;

                case "TL":
                    overlayFilter = "overlay=x=0:y=0";
                    break;

                case "BL":
                    overlayFilter = "overlay=x=0:y=H-h";
                    break;

                case "BR":
                    overlayFilter = "overlay=x=W-w:y=H-h";
                    break;
            }

            result = $"[0]setpts=PTS-STARTPTS,{mainScaleFilter}[o0];\r\n";
            result += $"[1]setpts=PTS-STARTPTS,{overlayScaleFilter}[o1];\r\n";
            result += $"[o0][o1]{overlayFilter}[overlay];\r\n";
            result += $"[overlay]fps={fps}";

            return result;
        }

        static public String FilterToScaleSize(Size srcSize, Size dstSize, int? fps = null)
        {
            string result;

            // Get ratio information
            var ratio_src = Ratio(srcSize);
            var ratio_dst = Ratio(dstSize);

            if ((ratio_src.Width == ratio_dst.Width) && (ratio_src.Height == ratio_dst.Height))
            {
                result = $"scale={dstSize.Width}:-2";
            }
            else
            {
                var scale_dst = ScaleSize(ratio_src, dstSize);
                result = $"scale={scale_dst.Width}:{scale_dst.Height},pad={dstSize.Width}:{dstSize.Height}:(ow-iw)/2:(oh-ih)/2";
            }

            if (fps != null)
                result += $",fps={fps}";

            return result;
        }

        static public Size ScaleSize(Size srcSize, Size dstSize)
        {
            // Get ratio information
            var r = Ratio(srcSize);

            // Check on Width first
            var factor = DivisionResultAsMultipleOf4(dstSize.Width, r.Width);
            if (factor * r.Height > dstSize.Height)
                factor = DivisionResultAsMultipleOf4(dstSize.Height, r.Height);

            return new Size(factor * r.Width, factor * r.Height);
        }

        static public int DivisionResultAsMultipleOf4(int dividend, int divisor)
        {
            var result = dividend / divisor;
            if (result % 2 != 0)
                result -= 1;

            while (result % 4 != 0)
                result -= 2;
            return result;
        }

        static public Size Ratio(Size size)
        {
            var primeFactors_w = PrimeFactors(size.Width);
            var primeFactors_h = PrimeFactors(size.Height);

            var w_prime = primeFactors_w.Keys.ToList();
            var h_prime = primeFactors_h.Keys.ToList();
            var primes = w_prime.Union(h_prime).ToList(); // Without duplicate get all prime factors used in width and height

            foreach (int prime in primes)
            {
                if (primeFactors_w.ContainsKey(prime) && primeFactors_h.ContainsKey(prime))
                {
                    if (primeFactors_w[prime] == primeFactors_h[prime])
                    {
                        primeFactors_w.Remove(prime);
                        primeFactors_h.Remove(prime);
                    }
                    else if (primeFactors_w[prime] > primeFactors_h[prime])
                    {
                        primeFactors_w[prime] -= primeFactors_h[prime];
                        primeFactors_h.Remove(prime);
                    }
                    else
                    {
                        primeFactors_h[prime] -= primeFactors_w[prime];

                        primeFactors_w.Remove(prime);
                    }
                }
            }

            int rw = PrimeFactorsResult(primeFactors_w);
            int rh = PrimeFactorsResult(primeFactors_h);

            return new Size(rw, rh);
        }

        static public int PrimeFactorsResult(Dictionary<int, int> primeFactors)
        {
            int result = 1;

            foreach (var primeItem in primeFactors)
            {
                var prime = primeItem.Key;
                var nb = primeItem.Value;
                for (int i = 0; i < nb; i++)
                    result *= prime;
            }

            return result;
        }

        static public Dictionary<int, int> PrimeFactors(int n)
        {
            Dictionary<int, int> result = [];
            int nb;

            // Prints all the numbers of 2  
            while (n % 2 == 0)
            {
                if (result.TryGetValue(2, out int value))
                    nb = value + 1;
                else
                    nb = 1;
                result[2] = nb;
                n /= 2;
            }

            // As no 2 can be further divided, this probably means that n
            // is now an odd number
            for (int i = 3; i <= Math.Sqrt(n); i += 2)
            {
                while (n % i == 0)
                {
                    if (result.TryGetValue(i, out int value))
                        nb = value + 1;
                    else
                        nb = 1;
                    result[i] = nb;
                    n /= i;
                }
            }

            // This is for case if n is greater than 2
            if (n > 2)
            {
                result[n] = 1;
            }
            return result;
        }

        static public Size? SizeFromString(String str)
        {
            if(String.IsNullOrEmpty(str))
                return null;

            var d = str.Split('x');
            if(d.Length != 2)
                return null;
            try
            {
                return new Size(Int32.Parse(d[0]), Int32.Parse(d[1]));
            }
            catch { }

            return null;
        }

    }
}
