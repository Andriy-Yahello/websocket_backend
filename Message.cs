using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace backend
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class SocketMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Message { get; set; }
    }
}