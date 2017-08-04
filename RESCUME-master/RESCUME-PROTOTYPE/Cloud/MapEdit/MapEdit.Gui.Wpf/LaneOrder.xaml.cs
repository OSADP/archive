using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MapEdit
{
	/// <summary>
	/// Interaction logic for LaneOrder.xaml
	/// </summary>
	public partial class LaneDetails : Window
	{
		private int _postedSpeed;
		public int PostedSpeed
		{
			get { return _postedSpeed; }
			set
			{
				_postedSpeed = value;
				postedSpeedTxtBox.Text = _postedSpeed.ToString();
			}
		}

		private int _laneOrder;
		public int Order
		{
			get { return _laneOrder; }
			set
			{
				_laneOrder = value;
				laneOrderTxtBox.Text = _laneOrder.ToString();
			}
		}

		private string _laneDirection;
		public string LaneDirection
		{
			get { return _laneDirection; }
			set
			{
				_laneDirection = value;
				laneDirectionTxtBox.Text = _laneDirection;
			}
		}

		private string _laneType;
		public string LaneType
		{
			get { return _laneType; }
			set
			{
				_laneType = value;
				laneTypeTxtBox.Text = _laneType;
			}
		}

		public LaneDetails()
		{
			InitializeComponent();
		}

		private void laneOrderOK_Click(object sender, RoutedEventArgs e)
		{

			if(laneOrderTxtBox.Text == "" && postedSpeedTxtBox.Text == "")
			{

			}
			else
			{
				_laneOrder = Convert.ToInt32(laneOrderTxtBox.Text);
				_postedSpeed = Convert.ToInt32(postedSpeedTxtBox.Text);
				_laneDirection = laneDirectionTxtBox.Text;
				_laneType = laneTypeTxtBox.Text;
				this.Close();
			}
		}
	}
}
