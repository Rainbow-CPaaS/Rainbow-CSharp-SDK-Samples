using System;
using Rainbow.Medias;

namespace SDK.UIForm.WebRTC
{
    public class MediaInputStreamItem
    {
        public String Id { get; set; }

        public String Name { get; set; }

        public MediaInputStreamItem(IMedia media)
        {
            if (media == null)
                throw new ArgumentNullException("media", "media parameter is null");

            if (String.IsNullOrEmpty(media.Id))
                throw new ArgumentNullException("media", "media.Id is null or empty");

            Id = media.Id;
            Name = "";

            //if (media.HasAudio())
            //{
            //    if (media.HasVideo())
            //        Name = "A + V";
            //    else
            //        Name = "A";
            //}
            //else if (media.HasVideo())
            //    Name = "V";
                

            //if (media is MediaInput mediaInput)
            //    Name = mediaInput.Path;
            //else if (media is MediaFiltered mediaFiltered)
            //{
            //    var mediaInputs = mediaFiltered.MediaInputs;
            //    if(mediaInputs != null)
            //        Name = String.Join(",", mediaInputs.Keys.ToList());
            //}
            //else
            //    throw new ArgumentNullException("media", "This type of media is not supported");

            if (String.IsNullOrEmpty(Name))
                Name = Id;
            else
                Name = $"{Id}-[{Name}]";

        }

        public override string ToString()
        {
            return Name;
        }
    }
}
