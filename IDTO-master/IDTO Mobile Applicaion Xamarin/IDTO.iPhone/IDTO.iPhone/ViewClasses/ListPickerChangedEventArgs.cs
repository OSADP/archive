using System;

namespace IDTO.iPhone
{
	public class ListPickerChangedEventArgs
	{
		public ListPickerChangedEventArgs (string value)
		{
			this.SelectedValue = value;
		}

		public string SelectedValue{ get; private set;}

	}
}

