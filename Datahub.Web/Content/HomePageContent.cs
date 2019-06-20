
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
                    Url = "https://hub.jncc.gov.uk/assets/4073feba-5a2f-4fe9-9682-ea098c84d73f",
                    Title = "Joint Committee Meeting - September 2016",
                    Abstract = "The one hundred and eighth meeting of the Joint Nature Conservation Committee."
                },
                new AssetSnippet
                {
                    Url = "https://hub.jncc.gov.uk/assets/328bf8f3-f1e9-499c-80a5-0ecc81fd7334",
                    Title = "The use of harbour porpoise sightings data to inform the development of Special Areas of Conservation in UK waters",
                    Abstract = "This paper describes how harbour porpoise sightings' data were used by the UK’s country nature conservation bodies (CNCBs) to identify possible Special Areas of Conservation (SACs)."
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
                    Value = "Geospatial datasets of seabed habitats, features and geology",
                    Description = "",
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
