using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureTestDriver.BSM
{
    /// <summary>
    /// Defines requirements for a utility to format Bsm Messages into a string representation to be send via Http POST's body.
    /// </summary>
    public interface IBsmBundleFormatter
    {
        /// <summary>
        /// Returns a string representing a BSM Bundle of the IBsmMessages passed into the method.
        /// </summary>
        /// <param name="bsmBundle">List of BSM Messages to be bundled.</param>
        /// <returns></returns>
        string GetFormattedString(ICollection<IBsmMessage> bsmBundle);
    }
}
