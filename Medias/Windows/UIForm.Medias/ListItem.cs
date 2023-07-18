using System;

namespace SDK.UIForm.WebRTC
{
    public class ListItem
    {
        public String Value { get; set; }
        public String Text { get; set; }

        public override string ToString()
        {
            if (String.IsNullOrEmpty(Text))
                return Value;
            return Text;
        }

        public ListItem(string text, string value)
        {
            Value = value;
            Text = text;
        }
    }
}
