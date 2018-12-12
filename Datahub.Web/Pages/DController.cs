using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Datahub.Web.Elasticsearch;
using Datahub.Web.Models;

namespace Datahub.Web.Pages
{
    [Route("api/[controller]")]
    [ApiController]
    public class DController : ControllerBase
    {
        private readonly IElasticsearchService _service;
        private readonly ElasticClient _client;

        private const string Index = "main";
        private const string Site = "datahub";
        private const int Start = 0;
        private const int Size = 10;

        public DController(IElasticsearchService elasticsearchService)
        {
            _service = elasticsearchService;
            _client = elasticsearchService.Client();
        }

        public async Task<IDeleteByQueryResponse> OnGet()
        {
            return await _service.DeleteDocument("");
        }
    }
}
