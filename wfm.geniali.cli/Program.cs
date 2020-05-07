using System;
using System.Collections.Generic;
using System.Linq;

using wfm.geniali.lib.Classes.Item;
using wfm.geniali.lib.Classes.Order;
using wfm.geniali.lib.Classes.ShortItem;
using wfm.geniali.lib.Classes.Statistics;
using wfm.geniali.rest;

namespace wfm.geniali.cli
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WfmClient         client     = new WfmClient();
            List<ShortItem> shortItems = client.GetShortItemsAsync().Result;

            foreach(ShortItem shortItem in shortItems)
            {
                if(shortItem.UrlName != null && shortItem.UrlName.EndsWith("prime_set"))
                {
                    Item item = client.GetItemAsynx(shortItem.UrlName).Result;

                    if(item != null)
                    {
                       //Console.WriteLine($"id: {item.Id}\t {shortItem.ItemName}");

                       // if(item.ItemsInSet.Count > 0)
                       // {
                       //     foreach(ItemsInSet inSet in item.ItemsInSet)
                       //     {
                               List<Order> orders = client.GetOrdersAsync(shortItem.UrlName).Result;
                               //Statistics statistics = client.GetStatisticsAsync(inSet.UrlName).Result;

                               Order bestSellOrder = orders.OrderByDescending(i => i.Platinum)
                                                           .FirstOrDefault(i => i.Platform.Equals("pc") 
                                                                       && i.OrderType.Equals("buy") 
                                                                       && i.Region.Equals("en")
                                                                       && (i.User.Status.Equals("online") || i.User.Status.Equals("ingame")));

                               Order bestBuyOrder = orders.OrderBy(i => i.Platinum)
                                                            .FirstOrDefault(i => i.Platform.Equals("pc") 
                                                                                 && i.OrderType.Equals("sell") 
                                                                                 && i.Region.Equals("en")
                                                                                 && (i.User.Status.Equals("online") || i.User.Status.Equals("ingame")));

                               if(bestSellOrder != null
                                  && bestBuyOrder != null)
                               {
                                   float div = bestSellOrder.Platinum - bestBuyOrder.Platinum;

                                   Console.WriteLine($"{shortItem.UrlName} \t kaufen: {bestBuyOrder.User.IngameName}\t{bestBuyOrder.Platinum} "
                                                     + $"\t verkaufen: {bestSellOrder.User.IngameName}\t{bestSellOrder.Platinum}\t"
                                                     + $"Gewinn:\t{div}");
                               }
                               else
                               {
                                   Console.WriteLine($"{shortItem.UrlName}");
                                   Console.WriteLine($"bestSellOrder is null {bestSellOrder == null}");
                                   Console.WriteLine($"bestBuyOrder is null {bestBuyOrder == null}");
                               }

                               //     }
                       // }
                    }
                }
            }
        }
    }
}
