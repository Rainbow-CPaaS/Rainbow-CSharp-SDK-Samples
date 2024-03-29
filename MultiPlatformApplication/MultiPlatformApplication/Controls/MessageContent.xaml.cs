﻿using MultiPlatformApplication.Effects;
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
using Xamarin.Essentials;
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

        private int BtnPosition = -1; // To know where to add BtnAction in the Grid OR the cancel edit button

        // Define elements related to BtnAction UI Components
        private CustomButton BtnAction = null; 
        private Boolean needActionButton = false; // To know if we need the BtnAction UI Component
        private MouseOverAndOutModel btnActionMouseCommands { get; set; } // Commands used to manage Mouse Out / Over on BtnAction - if used/needed

        // Define elements used in Long Press scenario to display eventually an Action Menu
        private CancelableDelay cancelableDelayForLongPress = null;
        private bool longPressStarted = false;
        private Point longPressInitialLocation;

        Color color;
        Color backgroundColor;
        Color interpolateColor;

        public MessageContent()
        {
            InitializeComponent();

            BowViewForMinimalWidth.WidthRequest = MINIMAL_MESSAGE_WIDTH;

            // We need another column to display:
            //      - Action button (Destop only with mouse over / out management)
            //      - Cancel message edition button

            ColumnDefinition columnDefinition = new ColumnDefinition();

            if (Helper.IsDesktopPlatform())
            {
                columnDefinition.Width = 24; // Need a fix size : Action button OR Cancel message edition button

                // Add Mouse Out/Over management only for desktop platforms
                needActionButton = true;
            }
            else
            {
                columnDefinition.Width = GridLength.Auto; // Necessary only for Cancel message edition button

                // Add touch effects for others platforms
                AddTouchEffect();
            }

            ContentGrid.ColumnDefinitions.Add(columnDefinition);
        }

        private void NeedToDisplayActionMenu(object sender, Point location = default)
        {
            if (sender is View view)
            {
                //Rect rect = Helper.GetRelativePosition(visualElement, typeof(RelativeLayout));
                Rect rect = Popup.GetRectOfView(view);

                // If sender is not a CustomButtom it means that a long press has been used
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

        protected override void OnBindingContextChanged()
        {
            if ((BindingContext != null) && (message == null))
            {
                message = (MessageElementModel)BindingContext;
                if (message != null)
                {

                    // Add Urgency element ?
                    if (message.Content.Urgency != UrgencyType.Std)
                    {
                        if (messageContentUrgency == null)
                        {
                            messageContentUrgency = new MessageContentUrgency();
                            messageContentUrgency.BindingContext = message;
                            ContentGrid.Children.Add(messageContentUrgency, 0, 0);
                            Grid.SetColumnSpan(messageContentUrgency, 2);
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
                            Grid.SetColumnSpan(messageContentForward, 2);
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
                            Grid.SetColumnSpan(messageContentReply, 2);
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

                        BtnPosition = 2;

                    }
                    else if (message.Content.Type == "deletedMessage")
                    {
                        // We don't need action button in this case
                        needActionButton = false;

                        // Create MessageContentDeleted
                        messageContentDeleted = new MessageContentDeleted();
                        messageContentDeleted.BindingContext = message;
                        ContentGrid.Children.Add(messageContentDeleted, 0, 2);
                        Grid.SetColumnSpan(messageContentDeleted, 2);
                    }

                    // Add Attachment element ?
                    if (message.Content.Attachment != null)
                    {
                        if (messageContentAttachment == null)
                        {
                            messageContentAttachment = new MessageContentAttachment();
                            messageContentAttachment.BindingContext = message;
                            ContentGrid.Children.Add(messageContentAttachment, 0, 3);

                            if (BtnPosition == -1)
                                BtnPosition = 3;
                            else
                                Grid.SetColumnSpan(messageContentAttachment, 2);
                        }
                    }

                    // Are we managing a msg of the current user ?
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
                    interpolateColor = ColorInterpolator.InterpolateBetween(color, backgroundColor, 0.5); 

                    
                    Frame.BackgroundColor = backgroundColor;

                    // Add Btn Action in the correct place
                    if (needActionButton && (BtnPosition != -1))
                    {
                        CreateBtnAction();

                        BtnAction.ImageSourceId = null;
                        BtnAction.BackgroundColor = backgroundColor;
                        BtnAction.BackgroundColorOnMouseOver = interpolateColor;

                        ContentGrid.Children.Add(BtnAction, 1, BtnPosition);
                    }
                }
            }
        }

#region TOUCH EFECT RELATED

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

            Boolean cancelLongPress = false;
            Boolean startLongPress = false;

            switch (e.Type)
            {
                case TouchActionType.Cancelled:
                case TouchActionType.Entered:
                case TouchActionType.Exited:
                case TouchActionType.Released:
                    if(longPressStarted)
                        cancelLongPress = true;
                    break;

                case TouchActionType.Moved:
                    if(longPressStarted)
                        cancelLongPress = (Math.Abs(e.Location.X - longPressInitialLocation.X) > 15)
                                            || (Math.Abs(e.Location.Y - longPressInitialLocation.Y) > 15);
                    break;

                case TouchActionType.Pressed:
                    cancelLongPress = true;
                    startLongPress = true;
                    break;
            }

            if(cancelLongPress)
            {
                longPressStarted = false;
                if (cancelableDelayForLongPress != null)
                {
                    cancelableDelayForLongPress.Cancel();
                    cancelableDelayForLongPress = null;
                }
            }

            if(startLongPress)
            {
                longPressStarted = true;
                longPressInitialLocation = e.Location; // Here e.Location get the press Location relatively to the MessageContext UI Component
                cancelableDelayForLongPress = CancelableDelay.StartAfter(Helper.DELAY_FOR_LONG_PRESS, () => NeedToDisplayActionMenu(sender, longPressInitialLocation));
            }

        }

#endregion TOUCH EFECT RELATED


#region BUTTON ACTION RELATED

        private void CreateBtnAction()
        {
            BtnAction = new CustomButton();
            BtnAction.HorizontalOptions = new LayoutOptions(LayoutAlignment.End, false);
            BtnAction.VerticalOptions = new LayoutOptions(LayoutAlignment.Start, false);

            BtnAction.CornerRadius = 2;
            BtnAction.Padding = new Thickness(2,2,0,0);
            BtnAction.Margin = new Thickness(1,2,2,2);

            BtnAction.HeightRequest = 22;
            BtnAction.WidthRequest = 22;
            BtnAction.ImageSize = 20;

            btnActionMouseCommands = new MouseOverAndOutModel();
            btnActionMouseCommands.MouseOverCommand = new RelayCommand<object>(new Action<object>(BtnActionMouseOverCommand));
            btnActionMouseCommands.MouseOutCommand = new RelayCommand<object>(new Action<object>(BtnActionMouseOutCommand));

            //Add mouse over effect
            MouseOverEffect.SetCommand(RootGrid, btnActionMouseCommands.MouseOverCommand);

            //Add mouse out effect
            MouseOutEffect.SetCommand(RootGrid, btnActionMouseCommands.MouseOutCommand);

            BtnAction.Command = new RelayCommand<object>(new Action<object>(BtnActionCommand));
        }

        private void BtnActionMouseOverCommand(object obj)
        {
            if (needActionButton)
            {
                // TODO - need to change 
                BtnAction.ImageSourceId = "Font_EllipsisV|" + color.ToHex();
                
            }
        }

        private void BtnActionMouseOutCommand(object obj)
        {
            if (needActionButton)
            {
                BtnAction.ImageSourceId = null;
            }
        }

        private void BtnActionCommand(object obj)
        {
            NeedToDisplayActionMenu(obj);
        }

#endregion BUTTON ACTION RELATED

    }
}