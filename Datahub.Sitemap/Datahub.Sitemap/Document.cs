using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Sitemap
{
    class Document
    {
        public Guid id { get; private set; }
        public String lastModified { get; private set; }
        public String changeFrequency { get; set; }
        public String Url { get; set; }
    }
}
