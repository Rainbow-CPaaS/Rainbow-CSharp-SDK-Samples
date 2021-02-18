using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace SDK.WpfApp.Model
{
    public class ConstraintModel: ObservableObject
    {
        String m_maxFrameRate;
        String m_width;
        String m_height;

        Boolean m_videoConstraint;

        ICommand m_buttonSetConstraintCommand;

        /// <summary>
        /// Max Frame Rate
        /// </summary>
        public String MaxFrameRateValue
        {
            get { return m_maxFrameRate; }
            set { SetProperty(ref m_maxFrameRate, value); }
        }

        /// <summary>
        /// Width
        /// </summary>
        public String WidthValue
        {
            get { return m_width; }
            set { SetProperty(ref m_width, value); }
        }

        /// <summary>
        /// Height
        /// </summary>
        public String HeightValue
        {
            get { return m_height; }
            set { SetProperty(ref m_height, value); }
        }

        /// <summary>
        /// Vvideo Constraint ? 
        /// </summary>
        public Boolean VideoConstraint
        {
            get { return m_videoConstraint; }
            set { SetProperty(ref m_videoConstraint, value); }
        }

        /// <summary>
        /// To Mute / Umute Local Sharing
        /// </summary>
        public ICommand ButtonSetConstraintCommand
        {
            get { return m_buttonSetConstraintCommand; }
            set { m_buttonSetConstraintCommand = value; }
        }

        public ConstraintModel()
        {
        }
    }
}
