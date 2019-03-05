using System;
using System.Collections.Generic;

namespace Datahub.Web.Models
{
    // see https://github.com/jncc/topcat/blob/master/Catalogue.Data/Model/Record.cs
    public class Asset
    {
        public string Id { get; set; }

        public Metadata Metadata { get; set; }

        public Image Image { get; set; }        
        public List<Data> Data { get; set; }

        public string DigitalObjectIdentifier { get; set; }
        public string Citation { get; set; }
    }

    // see https://github.com/jncc/topcat/blob/master/Catalogue.Gemini/Model/Metadata.cs
    public class Metadata
    {
        public string Title { get; set; }
        public string Abstract { get; set; }
        public string TopicCategory { get; set; }
        public List<Keyword> Keywords { get; set; }
        public TemporalExtent TemporalExtent { get; set; }
        public string DatasetReferenceDate { get; set; }
        public string Lineage { get; set; }
        public string AdditionalInformationSource { get; set; }
        public ResponsibleOrganisation ResponsibleOrganisation { get; set; }
        public string LimitationsOnPublicAccess { get; set; }
        public string UseConstraints { get; set; }
        public string Copyright { get; set; }
        public string SpatialReferenceSystem { get; set; }
        public string MetadataDate { get; set; }
        public ResponsibleOrganisation MetadataPointOfContact { get; set; }
        public string ResourceType { get; set; }  // dataset | series | service | nonGeographicDataset | (custom:| publication)
        public BoundingBox BoundingBox { get; set; }
    }

    public class TemporalExtent
    {
        public string Begin { get; set; }
        public string End { get; set; }
    }

    /// <summary>
    ///  Bounding box referenced to WGS 84.
    /// </summary>
    public class BoundingBox
    {
        public decimal North { get; set; }
        public decimal South { get; set; }
        public decimal East { get; set; }
        public decimal West { get; set; }
    }

    public class ResponsibleOrganisation
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class Keyword
    {
        public string Vocab { get; set; }
        public string Value { get; set; }
        public string Link { get; set; }
    }

    public class Data
    {
        public string Title { get; set; }
        public HttpResource Http { get; set; }
    }

    public class HttpResource
    {
        public string Url { get; set; }
        public string FileExtension { get; set; }
        public string FileBytes { get; set; }
    }

    public class Image
    {
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public ImageCrops Crops { get; set; }
    }

    public class ImageCrops
    {
        public string SquareUrl { get; set; }
        public string ThumbnailUrl { get; set; }
    }
}
