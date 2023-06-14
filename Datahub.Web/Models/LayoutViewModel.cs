using System;

namespace Datahub.Web.Models
{
    public class LayoutViewModel
    {
        public bool GOOGLE_ANALYTICS { get; private set; }
        public string JNCC_WEBSITE_URL { get; private set; }
        public string JNCC_SEARCH_URL { get; private set; }

        public LayoutViewModel(Env env)
        {
            Boolean.TryParse(env.GOOGLE_ANALYTICS, out bool googleAnalytics);
            
            this.GOOGLE_ANALYTICS = googleAnalytics;
            this.JNCC_WEBSITE_URL = env.JNCC_WEBSITE_URL;
            this.JNCC_SEARCH_URL = env.JNCC_SEARCH_URL;
        }
    }
}
