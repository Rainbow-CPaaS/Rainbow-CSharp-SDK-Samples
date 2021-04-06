using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace SDK.WpfApp.Model
{
    public class LayoutModel : ObservableObject
    {
        String m_CurrentExpanded;

        Boolean m_automaticDisplayMode;

        public VideoElementModel LocalVideo { get; set; }

        public VideoElementModel RemoteVideo { get; set; }

        public VideoElementModel Sharing { get; set; }

        /// <summary>
        /// To know if automatic display mode is used 
        /// </summary>
        public Boolean AutomaticDisplayMode
        { 
            get { return m_automaticDisplayMode; }
            set {
                if (SetProperty(ref m_automaticDisplayMode, value))
                {
                }

            }
        }

        /// <summary>
        /// To know which one is expanded
        /// </summary>
        public String CurrentExpanded
        {
            get { return m_CurrentExpanded; }
            set { SetProperty(ref m_CurrentExpanded, value); }
        }

        public LayoutModel ()
        {
            LocalVideo = new VideoElementModel();
            RemoteVideo = new VideoElementModel();
            Sharing = new VideoElementModel();

            AutomaticDisplayMode = true;
        }
    }
}
