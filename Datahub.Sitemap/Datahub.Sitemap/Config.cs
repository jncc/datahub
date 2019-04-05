using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Sitemap
{
    public class Config
    {
        // DynamoDB Variables
        public string Table { get; set; }

        // Hosting URL variables
        public string Scheme { get; set; }
        public string Host { get; set; }
        public string BasePath { get; set; }

        // S3 Sitemap hosting variables
        public string bucket { get; set; }
        public string key { get; set; }
    }
}
