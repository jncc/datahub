
using System;

namespace Datahub.Web
{
    public interface IEnv
    {
        string ES_INDEX { get; }
    }

    public class Env : IEnv
    {
        public string ES_INDEX { get; private set; }

        public Env()
        {
            this.ES_INDEX = Environment.GetEnvironmentVariable("ES_INDEX");
        }
    }
}
