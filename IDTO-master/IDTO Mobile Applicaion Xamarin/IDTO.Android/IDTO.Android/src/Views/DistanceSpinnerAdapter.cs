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
using IDTO.Common.Models;
namespace IDTO.Android
{
	public class DistanceSpinnerAdapter : BaseAdapter
	{

		protected LayoutInflater inflater;
		protected List<Distance> distances;
		protected int _count;

		public override int Count {
			get {
				return _count;
			}
		}
		public DistanceSpinnerAdapter(LayoutInflater inflater,Context c)
		{
			this.inflater = inflater;
			this.distances = Distance.GetPredefined ();
			_count = this.distances.Count;				
		}

		public override Java.Lang.Object GetItem(int position)
		{
			return new DistanceJavaObject(GetDistanceAtPosition (position));
		}

		public Distance GetDistanceAtPosition(int position)
		{
			return distances[position];
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			if (convertView == null) {
				convertView = inflater.Inflate (Resource.Layout.distance_item, null);
			}
			TextView tv = convertView.FindViewById<TextView> (Resource.Id.distance_item_tv);
			Distance distance = GetDistanceAtPosition (position);
			tv.Text = distance.GetFriendlyName ();
			return convertView;
		}

		protected class DistanceJavaObject : Java.Lang.Object 
		{
			public Distance distance{ get; set;}

			public DistanceJavaObject(Distance distance)
			{
				this.distance = distance;
			}
		}
	}
}

