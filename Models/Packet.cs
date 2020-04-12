using Newtonsoft.Json;

namespace TheP0ngServer.Models
{
    public class Packet
    {
        public Meta MetaData { get; set; }
        public string Message { get; set; }

        public Packet(Meta meta, string message)
        {
            Message = message;
            MetaData = meta;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Packet FromJson(string message)
        {
            return JsonConvert.DeserializeObject<Packet>(message);
        } 
    }
    
    public enum Meta
    {
        Connect,
        Reconnect,
        Disconnect,
        Message,
        Error
    }
}