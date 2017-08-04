/*!
    @file         InfloDb/TMEInput_CVData.cs
    @author       Joshua Branch

    @copyright
    Copyright (c) 2013 Battelle Memorial Institute. All rights reserved.

    @par
    Unauthorized use or duplication may violate state, federal and/or
    international laws including the Copyright Laws of the United States
    and of other international jurisdictions.

    @par
    @verbatim
    Battelle Memorial Institute
    505 King Avenue
    Columbus, Ohio  43201
    @endverbatim

    @brief
    TBD

    @details
    TBD
*/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace InfloCommon.InfloDb
{
    public partial class TMEInput_CVData
    {
        [Key]
        public int CVMessageIdentifier { get; set; }
        public string NomadicDeviceID { get; set; }
        public System.DateTimeOffset Timestamp { get; set; }
        public short Speed { get; set; }
        public short Heading { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double MileMarkerLocation { get; set; }
        public bool Queued { get; set; }
        public string RoadwayID { get; set; }
        public Nullable<double> CoefficientOfFriction { get; set; }
        public Nullable<short> Temperature { get; set; }
        public string LinkIdentifier { get; set; }
        public string SubLinkIdentifier { get; set; }

    }
}
