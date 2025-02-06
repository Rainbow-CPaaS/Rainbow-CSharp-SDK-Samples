using Rainbow.Model;
using Rainbow.SimpleJSON;

namespace BotLibrary.Model
{
    public class BotConfigurationUpdate
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public String Id { get; set; }

        /// <summary>
        /// <see cref="JSONNode"/> JSONNode describing the bot configuration
        /// </summary>
        public JSONNode JSONNodeBotConfiguration { get; set; }

        /// <summary>
        /// <see cref="String">Describe the update context. 
        /// Can be "internalMessage", "ackMessage", "instantMessage", "applicationMessage" or "configFile"
        /// 
        /// "configFile" is used when the bot is finally connected and permits to have the configuration set as file at startup. There is no ContextData is this case.
        /// </summary>
        public String Context {  get; set; }

        /// <summary>
        /// <see cref="Object">Provide the context data as object. 
        /// Can be <see cref="InternalMessage"/>, <see cref="AckMessage"/>, <see cref="Message"/>, <see cref="ApplicationMessage"/> or null
        /// </summary>
        public Object? ContextData { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="jSONNodeBotConfiguration"><see cref="JSONNode"/>Bot configuration</param>
        /// <param name="context"><see cref="String"/>Context</param>
        /// <param name="contextData"><see cref="Object"/>Context Data</param>
        public BotConfigurationUpdate(JSONNode jSONNodeBotConfiguration, string context, object? contextData)
        {
            Id = Rainbow.Util.GetGUID();
            JSONNodeBotConfiguration = jSONNodeBotConfiguration;
            Context = context;
            ContextData = contextData;
        }
    }
}
