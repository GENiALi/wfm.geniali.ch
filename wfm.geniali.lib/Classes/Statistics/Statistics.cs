using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace wfm.geniali.lib.Classes.Statistics
{
    public class StatisticsRoot
    {
        [JsonPropertyName("payload")]
        public Statistics Payload
        {
            get;
            set;
        }
    }

    public class Statistics
    {
        [JsonPropertyName("statistics_closed")]
        public StatisticsClosed StatisticsClosed
        {
            get;
            set;
        }

        [JsonPropertyName("statistics_live")]
        public StatisticsLive StatisticsLive
        {
            get;
            set;
        }
    }

    public class StatisticsClosed
    {
        [JsonPropertyName("48hours")]
        public List<StatisticsClosed48Hour> The48Hours
        {
            get;
            set;
        }

        [JsonPropertyName("90days")]
        public List<StatisticsClosed48Hour> The90Days
        {
            get;
            set;
        }
    }

    public class StatisticsClosed48Hour
    {
        [JsonPropertyName("datetime")]
        public DateTimeOffset? Datetime
        {
            get;
            set;
        }

        [JsonPropertyName("volume")]
        public long? Volume
        {
            get;
            set;
        }

        [JsonPropertyName("min_price")]
        public long? MinPrice
        {
            get;
            set;
        }

        [JsonPropertyName("max_price")]
        public long? MaxPrice
        {
            get;
            set;
        }

        [JsonPropertyName("open_price")]
        public long? OpenPrice
        {
            get;
            set;
        }

        [JsonPropertyName("closed_price")]
        public long? ClosedPrice
        {
            get;
            set;
        }

        [JsonPropertyName("avg_price")]
        public double? AvgPrice
        {
            get;
            set;
        }

        [JsonPropertyName("wa_price")]
        public double? WaPrice
        {
            get;
            set;
        }

        [JsonPropertyName("median")]
        public double? Median
        {
            get;
            set;
        }

        [JsonPropertyName("moving_avg")]
        public double? MovingAvg
        {
            get;
            set;
        }

        [JsonPropertyName("donch_top")]
        public long? DonchTop
        {
            get;
            set;
        }

        [JsonPropertyName("donch_bot")]
        public long? DonchBot
        {
            get;
            set;
        }

        [JsonPropertyName("id")]
        public string Id
        {
            get;
            set;
        }
    }

    public class StatisticsLive
    {
        [JsonPropertyName("48hours")]
        public List<StatisticsLive48Hour> The48Hours
        {
            get;
            set;
        }

        [JsonPropertyName("90days")]
        public List<StatisticsLive48Hour> The90Days
        {
            get;
            set;
        }
    }

    public class StatisticsLive48Hour
    {
        [JsonPropertyName("datetime")]
        public DateTimeOffset? Datetime
        {
            get;
            set;
        }

        [JsonPropertyName("volume")]
        public long? Volume
        {
            get;
            set;
        }

        [JsonPropertyName("min_price")]
        public long? MinPrice
        {
            get;
            set;
        }

        [JsonPropertyName("max_price")]
        public long? MaxPrice
        {
            get;
            set;
        }

        [JsonPropertyName("avg_price")]
        public double? AvgPrice
        {
            get;
            set;
        }

        [JsonPropertyName("wa_price")]
        public double? WaPrice
        {
            get;
            set;
        }

        [JsonPropertyName("median")]
        public double? Median
        {
            get;
            set;
        }

        [JsonPropertyName("order_type")]
        public string OrderType
        {
            get;
            set;
        }

        [JsonPropertyName("moving_avg")]
        public double? MovingAvg
        {
            get;
            set;
        }

        [JsonPropertyName("id")]
        public string Id
        {
            get;
            set;
        }
    }
}
