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
	/// Interaction logic for FilterWindow.xaml
	/// </summary>
	public partial class FilterWindow : Window
	{
		public double SelectedValue;
		public event TextChangedEventHandler ValueChanged;
		public FilterWindow()
		{
			InitializeComponent();
		}
		private void RangeBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			SelectedValue = Slider.Value;
			if (CurrentValuelLabel != null) CurrentValuelLabel.Content = Slider.Value.ToString();

			if (ValueChanged != null) ValueChanged.Invoke(this, args);
		}

		private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			this.Visibility = Visibility.Hidden;
		}

		public TextChangedEventArgs args { get; set; }
	}
}
