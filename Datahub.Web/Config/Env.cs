
using System;

namespace Datahub.Web.Config
{
    public interface IPublicEnv
    {
        string GTM_ID { get; }
        string GetVariable(string variable, bool optional = false);
    }

    public interface IEnv : IPublicEnv
    {
        string ES_AWS_REGION { get; }
        string ES_AWS_ACCESSKEY { get; }
        string ES_AWS_SECRETACCESSKEY { get; }
        string ES_AWS_PROFILE { get; }
        string ES_ENDPOINT_HOST { get; }
        string ES_ENDPOINT_SCHEME { get; }
        string ES_ENDPOINT_PORT { get; }
        string ES_INDEX { get; }
        string ES_SITE { get; }
    }

    public class PublicEnv : IPublicEnv
    {
        public string GTM_ID { get; private set; }

        public PublicEnv()
        {
            this.GTM_ID = this.GetVariable("GTM_ID");
        }

        public string GetVariable(string variable, bool optional = false)
        {
            string EnvVariable = Environment.GetEnvironmentVariable(variable);

            if (EnvVariable != null)
            {
                return EnvVariable;
            }

            if (optional)
            {
                return null;
            }

            throw new Exception($"The environment variable {variable} couldn't be read. You may need to define it in your .env file.");
        }
    }

    public class Env : IEnv
    {
        public string ES_AWS_REGION { get; private set; }
        public string ES_AWS_ACCESSKEY { get; private set; }
        public string ES_AWS_SECRETACCESSKEY { get; private set; }
        public string ES_AWS_PROFILE { get; private set; }
        public string ES_ENDPOINT_HOST { get; private set; }
        public string ES_ENDPOINT_SCHEME { get; private set; }
        public string ES_ENDPOINT_PORT { get; private set; }
        public string ES_INDEX { get; private set; }
        public string ES_SITE => "datahub"; // this doesn't really ever change except perhaps for occassional dev use
        public string GTM_ID { get; private set; }

        public Env()
        {
            this.ES_AWS_REGION = this.GetVariable("ES_AWS_REGION", true);
            this.ES_AWS_ACCESSKEY = GetVariable("ES_AWS_ACCESSKEY", true);
            this.ES_AWS_SECRETACCESSKEY = GetVariable("ES_AWS_SECRETACCESSKEY", true);
            this.ES_AWS_PROFILE = GetVariable("ES_AWS_PROFILE", true);

            this.ES_ENDPOINT_HOST = this.GetVariable("ES_ENDPOINT_HOST");
            this.ES_ENDPOINT_PORT = this.GetVariable("ES_ENDPOINT_PORT", true);
            this.ES_ENDPOINT_SCHEME = this.GetVariable("ES_ENDPOINT_SCHEME", true);

            this.ES_INDEX = this.GetVariable("ES_INDEX");
        }

        public string GetVariable(string variable, bool optional = false)
        {
            string EnvVariable = Environment.GetEnvironmentVariable(variable);

            if (EnvVariable != null)
            {
                return EnvVariable;
            }

            if (optional)
            {
                return null;
            }

            throw new Exception($"The environment variable {variable} couldn't be read. You may need to define it in your .env file.");
        }
    }
}
