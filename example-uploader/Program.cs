using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aws4RequestSigner;
using Newtonsoft.Json;

namespace example_uploader
{
    class Program
    {
        Env env = new Env();

        static void Main()
        {
            Console.WriteLine("Hello World!");
            new Program().Example().GetAwaiter().GetResult();
        }

        async Task Example()
        {
            Console.WriteLine($"Using AWS Access Key {env.AWS_ACCESSKEY}");

            var dbUrl = new Uri(new Uri(env.HUB_API_ENDPOINT), "latest/assets");
            Console.WriteLine(dbUrl);

            //string json = File.ReadAllText("../Datahub.Web/Data/topcat/doi_rock.json");

            // note: empty string values are NOT ALLOWED!
            var asset = new
            {
                id = "1122d9be-6f1b-42f2-bdf7-1e26b33779a2",
                digitalObjectIdentifier = "10.25603/840424.1.0.0",
                citation = (string) null, // nulls appear to be ok :-(
                image = new {
                    url = "/images/example-cover-image.png",
                    width = "100",
                    height = "100",
                },
                metadata = new {
                    title = "Prediction of outcrops or subcrops of rock in UK shelf seabed (public)",
                    @abstract = "Prediction of the presence of rock at outcrop or subcrop at the seabed across the UK shelf area. This shapefile was produced through a semi-automated approach, using a Random Forest model combined with an expert interpretation phase.\n\nUse the field \"ROCK_TYPE\" to distinguish between rock at the surface and rock covered by sediment when using the data for habitat analysis.\n\nThe \"CONF_TOTAL\" field provides the final value of the confidence analysis for the feature.\n\nThe audit dataset documents the changes applied to a modelled output detailing the presence of rock at outcrop, or subcrop, in the shapefile. These changes were made on the basis of expert judgement and this shapefile provides a record of additions, deletions and modifications.\n\nThis dataset is the result of 3 individual BGS/Cefas semi automated rock mapping predictions (found within the Raw folder of the BGS data). It maps rock at surface, or rock and thin sediment as predicted across the United Kingdom shelf. Confidence scores also provided for predictions of seabed character. Manual amendments to the rock in Welsh waters were made by BGS in February 2018 following a consultation with Natural Resources Wales.\n\nThe method consists of two elements, namely 1) the automated spatial prediction of the presence and absence of rock at the seabed using a random forest ensemble model, and 2) manual editing of the model outputs based on ancillary geological data and expert knowledge.\n\nA Random Forest model using bathymetric, derived bathymetric, geological and modelled hydrodynamic inputs was combined with an expert interpretation phase. The changes made by BGS during the interpretation phase are logged in a supplementary dataset.\n\nThe Defra Astrium bathymetry dataset was used both as a predictor variable and to provide derived predictor variables (roughness, slope, aspect, curvature, BPI) along with (Telmac & POLCOM) modelled hydrodynamic data and BGS geological inputs.\n\nFinal confidence scores are based on three-steep confidence assessment developed by JNCC (Lillis H., 2016, JNCC Report 591).\n\nThe individual reports are: \nDiesing, M. Green, S.L., Stephens, D., Cooper, R. & Mellett, C.L., 2015, Semi-automated mapping of rock in the English Channel and Celtic Sea, JNCC Report 569, A4, 19pp, ISSN 0963 8901;\nDownie, A.L., Dove, D., Westhead, R.K., Diesing, M., Green, S., Cooper, R., 2016. Semi-automated mapping of rock in the North Sea. JNCC Report 592;\nBrown, L.S., Green, S.L., Stewart, H.A., Diesing, M., Downie, A.-L., Cooper, R., Lillis, H. 2018. Semi-automated mapping of rock in the Irish Sea, Minches, western Scotland and Scottish continental shelf. JNCC Report 609\n \nThis version include updates requested by NRW in Welsh waters (manually edits by BGS in February 2018,  refer to audit shapefile for details). \n\nAvailable at: \nhttp://jncc.defra.gov.uk/page-2132\n\nThis dataset has a DOI: https://doi.org/10.25603/840424.1.0.0",
                    topicCategory = "monitoring",
                    keywords = new [] {
                        new {
                            value = "Marine",
                            vocab = "http://vocab.jncc.gov.uk/jncc-domain"
                        },
                        new {
                            value = "GIS Strategy",
                            vocab = "http://vocab.jncc.gov.uk/jncc-category"
                        },
                        new {
                            value = "Seabed Habitats and Geology",
                            vocab = "http://vocab.jncc.gov.uk/jncc-category"
                        },
                        new {
                            value = "Substrate only",
                            vocab = "http://vocab.jncc.gov.uk/original-seabed-classification-system"
                        },
                        new {
                            value = "Processed",
                            vocab = "http://vocab.jncc.gov.uk/seabed-survey-data-type"
                        },
                        new {
                            value = "Model",
                            vocab = "http://vocab.jncc.gov.uk/seabed-survey-technique"
                        },
                    },
                    temporalExtent = new {
                        begin = "2014-01-01T00:00:00Z",
                        end = "2018-02-10T00:00:00Z"
                    },
                    datasetReferenceDate = "2004-08-01",
                    lineage = "This was imagined by a developer.",
                    responsibleOrganisation = new {
                        name = "Joint Nature Conservation Committee",
                        email = "data@jncc.gov.uk",
                        role = "distributor"
                    },
                    limitationsOnPublicAccess = "no limitations",
                    useConstraints = "Released under the Open Government Licence v3.0. Attribution statement: “Contains data from © JNCC/BGS/Cefas 2018”.",
                    spatialReferenceSystem = "GCS_WGS_1984",
                    metadataDate = "2018-08-30T01:00:01.7578988Z",
                    metadataPointOfContact = new {
                        name = "Marine Monitoring & Evidence, JNCC",
                        Email = "habitatmapping@jncc.gov.uk",
                        role = "pointOfContact"
                    },
                    resourceType = "series",
                    metadataLanguage = "English",
                    boundingBox = new {
                        north = 62.139101,
                        south = 48.26365,
                        east = 4.339207,
                        west = -12.565943
                    }
                },
                data = new [] {
                    new {
                        title = "Prediction of outcrops or subcrops of rock in UK shelf seabed (public)",
                        http = new {
                            url = "http://data.jncc.gov.uk/data/7f632f1c-f2d3-464b-9886-5e73a4e5dfe9-c20180716-UKcombinedRock-WGS84.zip",
                            fileExtension = "zip",
                            fileBytes = "10023244"
                        }
                    }
                }
            };

            var json = JsonConvert.SerializeObject(asset);
            Console.WriteLine(json);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = dbUrl,
                Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                )
            };

            var signedRequest = await GetSignedRequest(request);
            var response = await new HttpClient().SendAsync(signedRequest);
            var responseString = await response.Content.ReadAsStringAsync();        

            Console.WriteLine(responseString);
        }



        async Task<HttpRequestMessage> GetSignedRequest(HttpRequestMessage request)
        {
            var signer = new AWS4RequestSigner(env.AWS_ACCESSKEY, env.AWS_SECRETACCESSKEY);
            return await signer.Sign(request, "execute-api", env.AWS_REGION);
        }

        // async Task GetDummyAssets()
        // {
            // let files = await glob('../../../Datahub.Web/Data/**/*.json')
            // var files = Directory.GetFiles("../Datahub.Web/Data", "*.json", SearchOption.AllDirectories);
        // }
    }
}
