using System;
using System.Collections.Generic;
using System.Linq;

namespace IDTO.Common.Models
{
    public class ModeType
    {
        public enum ModeId
        {
            WALK = 1,
			BUS = 2,
			RAIL = 4
        }

		public static string IdToString(int id)
		{
			if (((int)ModeId.WALK)==id)
				return "walk";
			if (((int)ModeId.BUS)==id)
				return "bus";
			if (((int)ModeId.RAIL)==id)
				return "rail";


			return "unknown";
		}

        public static int StringToId(string modeString)
        {
            if (modeString.Equals(ModeId.WALK.ToString()))
            {
                return (int) ModeId.WALK;
            }
            else if (modeString.Equals(ModeId.BUS.ToString()))
            {
                return (int) ModeId.BUS;
            }
			else if (modeString.Equals(ModeId.RAIL.ToString()))
			{
				return (int) ModeId.RAIL;
			}
            else
            {
                return (int) ModeId.WALK;
            }
        }
    }
}