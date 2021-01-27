using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;

namespace MyDemo.Models.Dto
{
    [ElasticsearchType(IdProperty = "Id")]
    public class Article
    {
        [Keyword]
        public string Id { get; set; }

        [Keyword]
        public string Title { get; set; }

        [Keyword]
        public string Auther { get; set; }

        [Keyword]
        public string SubTitle { get; set; }
    }
}
