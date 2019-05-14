using System;

namespace Datahub.Web.Models
{
    public class SearchParams
    {
        public SearchParams()
        {
            this.q = String.Empty;
            this.k = new string[] {};
            this.p = 1;
            this.size = 5;
        }

        public string   q    { get; set; }
        public string[] k    { get; set; }
        public int      p    { get; set; }
        public int      size { get; set; }
    }
}
