using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Web.Config
{
    public class AWS
    {
        public string AccessKey { get; set; }
        public string SecretAccessKey { get; set; }
        public string Region { get; set; }
    }

    public class Elasticsearch
    {
        public string Domain { get; set; }
        public AWS AWS { get; set; }
    }
}
