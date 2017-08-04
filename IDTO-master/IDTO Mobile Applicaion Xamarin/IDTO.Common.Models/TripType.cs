using System;
using System.Collections.Generic;
using System.Linq;

namespace IDTO.Common.Models
{
    public class TripType
    {
        public enum Type{
            Upcoming=1,
            InProgress=2,
            Past=3,
            All=4

        }
    }
}