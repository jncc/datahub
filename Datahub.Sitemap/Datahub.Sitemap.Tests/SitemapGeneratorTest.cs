using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using Datahub.Sitemap;

namespace Datahub.Sitemap.Tests
{
    public class SitemapGeneratorTest
    {
        [Fact]
        public async void TestSitemapGenerator()
        {
            // Invoke the lambda function and confirm the string was upper cased.
            var function = new SitemapGenerator();
            var context = new TestLambdaContext();
            var config = new Config
            {
                Table = "test-table",
                Host = "host.com",
                Scheme = "https"
            };

            var output = await function.SitemapGeneratorHandler(config, context);

            Assert.Equal(config, output);
        }
    }
}
