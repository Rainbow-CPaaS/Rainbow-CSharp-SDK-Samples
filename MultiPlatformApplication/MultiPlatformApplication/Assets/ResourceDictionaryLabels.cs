using Rainbow.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Assets
{
    // Used in App.Xaml file - permit from xaml file to access labels
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ResourceDictionaryLabels: ResourceDictionary
    {
        private static Languages Languages = null;

        public ResourceDictionaryLabels()
        {
            // Get Rainbow Languages service
            Languages = Rainbow.Common.Languages.Instance;

            UpdateLabels();

            // Want to update labels if langages is changed
            Languages.LanguageChanged += Languages_LanguageChanged;
        }

        private void Languages_LanguageChanged(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                UpdateLabels();
            });
        }

        private void UpdateLabels()
        {
            Dictionary<String, String> labels = Languages.GetLabels();

            foreach (String key in labels.Keys)
            {
                String labelKey = "Label_" + key;

                if (this.ContainsKey(labelKey))
                    this[labelKey] = labels[key];
                else
                    Add(labelKey, labels[key]);
            }
        }

    }
}
