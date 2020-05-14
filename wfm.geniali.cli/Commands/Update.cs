using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using wfm.geniali.lib.Classes.Item;
using wfm.geniali.lib.Classes.Order;
using wfm.geniali.lib.Classes.ShortItem;
using wfm.geniali.rest;

namespace wfm.geniali.cli.Commands
{
    public class Update
    {
        private static object _Lock = new object();
        public void Execute(Program main, WfmClient client)
        {
            main.CWL("Items runterladen und cachen...");

            Result<List<ShortItem>> res = client.GetShortItemsAsync().Result;

            main.CWL($"{res.Data.Count} Items gefunden. ");

            FileInfo fi = new FileInfo("cache/itmes.cache.json");

            main.CWL($"Cache Datei itmes.cache.json");
            using(StreamWriter sw = new StreamWriter(fi.FullName, false))
            {
                sw.Write(res.Raw);
            }

            ConcurrentDictionary<string, ItemsInSet> itemsInSets = new ConcurrentDictionary<string, ItemsInSet>();

            ParallelOptions options = new ParallelOptions()
                             {
                                 MaxDegreeOfParallelism = 8
                             };

            int loop = 0;

            Parallel.ForEach(res.Data, options, (shortItem) =>
            {
                lock(_Lock)
                {
                    loop++;
                    
                }
                
                WfmClient localClient = new WfmClient();
                

                Result<List<Order>> orders = localClient.GetOrdersAsync(shortItem.UrlName)?.Result;

                if(orders != null)
                {
                    FileInfo orderFi = new FileInfo($"cache\\orders\\{shortItem.UrlName}.item.cache.json");

                    using(StreamWriter sw = new StreamWriter(orderFi.FullName))
                    {
                        sw.Write(JsonSerializer.Serialize(orders.Data));
                    }
                }


                Result<Item> item = localClient.GetItemAsynx(shortItem.UrlName).Result;
                FileInfo fileInfo = new FileInfo($"cache/{shortItem.UrlName}.item.cache.json");
                main.CWL($"{loop} \\ {res.Data.Count}\t Cache Datei {fileInfo.Name}");
                using(StreamWriter sw = new StreamWriter(fileInfo.FullName))
                {
                    sw.Write(item.Raw);
                }

                foreach(ItemsInSet itemsInSet in item.Data.ItemsInSet)
                {
                    if(itemsInSets.ContainsKey(itemsInSet.Id) == false)
                    {
                        itemsInSets.TryAdd(itemsInSet.Id, itemsInSet);
                    }
                }
            });

            FileInfo itemsInSetFi = new FileInfo($"cache/items.in.set.cache.json");
            using(StreamWriter sw = new StreamWriter(itemsInSetFi.FullName))
            {
                sw.Write(JsonSerializer.Serialize(itemsInSets.Values.ToList()));
            }
        }
    }
}
