using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IDTO.Common.Models
{
	public class Distance
	{
		public enum UnitsOfDistance { METERS, MILES	};

		private double distance;
		private UnitsOfDistance units;
		private string friendlyName = "";

		public Distance (double distance, UnitsOfDistance units, string friendlyName="")
		{
			this.distance = distance;
			this.units = units;
			this.friendlyName=friendlyName;
		}

		public Distance (double distance, UnitsOfDistance currentUnits, UnitsOfDistance desiredUnits, string friendlyName="")
		{
			this.distance = distance;
			this.units = currentUnits;	
			this.friendlyName=friendlyName;
			this.ConvertTo (desiredUnits, friendlyName);
		}

		public bool ConvertTo(UnitsOfDistance finalUnits, string friendlyName="")
		{
			try{
				if (units == UnitsOfDistance.METERS && finalUnits == UnitsOfDistance.MILES) {
					this.distance = ConvertMetersToMiles (distance);
					this.units = finalUnits;
					this.friendlyName=friendlyName;
					return true;
				} else if (units == UnitsOfDistance.MILES && finalUnits == UnitsOfDistance.METERS) {
					this.distance = ConvertMilesToMeters (distance);	
					this.friendlyName=friendlyName;
					return true;
				}
			}catch{
			}
			return false;
		}

		public double GetDistanceValue()
		{
			return distance;
		}

		public string GetFriendlyName()
		{
			return friendlyName;
		}

		public UnitsOfDistance GetUnitsOfDistance()
		{
			return units;
		}

		public static double ConvertMilesToMeters(double miles)
		{		
			return miles * 1609.344;
		}

		public static double ConvertMetersToMiles(double meters)
		{
			return meters * 0.000621371;
		}

		public static List<Distance> GetPredefined()
		{
			List<Distance> predefined =new List<Distance> ();
			predefined.Add(new Distance(.25, UnitsOfDistance.MILES, UnitsOfDistance.METERS, "1/4 mi"));
			predefined.Add(new Distance(.5, UnitsOfDistance.MILES, UnitsOfDistance.METERS, "1/2 mi"));
			predefined.Add(new Distance(.75, UnitsOfDistance.MILES, UnitsOfDistance.METERS, "3/4 mi"));
			predefined.Add(new Distance(1, UnitsOfDistance.MILES, UnitsOfDistance.METERS, "1 mi"));
			return predefined;
		}

		public static Distance GetPredefinedDefault ()
		{
			return GetPredefined () [0];
		}

	}
}

