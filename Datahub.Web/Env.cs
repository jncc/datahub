
using System;

namespace Datahub.Web
{
    public class Env
    {
        public string ES_INDEX { get; private set; }
        public string ES_ENDPOINT_SCHEME { get; private set; }
        public string ES_ENDPOINT_HOST { get; private set; }
        public string ES_ENDPOINT_PORT { get; private set; }
        public string AWS_ACCESS_KEY_ID { get; private set; }
        public string AWS_SECRET_ACCESS_KEY { get; private set; }
        public string AWS_DEFAULT_REGION { get; private set; }
        public string GOOGLE_ANALYTICS { get; private set; }
        public string DB_TABLE { get; private set; }
        public string SITEMAP_S3_BUCKET { get; private set; }
        public string SITEMAP_S3_KEY { get; private set; }
        public string FORCE_HTTPS { get; private set; }
        public string JNCC_WEBSITE_URL { get; private set; }

        public Env()
        {
            this.ES_INDEX = GetVariable("ES_INDEX");
            this.ES_ENDPOINT_SCHEME = GetVariable("ES_ENDPOINT_SCHEME");
            this.ES_ENDPOINT_HOST = GetVariable("ES_ENDPOINT_HOST");
            this.ES_ENDPOINT_PORT = GetVariable("ES_ENDPOINT_PORT");
            this.AWS_ACCESS_KEY_ID = GetVariable("AWS_ACCESS_KEY_ID", false);
            this.AWS_SECRET_ACCESS_KEY = GetVariable("AWS_SECRET_ACCESS_KEY", false);
            this.AWS_DEFAULT_REGION = GetVariable("AWS_DEFAULT_REGION", false);
            this.GOOGLE_ANALYTICS = GetVariable("GOOGLE_ANALYTICS", false);
            this.DB_TABLE = GetVariable("DB_TABLE", false);
            this.SITEMAP_S3_BUCKET = GetVariable("SITEMAP_S3_BUCKET");
            this.SITEMAP_S3_KEY = GetVariable("SITEMAP_S3_KEY");
            this.FORCE_HTTPS = GetVariable("FORCE_HTTPS", false);
            this.JNCC_WEBSITE_URL = GetVariable("JNCC_WEBSITE_URL", false, "https://jncc.gov.uk");
        }

        string GetVariable(string name, bool required = true, string defaultValue = null)
        {
            var value = Environment.GetEnvironmentVariable(name) ?? defaultValue;

            if (required && value == null)
                throw new Exception($"Environment variable {name} is required but not set.");

            return value;
        }
    }
}
