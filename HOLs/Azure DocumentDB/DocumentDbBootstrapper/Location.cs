using System;
using Newtonsoft.Json;

namespace DocumentDbBootstrapper
{
    public class Location
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "startTime")]
        public DateEpoch StartTime { get; set; }

        [JsonProperty(PropertyName = "mainImageName")]
        public string MainImageName { get; set; }

        [JsonProperty(PropertyName = "junkerName")]
        public string JunkerName { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }
    }
}