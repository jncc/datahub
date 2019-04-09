using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Sitemap
{
    public class Parameters
    {
        // DynamoDB Variables
        public string Table { get; set; }

        // Hosting URL variables
        public string Scheme { get; set; }
        public string Host { get; set; }
        public string BasePath { get; set; }

        // S3 Sitemap hosting variables
        public string Bucket { get; set; }
        public string Key { get; set; }
    }
}
