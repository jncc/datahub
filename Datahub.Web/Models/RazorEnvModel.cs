namespace Datahub.Web.Models {
    public interface IRazorEnvModel
    {
        string GTM_ID { get; }
    }

    public class RazorEnvModel : IRazorEnvModel
    {
        public string GTM_ID { get; private set; }

        public RazorEnvModel(IEnv env)
        {
            this.GTM_ID = env.GTM_ID;
        }
    }
}
