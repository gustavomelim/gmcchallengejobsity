using CsvHelper.Configuration.Attributes;
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
        [Name("Symbol")]
        public string Symbol { get; set; }

        [JsonProperty("date")]
        [Name("Date")]
        public DateTime Date { get; set; }

        [JsonProperty("time")]
        [Name("Time")]
        public DateTime Time { get; set; }

        [JsonProperty("open")]
        [Name("Open")]
        public decimal Open { get; set; }

        [JsonProperty("high")]
        [Name("High")]
        public decimal High { get; set; }

        [JsonProperty("low")]
        [Name("Low")]
        public decimal Low { get; set; }

        [JsonProperty("close")]
        [Name("Close")]
        public decimal Close { get; set; }

        [JsonProperty("volume")]
        [Name("Volume")]
        public long Volume { get; set; }
        [Ignore]
        public bool HasError { 
            get
            {
                return Volume < 1;
            }
        } 
    }
}
