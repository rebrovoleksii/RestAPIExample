﻿using System;
using System.Net;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections.Generic;

using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Authenticators;

using Newtonsoft.Json;

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
            //url = ConfigurationManager.AppSettings[""];
            //connectionstring = ConfigurationManager.AppSettings[""];
            _client.AddHandler("application/json", NewtonsoftJsonSerializer.Default);
        }

        public IRestResponse ExecuteRequest(string Resource,RestSharp.Method method)
        {
            var request = new RestRequest(Resource, method);
            var url = _client.BaseUrl + request.Resource;
            var response = _client.Execute(request);
            return response;
        }

        public IRestResponse ExecuteRequestWithBody<T>(string Resource, T body,RestSharp.Method method)
        {
            var request = new RestRequest(Resource, method);
            request.AddJsonBody(body);
            var url = _client.BaseUrl + request.Resource;
            var response = _client.Execute(request);
            return response;
        }
    }
}
