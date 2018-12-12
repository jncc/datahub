using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Datahub.Web.Elasticsearch;
using Datahub.Web.Models;
using Nest;
using System.Threading.Tasks;

namespace Datahub.Web.Pages
{
    [Route("api/[controller]")]
    [ApiController]
    public class WController : ControllerBase
    {
        private readonly IElasticsearchService _service;
        private readonly ElasticClient _client;

        public WController(IElasticsearchService elasticsearchService)
        {
            _service = elasticsearchService;
            _client = elasticsearchService.Client();
        }

        public async Task<IIndexResponse> OnGet()
        {
            List<Keyword> keywords = new List<Keyword>(new Keyword[]{
                    new Keyword
                    {
                        Value = "Test",
                        Vocab = "http://vocab.jncc.gov.uk/web-category"
                    }
                });

            var t = await _service.CreateDocument(new SearchDocument
            {
                Id = new System.Guid("0e89d7be-3f69-4d5e-b0e9-4b1bd2444322"),
                Title = "Test Title",
                Content = "Test Document",
                Site = "datahub",
                URL = "http://test.com",
                Keywords = keywords
            });

            return t;
        }
    }
}
