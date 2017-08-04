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
	public class DateTimePicker 
	{

		public static DateTimePicker create(LayoutInflater inflater, ViewGroup parent, DateTime time, DateTime minTime)
		{
			return new DateTimePicker(time, minTime, inflater.Inflate (Resource.Layout.date_time_picker, parent, true));
		}
	

		private DateTime time;
		private DateTime minTime;
		private DateTimePickerBox hourPicker;
		private DateTimePickerBox minutePicker;
		private DateTimePickerBox ampmPicker;

		private  DateTimePicker(DateTime time, DateTime minTime, View view) 
		{
			this.minTime = minTime;
			List<string> hours = new List<string> ();
			List<string> minutes = new List<string> ();
			List<string> ampm = new List<string> ();

			for (int i = 1; i <= 12; i++) {
				hours.Add(i.ToString("D2"));
			}
			for (int i = 0; i < 60; i+=5) {
				minutes.Add(i.ToString("D2"));
			}
			ampm.Add ("AM");
			ampm.Add ("PM");
			hourPicker = new DateTimePickerBox (view, hours, 0, Resource.Id.date_time_picker_hour_arrow_up,

				Resource.Id.date_time_picker_hour_arrow_down, Resource.Id.date_time_picker_tv_hour);

			minutePicker = new DateTimePickerBox (view, minutes,  0, Resource.Id.date_time_picker_minute_arrow_up, 

				Resource.Id.date_time_picker_minute_arrow_down, Resource.Id.date_time_picker_tv_minute);

			ampmPicker = new DateTimePickerBox (view, ampm, 0, Resource.Id.date_time_picker_ampm_arrow_up, 

				Resource.Id.date_time_picker_ampm_arrow_down, Resource.Id.date_time_picker_tv_ampm);

			SetTime (time);
		}

		public void SetTime(DateTime time)
		{
			if (time < minTime)
				time = minTime;
			ParseAndUpdateTime (time);
		}

		public DateTime GetTime()
		{
			string hh = hourPicker.GetValue ();
			string mm = minutePicker.GetValue ();
			string tt = ampmPicker.GetValue ();
			string timeSTR = hh + ":" + mm + " " + tt;
			Console.WriteLine (timeSTR);
			try{
				time = DateTime.Parse (timeSTR);
			}catch(Exception e) {
				Console.WriteLine ("Failed to parse time");
				Console.WriteLine (e);
			}
			return time;
		}

		private void ParseAndUpdateTime (DateTime time)
		{
			string minute = time.ToString ("mm");
			int min = int.Parse (minute);
			if (min % 5 != 0) {
				time = time.AddMinutes (5-(min % 5));
				minute = time.ToString ("mm");
				min = int.Parse (minute);
			}
			int hour = int.Parse (time.ToString ("hh"));
			string ampm = time.ToString ("tt").ToUpper ();
			hourPicker.SetValue (hour.ToString ("D2"));
			minutePicker.SetValue (min.ToString ("D2"));
			ampmPicker.SetValue (ampm);
			this.time = time;
		}
	}
}

