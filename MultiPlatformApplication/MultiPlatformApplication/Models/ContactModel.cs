using MultiPlatformApplication.Helpers;
using Rainbow.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiPlatformApplication.Models
{
    public class ContactModel: GroupModel
    {
        public String DisplayName { get; set; }

        public String Detail { get; set; }

        public String Presence { get; set; }

        public String FirstNameForSort { get; set; }
        public String LastNameForSort { get; set; }
        public String CompanyForSort { get; set; }

        public ContactModel(String id, String displayName, String presence = null, String detail = null)
        {
            Id = id;
            DisplayName = displayName;
            Presence = presence;
            Detail = detail;
        }

        public ContactModel(Contact contact)
        {
            Id = contact.Id;
            DisplayName = Rainbow.Util.GetContactDisplayName(contact);
            Detail = contact.CompanyName;

            Rainbow.Model.Presence presence = Helper.SdkWrapper.GetAggregatedPresenceFromContactId(contact.Id);
            Presence = presence?.PresenceLevel;


            FirstNameForSort = contact.FirstName?.ToUpper();
            LastNameForSort = contact.FirstName?.ToUpper();
            CompanyForSort = contact.CompanyName?.ToUpper();
        }
    }
}
