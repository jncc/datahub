namespace Datahub.Web.Models {
    public interface ILayoutViewModel
    {
        string GTM_ID { get; }
    }

    public class LayoutViewModel : ILayoutViewModel
    {
        public string GTM_ID { get; private set; }

        public LayoutViewModel(IEnv env)
        {
            this.GTM_ID = env.GTM_ID;
        }
    }
}
