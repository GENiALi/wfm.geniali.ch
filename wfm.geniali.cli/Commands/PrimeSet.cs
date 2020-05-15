using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using DustInTheWind.ConsoleTools.TabularData;

using wfm.geniali.cli.Lib;
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
        private ConcurrentDictionary<ItemsInSet, List<Order>> _Orders = new ConcurrentDictionary<ItemsInSet, List<Order>>();
        private ConcurrentDictionary<ItemsInSet, List<Order>> _FastDownloadOrders = new ConcurrentDictionary<ItemsInSet, List<Order>>();
        private BlockingCollection<ItemsInSet> _ItemsInSet = new BlockingCollection<ItemsInSet>();
        private BlockingCollection<ItemsInSet> _ItemIsLoadet = new BlockingCollection<ItemsInSet>();
        private List<Item> _Items = new List<Item>();
        private Thread _SlowDownloadThread;
        private Thread _FastDownloadThread;

        public void Execute(Program main, WfmClient client)
        {
            string input = string.Empty;
            bool   exit  = false;
            main.CWL("Starte mit laden der Orders");

            _ItemsInSet = LoadItemsInSet();

            if(Directory.Exists("cache\\orders") == false)
            {
                Directory.CreateDirectory("cache\\orders");
            }

            if(_Orders.Count == 0)
            {
                _Orders = LoadOrders(_ItemsInSet);
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
                        exit = true;

                        break;
                    case "help":
                    case "h":
                        PrintHelp(main);

                        break;
                    case "ps":
                        Start(main, client, _ItemsInSet);
                        GenerateOutput();

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

        private void Start(Program main, WfmClient client, BlockingCollection<ItemsInSet> itemsInSet)
        {

            _SlowDownloadThread = new Thread(() => InitLoadOrders(itemsInSet, main, client));
            _SlowDownloadThread.IsBackground = true;
            _SlowDownloadThread.Start();


            _FastDownloadThread = new Thread(() => InitLoadFastOrders(itemsInSet, main, client));
            _FastDownloadThread.IsBackground = true;
            _FastDownloadThread.Start();
        }

        private void Stop()
        {
            _Stop = true;
            Thread.Sleep(2000);
            _FastDownloadOrders.Clear();
            _ItemIsLoadet = new BlockingCollection<ItemsInSet>();

            _FastDownloadThread = null;
            _SlowDownloadThread = null;

            if(_Timer != null)
            {
                _Timer.Dispose();
            }
        }

        private ConcurrentDictionary<ItemsInSet, List<Order>> ListPrimeSets(ConcurrentDictionary<ItemsInSet, List<Order>> orders, List<string> filer)
        {
            ConcurrentDictionary<ItemsInSet, List<Order>> retValue = new ConcurrentDictionary<ItemsInSet, List<Order>>();

            List<ItemsInSet> listWithTags = GetFiltertListTags(filer);

            foreach(KeyValuePair<ItemsInSet, List<Order>> keyValuePair in orders)
            {
                if(listWithTags.Contains(keyValuePair.Key))
                {
                    retValue.TryAdd(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return retValue;
        }

        private void GenerateOutput()
        {
            _Timer = new Timer((state) =>
            {
                ConcurrentDictionary<ItemsInSet, List<Order>> primeSets = ListPrimeSets(_Orders, new List<string>(0)
                                                                                                 {
                                                                                                     "set",
                                                                                                     "prime"
                                                                                                 });

                ConcurrentDictionary<ItemsInSet, Order> bestSellOrders = GenerateOrderdList(primeSets, "sell", "ingame", "asc");
                ConcurrentDictionary<ItemsInSet, Order> bestBuyOrders  = GenerateOrderdList(primeSets, "buy", "ingame", "desc");

                ConcurrentDictionary<ItemsInSet, Order> bestSellSingleOrder = GenerateOrderdList(_Orders, "sell", "ingame", "asc");

                List<PrimeSetDr> records = new List<PrimeSetDr>();
                foreach(KeyValuePair<ItemsInSet, Order> keyValuePair in bestSellOrders.OrderByDescending(i => i.Value.Platinum).Take(_Top))
                {
                    if(_ItemIsLoadet.Any(i => i.Id == keyValuePair.Key.Id) == false)
                    {
                        Item itemForSet = LoadItemsForSet(keyValuePair.Key.UrlName);
                        _Items.Add(itemForSet);
                        AddToFastLoad(itemForSet);
                        _ItemIsLoadet.TryAdd(keyValuePair.Key);
                    }

                    float totalKaufen = 0f;

                    List<KeyValuePair<ItemsInSet, Order>> set = new List<KeyValuePair<ItemsInSet, Order>>();

                    if(_Items.Any(i => i.Id == keyValuePair.Key.Id))
                    {
                        foreach(ItemsInSet setItem in _Items.First(i => i.Id == keyValuePair.Key.Id).ItemsInSet.Where(i => i.SetRoot == false))
                        {
                            set.Add(bestSellSingleOrder.First(i => i.Key.Id == setItem.Id));
                        }

                        totalKaufen = set.Sum(i => i.Value.Platinum);
                    }

                    float sell   = keyValuePair.Value.Platinum;
                    float buy    = bestBuyOrders[keyValuePair.Key].Platinum;

                    string suffix = "";

                    if(buy > sell)
                    {
                        suffix = "+ ";

                        if((sell / buy) <= 0.9)
                        {
                            suffix = $"++ ";
                        }
                    }
                        PrimeSetDr record = new PrimeSetDr();
                        record.Name = $"{(suffix.Length > 0 ? "* " : "")}{keyValuePair.Key.En.ItemName}";
                        record.UrlName = keyValuePair.Key.UrlName;
                        record.SellText = $"{sell} ({keyValuePair.Value.User.IngameName} {keyValuePair.Value.User.Region})";
                        record.Sell = sell;
                        record.BuyText = $"{suffix}{buy} ({bestBuyOrders[keyValuePair.Key].User.IngameName} {bestBuyOrders[keyValuePair.Key].User.Region})";
                        record.Buy = buy;
                        record.TotalKauf = totalKaufen;
                        record.Gewinn = sell - totalKaufen;
                        record.GewinnText = $"{sell - totalKaufen} ({buy - totalKaufen})";
                        record.SetItems = GenerateItemList(set);
                        
                        records.Add(record);
                }

                

                DataGrid dataGrid = new DataGrid($"Preise Sets {DateTime.Now.ToShortTimeString()} "
                                                 + $"- Items {_Orders.Count} "
                                                 + $"- Fastdownload Items {_FastDownloadOrders.Count()}");

                dataGrid.Columns.Add("Name");
                dataGrid.Columns.Add("URL Name");
                dataGrid.Columns.Add("zu kaufen für");
                dataGrid.Columns.Add("zu verkaufen für");
                dataGrid.Columns.Add("total kaufen");
                dataGrid.Columns.Add("+-/ Gewinn");
                dataGrid.Columns.Add("Items");
                foreach(PrimeSetDr dr in records.OrderByDescending(i => i.Gewinn))
                {
                    dataGrid.Rows.Add(dr.Name, dr.UrlName, dr.SellText, dr.BuyText, dr.TotalKauf.ToString(), dr.GewinnText, dr.SetItems);
                }

                Console.Clear();
                dataGrid.Display();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(_UpdateIntervall));
        }

        private string GenerateItemList(List<KeyValuePair<ItemsInSet, Order>> set)
        {
            StringBuilder sb = new StringBuilder();

            foreach(KeyValuePair<ItemsInSet, Order> keyValuePair in set)
            {
                string[] bez = keyValuePair.Key.En.ItemName.Split(' ');
                sb.Append($"{bez[bez.Length - 1]} ({keyValuePair.Value.Platinum}) ");
            }

            return sb.ToString();
        }

        private void AddToFastLoad(Item itemForSet)
        {
            foreach(ItemsInSet itemsInSet in itemForSet.ItemsInSet)
            {
                if(_FastDownloadOrders.Any(i => i.Key.Id == itemsInSet.Id) == false)
                {
                    _FastDownloadOrders.TryAdd(itemsInSet, _Orders.First(i => i.Key.Id == itemsInSet.Id).Value);
                }
            }
        }

        private Item LoadItemsForSet(string urlName)
        {
            FileInfo itemFi = new FileInfo($"cache/{urlName}.item.cache.json");

            if(itemFi.Exists)
            {
                using(StreamReader sr = new StreamReader(itemFi.FullName))
                {
                    string json = sr.ReadToEnd();

                    return JsonSerializer.Deserialize<ItemRoot>(json).Payload.Item;
                }
            }

            return null;
        }

        private ConcurrentDictionary<ItemsInSet, Order> GenerateOrderdList(ConcurrentDictionary<ItemsInSet, List<Order>> orders, string orderType = "sell", string status = "ingame",
                                                                           string sortType = "asc")
        {
            ConcurrentDictionary<ItemsInSet, Order> retValue = new ConcurrentDictionary<ItemsInSet, Order>();

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
                                retValue.TryAdd(item.Key, orderList.OrderBy(i => i.Platinum).First());
                            }
                            else
                            {
                                retValue.TryAdd(item.Key, orderList.OrderByDescending(i => i.Platinum).First());
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

        private List<ItemsInSet> GetFiltertListTags(List<string> list)
        {
            List<ItemsInSet> retValue = new List<ItemsInSet>();

            foreach(ItemsInSet inSet in _ItemsInSet)
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

        private BlockingCollection<ItemsInSet> LoadItemsInSet()
        {
            FileInfo itemsInSetFi = new FileInfo($"cache/items.in.set.cache.json");

            using(StreamReader sr = new StreamReader(itemsInSetFi.FullName))
            {
                string json = sr.ReadToEnd();

                List<ItemsInSet> retValue = JsonSerializer.Deserialize<List<ItemsInSet>>(json);

                return new BlockingCollection<ItemsInSet>(new ConcurrentQueue<ItemsInSet>(retValue));
            }
        }

        private void InitLoadOrders(BlockingCollection<ItemsInSet> itemsInSet, Program main, WfmClient client)
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
                    _Orders.TryAdd(inSet, res.Data);
                }
            }

            Thread.Sleep(10000);

            if(_Stop == false)
            {
                InitLoadOrders(itemsInSet, main, client);
            }
        }

        private void InitLoadFastOrders(BlockingCollection<ItemsInSet> itemsInSet, Program main, WfmClient client)
        {
            ParallelOptions options = new ParallelOptions()
                                      {
                                          MaxDegreeOfParallelism = 8
                                      };

            Parallel.ForEach(itemsInSet, options, (inSet) =>
            {
                if(_FastDownloadOrders.Any(i => i.Key.Id == inSet.Id) == false)
                {
                    return;
                }

                if(_Stop)
                {
                    return;
                }

                Result<List<Order>> res = client.GetOrdersAsync(inSet.UrlName)?.Result;

                if(res == null)
                {
                    return;
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
                    _Orders.TryAdd(inSet, res.Data);
                }
            });
                
            // foreach(ItemsInSet inSet in itemsInSet)
            // {
            //     if(_FastDownloadOrders.Any(i => i.Key.Id == inSet.Id) == false)
            //     {
            //         continue;
            //     }
            //
            //     if(_Stop)
            //     {
            //         break;
            //     }
            //
            //     Result<List<Order>> res = client.GetOrdersAsync(inSet.UrlName)?.Result;
            //
            //     if(res == null)
            //     {
            //         continue;
            //     }
            //
            //     FileInfo orderFi = new FileInfo($"cache\\orders\\{inSet.UrlName}.item.cache.json");
            //
            //     using(StreamWriter sw = new StreamWriter(orderFi.FullName))
            //     {
            //         sw.Write(JsonSerializer.Serialize(res.Data));
            //     }
            //
            //     if(_Orders.ContainsKey(inSet))
            //     {
            //         _Orders[inSet] = res.Data;
            //     }
            //     else
            //     {
            //         _Orders.TryAdd(inSet, res.Data);
            //     }
            // }

            if(_FastDownloadOrders.Count == 0)
            {
                Thread.Sleep(10000);
            }

            if(_Stop == false)
            {
                InitLoadFastOrders(itemsInSet, main, client);
            }
        }

        private ConcurrentDictionary<ItemsInSet, List<Order>> LoadOrders(BlockingCollection<ItemsInSet> itemsInSet)
        {
            string[] files = Directory.GetFiles("cache\\orders", "*.item.cache.json");

            ConcurrentDictionary<ItemsInSet, List<Order>> retValue = new ConcurrentDictionary<ItemsInSet, List<Order>>();

            foreach(string file in files)
            {
                FileInfo fi = new FileInfo(file);

                using(StreamReader sr = new StreamReader(fi.FullName))
                {
                    try
                    {
                        string      json = sr.ReadToEnd();
                        List<Order> res  = JsonSerializer.Deserialize<List<Order>>(json);

                        retValue.TryAdd(itemsInSet.First(i => i.UrlName.Equals(fi.Name.Replace(".item.cache.json", ""), StringComparison.CurrentCultureIgnoreCase)), res);
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
