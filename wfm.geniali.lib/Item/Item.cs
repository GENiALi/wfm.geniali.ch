using System;
using System.Collections.Generic;

namespace wfm.geniali.lib
{
    public partial class Item
    {
        public Payload Payload
        {
            get;
            set;
        }
    }

    public partial class Payload
    {
        public ItemClass Item
        {
            get;
            set;
        }
    }

    public partial class ItemClass
    {
        public string Id
        {
            get;
            set;
        }

        public List<ItemsInSet> ItemsInSet
        {
            get;
            set;
        }
    }

    public partial class ItemsInSet
    {
        public De Ru
        {
            get;
            set;
        }

        public De Sv
        {
            get;
            set;
        }

        public string Thumb
        {
            get;
            set;
        }

        public De Zh
        {
            get;
            set;
        }

        public De Fr
        {
            get;
            set;
        }

        public De De
        {
            get;
            set;
        }

        public long? TradingTax
        {
            get;
            set;
        }

        public List<string> Tags
        {
            get;
            set;
        }

        public De Pt
        {
            get;
            set;
        }

        public string UrlName
        {
            get;
            set;
        }

        public De En
        {
            get;
            set;
        }

        public bool? SetRoot
        {
            get;
            set;
        }

        public long? Ducats
        {
            get;
            set;
        }

        public De Ko
        {
            get;
            set;
        }

        public string IconFormat
        {
            get;
            set;
        }

        public string Icon
        {
            get;
            set;
        }

        public string Id
        {
            get;
            set;
        }

        public string SubIcon
        {
            get;
            set;
        }
    }

    public partial class De
    {
        public string ItemName
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public Uri WikiLink
        {
            get;
            set;
        }

        public List<Drop> Drop
        {
            get;
            set;
        }
    }

    public partial class Drop
    {
        public string Name
        {
            get;
            set;
        }

        public object Link
        {
            get;
            set;
        }
    }

}
