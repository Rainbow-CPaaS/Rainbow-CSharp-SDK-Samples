using MultiPlatformApplication.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MultiPlatformApplication.Models
{
    public class PopupModel: ObservableObject
    {
        String title;
        String acceptText;
        String cancelText;

        Boolean acceptIsVisible;
        Boolean cancelIsVisible;

        ICommand buttonAcceptCommand;
        ICommand buttonCancelCommand;

        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public string AcceptText
        {
            get { return acceptText; }
            set { SetProperty(ref acceptText, value); }
        }

        public string CancelText
        {
            get { return cancelText; }
            set { SetProperty(ref cancelText, value); }
        }

        public Boolean AcceptIsVisible
        {
            get { return acceptIsVisible; }
            set { SetProperty(ref acceptIsVisible, value); }
        }

        public Boolean CancelIsVisible
        {
            get { return cancelIsVisible; }
            set { SetProperty(ref cancelIsVisible, value); }
        }

        public ICommand ButtonAcceptCommand
        {
            get { return buttonAcceptCommand; }
            set { SetProperty(ref buttonAcceptCommand, value); }
        }

        public ICommand ButtonCancelCommand
        {
            get { return buttonCancelCommand; }
            set { SetProperty(ref buttonCancelCommand, value); }
        }

        public PopupModel()
        {
            AcceptText = Helper.GetLabel("ok");
            CancelText = Helper.GetLabel("cancel");
        }

    }
}
