using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using wfm.geniali.lib.ShortItem;
using wfm.geniali.rest;

namespace wfm.geniali.cli
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WfmClient         client     = new WfmClient();
            List<ItemElement> shortItems = client.GetShortItemsAsync().Result.Payload.Items;

            foreach(ItemElement shortItem in shortItems)
            {
                Console.WriteLine(shortItem.ItemName);
            }

            Console.Read();
        }
    }
}
