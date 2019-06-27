using System.Collections.Generic;
using Nest;

namespace Datahub.Web.Models
{
    [ElasticsearchType(Name = "_doc")]
    public class SearchResult
    {
        [Text(Name = "id")]
        public string Id { get; set; }

        [Text(Name = "site")]
        public string Site { get; set; }

        [Text(Name = "url")]
        public string Url { get; set; }

        [Text(Name = "title")]
        public string Title { get; set; }

        [Text(Name = "content")]
        public string Content { get; set; }

        [Text(Name = "content_truncated")]
        public string ContentTruncated { get; set; }

        [Nested]
        [PropertyName("keywords")]
        public List<Keyword> Keywords { get; set; }

        [Text(Name = "published_date")]
        public string PublishedDate { get; set; }

        [Text(Name = "resource_type")]
        public string ResourceType { get; set; }

        [Text(Name = "parent_id")]
        public string ParentId { get; set; }

        [Text(Name = "parent_title")]
        public string ParentTitle { get; set; }

        [Text(Name = "parent_resource_type")]
        public string ParentResourceType { get; set; }

        [Text(Name = "file_extension")]
        public string FileExtension {get; set;}

        [Number(Name = "file_bytes")]
        public string FileBytes { get; set; }

        // [GeoShape(Name = "footprint")]
        // public IGeoShape Footprint { get; set; }
    }
}
