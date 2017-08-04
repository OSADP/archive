/*!
    @file         InfloCommon/Models/TimMessage.cs
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
using System.ComponentModel.DataAnnotations;

namespace InfloCommon.Models
{
    public class TimMessage
    {
        public TimMessage()
        {
            typeid = "TIM";
        }

        [Required]
        [TimTypeId]
        public string typeid { get; set; }

        [Required]
        [Base16String]
        public string payload { get; set; }
    }
}
