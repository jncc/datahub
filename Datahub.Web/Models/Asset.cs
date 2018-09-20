using System;
using System.Collections.Generic;

namespace Datahub.Web.Models {
    public class Asset {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Abstract { get; set; }
        public string TopicCategory { get; set; }
        public List<string> Keywords { get; set; }
        public TemporalExtent TemporalExtent { get; set; }
        public string DatasetReferenceDate { get; set; }
        public string Lineage { get; set; }
        public string ResourceLocator { get; set; }
        public string AdditionalInformationSource { get; set; }
        public string DataFormat { get; set; }
        public ResponsibleParty ResponsibleOrganisation { get; set; }
        public string LimitationsOnPublicAccess { get; set; }
        public string UseConstraints { get; set; }
        public string Copyright { get; set; }
        public string SpatialReferenceSystem { get; set; }
        public DateTime MetadataDate { get; set; }
        public ResponsibleParty MetadataPointOfContact { get; set; }
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

    public class ResponsibleParty
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }}