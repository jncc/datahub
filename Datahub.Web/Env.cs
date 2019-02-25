
using System;

namespace Datahub.Web
{
    public interface IEnv
    {
        string ES_INDEX { get; }
        string ES_SITE { get; }
    }

    public class Env : IEnv
    {
        public string ES_INDEX { get; private set; }
        public string ES_SITE => "datahub"; // this doesn't really ever change except perhaps for occassional dev use

    public Env()
        {
            this.ES_INDEX = Environment.GetEnvironmentVariable("ES_INDEX");
        }
    }
}
