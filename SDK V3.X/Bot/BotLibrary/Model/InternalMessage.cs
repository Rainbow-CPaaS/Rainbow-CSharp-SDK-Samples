namespace BotLibrary.Model
{
    public class InternalMessage
    {
        public String Type { get; set; }

        public Object Data { get; set; }

        public InternalMessage(String type, Object data)
        {  Type = type; Data = data; }
    }
}
