
using System;

namespace Datahub.Web
{
    public interface IEnv
    {
        string ES_INDEX { get; }
        string ES_ENDPOINT_SCHEME { get; }
        string ES_ENDPOINT_HOST { get; }
        string ES_ENDPOINT_PORT { get; }
        string ES_AWS_ACCESSKEY { get; }
        string ES_AWS_SECRETACCESSKEY { get; }
        string ES_AWS_REGION { get; }
        string ES_AWS_PROFILE { get; }
    }

    public class Env : IEnv
    {
        public string ES_INDEX { get; private set; }
        public string ES_ENDPOINT_SCHEME { get; private set; }
        public string ES_ENDPOINT_HOST { get; private set; }
        public string ES_ENDPOINT_PORT { get; private set; }
        public string ES_AWS_ACCESSKEY { get; private set; }
        public string ES_AWS_SECRETACCESSKEY { get; private set; }
        public string ES_AWS_REGION { get; private set; }
        public string ES_AWS_PROFILE { get; private set; }

        public Env()
        {
            this.ES_INDEX = GetVariable("ES_INDEX");
            this.ES_ENDPOINT_SCHEME = GetVariable("ES_ENDPOINT_SCHEME");
            this.ES_ENDPOINT_HOST = GetVariable("ES_ENDPOINT_HOST");
            this.ES_ENDPOINT_PORT = GetVariable("ES_ENDPOINT_PORT");
            this.ES_AWS_ACCESSKEY = GetVariable("ES_AWS_ACCESSKEY");
            this.ES_AWS_SECRETACCESSKEY = GetVariable("ES_AWS_SECRETACCESSKEY");
            this.ES_AWS_REGION = GetVariable("ES_AWS_REGION");
            this.ES_AWS_PROFILE = GetVariable("ES_AWS_PROFILE", false);
        }

        string GetVariable(string name, bool required = true, string defaultValue = null)
        {
            string value = Environment.GetEnvironmentVariable(name) ?? defaultValue;
            
            if (required && value == null)
                throw new Exception($"Environment variable ${name} is required but not set.");

            return value;
        }
    }
}
