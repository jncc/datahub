namespace Datahub.Web.Models
{
    public interface ILayoutViewModel
    {
        string GTM_ID { get; }
        string JNCC_WEBSITE_URL { get; }
    }

    public class LayoutViewModel : ILayoutViewModel
    {
        public string GTM_ID { get; private set; }
        public string JNCC_WEBSITE_URL { get; private set; }

        public LayoutViewModel(IEnv env)
        {
            this.GTM_ID = env.GTM_ID;
            this.JNCC_WEBSITE_URL = env.JNCC_WEBSITE_URL;
        }
    }
}
