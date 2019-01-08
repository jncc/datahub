using System.Collections.Generic;
using Newtonsoft.Json;

namespace Datahub.Web.Models
{
    public class SearchDocument
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "site")]
        public string Site { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }

        [JsonProperty(PropertyName = "content_truncated")]
        public string ContentTruncated { get; set; }

        [JsonProperty(PropertyName = "keywords")]
        public List<Keyword> Keywords { get; set; }

        [JsonProperty(PropertyName = "published_date")]
        public string PublishedDate { get; set; }

        [JsonProperty(PropertyName = "parent_id")]
        public string ParentId { get; set; }

        [JsonProperty(PropertyName = "parent_title")]
        public string ParentTitle { get; set; }

        [JsonProperty(PropertyName = "mime_type")]
        public string MimeType { get; set; }

        [JsonProperty(PropertyName = "data_type")]
        public string DataType { get; set; }

        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }
    }
}
