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
using Android.Graphics;
using Android.Views.InputMethods;

namespace IDTO.Android
{
	public class TimePickerDialogIntervals : TimePickerDialog
	{
		public const int TimePickerInterval = 5; 
		private bool _ignoreEvent = false;

		public TimePickerDialogIntervals(Context context, EventHandler<TimePickerDialog.TimeSetEventArgs> callBack, int hourOfDay, int minute, bool is24HourView)
			: base(context, (sender, e) => {
				callBack (sender, new TimePickerDialog.TimeSetEventArgs (e.HourOfDay, e.Minute * TimePickerInterval));
			}, hourOfDay, minute/TimePickerInterval, is24HourView)
		{
		}

		protected TimePickerDialogIntervals(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public override void SetView(View view)
		{
			SetupMinutePicker (view);
			base.SetView(view);
		}

		void SetupMinutePicker (View view)
		{
			var numberPicker = FindMinuteNumberPicker (view as ViewGroup);
			if (numberPicker != null) {
				numberPicker.MinValue = 0;
				numberPicker.MaxValue = 11;
				numberPicker.SetDisplayedValues (new String[] { "00", "05", "10", "15", 
					"20", "25", "30", "35", "40", "45", "50", "55" });
			}
		}

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			GetButton((int)DialogButtonType.Negative).Visibility = ViewStates.Gone;
			this.SetCanceledOnTouchOutside (false);

		}

		private NumberPicker FindMinuteNumberPicker(ViewGroup viewGroup)
		{
			for (var i = 0; i < viewGroup.ChildCount; i++)
			{
				var child = viewGroup.GetChildAt(i);
				var numberPicker = child as NumberPicker;
				if (numberPicker != null)
				{
					if (numberPicker.MaxValue == 59)
					{
						return numberPicker;
					}
				}

				var childViewGroup = child as ViewGroup;
				if (childViewGroup != null)
				{
					var childResult = FindMinuteNumberPicker (childViewGroup);
					if(childResult !=null)
						return childResult;
				}
			}

			return null;
		}
	}
}

