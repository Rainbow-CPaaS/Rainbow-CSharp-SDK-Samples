using System;
using System.Collections.Generic;
using System.Text;

namespace MultiPlatformApplication.Models
{
    public class ContactModel: GroupModel
    {
        public String DisplayName { get; set; }

        public String Detail { get; set; }

        public ContactModel(String id, String displayName, String detail = null)
        {
            Id = id;
            DisplayName = displayName;
            Detail = detail;
        }
    }
}
