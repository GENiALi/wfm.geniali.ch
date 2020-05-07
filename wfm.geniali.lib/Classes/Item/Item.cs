using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace wfm.geniali.lib.Classes.Item
{
    public class ItemRoot
    {
        [JsonPropertyName("payload")]
        public Payload Payload
        {
            get;
            set;
        }
    }

    public class Payload
    {
        [JsonPropertyName("item")]
        public Item Item
        {
            get;
            set;
        }
    }

    public class Item
    {
        [JsonPropertyName("id")]
        public string Id
        {
            get;
            set;
        }

        [JsonPropertyName("items_in_set")]
        public List<ItemsInSet> ItemsInSet
        {
            get;
            set;
        }
    }

    public class ItemsInSet
    {
        [JsonPropertyName("ru")]
        public Translate Ru
        {
            get;
            set;
        }

        [JsonPropertyName("sv")]
        public Translate Sv
        {
            get;
            set;
        }

        [JsonPropertyName("thumb")]
        public string Thumb
        {
            get;
            set;
        }

        [JsonPropertyName("zh")]
        public Translate Zh
        {
            get;
            set;
        }

        [JsonPropertyName("fr")]
        public Translate Fr
        {
            get;
            set;
        }

        [JsonPropertyName("de")]
        public Translate Translate
        {
            get;
            set;
        }

        [JsonPropertyName("trading_tax")]
        public long? TradingTax
        {
            get;
            set;
        }

        [JsonPropertyName("tags")]
        public List<string> Tags
        {
            get;
            set;
        }

        [JsonPropertyName("pt")]
        public Translate Pt
        {
            get;
            set;
        }

        [JsonPropertyName("url_name")]
        public string UrlName
        {
            get;
            set;
        }

        [JsonPropertyName("en")]
        public Translate En
        {
            get;
            set;
        }

        [JsonPropertyName("set_root")]
        public bool? SetRoot
        {
            get;
            set;
        }

        [JsonPropertyName("ducats")]
        public long? Ducats
        {
            get;
            set;
        }

        [JsonPropertyName("ko")]
        public Translate Ko
        {
            get;
            set;
        }

        [JsonPropertyName("icon_format")]
        public string IconFormat
        {
            get;
            set;
        }

        [JsonPropertyName("icon")]
        public string Icon
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

        [JsonPropertyName("sub_icon")]
        public string SubIcon
        {
            get;
            set;
        }
    }

    public class Translate
    {
        [JsonPropertyName("item_name")]
        public string ItemName
        {
            get;
            set;
        }

        [JsonPropertyName("description")]
        public string Description
        {
            get;
            set;
        }

        [JsonPropertyName("wiki_link")]
        public Uri WikiLink
        {
            get;
            set;
        }

        [JsonPropertyName("drop")]
        public List<Drop> Drop
        {
            get;
            set;
        }
    }

    public class Drop
    {
        [JsonPropertyName("name")]
        public string Name
        {
            get;
            set;
        }

        [JsonPropertyName("link")]
        public object Link
        {
            get;
            set;
        }
    }
}
