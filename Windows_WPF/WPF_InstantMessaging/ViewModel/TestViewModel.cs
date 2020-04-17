using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

using InstantMessaging.Helpers;

namespace InstantMessaging.ViewModel
{
    public class TestViewModel : ObservableObject
    {

        ICommand m_buttonTestAdd2AtStartCommand;
        public ICommand ButtonTestAdd2AtStartCommand
        {
            get { return m_buttonTestAdd2AtStartCommand; }
            set { m_buttonTestAdd2AtStartCommand = value; }
        }

        ICommand m_buttonTestAdd2InMiddleCommand;
        public ICommand ButtonTestAdd2InMiddleCommand
        {
            get { return m_buttonTestAdd2InMiddleCommand; }
            set { m_buttonTestAdd2InMiddleCommand = value; }
        }

        ICommand m_buttonTestAdd2AtEndCommand;
        public ICommand ButtonTestAdd2AtEndCommand
        {
            get { return m_buttonTestAdd2AtEndCommand; }
            set { m_buttonTestAdd2AtEndCommand = value; }
        }


        public TestViewModel()
        {
            m_buttonTestAdd2AtStartCommand = new RelayCommand<object>(new Action<object>(TestAdd2AtStartCommand));
        }

        public void TestAdd2AtStartCommand(object obj)
        {
            // TO DO
        }

    }
}
