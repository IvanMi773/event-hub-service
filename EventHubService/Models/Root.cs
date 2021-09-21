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
    }
}