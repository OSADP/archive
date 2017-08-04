using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDTO.TravelerPortal.Common
{
    public class ModeType
    {
        public enum ModeId
        {
            WALK = 1,
            BUS = 2
        }

        public static string IdToString(int id)
        {
            if (((int)ModeId.WALK) == id)
                return "walk";
            if (((int)ModeId.BUS) == id)
                return "bus";

            return "unknown";
        }

        public static int StringToId(string modeString)
        {
            if (modeString.Equals(ModeId.WALK.ToString(), StringComparison.CurrentCultureIgnoreCase))
            {
                return (int)ModeId.WALK;
            }
            else if (modeString.Equals(ModeId.BUS.ToString(), StringComparison.CurrentCultureIgnoreCase))
            {
                return (int)ModeId.BUS;
            }
            else
            {
                return (int)ModeId.WALK;
            }
        }
    }
}