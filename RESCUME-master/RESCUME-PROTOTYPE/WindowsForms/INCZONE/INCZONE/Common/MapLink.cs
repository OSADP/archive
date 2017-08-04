using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace INCZONE.Common
{
    public class MapLink
    {
        public Guid Id { get; set; }
        public Guid? mapSetId { get; set; }

        public Guid? startMapNodeId { get; set; }
        public Guid? endMapNodeId { get; set; }
    }
}

