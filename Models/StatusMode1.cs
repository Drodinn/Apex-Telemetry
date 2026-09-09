using System;
using Newtonsoft.Json;

namespace ApexTelemetry.Models
{
    public class StatusModel
    {
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("Flags")]
        public long Flags { get; set; }

        [JsonProperty("Pips")]
        public int[] Pips { get; set; } = new int[3];

        [JsonProperty("Firegroup")]
        public int Firegroup { get; set; }

        [JsonProperty("GuiFocus")]
        public int GuiFocus { get; set; }

        [JsonProperty("Fuel")]
        public FuelInfo Fuel { get; set; }

        [JsonProperty("Cargo")]
        public double Cargo { get; set; }

        public class FuelInfo
        {
            [JsonProperty("FuelMain")]
            public double FuelMain { get; set; }

            [JsonProperty("FuelReservoir")]
            public double FuelReservoir { get; set; }
        }
        // Planetary Telemetry
        [JsonProperty("Latitude")]
        public double? Latitude { get; set; }

        [JsonProperty("Longitude")]
        public double? Longitude { get; set; }

        [JsonProperty("Altitude")]
        public double? Altitude { get; set; }

        [JsonProperty("Heading")]
        public double? Heading { get; set; }

        [JsonProperty("PlanetRadius")]
        public double? PlanetRadius { get; set; }
    }
}