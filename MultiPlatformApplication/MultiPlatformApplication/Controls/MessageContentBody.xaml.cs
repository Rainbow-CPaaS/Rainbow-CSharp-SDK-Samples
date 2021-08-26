using MultiPlatformApplication.Effects;
using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using Rainbow.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MultiPlatformApplication.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessageContentBody : ContentView
    {
        Object lockEditor = new Object();
        private MessageElementModel message = null;
        private EditorExpandableWithMaxLines editor;

        public MessageContentBody()
        {
            InitializeComponent();

            // On Desktop platform, Label can be selected
            if (Helper.IsDesktopPlatform())
                SelectableLabelEffect.SetEnabled(Label, true);

            this.BindingContextChanged += MessageContentBody_BindingContextChanged;
        }

        private void MessageContentBody_BindingContextChanged(object sender, EventArgs e)
        {
            if ( (BindingContext != null) && (message == null) )
            {
                message = (MessageElementModel)BindingContext;
                if (message != null)
                {
                    String colorKey;
                    String backgroundColorKey;

                    if (message.Peer.Id == Helper.SdkWrapper.GetCurrentContactId())
                    {
                        colorKey = "ColorConversationStreamMessageCurrentUserFont";
                        backgroundColorKey = "ColorConversationStreamMessageCurrentUserBackGround";

                        // We need to check if this message will be edited or not
                        Helper.SdkWrapper.StartMessageEdition += SdkWrapper_StartMessageEdition;

                        // We need to check if we have to stop message edition
                        Helper.SdkWrapper.StopMessageEdition += SdkWrapper_StopMessageEdition;

                        // We need to check if we have to send message edition
                        Helper.SdkWrapper.SendMessageEdition += SdkWrapper_SendMessageEdition; ;
                    }
                    else
                    {
                        colorKey = "ColorConversationStreamMessageOtherUserFont";
                        backgroundColorKey = "ColorConversationStreamMessageOtherUserBackGround";
                    }

                    Label.TextColor = Helper.GetResourceDictionaryById<Color>(colorKey);
                    BackgroundColor = Helper.GetResourceDictionaryById<Color>(backgroundColorKey);
                }
            }
        }

        private void SdkWrapper_SendMessageEdition(object sender, IdEventArgs e)
        {
            if (e.Id == message?.Id)
                EditorValidationCommand(null);
        }

        private void SdkWrapper_StartMessageEdition(object sender, Rainbow.Events.IdEventArgs e)
        {
            if(e.Id == message?.Id)
                AddEditionUI();
        }

        private void SdkWrapper_StopMessageEdition(object sender, StringListEventArgs e)
        {
            if ( (e.Values?.Count > 0) && (e.Values[0] == message?.Id) )
                RemoveEditionUI();
        }

        private void CreateEditorComponent()
        {
            editor = new EditorExpandableWithMaxLines();
            editor.BackgroundColor = Helper.GetResourceDictionaryById<Color>("ColorEntryBackground");
            editor.VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false);
            editor.MaxLines = 5;
            editor.BreakLineModifier = "shift";
            editor.ValidationCommand = new RelayCommand<object>(new Action<object>(EditorValidationCommand));
            editor.MinimumWidth = MessageContent.MINIMAL_MESSAGE_WIDTH * 2;

            editor.PropertyChanged += Editor_PropertyChanged;
        }

        private void Editor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // To ensure to give focus to the editor we have to wait a litle after the Renderer has been set
            //  - OK on UWP (Debug mode)
            //  - OK on Android Emulator
            if (e.PropertyName == "Renderer")
            {
                Device.StartTimer(new TimeSpan(0, 0, 0, 0, 200), () =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        editor.SetFocus(true);
                    });
                    return false;
                });
            }
        }

        private void AddEditionUI()
        {
            lock (lockEditor)
            {
                if (editor == null)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        CreateEditorComponent();

                        // Hide current label
                        Label.IsVisible = false;

                        // Set default text of the editor
                        editor.Text = Label.Text;

                        // Add it to the stack layout
                        MainGrid.Children.Add(editor);

                    });
                }
            }
        }

        private void RemoveEditionUI()
        {
            lock (lockEditor)
            {
                if (editor != null)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        editor.PropertyChanged -= Editor_PropertyChanged;

                        // To close keyboard in Android / iOS
                        editor.SetFocus(false);

                        // Remove editor from stack layout
                        MainGrid.Children.Remove(editor);

                        // Set to null
                        editor = null;

                        // Display label instead
                        Label.IsVisible = true;
                    });
                }
            }
        }

        private void EditorValidationCommand(object obj)
        {
            List<String> info = new List<string>();
            info.Add(message?.Id);

            String newText = editor.GetEditorText().Trim();
            String oldTExt = Label.Text;

            if(!newText.Equals(oldTExt))
                info.Add(newText);

            Helper.SdkWrapper.OnStopMessageEdition(this, new StringListEventArgs(info));
        }
    }
}