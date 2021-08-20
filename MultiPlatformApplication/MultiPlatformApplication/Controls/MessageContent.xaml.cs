using MultiPlatformApplication.Effects;
using MultiPlatformApplication.Events;
using MultiPlatformApplication.Helpers;
using MultiPlatformApplication.Models;
using Rainbow;
using Rainbow.Model;
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
    public partial class MessageContent : ContentView
    {
        public static double MINIMAL_MESSAGE_WIDTH = 120;

        // Event raised when we want to display an Action menu related to a message
        // To propose actions like: Edit, Dwl, Reply, Fwd, Save, Copy, Delete
        // Rect provided is the position relative to the frits parent as "Relativelayout"
        public event EventHandler<RectEventArgs> ActionMenuToDisplay;

        private MessageElementModel message = null;

        private MessageContentUrgency messageContentUrgency = null;
        private MessageContentReply messageContentReply = null;
        private MessageContentBody messageContentBody = null;
        private MessageContentBodyWithImg messageContentBodyWithImg = null;
        private MessageContentBodyWithCode messageContentBodyWithCode = null;
        private MessageContentAttachment messageContentAttachment = null;
        private MessageContentDeleted messageContentDeleted = null;
        private MessageContentForward messageContentForward = null;

        // Define elements related to BtnAction UI Components
        private CustomButton BtnAction = null; // BtnAction UI Component
        private Boolean needActionButton = false; // To know if we need the BtnAction UI Component
        private int BtnActionIndex = -1; // To know where to add BtnAction in the Grid
        private MouseOverAndOutModel mouseCommands { get; set; } // Commands used to manage Mouse Out / Over  - Useful only we the BtnAction is needed


        // Define elements used in Long Press scenario to display eventually an Action Menu
        private CancelableDelay cancelableDelayToAskActionMenuDisplay = null;
        private int delayBeforeActionMenuDisplay = 500; // in ms - default value in iOS
        private bool longPressStarted = false;

        public MessageContent()
        {
            InitializeComponent();

            BowViewForMinimalWidth.WidthRequest = MINIMAL_MESSAGE_WIDTH;

            if (Helper.IsDesktopPlatform())
            {
                // Add Mouse Out/Over management only for desktop platforms
                needActionButton = true;
            }
            else
            {
                // Add touch effects for others platforms
                AddTouchEffect();
            }
        }

        private void AddTouchEffect()
        {
            TouchEffect touchEffect = new TouchEffect();
            touchEffect.TouchAction += TouchEffect_TouchAction;
            Helper.AddEffect(RootGrid, touchEffect);

            longPressStarted = false;
        }

        private void TouchEffect_TouchAction(object sender, TouchActionEventArgs e)
        {
            // We don't care if Middle or Right Mouse button is used.
            if ((e.MouseButton == TouchMouseButton.Middle)
                        || (e.MouseButton == TouchMouseButton.Right))
                return;

            switch (e.Type)
            {
                case TouchActionType.Cancelled:
                case TouchActionType.Entered:
                case TouchActionType.Exited:
                case TouchActionType.Moved:
                case TouchActionType.Released:
                    if (longPressStarted)
                    {
                        longPressStarted = false;
                        if (cancelableDelayToAskActionMenuDisplay != null)
                        {
                            cancelableDelayToAskActionMenuDisplay.Cancel();
                            cancelableDelayToAskActionMenuDisplay = null;
                        }
                    }
                    break;

                case TouchActionType.Pressed:
                    longPressStarted = true;
                    if (cancelableDelayToAskActionMenuDisplay != null)
                    {
                        cancelableDelayToAskActionMenuDisplay.Cancel();
                        cancelableDelayToAskActionMenuDisplay = null;
                    }
                    
                    // Here e.Location get the press Location relatively to the MessageContext UI Component
                    cancelableDelayToAskActionMenuDisplay = CancelableDelay.StartAfter(delayBeforeActionMenuDisplay, () => NeedToDisplayActionMenu(sender, e.Location) );
                    break;
            }
        }

        private void CreateBtnAction()
        {
            BtnAction = new CustomButton();
            BtnAction.HorizontalOptions = new LayoutOptions(LayoutAlignment.End, false);
            BtnAction.VerticalOptions = new LayoutOptions(LayoutAlignment.Start, false);

            BtnAction.IsVisible = false;

            BtnAction.CornerRadius = 2;
            BtnAction.Padding = new Thickness(2,2,0,0);
            BtnAction.Margin = new Thickness(1,2,2,2);

            BtnAction.HeightRequest = 22;
            BtnAction.WidthRequest = 22;
            BtnAction.ImageSize = 20;

            mouseCommands = new MouseOverAndOutModel();
            mouseCommands.MouseOverCommand = new RelayCommand<object>(new Action<object>(MouseOverCommand));
            mouseCommands.MouseOutCommand = new RelayCommand<object>(new Action<object>(MouseOutCommand));

            //Add mouse over effect
            MouseOverEffect.SetCommand(RootGrid, mouseCommands.MouseOverCommand);

            //Add mouse out effect
            MouseOutEffect.SetCommand(RootGrid, mouseCommands.MouseOutCommand);

            BtnAction.Command = new RelayCommand<object>(new Action<object>(BtnActionCommand));
        }

        private void NeedToDisplayActionMenu(object sender, Point location = default)
        {
            if (sender is VisualElement visualElement)
            {
                Rect rect = Helper.GetRelativePosition(visualElement, typeof(RelativeLayout));

                // If sender is not a CustomButtom it means that a long press has bee used
                // We simulate the finger size by a square of 24x24
                // And we need to take care of the location too
                if (!(sender is CustomButton))
                {
                    rect.Width = 24;
                    rect.Height = 24;

                    rect.X += location.X;
                    rect.Y += location.Y;
                }
                ActionMenuToDisplay.Raise(this, new RectEventArgs(rect));
            }
        }

        private void BtnActionCommand(object obj)
        {
            NeedToDisplayActionMenu(obj);
        }

        private void MouseOverCommand(object obj)
        {
            if (needActionButton)
            {
                BtnAction.IsVisible = true;
            }
        }

        private void MouseOutCommand(object obj)
        {
            if (needActionButton)
            {
                BtnAction.IsVisible = false;
            }
        }

        protected override void OnBindingContextChanged()
        {
            if ((BindingContext != null) && (message == null))
            {
                message = (MessageElementModel)BindingContext;
                if (message != null)
                {
                    Color color;
                    Color backgroundColor;

                    // Add Urgency element ?
                    if (message.Content.Urgency != UrgencyType.Std)
                    {
                        if (messageContentUrgency == null)
                        {
                            messageContentUrgency = new MessageContentUrgency();
                            messageContentUrgency.BindingContext = message;
                            ContentGrid.Children.Add(messageContentUrgency, 0, 0);

                            // Need to define here the BackgroundColor of the HeaderGrid to ensure to have correct display if the message body is short
                            Helper.GetUrgencyInfo(message.Content.Urgency.ToString(), out backgroundColor, out color, out String title, out String label, out String imageSourceId);
                            ContentGrid.BackgroundColor = backgroundColor;
                        }
                    }

                    // Add forward header
                    if (message.IsForwarded)
                    {
                        if (messageContentForward == null)
                        {
                            messageContentForward = new MessageContentForward();
                            messageContentForward.BindingContext = message;
                            ContentGrid.Children.Add(messageContentForward, 0, 0);
                        }
                    }

                    // Add Reply element ?
                    if (message.Reply != null)
                    {
                        if (messageContentReply == null)
                        {
                            messageContentReply = new MessageContentReply();
                            messageContentReply.BindingContext = message;
                            ContentGrid.Children.Add(messageContentReply, 0, 1);
                        }
                    }

                    // Add Body element ?
                    if (!String.IsNullOrEmpty(message.Content?.Body))
                    {
                        if (message.Content.Body.StartsWith("/img", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (messageContentBodyWithImg == null)
                            {
                                messageContentBodyWithImg = new MessageContentBodyWithImg();
                                messageContentBodyWithImg.BindingContext = message;
                                ContentGrid.Children.Add(messageContentBodyWithImg, 0, 2);
                            }
                        }
                        else if (message.Content.Body.StartsWith("/code", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (messageContentBodyWithCode == null)
                            {
                                messageContentBodyWithCode = new MessageContentBodyWithCode();
                                messageContentBodyWithCode.BindingContext = message;
                                ContentGrid.Children.Add(messageContentBodyWithCode, 0, 2);
                            }
                        }
                        else if (messageContentBody == null)
                        {
                            messageContentBody = new MessageContentBody();
                            messageContentBody.BindingContext = message;
                            ContentGrid.Children.Add(messageContentBody, 0, 2);
                        }

                        BtnActionIndex = 2;

                    }
                    else if (message.Content.Type == "deletedMessage")
                    {
                        // We don't need action button in this case
                        needActionButton = false;

                        // Create MessageContentDeleted
                        messageContentDeleted = new MessageContentDeleted();
                        messageContentDeleted.BindingContext = message;
                        ContentGrid.Children.Add(messageContentDeleted, 0, 2);
                    }

                    // Add Attachment element ?
                    if (message.Content.Attachment != null)
                    {
                        if (messageContentAttachment == null)
                        {
                            messageContentAttachment = new MessageContentAttachment();
                            messageContentAttachment.BindingContext = message;
                            ContentGrid.Children.Add(messageContentAttachment, 0, 3);

                            if (BtnActionIndex == -1)
                                BtnActionIndex = 3;
                        }
                    }

                    // Set background color
                    if (message.Peer.Id == Helper.SdkWrapper.GetCurrentContactId())
                    {
                        color = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageCurrentUserFont");
                        backgroundColor = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageCurrentUserBackGround");
                    }
                    else
                    {
                        color = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageOtherUserFont");
                        backgroundColor = Helper.GetResourceDictionaryById<Color>("ColorConversationStreamMessageOtherUserBackGround");
                    }

                    Frame.BackgroundColor = backgroundColor;

                    // Add Btn Action in the correct place
                    if (needActionButton && (BtnActionIndex != -1))
                    {
                        CreateBtnAction();

                        BtnAction.ImageSourceId = "Font_EllipsisV|" + color.ToHex();
                        BtnAction.BackgroundColor = backgroundColor;
                        BtnAction.BackgroundColorOnMouseOver = ColorInterpolator.InterpolateBetween(color, backgroundColor, 0.5);

                        ContentGrid.Children.Add(BtnAction, 0, BtnActionIndex);
                    }
                }
            }
        }
    }
}