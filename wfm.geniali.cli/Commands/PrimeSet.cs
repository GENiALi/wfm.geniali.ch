using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;


using DustInTheWind.ConsoleTools.TabularData;

using wfm.geniali.lib.Classes.Item;
using wfm.geniali.lib.Classes.Order;
using wfm.geniali.rest;

namespace wfm.geniali.cli.Commands
{
    public class PrimeSet
    {
        private int _Top = 20;
        private int _UpdateIntervall = 30;
        private bool _Stop = false;
        private Timer _Timer = null;
        private Dictionary<ItemsInSet, List<Order>> _Orders = new Dictionary<ItemsInSet, List<Order>>();
        private Dictionary<ItemsInSet, List<Order>> _FastDownloadOrders = new Dictionary<ItemsInSet, List<Order>>();

        public void Execute(Program main, WfmClient client)
        {
            string input = string.Empty;
            bool   exit  = false;
            main.CWL("Starte mit laden der Orders");

            List<ItemsInSet> itemsInSet = LoadItemsInSet();

            if(Directory.Exists("cache\\orders") == false)
            {
                Directory.CreateDirectory("cache\\orders");
            }

            do
            {
                input = main.ReadInput("wfm Prime Set>").ToLower();
                _Stop = false;

                switch(input)
                {
                    case "e":
                        Stop();

                        break;
                    case "b":
                    case "back":
                        Stop();
                        exit  = true;
                        break;
                    case "help":
                    case "h":
                        PrintHelp(main);

                        break;
                    case "ps":
                        Start(main, client, itemsInSet);
                        ListPrimeSets(itemsInSet, main, client);
                        break;
                    case "wfps":
                        break;
                    case "wps":
                        break;

                    case "set top":
                    case "st":
                        SetTop(main);

                        break;
                    case "update intervall":
                    case "ui":
                        SetUpdateIntervall(main);

                        break;
                    default:
                        continue;
                }

                main.CWL();
            } while(exit == false);
        }

        private void Start(Program main, WfmClient client, List<ItemsInSet> itemsInSet)
        {
            Thread t = new Thread(() => InitLoadOrders(itemsInSet, main, client));
            t.IsBackground = true;
            t.Start();

            Thread t2 = new Thread(() => InitLoadFastOrders(itemsInSet, main, client));
            t2.IsBackground = true;
            t2.Start();
        }

        private void Stop()
        {
            _Stop = true;
            Thread.Sleep(2000);
            _Orders.Clear();
            _FastDownloadOrders.Clear();

            if(_Timer != null)
            {
                _Timer.Dispose();
            }
        }

        private void ListPrimeSets(List<ItemsInSet> itemsInSet, Program main, WfmClient client)
        {
            _Orders.Clear();
            _FastDownloadOrders.Clear();

            List<ItemsInSet> listWithTags = GetFiltertListTags(itemsInSet, new List<string>()
                                                                           {
                                                                               "prime",
                                                                               "set"
                                                                           });

            _Orders = GenerateOrdersList(listWithTags, main, client);

            GenerateOutput();
        }

        private void GenerateOutput()
        {
            _Timer = new Timer((state) =>
            {
                Console.Clear();

                Dictionary<ItemsInSet, Order> bestSellOrders = GenerateOrderdList(_Orders);
                Dictionary<ItemsInSet, Order> bestBuyOrders = GenerateOrderdList(_Orders, "buy", "ingame", "desc");

                DataGrid dataGrid = new DataGrid($"Preise Sets {DateTime.Now.ToShortTimeString()} "
                                                 + $"- Items {_Orders.Count} "
                                                 + $"- Fastdownload Items {_FastDownloadOrders.Count}");

                dataGrid.Columns.Add("Name");
                dataGrid.Columns.Add("URL Name");
                dataGrid.Columns.Add("zu kaufen für");
                dataGrid.Columns.Add("zu verkaufen für");

                _FastDownloadOrders.Clear();

                foreach(KeyValuePair<ItemsInSet, Order> keyValuePair in bestSellOrders.OrderByDescending(i => i.Value.Platinum).Take(_Top * 2))
                {
                    _FastDownloadOrders.Add(keyValuePair.Key, new List<Order>()
                                                              {
                                                                  keyValuePair.Value
                                                              });
                }

                foreach(KeyValuePair<ItemsInSet, Order> keyValuePair in bestSellOrders.OrderByDescending(i => i.Value.Platinum).Take(_Top))
                {
                    float sell = keyValuePair.Value.Platinum;
                    float buy = bestBuyOrders[keyValuePair.Key].Platinum;
                    string suffix = "";

                    if(buy > sell)
                    {
                        suffix = "+ ";

                        if((100 - (sell / buy)) > 20)
                        {
                            suffix = "++ ";
                        }
                    }
                    
                    dataGrid.Rows.Add(keyValuePair.Key.En.ItemName
                                    , keyValuePair.Key.UrlName
                                    , $"{sell} ({keyValuePair.Value.User.IngameName} {keyValuePair.Value.User.Region})"
                                    , $"{suffix}{buy} ({bestBuyOrders[keyValuePair.Key].User.IngameName} {bestBuyOrders[keyValuePair.Key].User.Region})");
                }

                dataGrid.Display();
                
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(_UpdateIntervall));
        }

        private Dictionary<ItemsInSet, Order> GenerateOrderdList(Dictionary<ItemsInSet, List<Order>> orders, string orderType = "sell", string status = "ingame", string sortType = "asc")
        {
            Dictionary<ItemsInSet, Order> retValue = new Dictionary<ItemsInSet, Order>();

            for(int i = 0; i < orders.Count; i++)
            {
                KeyValuePair<ItemsInSet, List<Order>> item = orders.ElementAt(i);

                try
                {
                    if(item.Value != null)
                    {
                        List<Order> orderList = item.Value.Where(i => i.OrderType.Equals(orderType) 
                                                                      && i.User.Status.Equals(status))
                                                    .ToList();

                        if(orderList != null
                           && orderList.Count > 0)
                        {
                            if(sortType.Equals("asc", StringComparison.CurrentCultureIgnoreCase))
                            {
                                retValue.Add(item.Key, orderList.OrderBy(i => i.Platinum).First());
                            }
                            else
                            {
                                retValue.Add(item.Key, orderList.OrderByDescending(i => i.Platinum).First());
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                }
            }

            return retValue;
        }

        private Dictionary<ItemsInSet, List<Order>> GenerateOrdersList(List<ItemsInSet> listWithTags, Program main, WfmClient client)
        {
            Dictionary<ItemsInSet, List<Order>> retValue = new Dictionary<ItemsInSet, List<Order>>();

            foreach(ItemsInSet itemInSet in listWithTags)
            {
                FileInfo fi = new FileInfo($"cache\\orders\\{itemInSet.UrlName}.item.cache.json");

                if(fi.Exists)
                {
                    using(StreamReader sr = new StreamReader(fi.FullName))
                    {
                        try
                        {
                            string      json = sr.ReadToEnd();
                            List<Order> res  = JsonSerializer.Deserialize<List<Order>>(json);

                            retValue.Add(itemInSet, res);
                        }
                        catch(Exception e)
                        {
                        }
                    }
                }
            }

            return retValue;
        }

        private List<ItemsInSet> GetFiltertListTags(List<ItemsInSet> itemsInSet, List<string> list)
        {
            List<ItemsInSet> retValue = new List<ItemsInSet>();

            foreach(ItemsInSet inSet in itemsInSet)
            {
                bool contain = true;

                foreach(string tag in list)
                {
                    if(inSet.Tags.Contains(tag) == false)
                    {
                        contain = false;

                        break;
                    }
                }

                if(contain)
                {
                    retValue.Add(inSet);
                }
            }

            return retValue;
        }

        private List<ItemsInSet> LoadItemsInSet()
        {
            FileInfo itemsInSetFi = new FileInfo($"cache/items.in.set.cache.json");

            using(StreamReader sr = new StreamReader(itemsInSetFi.FullName))
            {
                string json = sr.ReadToEnd();

                return JsonSerializer.Deserialize<List<ItemsInSet>>(json);
            }
        }

        private void InitLoadOrders(List<ItemsInSet> itemsInSet, Program main, WfmClient client)
        {
            foreach(ItemsInSet inSet in itemsInSet)
            {
                if(_FastDownloadOrders.ContainsKey(inSet)
                   || _Orders.ContainsKey(inSet) == false)
                {
                    continue;
                }

                if(_Stop)
                {
                    break;
                }

                Result<List<Order>> res = client.GetOrdersAsync(inSet.UrlName)?.Result;

                if(res == null)
                {
                    continue;
                }

                FileInfo orderFi = new FileInfo($"cache\\orders\\{inSet.UrlName}.item.cache.json");

                using(StreamWriter sw = new StreamWriter(orderFi.FullName))
                {
                    sw.Write(JsonSerializer.Serialize(res.Data));
                }

                if(_Orders.ContainsKey(inSet))
                {
                    _Orders[inSet] = res.Data;
                }
                else
                {
                    _Orders.Add(inSet, res.Data);
                }
            }

            Thread.Sleep(10000);

            if(_Stop == false)
            {
                InitLoadOrders(itemsInSet, main, client);
            }
        }

        private void InitLoadFastOrders(List<ItemsInSet> itemsInSet, Program main, WfmClient client)
        {
            foreach(ItemsInSet inSet in itemsInSet)
            {
                if(_FastDownloadOrders.ContainsKey(inSet) == false)
                {
                    continue;
                }

                if(_Stop)
                {
                    break;
                }

                Result<List<Order>> res = client.GetOrdersAsync(inSet.UrlName)?.Result;

                if(res == null)
                {
                    continue;
                }

                FileInfo orderFi = new FileInfo($"cache\\orders\\{inSet.UrlName}.item.cache.json");

                using(StreamWriter sw = new StreamWriter(orderFi.FullName))
                {
                    sw.Write(JsonSerializer.Serialize(res.Data));
                }

                if(_Orders.ContainsKey(inSet))
                {
                    _Orders[inSet] = res.Data;
                }
                else
                {
                    _Orders.Add(inSet, res.Data);
                }
            }

            if(_FastDownloadOrders.Count == 0)
            {
                Thread.Sleep(10000);
            }

            if(_Stop == false)
            {
                InitLoadFastOrders(itemsInSet, main, client);
            }
        }

        private Dictionary<ItemsInSet, List<Order>> LoadOrders(List<ItemsInSet> itemsInSet)
        {
            string[] files = Directory.GetFiles("cache\\orders", "*.item.cache.json");

            Dictionary<ItemsInSet, List<Order>> retValue = new Dictionary<ItemsInSet, List<Order>>();

            foreach(string file in files)
            {
                FileInfo fi = new FileInfo(file);

                using(StreamReader sr = new StreamReader(fi.FullName))
                {
                    try
                    {
                        string      json = sr.ReadToEnd();
                        List<Order> res  = JsonSerializer.Deserialize<List<Order>>(json);

                        retValue.Add(itemsInSet.First(i => i.UrlName.Equals(fi.Name.Replace(".item.cache.json", ""), StringComparison.CurrentCultureIgnoreCase)), res);
                    }
                    catch(Exception e)
                    {
                    }
                }
            }

            return retValue;
        }

        private void SetTop(Program main)
        {
            string input = main.ReadInput($"aktuell {_Top}>").ToLower();

            int top = _Top;

            if(int.TryParse(input, out top))
            {
                _Top = top;
            }

            main.CWL($"Top gesetzt auf {_Top} Items");
        }

        private void SetUpdateIntervall(Program main)
        {
            string input = main.ReadInput($"aktuell {_UpdateIntervall}>").ToLower();

            int sekunden = _UpdateIntervall;

            if(int.TryParse(input, out sekunden))
            {
                _UpdateIntervall = sekunden;

                if(_UpdateIntervall > 600
                   || _UpdateIntervall < 20)
                {
                    _UpdateIntervall = 60;
                }
            }

            main.CWL($"Updateintervall gesetzt auf {_UpdateIntervall} Sekunden");
        }

        private void PrintHelp(Program main)
        {
            main.CWL("ps \t Alle Prime Sets");
            main.CWL("wfps \t Warframe Prime Sets");
            main.CWL("wps \t Waffen Prime Sets");
            main.CWL("st \t  Top (default 20)");
            main.CWL("ui \t  Update Intervall (default 60 Sekunden)");
        }
    }
}
