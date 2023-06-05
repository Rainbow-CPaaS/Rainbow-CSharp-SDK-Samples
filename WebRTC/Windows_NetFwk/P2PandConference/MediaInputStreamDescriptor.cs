using System;
using System.Collections.Generic;

namespace SDK.UIForm.WebRTC
{
    internal class MediaInputStreamDescriptor
    {
        private static List<String> _validTypes = new List<string>() { "ScreenDevice", "WebcamDevice", "AudioInputDevice", "FileOrStream", "MultiInputs" };

        /// <summary>
        /// Id (name) of Media Input - must be unique to all Media Input 
        /// </summary>
        public String Id { get; set; }

        /// <summary>
        /// To know the type of Media Input
        /// 
        /// Possibles values are: ScreenDevice, WebcamDevice, AudioInputDevice, FileOrStream, MultiInputs
        /// </summary>
        public String Type { get; set; }

        /// <summary>
        /// Uri used by this MediaInput - not used if it's a MultiInputs
        /// </summary>
        public String? Uri { get; set; }

        /// <summary>
        /// List of Media Input Id which are used - only used if it's a MultiInputs
        /// </summary>
        public List<String>? MediaInputIdList { get; set; }

        /// <summary>
        /// List of Media Input Id which are used for the filter - only used if it's a MultiInputs
        /// </summary>
        public List<String>? MediaInputIdListForVideoFilter { get; set; }

        /// <summary>
        /// Video Filter used for this Media Inputs - only used if it's a MultiInputs
        /// </summary>
        public String? VideoFilter { get; set; }

        /// <summary>
        /// To know if Audio is used by this Media Input
        /// </summary>
        public Boolean Audio{ get; set; }

        /// <summary>
        /// To know if Video is used by this Media Input
        /// </summary>
        public Boolean Video { get; set; }

        /// <summary>
        /// To know if we loop automatically on this stream
        /// </summary>
        public Boolean Loop { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MediaInputStreamDescriptor()
        {
            Id = "";
            Type = "";
            Uri = null;
            MediaInputIdList = null;
            VideoFilter = null;

            Audio = false;
            Video = false;
            Loop = true;

        }

        public Boolean IsValid()
        {
            if (String.IsNullOrEmpty(Id))
                return false;

            if (!_validTypes.Contains(Type))
                return false;

            if ((!Audio) && (!Video))
                return false;

            if(String.IsNullOrEmpty(Uri))
            {
                if (MediaInputIdList == null)
                    return false;

                if (MediaInputIdList.Count == 0)
                    return false;

            }
                

            return true;

        }
    }
}
