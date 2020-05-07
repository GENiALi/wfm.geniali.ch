using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Serializers.SystemTextJson;
using wfm.geniali.lib.Classes.Item;
using wfm.geniali.lib.Classes.Order;
using wfm.geniali.lib.Classes.ShortItem;
using wfm.geniali.lib.Classes.Statistics;

namespace wfm.geniali.rest
{
    public class WfmClient
    {
        public string _BaseUrl
        {
            get;
            set;
        }

        public IRestClient _Client
        {
            get;
            set;
        }

        public WfmClient()
        {
            _BaseUrl = "https://api.warframe.market/v1";

            _Client = new RestClient(_BaseUrl);
            _Client.UseSystemTextJson();
        }

        public async Task<List<ShortItem>> GetShortItemsAsync()
        {
            RestRequest request = new RestRequest("/items");

            ShortItemRoot res = await Execute<ShortItemRoot>(request);

            return res.Payload.Items;
        }

        public async Task<Item> GetItemAsynx(string urlName)
        {
            RestRequest request = new RestRequest("/items/{urlName}");
            request.AddParameter("urlName", urlName, ParameterType.UrlSegment);

            ItemRoot res = await Execute<ItemRoot>(request);

            return res.Payload.Item;
        }

        public async Task<List<Order>> GetOrdersAsync(string urlName)
        {
            RestRequest request = new RestRequest("/items/{urlName}/orders");
            request.AddParameter("urlName", urlName, ParameterType.UrlSegment);

            OrderRoot res = await Execute<OrderRoot>(request);

            return res.Payload.Orders;
        }

        public async Task<Statistics> GetStatisticsAsync(string urlName)
        {
            RestRequest request = new RestRequest("/items/{urlName}/statistics");
            request.AddParameter("urlName", urlName, ParameterType.UrlSegment);

            StatisticsRoot res = await Execute<StatisticsRoot>(request);

            return res.Payload;
        }

        private async Task<T> Execute<T>(RestRequest request)
            where T : new()
        {
            //request.AddParameter("AccountSid", _accountSid, ParameterType.UrlSegment); // used on every request
            IRestResponse<T> response = await _Client.ExecuteAsync<T>(request);

            if(response.ErrorException != null)
            {
                const string message = "Error retrieving response. Check inner details for more info.";
                Exception    ex      = new Exception(message, response.ErrorException);

                throw ex;
            }

            return response.Data;
        }
    }
}
