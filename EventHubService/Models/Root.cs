using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace EventHubService.Models
{
    public class Root
    {
        public string Type { get; set; }
        
        [JsonProperty(Required = Required.Always)]
        public string Id { get; set; }
        
        [JsonProperty(Required = Required.Always)]
        public string Timestamp { get; set; }
        
        [JsonProperty(Required = Required.Always)]
        public string UserId { get; set; }
        
        public string DeviceId { get; set; }

        public Root(string type, string id, string timestamp, string userId, string deviceId)
        {
            Type = type;
            Id = id;
            Timestamp = timestamp;
            UserId = userId;
            DeviceId = deviceId;
        }

        public Root()
        {
            
        }
    }
}