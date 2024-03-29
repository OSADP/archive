//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace InfloCommon.InfloDb
{
    using System;
    using System.Collections.Generic;
    
    public partial class TME_CVData_SubLink
    {
        public string RoadwayId { get; set; }
        public string SubLinkId { get; set; }
        public System.DateTime DateProcessed { get; set; }
        public Nullable<short> IntervalLength { get; set; }
        public Nullable<double> TSSAvgSpeed { get; set; }
        public Nullable<double> WRTMSpeed { get; set; }
        public double CVAvgSpeed { get; set; }
        public short NumberCVs { get; set; }
        public short NumberQueuedCVs { get; set; }
        public Nullable<short> PercentQueuedCVs { get; set; }
        public Nullable<bool> CVQueuedState { get; set; }
        public Nullable<bool> CVCongestedState { get; set; }
        public Nullable<short> RecommendedTargetSpeed { get; set; }
        public string RecommendedTargetSpeedSource { get; set; }
    }
}
