using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using wfm.geniali.lib.Classes.Item;
using wfm.geniali.lib.Classes.ShortItem;
using wfm.geniali.rest;

namespace wfm.geniali.cli.Commands
{
    public class Update
    {
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
            
            Dictionary<string, ItemsInSet> itemsInSets = new Dictionary<string, ItemsInSet>();
            
            foreach(ShortItem shortItem in res.Data)
            {
                Result<Item> item = client.GetItemAsynx(shortItem.UrlName).Result;
                
                FileInfo fileInfo = new FileInfo($"cache/{shortItem.UrlName}.item.cache.json");

                main.CWL($"Cache Datei {fileInfo.Name}");
                using(StreamWriter sw = new StreamWriter(fileInfo.FullName))
                {
                    sw.Write(item.Raw);
                }
                
                foreach(ItemsInSet itemsInSet in item.Data.ItemsInSet)
                {
                    if(itemsInSets.ContainsKey(itemsInSet.Id) == false)
                    {
                        itemsInSets.Add(itemsInSet.Id, itemsInSet);
                    }
                }
            }

            FileInfo itemsInSetFi = new FileInfo($"cache/items.in.set.cache.json");
            using(StreamWriter sw = new StreamWriter(itemsInSetFi.FullName))
            {
                sw.Write(JsonSerializer.Serialize(itemsInSets.Values.ToList()));
            }
        }
    }
}
