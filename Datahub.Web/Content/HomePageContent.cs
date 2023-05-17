
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
                    Url = "https://hub.jncc.gov.uk/assets/fc25e10e-857b-4c08-a591-30fcd65d96dc",
                    Title = "EO4cultivar Colombia Resources",
                    Abstract = "The resources, including reports, data, and management guides, produced for the Eo4cultivar project’s case study in Colombia, were published in November 2020."
                },
            },
            Column2Resources = new List<AssetSnippet>
            {
                new AssetSnippet
                {
                    Url = "https://hub.jncc.gov.uk/assets/ccb9f624-7121-4c32-aefa-e0579d7eaaa1",
                    Title = "JNCC Strategy 2020–2025",
                    Abstract = "Our strategy to 2025 describes how we will enable decision makers to address the inter-related environmental crises of climate change and biodiversity loss."
                },
                new AssetSnippet
                {
                    Url = "https://hub.jncc.gov.uk/assets/d6381e39-baa4-4f12-93d7-fa16dd3600b8",
                    Title = "JNCC Open Data Policy",
                    Abstract = "Open data is “data that anyone is free to access, use, modify, and share for any purpose – subject, at most, to measures that preserve provenance and openness” (The Open Definition). JNCC collects and processes a wide range of scientific data, and produces and maintains a large number of datasets. Our open data approach is provided in our Open Data Policy.",
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
