using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleChannels
{
    public class ListItem
    {
        public String Id { get; set; }

        public String Text { get; set; }

        public ListItem(String id, String text)
        {
            Id = id;
            if(String.IsNullOrEmpty(text))
                Text = id;
            else
                Text = text;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
