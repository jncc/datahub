
using System.Collections.Generic;

namespace Datahub.Web.Content
{
    public class HomePageContent
    {
        public static HomePageContent Content = new HomePageContent
        {
            FeaturedPublications = new List<AssetSnippet>
            {
                new AssetSnippet
                {
                    Url = "https://hub.jncc.gov.uk/assets/12345",
                    Title = "JNCC guidelines for minimising the risk of injury to marine mammals from geophysical surveys 2017",
                    Abstract = "These guidelines outline measures to minimise potential injury to marine mammals (cetaceans and seals)from geophysical surveys (e.g. seismic air- guns, sub-bottom profiling equipment). In addition to the current guidelines..."
                },
                new AssetSnippet
                {
                    Url = "https://hub.jncc.gov.uk/assets/12345",
                    Title = "Marine Recorder Public UK snapshot - v20190208 2019",
                    Abstract = "The Spring 2019 Marine Recorder version 51 snapshot. The Marine Recorder database holds information on UK marine benthic data such as species, biotopes and physical attributes. Data extracted from a Marine Recorder database..."
                },
            },
            FeaturedDatasets = new List<AssetSnippet>
            {
                new AssetSnippet
                {
                    Url = "https://hub.jncc.gov.uk/assets/12345",
                    Title = "Marine Recorder Public UK snapshot - v20190208 2019",
                    Abstract = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi...",
                },
                new AssetSnippet
                {
                    Url = "https://hub.jncc.gov.uk/assets/12345",
                    Title = " JNCC guidelines for minimising the risk of injury to marine mammals from geophysical surveys 2017",
                    Abstract = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi...",
                },
            },
            FeaturedCategories = new List<CategorySnippet>
            {
                new CategorySnippet
                {
                    Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                    Value = "JNCC Publications",
                    Description = "Official publications produced by Joint Nature Conservation Committee (JNCC)",
                },
                new CategorySnippet
                {
                    Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                    Value = "Another Category",
                    Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
                },
                new CategorySnippet
                {
                    Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                    Value = "Another Category",
                    Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
                },
                new CategorySnippet
                {
                    Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                    Value = "Another Category",
                    Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
                },
                new CategorySnippet
                {
                    Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                    Value = "Another Category",
                    Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
                },
                new CategorySnippet
                {
                    Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                    Value = "Another Category",
                    Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
                },
            }
        };

        // end of editable content

        public List<AssetSnippet>    FeaturedPublications { get; private set; }
        public List<AssetSnippet>    FeaturedDatasets     { get; private set; }
        public List<CategorySnippet> FeaturedCategories   { get; private set; }

    }

    public class AssetSnippet
    {
        public string Url      { get; set; }
        public string Title    { get; set; }
        public string Abstract { get; set; }
    }

    public class CategorySnippet
    {
        public string Vocab       { get; set; }
        public string Value       { get; set; }
        public string Description { get; set; }
    }
}
