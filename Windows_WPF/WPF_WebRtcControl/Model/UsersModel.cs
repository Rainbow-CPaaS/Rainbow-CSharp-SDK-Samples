using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Rainbow;
using Rainbow.Model;

namespace SDK.WpfApp.Model
{
    public class UsersModel : ObservableObject
    {
        private Object lockUsers = new Object();

        private String m_currentContactId;
        private ObservableCollection<UserModel> m_users;
        private UserModel m_userSelected;

        public ObservableCollection<UserModel> Users
        {
            get { return m_users; }
            set { SetProperty(ref m_users, value); }
        }

        public UserModel UserSelected
        {
            get { return m_userSelected; }
            set { SetProperty(ref m_userSelected, value); }
        }

        public String CurrentContactId
        {
            get { return m_currentContactId; }
            set { SetProperty(ref m_currentContactId, value); }
        }

        public UsersModel()
        {
            Users = new ObservableCollection<UserModel>();

            UserSelected = new UserModel();
        }

        public void AddContact(Contact contact)
        {
            if (contact == null)
                return;

            if (contact.Id == CurrentContactId)
                return;

            lock (lockUsers)
            {
                //TODO - need to check if not already present
                Users.Add(UserModelFromContact(contact));
                SortByDisplayName();
            }
        }

        public void AddContacts(List<Contact> contacts)
        {
            if (contacts == null)
                return;

            lock (lockUsers)
            {
                Users.Clear();

                foreach (Contact contact in contacts)
                {
                    if (contact.Id != CurrentContactId)
                    {
                        //TODO - need to check if not already present
                        Users.Add(UserModelFromContact(contact));
                    }
                }
                SortByDisplayName();
            }
        }

        /// <summary>
        /// To sort by display name users' list
        /// </summary>
        public void SortByDisplayName()
        {
            Users = new ObservableCollection<UserModel>(from i in Users orderby i.DisplayName select i);
        }

        private UserModel UserModelFromContact(Contact contact)
        {
            return new UserModel() { Id = contact.Id, Jid = contact.Jid_im, DisplayName = Util.GetContactDisplayName(contact) };
        }
    }
}
