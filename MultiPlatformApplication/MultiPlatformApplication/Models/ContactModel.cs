using MultiPlatformApplication.Helpers;
using Rainbow.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MultiPlatformApplication.Models
{
    public class ContactModel: GroupModel
    {
        public PeerModel Peer { get; set; }

        public String Detail { get; set; }

        public String Presence { get; set; }

        public String FirstNameForSort { get; set; }
        public String LastNameForSort { get; set; }
        public String CompanyForSort { get; set; }

        public ICommand SelectionCommand { get; set; }

        public ContactModel(String groupdDisplayName)
        {
            GroupName = groupdDisplayName;
            Peer = new PeerModel();
        }

        public ContactModel(Contact contact)
        {
            Peer = new PeerModel(contact);
            Peer.DisplayPresence = true;

            Detail = contact.CompanyName;

            Rainbow.Model.Presence presence = Helper.SdkWrapper.GetAggregatedPresenceFromContactId(contact.Id);
            Presence = presence?.PresenceLevel;

            if(String.IsNullOrEmpty(contact.FirstName))
                FirstNameForSort = Peer.DisplayName.ToUpper();
            else
                FirstNameForSort = contact.FirstName.ToUpper();

            if (String.IsNullOrEmpty(contact.LastName))
                LastNameForSort = Peer.DisplayName.ToUpper();
            else
                LastNameForSort = contact.LastName.ToUpper();

            CompanyForSort = contact.CompanyName?.ToUpper();

        }
    }
}
