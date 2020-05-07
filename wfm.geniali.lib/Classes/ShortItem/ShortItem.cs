using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace wfm.geniali.lib.Classes.ShortItem
{
    public class ShortItemRoot
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
        public Payload()
        {
            
        }

        [JsonPropertyName("items")]
        public List<ShortItem> Items
        {
            get;
            set;
        }
    }

    public class ShortItem
    {
        public ShortItem()
        {
            
        }
        
        [JsonPropertyName("url_name")]
        public string UrlName
        {
            get;
            set;
        }

        [JsonPropertyName("item_name")]
        public string ItemName
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

        [JsonPropertyName("id")]
        public string Id
        {
            get;
            set;
        }
    }

}
