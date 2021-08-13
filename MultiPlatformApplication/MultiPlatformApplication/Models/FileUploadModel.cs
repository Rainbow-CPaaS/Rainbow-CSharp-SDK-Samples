using Rainbow.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Essentials;

namespace MultiPlatformApplication.Models
{
    public class FileUploadModel
    {
        public String PeerId { get; set; }

        public String PeerType { get; set; }

        public UrgencyType Urgency { get; set; }

#region FILE ATTRIBUTES        

        public Stream Stream { get; set; }

        public String FileFullPath{ get; set; }

        public String FileName { get; set; }

        public long FileSize { get; set; }

#endregion FILE ATTRIBUTES        


#region RB SDK ATTRIBUTES        

        public FileDescriptor FileDescriptor { get; set; }

        public Rainbow.Model.Message RbMessage { get; set; }

#endregion RB SDK ATTRIBUTES        
    }
}
