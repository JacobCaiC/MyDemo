using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyDemo.Models.Dto;
using MyDemo.Models.Elasticsearch;
using MyDemo.Services;
using Nest;

namespace MyDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ESController : ControllerBase
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly ElasticClient _client;

        public ESController(ICompanyRepository companyRepository, IESClientProvider clientProvider)
        {
            _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
            _client = clientProvider.GetClient() ?? throw new ArgumentNullException(nameof(clientProvider));
        }


        [HttpGet]
        [Route("CreateIndex")]
        public bool CreateIndex(string indexName)
        {
            var res = _client.Indices.Create(indexName, c => c.Map<Article>(h => h.AutoMap().Properties(ps => ps
                    .Text(s => s
                        .Name(n => n.Title)
                        .Analyzer("ik_smart")
                        .SearchAnalyzer("ik_smart")
                    )
                    .Text(s => s
                        .Name(n => n.SubTitle)
                        .Analyzer("ik_smart")
                        .SearchAnalyzer("ik_smart")
                    )
                )
            ));
            return res.IsValid;
        }

        // [HttpPost]
        // [Route("AddArticles")]
        // public bool AddArticles()
        // {
        //     // 获取数据批量进行插入
        //     List<Article> listArticle = GetArticles();
        //
        //     return _client.IndexMany(listArticle).IsValid;
        // }


        [HttpPost]
        [Route("AddCompanies")]
        public bool AddCompanies()
        {
            // 获取数据批量进行插入
            var listCompanies = _companyRepository.GetAllCompanies();

            return _client.IndexMany(listCompanies).IsValid;
        }

        [HttpPost]
        [Route("SearchHighlight")]
        public List<Article> SearchHighlight(string key, int pageIndex = 0, int pageSize = 10)
        {
            var searchAll = _client.Search<Article>(s => s
                .From(pageIndex)
                .Size(pageSize)
                .Query(q => q
                    .QueryString(qs => qs
                        .Query(key)
                        .DefaultOperator(Operator.Or)))
                .Highlight(h => h
                    .PreTags("<span class='color:red;'>")
                    .PostTags("</span>")
                    .Encoder(HighlighterEncoder.Html)
                    .Fields(
                        fs => fs.Field(p => p.Title),
                        fs => fs.Field(p => p.SubTitle)
                    )
                )
            );

            foreach (var hit in searchAll.Hits)
            {
                foreach (var highlightField in hit.Highlight)
                {
                    if (highlightField.Key == "title")
                    {
                        foreach (var highlight in highlightField.Value)
                        {
                            hit.Source.Title = highlight.ToString();
                        }
                    }
                    else if (highlightField.Key == "subTitle")
                    {
                        foreach (var highlight in highlightField.Value)
                        {
                            hit.Source.SubTitle = highlight.ToString();
                        }
                    }
                }
            }

            return searchAll.Documents.ToList();
        }
    }
}