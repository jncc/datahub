
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
        public string BASE_URL { get; private set; }
        public string FORCE_HTTPS { get; private set; }
        public string JNCC_WEBSITE_URL { get; private set; }

        public Env()
        {
            this.ES_INDEX = GetVariable("ES_INDEX", Required.Yes);
            this.ES_ENDPOINT_SCHEME = GetVariable("ES_ENDPOINT_SCHEME", Required.Yes);
            this.ES_ENDPOINT_HOST = GetVariable("ES_ENDPOINT_HOST", Required.Yes);
            this.ES_ENDPOINT_PORT = GetVariable("ES_ENDPOINT_PORT", Required.Yes);
            this.SITEMAP_S3_BUCKET = GetVariable("SITEMAP_S3_BUCKET", Required.Yes);
            this.SITEMAP_S3_KEY = GetVariable("SITEMAP_S3_KEY", Required.Yes);
            this.AWS_ACCESS_KEY_ID = GetVariable("AWS_ACCESS_KEY_ID", Required.No);
            this.AWS_SECRET_ACCESS_KEY = GetVariable("AWS_SECRET_ACCESS_KEY", Required.No);
            this.AWS_DEFAULT_REGION = GetVariable("AWS_DEFAULT_REGION", Required.No);
            this.GOOGLE_ANALYTICS = GetVariable("GOOGLE_ANALYTICS", Required.No);
            this.DB_TABLE = GetVariable("DB_TABLE", Required.No);
            this.BASE_URL = GetVariable("BASE_URL", Required.No);
            this.FORCE_HTTPS = GetVariable("FORCE_HTTPS", Required.No);
            this.JNCC_WEBSITE_URL = GetVariable("JNCC_WEBSITE_URL", Required.No, "https://jncc.gov.uk");
        }

        string GetVariable(string name, Required required, string defaultValue = null)
        {
            var value = Environment.GetEnvironmentVariable(name) ?? defaultValue;

            if (required == Required.Yes && value == null)
                throw new Exception($"Environment variable {name} is required but not set.");

            return value;
        }
    }

    enum Required { Yes, No }
}
