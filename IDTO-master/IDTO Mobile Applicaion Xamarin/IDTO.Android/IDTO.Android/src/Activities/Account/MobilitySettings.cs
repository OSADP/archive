using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace IDTO.Android
{
	class MobilitySettings
	{

		public readonly Boolean bike;
		public readonly Boolean wheelchair;
		public readonly Boolean taxicab;
		public readonly Boolean zimride;

		public MobilitySettings(Boolean bike, Boolean wheelchair, Boolean taxicab, Boolean zimride)
		{
			this.bike = bike;
			this.wheelchair = wheelchair;
			this.taxicab = taxicab;
			this.zimride = zimride;
		}
	}
}

