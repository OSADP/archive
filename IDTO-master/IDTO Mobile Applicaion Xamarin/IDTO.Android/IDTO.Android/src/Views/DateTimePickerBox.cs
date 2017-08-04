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
	public class DateTimePickerBox
	{
		private List<string> values;
		private int index;
		private TextView tvValue;

		public DateTimePickerBox(View dateTimePickerView, List<string> values, 
			int defaultValueIndex, int upResId, int downResId, int valueResId)
		{
			this.values = values;
			this.index = defaultValueIndex;
			ImageButton btnUp =  dateTimePickerView.FindViewById<ImageButton> (upResId);
			ImageButton btnDown =  dateTimePickerView.FindViewById<ImageButton> (downResId);
			this.tvValue =  dateTimePickerView.FindViewById<TextView> (valueResId);

			btnUp.Click += delegate(object sender, EventArgs e) {
				if (index < values.Count - 1)
					index++;
				else
					index = 0;

				Update ();
			};
			btnDown.Click += delegate(object sender, EventArgs e) {
				if (index > 0)
					index--;
				else
					index = values.Count - 1;

				Update ();
			};

			Update ();
		}

		public string GetValue(){
			return values.ElementAt (index);
		}

		public void SetValue(string value)
		{
			int newIndex = values.IndexOf (value);
			if (newIndex >= 0 && newIndex <= values.Count - 1) {
				index = newIndex;
				Update ();
			}
		}

		private void Update()
		{
			tvValue.Text = GetValue();
		}

	}
}

