using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace SDK.WpfApp.Model
{
    public class VideoElementModel : ObservableObject
    {
        Boolean m_LeftPositionUsed;
        Boolean m_BottomPositionUsed;
        String m_VerticalValue;
        String m_HorizontalValue;

        String m_WidthValue;
        String m_HeightValue;

        Boolean m_Hidden;

        String m_HorizontalTranslateValue;
        String m_VerticalTranslateValue;

        Boolean m_PictureInPictureEnabled;

        ICommand m_SetCommand;

        ICommand m_ButtonPictureInPictureSwitchCommand;

        /// <summary>
        /// Do we use 'Left' for Position
        /// </summary>
        public Boolean LeftPositionUsed
        {
            get { return m_LeftPositionUsed; }
            set { SetProperty(ref m_LeftPositionUsed, value); }
        }

        /// <summary>
        /// Do we use 'Bottom' for Position
        /// </summary>
        public Boolean BottomPositionUsed
        {
            get { return m_BottomPositionUsed; }
            set { SetProperty(ref m_BottomPositionUsed, value); }
        }

        /// <summary>
        /// Vertical Value
        /// </summary>
        public String VerticalValue
        {
            get { return m_VerticalValue; }
            set { SetProperty(ref m_VerticalValue, value); }
        }

        /// <summary>
        /// Horizontal Value
        /// </summary>
        public String HorizontalValue
        {
            get { return m_HorizontalValue; }
            set { SetProperty(ref m_HorizontalValue, value); }
        }

        /// <summary>
        /// Width Value
        /// </summary>
        public String WidthValue
        {
            get { return m_WidthValue; }
            set { SetProperty(ref m_WidthValue, value); }
        }

        /// <summary>
        /// Height Value
        /// </summary>
        public String HeightValue
        {
            get { return m_HeightValue; }
            set { SetProperty(ref m_HeightValue, value); }
        }

        /// <summary>
        /// Is hidden ?
        /// </summary>
        public Boolean Hidden
        {
            get { return m_Hidden; }
            set { SetProperty(ref m_Hidden, value); }
        }

        /// <summary>
        /// Horizontal Translate Value
        /// </summary>
        public String HorizontalTranslateValue
        {
            get { return m_HorizontalTranslateValue; }
            set { SetProperty(ref m_HorizontalTranslateValue, value); }
        }

        /// <summary>
        /// Vertical Translate Value
        /// </summary>
        public String VerticalTranslateValue
        {
            get { return m_VerticalTranslateValue; }
            set { SetProperty(ref m_VerticalTranslateValue, value); }
        }

        /// <summary>
        /// Is Picture In Picture Mode enabled ?
        /// </summary>
        public Boolean PictureInPictureEnabled
        {
            get { return m_PictureInPictureEnabled; }
            set { SetProperty(ref m_PictureInPictureEnabled, value); }
        }

        /// <summary>
        /// To manage action on the Set button
        /// </summary>
        public ICommand ButtonSetCommand
        {
            get { return m_SetCommand; }
            set { m_SetCommand = value; }
        }

        /// <summary>
        /// To manage action on the "Switch Picture In Picture Mode"
        /// </summary>
        public ICommand ButtonPictureInPictureSwitchCommand
        {
            get { return m_ButtonPictureInPictureSwitchCommand; }
            set { m_ButtonPictureInPictureSwitchCommand = value; }
        }

        public VideoElementModel()
        {
            m_Hidden = true;

        }

    }
}
