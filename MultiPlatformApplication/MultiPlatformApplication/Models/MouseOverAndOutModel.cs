using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiPlatformApplication.Models
{
    public class MouseOverAndOutModel : ObservableObject
    {
        RelayCommand<object> m_mouseOverCommand;
        RelayCommand<object> m_mouseOutCommand;

        public RelayCommand<object> MouseOverCommand
        {
            get { return m_mouseOverCommand; }
            set { SetProperty(ref m_mouseOverCommand, value); }
        }

        
        public RelayCommand<object> MouseOutCommand
        {
            get { return m_mouseOutCommand; }
            set { SetProperty(ref m_mouseOutCommand, value); }
        }
    }
}
