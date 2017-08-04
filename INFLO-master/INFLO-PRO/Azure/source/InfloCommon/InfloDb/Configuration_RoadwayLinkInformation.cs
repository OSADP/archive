/*!
    @file         InfloDb/Configuration_RoadwayLinkInformation.cs
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
    public partial class Configuration_RoadwayLinkInformation
    {
        public string RoadwayIdentifier { get; set; }
        [Key]
        public string RoadwayLinkIdentifier { get; set; }
        public double BeginMileMarker { get; set; }
        public double EndMileMarker { get; set; }
        public string BeginCrossStreetName { get; set; }
        public string EndCrossStreetName { get; set; }
        public string UpstreamRoadwayLinkIdentifier { get; set; }
        public string DownstreamRoadwayLinkIdentifier { get; set; }
        public Nullable<short> NumberOfLanes { get; set; }
        public short SpeedLimit { get; set; }
        public string VSLSignID { get; set; }
        public string DMSID { get; set; }
        public string RSEID { get; set; }
        public string DirectionOfTravel { get; set; }
    }
}
