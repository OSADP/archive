/*!
    @file         InfloDb/TMEOutput_Recommended_QWARN_CVMessage.cs
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
    public partial class TMEOutput_Recommended_QWARN_CVMessage
    {
        [Key]
        public System.DateTime Timestamp { get; set; }
        public string RoadwayLinkID { get; set; }
        public string RoadwayID { get; set; }
        public short BOQMMLocation { get; set; }
        public short FOQMMLocation { get; set; }
        public Nullable<short> SpeedInQueue { get; set; }
        public Nullable<double> RateOfQueueGrowth { get; set; }
    }
}
