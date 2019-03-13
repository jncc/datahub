using System;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Datahub.Web.Pages.Helpers;

namespace Datahub.Web.Data 
{
    public interface IDynamodbService
    {
        AmazonDynamoDBClient Client();
    }

    public class DynamodbService : IDynamodbService
    {
        readonly IEnv env;
        readonly AmazonDynamoDBClient client;

        public DynamodbService(IEnv env)
        {
            this.env = env;
            this.client = InitialiseClient();
        }

        AmazonDynamoDBClient InitialiseClient()
        {
    
            if (env.AWS_ACCESS_KEY_ID.IsNotBlank() && env.AWS_SECRET_ACCESS_KEY.IsNotBlank())
            {
                return new AmazonDynamoDBClient(
                    new BasicAWSCredentials(env.AWS_ACCESS_KEY_ID, env.AWS_SECRET_ACCESS_KEY),
                    RegionEndpoint.GetBySystemName(env.AWS_DEFAULT_REGION));

            } 
            else 
            {
                return new AmazonDynamoDBClient(
                    new InstanceProfileAWSCredentials()
                    , RegionEndpoint.GetBySystemName(env.AWS_DEFAULT_REGION));
            }

        }

        public AmazonDynamoDBClient Client()
        {
            return client;
        }


    }

}
