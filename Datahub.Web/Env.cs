
using System;

namespace Datahub.Web
{
    public interface IEnv
    {
        string ES_INDEX { get; }
        string ES_ENDPOINT_SCHEME { get; }
        string ES_ENDPOINT_HOST { get; }
        string ES_ENDPOINT_PORT { get; }
        string DB_TABLE { get; }
        string AWS_ACCESS_KEY_ID { get; }
        string AWS_SECRET_ACCESS_KEY { get; }
        string AWS_DEFAULT_REGION { get; }
        string AWS_DEFAULT_PROFILE { get; }
        string GTM_ID { get; }
    }

    public class Env : IEnv
    {
        public string ES_INDEX { get; private set; }
        public string ES_ENDPOINT_SCHEME { get; private set; }
        public string ES_ENDPOINT_HOST { get; private set; }
        public string ES_ENDPOINT_PORT { get; private set; }
        public string AWS_ACCESS_KEY_ID { get; private set; }
        public string AWS_SECRET_ACCESS_KEY { get; private set; }
        public string AWS_DEFAULT_REGION { get; private set; }
        public string AWS_DEFAULT_PROFILE { get; private set; }
        public string GTM_ID { get; private set; }
        public string DB_TABLE { get; private set; }

        public Env()
        {
            this.ES_INDEX = GetVariable("ES_INDEX");
            this.ES_ENDPOINT_SCHEME = GetVariable("ES_ENDPOINT_SCHEME");
            this.ES_ENDPOINT_HOST = GetVariable("ES_ENDPOINT_HOST");
            this.ES_ENDPOINT_PORT = GetVariable("ES_ENDPOINT_PORT");
            this.AWS_ACCESS_KEY_ID = GetVariable("ES_AWS_ACCESSKEY", false);
            this.AWS_SECRET_ACCESS_KEY = GetVariable("ES_AWS_SECRETACCESSKEY", false);
            this.AWS_DEFAULT_REGION = GetVariable("ES_AWS_REGION", false);
            this.AWS_DEFAULT_PROFILE = GetVariable("ES_AWS_PROFILE", false);
            this.GTM_ID = GetVariable("GTM_ID", false, "");
            this.DB_TABLE = GetVariable("DB_TABLE");
        }

        string GetVariable(string name, bool required = true, string defaultValue = null)
        {
            string value = Environment.GetEnvironmentVariable(name) ?? defaultValue;

            if (required && value == null)
                throw new Exception($"Environment variable {name} is required but not set.");

            return value;
        }
    }
}
