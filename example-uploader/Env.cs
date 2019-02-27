
using System;
using dotenv.net;

namespace example_uploader
{
    public class Env
    {
        public string AWS_REGION { get; private set; }
        public string AWS_ACCESSKEY { get; private set; }
        public string AWS_SECRETACCESSKEY { get; private set; }

        public Env()
        {
            DotEnv.Config();

            this.AWS_REGION = GetVariable("AWS_REGION");
            this.AWS_ACCESSKEY = GetVariable("AWS_ACCESSKEY");
            this.AWS_SECRETACCESSKEY = GetVariable("AWS_SECRETACCESSKEY");
        }

        string GetVariable(string name, bool required = true, string defaultValue = null)
        {
            string value = Environment.GetEnvironmentVariable(name) ?? defaultValue;

            if (required && value == null)
                throw new Exception($"Environment variable {name} is required but not set.");

            return value;
        }
    }
}
