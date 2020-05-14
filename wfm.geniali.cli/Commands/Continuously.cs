using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Timers;

using DustInTheWind.ConsoleTools.TabularData;

using wfm.geniali.lib.Classes.Item;
using wfm.geniali.lib.Classes.Order;
using wfm.geniali.rest;

using Timer = System.Timers.Timer;

namespace wfm.geniali.cli.Commands
{
    public class Continuously
    {
        private int _Top = 20;
        private int _UpdateIntervall = 60;
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

            if(_Orders.Count == 0)
            {
                _Orders = LoadOrders(itemsInSet);
            }

            Thread t = new Thread(() => InitLoadOrders(itemsInSet, main, client));
            t.IsBackground = true;
            t.Start();

            Thread t2 = new Thread(() => InitLoadFastOrders(itemsInSet, main, client));
            t2.IsBackground = true;
            t2.Start();

            do
            {
                input = main.ReadInput("wfm Continuously>").ToLower();

                switch(input)
                {
                    case "b":
                    case "back":
                        exit  = true;
                        _Stop = true;

                        if(_Timer != null)
                        {
                            _Timer.Dispose();
                        }

                        break;
                    case "help":
                    case "h":
                        PrintHelp(main);

                        break;
                    case "ti":
                        DisplayTeuerstesItem(itemsInSet, main, client);

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

        private void InitLoadOrders(List<ItemsInSet> itemsInSet, Program main, WfmClient client)
        {
            foreach(ItemsInSet inSet in itemsInSet)
            {
                if(_FastDownloadOrders.ContainsKey(inSet))
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

            Thread.Sleep(20000);

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

            Thread.Sleep(30000);

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

        private void DisplayTeuerstesItem(List<ItemsInSet> itemsInSet, Program main, WfmClient client)
        {
            _Timer = new Timer();

            _Timer.Elapsed += (sender, args) =>
            {
                Console.Clear();

                Dictionary<ItemsInSet, Order> bestSellOrders = GenerateBestSellOrderList(_Orders);

                DataGrid dataGrid = new DataGrid($"Preis teuerste Items {DateTime.Now.ToShortTimeString()} "
                                                 + $"- Items {_Orders.Count} "
                                                 + $"- Fastdownload Items {_FastDownloadOrders.Count}");

                dataGrid.Columns.Add("Name");
                dataGrid.Columns.Add("URL Name");
                dataGrid.Columns.Add("zu kaufen für");

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
                    dataGrid.Rows.Add(itemsInSet.First(i => i == keyValuePair.Key).En.ItemName, keyValuePair.Key.UrlName,
                                      keyValuePair.Value.Platinum);
                }

                dataGrid.Display();

                _Timer.Interval = _UpdateIntervall * 1000;
            };

            _Timer.Interval  = 2500;
            _Timer.Enabled   = true;
            _Timer.AutoReset = true;
        }

        private Dictionary<ItemsInSet, Order> GenerateBestSellOrderList(Dictionary<ItemsInSet, List<Order>> orders)
        {
            Dictionary<ItemsInSet, Order> retValue = new Dictionary<ItemsInSet, Order>();

            for(int i = 0; i < orders.Count; i++)
            {
                KeyValuePair<ItemsInSet, List<Order>> item = orders.ElementAt(i);

                try
                {
                    if(item.Value != null)
                    {
                        List<Order> orderList = item.Value.Where(i => i.OrderType.Equals("sell") && i.User.Status.Equals("ingame")).ToList();

                        if(orderList != null
                           && orderList.Count > 0)
                        {
                            retValue.Add(item.Key, orderList.OrderBy(i => i.Platinum).First());
                        }
                    }
                }
                catch(Exception e)
                {
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

        private List<ItemsInSet> LoadItemsInSet()
        {
            FileInfo itemsInSetFi = new FileInfo($"cache/items.in.set.cache.json");

            using(StreamReader sr = new StreamReader(itemsInSetFi.FullName))
            {
                string json = sr.ReadToEnd();

                return JsonSerializer.Deserialize<List<ItemsInSet>>(json);
            }
        }

        private void PrintHelp(Program main)
        {
            main.CWL("ti \t teuerstes Item");
            main.CWL("st | set top \t  default 20");
            main.CWL("ui | update intervall \t  default 60 sekunden");
        }
    }
}
