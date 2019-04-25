namespace Datahub.Web.Models
{
    public class LayoutViewModel
    {
        public string GTM_ID { get; private set; }
        public string JNCC_WEBSITE_URL { get; private set; }

        public LayoutViewModel(Env env)
        {
            this.GTM_ID = env.GTM_ID;
            this.JNCC_WEBSITE_URL = env.JNCC_WEBSITE_URL;
        }
    }
}
