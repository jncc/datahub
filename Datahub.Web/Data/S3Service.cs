using Amazon.S3;
using Amazon.S3.Model;
using Amazon;
using Amazon.Runtime;
using Datahub.Web.Pages.Helpers;
using System.IO;
using System.Threading.Tasks;

namespace Datahub.Web.Data
{
    public interface IS3Service
    {
        Task<Stream> GetObjectAsStream(string bucket, string key);
    }

    public class S3Service : IS3Service
    {
        private AmazonS3Client _client;
        private IEnv _env;

        public S3Service(IEnv env)
        {
            _client = InitialiseClient();
            _env = env;
        }

        private AmazonS3Client InitialiseClient()
        {
            if (_env.AWS_ACCESS_KEY_ID.IsNotBlank() && _env.AWS_SECRET_ACCESS_KEY.IsNotBlank())
            {
                return new AmazonS3Client(
                    new BasicAWSCredentials(_env.AWS_ACCESS_KEY_ID, _env.AWS_SECRET_ACCESS_KEY),
                    RegionEndpoint.GetBySystemName(_env.AWS_DEFAULT_REGION));

            }
            else
            {
                return new AmazonS3Client(
                    new InstanceProfileAWSCredentials(), RegionEndpoint.GetBySystemName(_env.AWS_DEFAULT_REGION));
            }
        }

        public async Task<Stream> GetObjectAsStream(string bucket, string key)
        {
            // Error can pop out of here, but should be handled/logged by error request middleware further up
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucket,
                Key = key
            };

            GetObjectResponse response = await _client.GetObjectAsync(request);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.ResponseStream;
            }

            throw new FileNotFoundException(string.Format("Got a {0} response when trying to retrieve s3://{1}/{2}", response.HttpStatusCode, bucket, key));
        }
    }
}
