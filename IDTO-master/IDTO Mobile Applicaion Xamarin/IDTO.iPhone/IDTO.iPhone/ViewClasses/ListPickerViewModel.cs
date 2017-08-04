using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace IDTO.iPhone
{
	public class ListPickerViewModel :UIPickerViewModel
	{
		public event EventHandler<ListPickerChangedEventArgs> SelectedValueChanged;

		public ListPickerViewModel ()
		{
		}

		public List<string> Items
		{
			get { return this._items;} 
			set { this._items = value;}
		}
		List<string> _items = new List<string>();

		public override int GetRowsInComponent (UIPickerView picker, int component)
		{
			return this._items.Count;
		}

		public override string GetTitle (UIPickerView picker, int row, int component)
		{
			return this._items[row];
		}

		public override int GetComponentCount (UIPickerView picker)
		{
			return 1;
		}

		public override void Selected (UIPickerView picker, int row, int component)
		{
			if (this.SelectedValueChanged != null)
			{
				this.SelectedValueChanged(this, new ListPickerChangedEventArgs(this._items[row]));
			}
		}
	}
}

