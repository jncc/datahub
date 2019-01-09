using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Datahub.Web.Models;
using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.Runtime;
using Amazon;
using Amazon.SQS.ExtendedClient;
using Newtonsoft.Json;

namespace Datahub.Web.Api
{
    [Route("api/sqs")]
    [ApiController]
    public class SQSExtendedController : ControllerBase
    {
        AmazonSQSExtendedClient _sqsExtendedClient;
        AmazonSQSClient _sqsClient;

        public SQSExtendedController()
        {
            Env env = Env.Var;

            _sqsClient = new AmazonSQSClient(new BasicAWSCredentials(env.ESAwsAccessKey, env.ESAwsSecretAccessKey), RegionEndpoint.GetBySystemName(env.ESAwsRegion));
            AmazonS3Client s3Client = new AmazonS3Client(new BasicAWSCredentials(env.ESAwsAccessKey, env.ESAwsSecretAccessKey), RegionEndpoint.GetBySystemName(env.ESAwsRegion));

            _sqsExtendedClient = new AmazonSQSExtendedClient(_sqsClient, new ExtendedClientConfiguration().WithLargePayloadSupportEnabled(s3Client, env.SQSPayloadBucket));
        }

        [HttpPut("/api/sqs/sqs-ext-send")]
        public async Task<SendMessageResponse> SQSExtSend([FromBody] SearchDocument searchDocument)
        {
            return await _sqsExtendedClient.SendMessageAsync(Env.Var.SQSIngestQueueURL, JsonConvert.SerializeObject(searchDocument, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        [HttpGet("/api/sqs/sqs-ext-send-test")]
        public async Task<SendMessageResponse> SQSExtSendTest()
        {
            SearchDocument document = new SearchDocument()
            {
                Id = "1122d9be-6f1b-42f2-bdf7-1e26b33779a2",
                Site = "datahub",
                Title = "Prediction of outcrops or subcrops of rock in UK shelf seabed (public)",
                Content = "Prediction of the presence of rock at outcrop or subcrop at the seabed across the UK shelf area. This shapefile was produced through a semi-automated approach, using a Random Forest model combined with an expert interpretation phase.\n\nUse the field \"ROCK_TYPE\" to distinguish between rock at the surface and rock covered by sediment when using the data for habitat analysis.\n\nThe \"CONF_TOTAL\" field provides the final value of the confidence analysis for the feature.\n\nThe audit dataset documents the changes applied to a modelled output detailing the presence of rock at outcrop, or subcrop, in the shapefile. These changes were made on the basis of expert judgement and this shapefile provides a record of additions, deletions and modifications.\n\nThis dataset is the result of 3 individual BGS/Cefas semi automated rock mapping predictions (found within the Raw folder of the BGS data). It maps rock at surface, or rock and thin sediment as predicted across the United Kingdom shelf. Confidence scores also provided for predictions of seabed character. Manual amendments to the rock in Welsh waters were made by BGS in February 2018 following a consultation with Natural Resources Wales.\n\nThe method consists of two elements, namely 1) the automated spatial prediction of the presence and absence of rock at the seabed using a random forest ensemble model, and 2) manual editing of the model outputs based on ancillary geological data and expert knowledge.\n\nA Random Forest model using bathymetric, derived bathymetric, geological and modelled hydrodynamic inputs was combined with an expert interpretation phase. The changes made by BGS during the interpretation phase are logged in a supplementary dataset.\n\nThe Defra Astrium bathymetry dataset was used both as a predictor variable and to provide derived predictor variables (roughness, slope, aspect, curvature, BPI) along with (Telmac & POLCOM) modelled hydrodynamic data and BGS geological inputs.\n\nFinal confidence scores are based on three-steep confidence assessment developed by JNCC (Lillis H., 2016, JNCC Report 591).\n\nThe individual reports are: \nDiesing, M. Green, S.L., Stephens, D., Cooper, R. & Mellett, C.L., 2015, Semi-automated mapping of rock in the English Channel and Celtic Sea, JNCC Report 569, A4, 19pp, ISSN 0963 8901;\nDownie, A.L., Dove, D., Westhead, R.K., Diesing, M., Green, S., Cooper, R., 2016. Semi-automated mapping of rock in the North Sea. JNCC Report 592;\nBrown, L.S., Green, S.L., Stewart, H.A., Diesing, M., Downie, A.-L., Cooper, R., Lillis, H. 2018. Semi-automated mapping of rock in the Irish Sea, Minches, western Scotland and Scottish continental shelf. JNCC Report 609\n \nThis version include updates requested by NRW in Welsh waters (manually edits by BGS in February 2018,  refer to audit shapefile for details). \n\nAvailable at: \nhttp://jncc.defra.gov.uk/page-2132\n\nThis dataset has a DOI: https://doi.org/10.25603/840424.1.0.0",
                Keywords = new List<Keyword>() {
                  new Keyword()
                  {
                      Value = "Marine",
                      Vocab = "http://vocab.jncc.gov.uk/jncc-domain"
                  },
                  new Keyword()
                  {
                      Value = "GIS Strategy",
                      Vocab = "http://vocab.jncc.gov.uk/jncc-category"
                  },
                  new Keyword()
                  {
                      Value = "Seabed Habitats and Geology",
                      Vocab = "http://vocab.jncc.gov.uk/jncc-category"
                  },
                  new Keyword()
                  {
                      Value = "Substrate only",
                      Vocab = "http://vocab.jncc.gov.uk/original-seabed-classification-system"
                  },
                  new Keyword()
                  {
                      Value = "Processed",
                      Vocab = "http://vocab.jncc.gov.uk/seabed-survey-data-type"
                  },
                  new Keyword()
                  {
                      Value = "Model",
                      Vocab = "http://vocab.jncc.gov.uk/seabed-survey-technique"
                  },
                },
                PublishedDate = "2018-08-30T01:00:01.7578988Z",
                DataType = "Publication"
            };

            return await _sqsExtendedClient.SendMessageAsync(Env.Var.SQSIngestQueueURL, JsonConvert.SerializeObject(document, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }

        [HttpGet("/api/sqs/sqs-ext-get-reg")]
        public async Task<List<Message>> SQSExtGetReg()
        {
            ReceiveMessageResponse req = await _sqsClient.ReceiveMessageAsync(Env.Var.SQSIngestQueueURL);
            List<Message> messages = req.Messages;
            foreach (Message message in messages)
            {
                DeleteMessageRequest delreq = new DeleteMessageRequest(Env.Var.SQSIngestQueueURL, message.ReceiptHandle);
                await _sqsClient.DeleteMessageAsync(delreq);
            }

            return messages;
        }

        [HttpGet("/api/sqs/sqs-ext-get")]
        public async Task<List<Message>> SQSExtGet()
        {
            ReceiveMessageResponse req = await _sqsExtendedClient.ReceiveMessageAsync(Env.Var.SQSIngestQueueURL);
            List<Message> messages = req.Messages;
            foreach (Message message in messages)
            {
                DeleteMessageRequest delReq = new DeleteMessageRequest(Env.Var.SQSIngestQueueURL, message.ReceiptHandle);
                await _sqsExtendedClient.DeleteMessageAsync(delReq);
            }

            return messages;
        }
    }
}
