using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Nest;

namespace MyDemo.Models.Elasticsearch
{
    public class ESClientProvider : IESClientProvider
    {
        private readonly IConfiguration _configuration;
        private ElasticClient _client;

        public ESClientProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public ElasticClient GetClient()
        {
            if (_client != null)
                return _client;

            InitClient();
            return _client;
        }

        private void InitClient()
        {
            var ss = _configuration["EsUrl"];
            var node = new Uri("http://127.0.0.1:9200/");
            _client = new ElasticClient(new ConnectionSettings(node).DefaultIndex("articles"));
        }
    }

    public interface IESClientProvider
    {
        ElasticClient GetClient();
    }
}