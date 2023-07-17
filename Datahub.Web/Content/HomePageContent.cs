
using System.Collections.Generic;

namespace Datahub.Web.Content
{
    public class HomePageContent
    {
        public static HomePageContent Content = new HomePageContent
        {
            Column1Resources = new List<AssetSnippet>
            {
                new AssetSnippet
                {
                    Url = "https://hub.jncc.gov.uk/assets/ccb9f624-7121-4c32-aefa-e0579d7eaaa1",
                    Title = "JNCC Strategy",
                    Abstract = "Our strategy outlines the areas of work that JNCC will undertake, focusing on our role in terrestrial and marine nature conservation and recovery."
                },
                new AssetSnippet
                {
                    Url = "https://hub.jncc.gov.uk/assets/8fab1c70-e45f-4c41-ad5c-5c23fad46dbb",
                    Title = "UK Biodiversity Indicators 2022",
                    Abstract = "A summary of the 2022 update of the UK Biodiversity Indicators, published in December 2022."
                },
            },
            Column2Resources = new List<AssetSnippet>
            {
                new AssetSnippet
                {
                    Url = "https://hub.jncc.gov.uk/assets/19298f4e-a0b8-4b63-895f-4ac21c887ccd",
                    Title = "Nature Recovery Joint Statement",
                    Abstract = "Published in November 2022, Nature Recovery for Our Survival, Prosperity and Wellbeing is a Joint Statement produced by the Statutory Nature Conservation Bodies of the UK."
                },
                new AssetSnippet
                {
                    Url = "https://hub.jncc.gov.uk/assets/6de7bf27-055e-4407-ad29-4814e1613d90",
                    Title = "Nature Positive 2030",
                    Abstract = "Nature Positive 2030 was produced by JNCC, Natural England, Natural Resources Wales, NatureScot and the Northern Ireland Environment Agency in September 2021.",
                },
            },
            FeaturedCategories = new List<CategorySnippet>
            {
                new CategorySnippet
                {
                    Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                    Value = "Human Activities",
                    Description = "Geospatial datasets of activities undertaken by humans in the UK marine environment",
                },
                new CategorySnippet
                {
                    Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                    Value = "Seabed Habitats and Geology",
                    Description = "Geospatial datasets of activities undertaken by humans in the UK marine environment",
                },
                new CategorySnippet
                {
                    Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                    Value = "JNCC Publications",
                    Description = "Official publications produced by Joint Nature Conservation Committee",
                },
                new CategorySnippet
                {
                    Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                    Value = "Offshore Seabed Survey",
                    Description = "Geospatial datasets and other outputs from marine survey cruises",
                },
                new CategorySnippet
                {
                    Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                    Value = "Overseas Territories",
                    Description = "Publications and resources related to British Overseas Territories",
                },
                new CategorySnippet
                {
                    Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                    Value = "Protected Areas",
                    Description = "Spatial boundaries for conservation zones, protected areas, etc.",
                },
                new CategorySnippet
                {
                    Vocab = "http://vocab.jncc.gov.uk/jncc-category",
                    Value = "Earth Observation",
                    Description = "Various mapping outputs using Earth Observation data",
                },
            }
        };

        // end of editable content

        public List<AssetSnippet>    Column1Resources     { get; private set; }
        public List<AssetSnippet>    Column2Resources     { get; private set; }
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
