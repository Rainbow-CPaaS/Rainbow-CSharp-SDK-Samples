using System;
using System.Collections.Generic;
using System.Text;

namespace SDK.WpfApp.Model
{
    public class DeviceModel : ObservableObject
    {
        String m_name;
        String m_id;

        /// <summary>
        /// The name of the device
        /// </summary>
        public String Name
        {
            get { return m_name; }
            set { SetProperty(ref m_name, value); }
        }

        /// <summary>
        /// Id of the device
        /// </summary>
        public String Id
        {
            get { return m_id; }
            set { SetProperty(ref m_id, value); }
        }
    }
}
