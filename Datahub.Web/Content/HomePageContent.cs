
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
                    Url = "https://hub.jncc.gov.uk/assets/daa8e792-a36e-436b-98d7-e2f38e860650",
                    Title = "The Linking Environment to Trade (LET) Guide",
                    Abstract = "Published in November 2020, The LET Guide summarises results of an assessment of current thinking and best practice designed to help practitioners and policy makers reduce the embodied environmental impact associated with global trade and consumption."
                },
                new AssetSnippet
                {
                    Url = "https://hub.jncc.gov.uk/assets/fc25e10e-857b-4c08-a591-30fcd65d96dc",
                    Title = "EO4cultivar Colombia Resources",
                    Abstract = "The resources, including reports, data, and management guides, produced for the Eo4cultivar project’s case study in Colombia, were published in November 2020."
                },
                new AssetSnippet
                {
                    Url = "https://hub.jncc.gov.uk/assets/048f7e78-a2c6-4982-91c3-e496f063bf2b",
                    Title = "UK Biodiversity Indicators 2020",
                    Abstract = "Summary of the 2020 update of the UK Biodiversity Indicators, published on 15 October 2020."
                },
                new AssetSnippet
                {
                    Url = "https://hub.jncc.gov.uk/assets/ccb9f624-7121-4c32-aefa-e0579d7eaaa1",
                    Title = "JNCC strategy 2020–2025",
                    Abstract = "Our strategy to 2025 describes how we will enable decision makers to address the inter-related environmental crises of climate change and biodiversity loss."
                },
            },
            FeaturedDatasets = new List<AssetSnippet>
            {
                new AssetSnippet
                {
                    Url = "https://hub.jncc.gov.uk/assets/e2a46de5-43d4-43f0-b296-c62134397ce4",
                    Title = "JNCC guidelines for minimising the risk of injury to marine mammals from geophysical surveys",
                    Abstract = "These guidelines outline measures to minimise potential injury to marine mammals (cetaceans and seals) from geophysical surveys (e.g. seismic air-guns, sub-bottom profiling equipment).",
                },
                new AssetSnippet
                {
                    Url = "https://hub.jncc.gov.uk/assets/d6381e39-baa4-4f12-93d7-fa16dd3600b8",
                    Title = "JNCC Open Data Policy",
                    Abstract = "Open data is “data that anyone is free to access, use, modify, and share for any purpose – subject, at most, to measures that preserve provenance and openness” (The Open Definition). Open data is recognised in government policy and by civil society as a system of principles..",
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
