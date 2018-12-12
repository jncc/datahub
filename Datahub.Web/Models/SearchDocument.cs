using Nest;
using System;

namespace Datahub.Web.Models
{
    [ElasticsearchType(Name = "_doc")]
    public class SearchDocument : SearchResult
    {
        [Text(Name = "id")]
        public Guid Id { get; set; }

        [Text(Name = "data")]
        public string Data { get; set; }
    }
}
