// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using BlockFlash.Providers.Gatecoin;
//
//    var data = MarketDepth.FromJson(jsonString);
//
namespace BlockFlash.Providers.Gatecoin
{
    using System;
    using System.Net;
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public partial class MarketDepth
    {
        [JsonProperty("bids")]
        public Ask[] Bids { get; set; }

        [JsonProperty("asks")]
        public Ask[] Asks { get; set; }

        [JsonProperty("responseStatus")]
        public ResponseStatus ResponseStatus { get; set; }
    }

    public partial class Ask
    {
        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("volume")]
        public double Volume { get; set; }
    }

    public partial class ResponseStatus
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public partial class MarketDepth
    {
        public static MarketDepth FromJson(string json) => JsonConvert.DeserializeObject<MarketDepth>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this MarketDepth self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    public class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }
}
