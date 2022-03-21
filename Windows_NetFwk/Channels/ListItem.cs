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
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
