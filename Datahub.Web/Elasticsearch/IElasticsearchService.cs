using System.Threading.Tasks;
using Datahub.Web.Models;
using Nest;

namespace Datahub.Web.Elasticsearch
{
    public interface IElasticsearchService
    {
        ElasticClient Client();

        Task<IIndexResponse> CreateDocument(SearchDocument result);
        Task<IDeleteByQueryResponse> DeleteDocument(string search);
    }
}
