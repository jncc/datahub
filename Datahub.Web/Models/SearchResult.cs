using System.Collections.Generic;
using Nest;

namespace Datahub.Web.Models
{
    [ElasticsearchType(Name = "_doc")]
    public class SearchResult
    {
        [Text(Name = "site")]
        public string Site { get; set; }

        [Text(Name = "title")]
        public string Title { get; set; }

        [Text(Name = "content")]
        public string Content { get; set; }

        [Text(Name = "content_truncated", Index = false)]
        public string ContentTruncated { get; set; }

        [Text(Name = "url")]
        public string URL { get; set; }

        [Nested]
        [PropertyName("keywords")]
        public List<Keyword> Keywords { get; set; }

        [Date(Format = "yyyy-MM-ddTHH:mm:ssZ")]
        public string PublishedDate { get; set; }

        [Text(Name = "parent_id")]
        public string ParentId { get; set; }

        [Text(Name = "parent_title")]
        public string ParentTitle { get; set; }

        [Text(Name = "mime_type")]
        public string MimeType { get; set; }

        [Text(Name = "data_type")]
        public string DataType { get; set; }

        [Nested]
        [PropertyName("datahub_keywords")]
        public Keyword[] DatahubKeywords { get; set; }

        [GeoShape(Name = "footprint")]
        public IGeoShape Footprint { get; set; }
    }
}
