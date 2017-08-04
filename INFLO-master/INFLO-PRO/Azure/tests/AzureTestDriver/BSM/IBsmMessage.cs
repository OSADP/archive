using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureTestDriver.BSM
{
    /// <summary>
    /// Defines the base requirements for a BSM Message
    /// </summary>
    public interface IBsmMessage
    {
        /// <summary>
        /// Unique message Id Type (typically "BSM", "BsmPart1", "BsmPart2" etc).
        /// </summary>
        string typeid { get; }

        /// <summary>
        /// Some format of encoded data that makes up the BSM Message.
        /// </summary>
        byte[] data { get; }
    }
}
