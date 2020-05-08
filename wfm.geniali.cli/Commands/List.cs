using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using DustInTheWind.ConsoleTools.TabularData;

using wfm.geniali.lib.Classes.Item;
using wfm.geniali.lib.Classes.ShortItem;
using wfm.geniali.rest;

namespace wfm.geniali.cli.Commands
{
    public class List
    {
        public void Execute(Program main, WfmClient client)
        {
            string input = string.Empty;
            bool   exit  = false;

            List<ShortItem> shortItmes = LoadShortItems();
            Dictionary<ShortItem, Item> items = LoadAllItems(shortItmes);

            do
            {
                input = main.ReadInput("wfm list>").ToLower();

                switch(input)
                {
                    case "b":
                    case "back":
                        exit = true;

                        break;
                    case "help":
                    case "h":
                        PrintHelp(main);

                        break;
                    case "prime frame set":
                    case "pfs":
                        ListPrimeFrameSet(main, items, shortItmes);
                        break;
                    default:
                        continue;
                }
                main.CWL();
            } while(exit == false);
        }

        private List<ShortItem> LoadShortItems()
        {
            FileInfo fi = new FileInfo("cache/itmes.cache.json");
            
            using(StreamReader sr = new StreamReader(fi.FullName))
            {
                string json = sr.ReadToEnd();
                return JsonSerializer.Deserialize<ShortItemRoot>(json).Payload.Items;
            }
        }

        private Dictionary<ShortItem, Item> LoadAllItems(List<ShortItem> shortItmes)
        {
            string[] files = Directory.GetFiles("cache", "*.item.cache.json");

            Dictionary<ShortItem, Item> retValue = new Dictionary<ShortItem, Item>();

            foreach(string file in files)
            {
                FileInfo fi = new FileInfo(file);
                
                using(StreamReader sr = new StreamReader(fi.FullName))
                {
                    string json = sr.ReadToEnd();
                    Item res = JsonSerializer.Deserialize<ItemRoot>(json).Payload.Item;
                    
                    retValue.Add(shortItmes.First(i => i.Id == res.Id), res);
                }
            }

            return retValue;
        }

        private void ListPrimeFrameSet(Program main, Dictionary<ShortItem, Item> items, List<ShortItem> shortItmes)
        {
            DataGrid dataGrid = new DataGrid("Prime Frame Sets");
            dataGrid.Columns.Add("Name");
            dataGrid.Columns.Add("URL Name");
            
            foreach(KeyValuePair<ShortItem, Item> item in items)
            {
                string id = item.Value.Id;
                ItemsInSet itemInSet = item.Value.ItemsInSet.First(i => i.Id == id);

                if(itemInSet.Tags.Contains("set")
                   && itemInSet.Tags.Contains("prime")
                   && itemInSet.Tags.Contains("warframe"))
                {
                    dataGrid.Rows.Add(item.Key.ItemName, item.Key.UrlName);
                }
            }
            
            dataGrid.Display();
        }

        private void PrintHelp(Program main)
        {
            main.CWL("pfs | prime frame set");
            main.CWL("");
        }
    }
}
