using System;
using System.Net.Http;

using wfm.geniali.rest;
using wfm.geniali.rest.contracts;

namespace wfm.geniali.cli
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using(HttpClient httpClient = new HttpClient())
            {
                WfmClient client = new WfmClient(httpClient);

                SwaggerResponse<Items_payload> result = client.ItemsAsync().Result;

                foreach(Short_item shortItem in result.Result.Payload.Items)
                {
                    Console.WriteLine(shortItem.ToString());
                }
            }
        }
    }
}
