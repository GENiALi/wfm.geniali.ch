using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace wfm.geniali.lib.ShortItem
{
    public partial class Item
    {
        [JsonPropertyName("payload")]
        public Payload Payload
        {
            get;
            set;
        }
    }

    public partial class Payload
    {
        public Payload()
        {
            
        }

        [JsonPropertyName("items")]
        public List<ItemElement> Items
        {
            get;
            set;
        }
    }

    public partial class ItemElement
    {
        public ItemElement()
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
