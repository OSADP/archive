using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INCZONE.Exceptions
{
    public class IncZoneException : Exception
    {
        public IncZoneException()
        {
        }

        public IncZoneException(string message)
            : base(message)
        {
        }

        public IncZoneException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
