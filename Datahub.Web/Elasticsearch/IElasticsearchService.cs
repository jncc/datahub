using Nest;

namespace Datahub.Web.Elasticsearch
{
    public interface IElasticsearchService
    {
        ElasticClient Client();
    }
}
