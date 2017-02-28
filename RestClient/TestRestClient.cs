using System;
using System.Net;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections.Generic;

using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Authenticators;

using Newtonsoft.Json;

using TestTask.Models;

namespace TestTask.WebServices.TestRestClient
{
    public class TestRestClient
    {
        private IRestClient _client;

        private string url;
        private string connectionstring;


        public TestRestClient(string baseURL)
        {
            _client = new RestClient(baseURL);
            //url = ConfigurationManager.AppSettings["budgetConfluenceLogin"];
            //connectionstring = ConfigurationManager.AppSettings["budgetConfluencePassword"];
            _client.AddHandler("application/json", NewtonsoftJsonSerializer.Default);
        }


        /// <summary>
        /// API notes:
        /// 1. If content of the page will not be changed by PUT request => response will still have status 200, 
        /// and version isn't incremented (e.g. if send new version number 5 but content the same response will have version 4)
        /// 
        /// </summary>
        /// <param name="pageId"></param>
        //public ConfluecePage UpdateWikiPage(string resource, string body)
        //{
        //    var pageToUpdate = GetPage(resource + "?expand=body.storage,version");
        //    var newPage = pageToUpdate.DeepClone();
        //    newPage.version.number++;
        //    newPage.body.storage.value = body;

        //    var updateRequest = new RestRequest(resource, Method.PUT);
        //    updateRequest.AddJsonBody(newPage);

        //    var response = ExecuteRequest(updateRequest);

        //    var page = _deserializer.Deserialize<ConfluecePage>(response);
        //    return page;
        //}

        public IRestResponse ExecuteRequest(string Resource,RestSharp.Method method)
        {
            var request = new RestRequest(Resource, method);
            var url = _client.BaseUrl + request.Resource;
            var response = _client.Execute(request);
            return response;
        }
    }
}
