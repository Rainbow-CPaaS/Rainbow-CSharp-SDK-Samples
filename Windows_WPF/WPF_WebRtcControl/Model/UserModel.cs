using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.WpfApp.Model
{
    public class UserModel : ObservableObject
    {
        String m_displayName;
        String m_id;
        String m_jid;

        /// <summary>
        /// User display name
        /// </summary>
        public String DisplayName
        {
            get { return m_displayName; }
            set { SetProperty(ref m_displayName, value); }
        }

        /// <summary>
        /// Id of the user
        /// </summary>
        public String Id
        {
            get { return m_id; }
            set { SetProperty(ref m_id, value); }
        }

        /// <summary>
        /// Jid of the user
        /// </summary>
        public String Jid
        {
            get { return m_jid; }
            set { SetProperty(ref m_jid, value); }
        }
    }
}
