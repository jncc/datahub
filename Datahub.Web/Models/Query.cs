using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Datahub.Web.Models
{
    public class Query
    {
        [JsonProperty(PropertyName = "query")]
        string test;
    }
}
