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

        public async Task<Result<List<ShortItem>>> GetShortItemsAsync()
        {
            RestRequest request = new RestRequest("/items");

            Result<ShortItemRoot> res = await Execute<ShortItemRoot>(request);

            return new Result<List<ShortItem>>(res, res.Data.Payload.Items);
        }

        public async Task<Result<Item>> GetItemAsynx(string urlName)
        {
            RestRequest request = new RestRequest("/items/{urlName}");
            request.AddParameter("urlName", urlName, ParameterType.UrlSegment);

            Result<ItemRoot> res = await Execute<ItemRoot>(request);

            return new Result<Item>(res, res.Data.Payload.Item);
        }

        public async Task<Result<List<Order>>> GetOrdersAsync(string urlName)
        {
            RestRequest request = new RestRequest("/items/{urlName}/orders");
            request.AddParameter("urlName", urlName, ParameterType.UrlSegment);

            Result<OrderRoot> res = await Execute<OrderRoot>(request);

            return new Result<List<Order>>(res, res.Data.Payload.Orders);
        }

        public async Task<Result<Statistics>> GetStatisticsAsync(string urlName)
        {
            RestRequest request = new RestRequest("/items/{urlName}/statistics");
            request.AddParameter("urlName", urlName, ParameterType.UrlSegment);

            Result<StatisticsRoot> res = await Execute<StatisticsRoot>(request);

            return new Result<Statistics>(res, res.Data.Payload);
        }

        private async Task<Result<T>> Execute<T>(RestRequest request)
            where T : new()
        {
            IRestResponse<T> response = await _Client.ExecuteAsync<T>(request);

            if(response.ErrorException != null)
            {
                return new Result<T>().Failure(new Exception("Error retrieving response. Check inner details for more info.", response.ErrorException));
            }

            return new Result<T>().Success(response);
        }
    }
}
