using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace IDTO.iPhone {

	public delegate void ActionSheetPickerDone();
	public delegate void ActionSheetPickerValueChanged(String newValue);

	public class ActionSheetPicker {
		#region -= declarations =-

		public event ActionSheetDatePickerDone ActionSheetComplete;
		public event ActionSheetPickerValueChanged ActionSheetValChanged;

		UIActionSheet actionSheet;
		UIButton doneButton = UIButton.FromType (UIButtonType.RoundedRect);
		UIView owner;
		UILabel titleLabel = new UILabel ();

		#endregion

		#region -= properties =-

		/// <summary>
		/// Set any datepicker properties here
		/// </summary>
		public UIPickerView PickerView
		{
			get { return pickerView; }
			set { pickerView = value; }
		}
		UIPickerView pickerView = new UIPickerView(RectangleF.Empty);

		/// <summary>
		/// The title that shows up for the date picker
		/// </summary>
		public string Title
		{
			get { return titleLabel.Text; }
			set { titleLabel.Text = value; }
		}

		#endregion

		#region -= constructor =-

		/// <summary>
		/// 
		/// </summary>
		public ActionSheetPicker (UIView owner)
		{
			// save our uiview owner
			this.owner = owner;

			ListPickerViewModel pv = new ListPickerViewModel ();
			pv.Items.Add ("1/4 Mile");
			pv.Items.Add ("1/2 Mile");
			pv.Items.Add ("3/4 Mile");
			pv.Items.Add ("1 Mile");

			pickerView.Model = pv;
	
			pv.SelectedValueChanged += (object sender, ListPickerChangedEventArgs e) => {
				if(ActionSheetValChanged!=null)
				{
					ActionSheetValChanged(e.SelectedValue);
				}

			};
			// configure the title label
			titleLabel.BackgroundColor = UIColor.Clear;
			titleLabel.TextColor = UIColor.LightTextColor;
			titleLabel.Font = UIFont.BoldSystemFontOfSize (18);

			// configure the done button
			doneButton.SetTitle ("done", UIControlState.Normal);
			doneButton.TouchUpInside += (s, e) => { 
				if(ActionSheetComplete!=null)
					ActionSheetComplete();

				actionSheet.DismissWithClickedButtonIndex (0, true); 
			};

			// create + configure the action sheet
			actionSheet = new UIActionSheet () { Style = UIActionSheetStyle.BlackTranslucent };
			actionSheet.Clicked += (s, e) => { Console.WriteLine ("Clicked on item {0}", e.ButtonIndex); };

			// add our controls to the action sheet
			actionSheet.AddSubview (pickerView);
			actionSheet.AddSubview (titleLabel);
			actionSheet.AddSubview (doneButton);

		}

		#endregion

		#region -= public methods =-

		/// <summary>
		/// Shows the action sheet picker from the view that was set as the owner.
		/// </summary>
		public void Show ()
		{
			// declare vars
			float titleBarHeight = 40;
			SizeF doneButtonSize = new SizeF (71, 30);
			SizeF actionSheetSize = new SizeF (owner.Frame.Width, pickerView.Frame.Height + titleBarHeight);
			RectangleF actionSheetFrame = new RectangleF (0, owner.Frame.Height - actionSheetSize.Height
				, actionSheetSize.Width, actionSheetSize.Height);

			// show the action sheet and add the controls to it
			actionSheet.ShowInView (owner);

			// resize the action sheet to fit our other stuff
			actionSheet.Frame = actionSheetFrame;

			// move our picker to be at the bottom of the actionsheet (view coords are relative to the action sheet)
			pickerView.Frame = new RectangleF 
				(pickerView.Frame.X, titleBarHeight, pickerView.Frame.Width, pickerView.Frame.Height);

			// move our label to the top of the action sheet
			titleLabel.Frame = new RectangleF (10, 4, owner.Frame.Width - 100, 35);

			// move our button
			doneButton.Frame = new RectangleF (actionSheetSize.Width - doneButtonSize.Width - 10, 7, doneButtonSize.Width, doneButtonSize.Height);
		}

		/// <summary>
		/// Dismisses the action sheet date picker
		/// </summary>
		public void Hide (bool animated)
		{
			actionSheet.DismissWithClickedButtonIndex (0, animated);
		}

		#endregion		
	}
}

