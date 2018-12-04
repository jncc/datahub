using System;
using System.Collections.Generic;
using Nest;

namespace Datahub.Web.Models
{
    public class Asset
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DigitalObjectIdentifier { get; set; }
        public Metadata Metadata { get; set; }
        public List<Data> Data { get; set; }
    }

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
        public string LimitationsOnOublicAccess { get; set; }
        public string UseConstraints { get; set; }
        public string Copyright { get; set; }
        public string SpatialReferenceSystem { get; set; }
        public DateTime MetadataDate { get; set; }
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
        [Text(Name = "vocab")]
        public string Vocab { get; set; }

        [Text(Name = "value")]
        public string Value { get; set; }
    }

    public class Data
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public HTTP HTTP { get; set; }
    }

    public interface IDataType
    {
        string Type { get; set; }
        //string DataFormat { get; set; }
    }

    public class HTTP : IDataType
    {
        public string URL { get; set; }
        public string Type { get; set; }
        //public string DataFormat { get; set; }
    }
}

namespace Datahub.Web.Models.DataTypes
{
}