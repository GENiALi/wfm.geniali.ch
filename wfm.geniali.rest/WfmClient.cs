using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RestSharp;
using RestSharp.Serializers.SystemTextJson;

using ShortItem = wfm.geniali.lib.ShortItem;

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
        
        public async Task<ShortItem.Item> GetShortItemsAsync()
        {
            RestRequest request = new RestRequest("/items");
            return Execute<ShortItem.Item>(request);
        }

        private T Execute<T>(RestRequest request)
            where T : new()
        {
            //request.AddParameter("AccountSid", _accountSid, ParameterType.UrlSegment); // used on every request
            var response = _Client.Execute<T>(request);

            if(response.ErrorException != null)
            {
                const string message         = "Error retrieving response. Check inner details for more info.";
                Exception ex = new Exception(message, response.ErrorException);

                throw ex;
            }

            return response.Data;
        }
    }
}
