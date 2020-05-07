using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace wfm.geniali.lib.Classes.Order
{
    public class OrderRoot
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
        [JsonPropertyName("orders")]
        public List<Order> Orders
        {
            get;
            set;
        }
    }

    public class Order
    {
        [JsonPropertyName("order_type")]
        public string OrderType
        {
            get;
            set;
        }

        [JsonPropertyName("platinum")]
        public float Platinum
        {
            get;
            set;
        }

        [JsonPropertyName("quantity")]
        public long Quantity
        {
            get;
            set;
        }

        [JsonPropertyName("user")]
        public User User
        {
            get;
            set;
        }

        [JsonPropertyName("platform")]
        public string Platform
        {
            get;
            set;
        }

        [JsonPropertyName("region")]
        public string Region
        {
            get;
            set;
        }

        [JsonPropertyName("creation_date")]
        public DateTime CreationDate
        {
            get;
            set;
        }

        [JsonPropertyName("last_update")]
        public DateTime LastUpdate
        {
            get;
            set;
        }

        [JsonPropertyName("visible")]
        public bool Visible
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

    public class User
    {
        [JsonPropertyName("reputation")]
        public long Reputation
        {
            get;
            set;
        }

        [JsonPropertyName("reputation_bonus")]
        public long ReputationBonus
        {
            get;
            set;
        }

        [JsonPropertyName("region")]
        public string Region
        {
            get;
            set;
        }

        [JsonPropertyName("last_seen")]
        public DateTime? LastSeen
        {
            get;
            set;
        }

        [JsonPropertyName("ingame_name")]
        public string IngameName
        {
            get;
            set;
        }

        [JsonPropertyName("avatar")]
        public string Avatar
        {
            get;
            set;
        }

        [JsonPropertyName("status")]
        public string Status
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
