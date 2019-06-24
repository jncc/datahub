using System;
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
        Task<byte[]> GetObjectAsByteArray(string bucket, string key);
    }

    public class S3Service : IS3Service
    {
        private AmazonS3Client _client;
        private Env _env;

        public S3Service(Env env)
        {
            _env = env;
            _client = InitialiseClient();
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

        public async Task<byte[]> GetObjectAsByteArray(string bucket, string key)
        {
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucket,
                Key = key
            };

            using (GetObjectResponse response = await _client.GetObjectAsync(request))
            using (Stream responseStream = response.ResponseStream)
            using (StreamReader reader = new StreamReader(responseStream))
            {
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    string bodyContent = reader.ReadToEnd();
                    return System.Text.Encoding.UTF8.GetBytes(bodyContent);
                }

                throw new Exception(string.Format("Got a {0} response when trying to retrieve s3://{1}/{2}", response.HttpStatusCode, bucket, key));
            }
        }
    }
}
