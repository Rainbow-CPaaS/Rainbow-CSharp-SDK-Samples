using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace SDK.WpfApp.Model
{
    public class LanguagesModel : ObservableObject
    {
        private ObservableCollection<String> m_languages;

        private String m_languageSelected;

        ICommand m_buttonSetCommand;

        public ObservableCollection<String> Languages
        {
            get { return m_languages; }
            set { SetProperty(ref m_languages, value); }
        }

        public String LanguageSelected
        {
            get { return m_languageSelected; }
            set { SetProperty(ref m_languageSelected, value); }
        }

        /// <summary>
        /// To set new language
        /// </summary>
        public ICommand ButtonSetCommand
        {
            get { return m_buttonSetCommand; }
            set { m_buttonSetCommand = value; }
        }

        public LanguagesModel()
        {
            Languages = new ObservableCollection<String>();

            LanguageSelected = "en";
        }
    }
}
