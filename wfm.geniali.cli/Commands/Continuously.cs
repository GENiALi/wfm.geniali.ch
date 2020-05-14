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
        private Dictionary<string, List<Order>> _Orders = new Dictionary<string, List<Order>>();
        private Dictionary<string, List<Order>> _FastDownloadOrders = new Dictionary<string, List<Order>>();

        public void Execute(Program main, WfmClient client)
        {
            string input = string.Empty;
            bool   exit  = false;
            main.CWL("Starte mit laden der Orders");

            if(Directory.Exists("cache\\orders") == false)
            {
                Directory.CreateDirectory("cache\\orders");
            }

            if(_Orders.Count == 0)
            {
                _Orders = LoadOrders();
            }

            List<ItemsInSet> itemsInSet = LoadItemsInSet();

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
                        _Timer.Dispose();

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
                if(_FastDownloadOrders.ContainsKey(inSet.UrlName))
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

                if(_Orders.ContainsKey(inSet.UrlName))
                {
                    _Orders[inSet.UrlName] = res.Data;
                }
                else
                {
                    _Orders.Add(inSet.UrlName, res.Data);
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
                if(_FastDownloadOrders.ContainsKey(inSet.UrlName) == false)
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

                if(_Orders.ContainsKey(inSet.UrlName))
                {
                    _Orders[inSet.UrlName] = res.Data;
                }
                else
                {
                    _Orders.Add(inSet.UrlName, res.Data);
                }
            }

            Thread.Sleep(30000);

            if(_Stop == false)
            {
                InitLoadFastOrders(itemsInSet, main, client);
            }
        }

        private Dictionary<string, List<Order>> LoadOrders()
        {
            string[] files = Directory.GetFiles("cache\\orders", "*.item.cache.json");

            Dictionary<string, List<Order>> retValue = new Dictionary<string, List<Order>>();

            foreach(string file in files)
            {
                FileInfo fi = new FileInfo(file);

                using(StreamReader sr = new StreamReader(fi.FullName))
                {
                    try
                    {
                        string      json = sr.ReadToEnd();
                        List<Order> res  = JsonSerializer.Deserialize<List<Order>>(json);

                        retValue.Add(fi.Name.Replace(".item.cache.json", ""), res);
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

                Dictionary<string, Order> bestSellOrders = GenerateBestSellOrderList(_Orders);

                DataGrid dataGrid = new DataGrid($"Preis teuerste Items {DateTime.Now.ToShortTimeString()}");
                dataGrid.Columns.Add("Name");
                dataGrid.Columns.Add("URL Name");
                dataGrid.Columns.Add("zu kaufen für");

                _FastDownloadOrders.Clear();
                foreach(KeyValuePair<string, Order> keyValuePair in bestSellOrders.OrderByDescending(i => i.Value.Platinum).Take(_Top * 2))
                {
                    _FastDownloadOrders.Add(keyValuePair.Key, new List<Order>()
                                                              {
                                                                  keyValuePair.Value
                                                              });
                }

                foreach(KeyValuePair<string, Order> keyValuePair in bestSellOrders.OrderByDescending(i => i.Value.Platinum).Take(_Top))
                {
                    dataGrid.Rows.Add(itemsInSet.First(i => i.UrlName == keyValuePair.Key).En.ItemName, keyValuePair.Key,
                                      keyValuePair.Value.Platinum);
                }

                dataGrid.Display();

                _Timer.Interval = _UpdateIntervall * 1000;
            };

            _Timer.Interval = 2500;
            _Timer.Enabled   = true;
            _Timer.AutoReset = true;
        }

        private Dictionary<string, Order> GenerateBestSellOrderList(Dictionary<string, List<Order>> orders)
        {
            Dictionary<string, Order> retValue = new Dictionary<string, Order>();

            for(int i = 0; i < orders.Count; i++)
            {
                KeyValuePair<string, List<Order>> item = orders.ElementAt(i);

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
