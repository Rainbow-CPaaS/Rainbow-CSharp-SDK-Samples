using System;
using System.Collections.Generic;
using System.Text;

namespace Rainbow.Example.Common
{
    public class StreamUsage
    {
        public int Media; // Medias used by this stream : Rainbow.Consts.Media.Audio, Rainbow.Consts.Media.Video BUT NOT Rainbow.Consts.Media.Sharing, 

        public String Id = ""; // Id of the Stream


        static public int MediaFromString(String mediasAsString) //  "audio", "video", "audio+video", "composition";
        {
            switch(mediasAsString)
            {
                case "audio":
                    return Rainbow.Consts.Media.AUDIO;

                case "video":
                case "composition":
                    return Rainbow.Consts.Media.VIDEO;

                case "audio+video":
                    return Rainbow.Consts.Media.AUDIO + Rainbow.Consts.Media.VIDEO;

               default:
                    return Rainbow.Consts.Media.NONE;
            }

        }

    }
}
