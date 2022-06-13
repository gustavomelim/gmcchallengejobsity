using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace JobsityNetChallenge.Domain
{
    public class StockInfoList
    {
        [JsonProperty("symbols")]
        public List<StockInfo> Symbols { get; set; }
    }

    public class StockInfo
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("open")]
        public decimal Open { get; set; }

        [JsonProperty("high")]
        public decimal High { get; set; }

        [JsonProperty("low")]
        public decimal Low { get; set; }

        [JsonProperty("close")]
        public decimal Close { get; set; }

        [JsonProperty("volume")]
        public long Volume { get; set; }
    }
}
