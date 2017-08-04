using System.Diagnostics;
using Newtonsoft.Json;
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
using System.Net.Http;
using System.Net.Http.Headers;
using MapEdit.Data.Models;
using System.Data;

namespace MapEdit
{
	public enum OpenMode
	{
		Open = 1,
		Delete = 2
	}

	public enum ResultMode
	{
		Ok,
		Cancel
	}

	public partial class OpenCloudMapSet : Window
	{
		private HttpClient client = new HttpClient();
		List<mapSet> mapSetList;
		public Guid mapSetId; 
		public List<mapSet> MapSetList = new List<mapSet>();

		public OpenMode WindowMode = OpenMode.Open;
		public ResultMode Result = ResultMode.Cancel;

		public OpenCloudMapSet(OpenMode mode)
		{
			WindowMode = mode;

			InitializeComponent();

			client.BaseAddress = new Uri("http://inczonemap.cloudapp.net/");
			//client.BaseAddress = new Uri("http://localhost:65130/");
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			if (WindowMode == OpenMode.Delete)
			{
				SelectButton.Content = "Delete Selected";
			}

			LoadMapSet();
		}

		private void LoadMapSet()
		{
			//get list of map nodes for mapset
			//GetmapSets
			//string getString = "api/MapNode/GetmapNodes/"

			var resp = client.GetAsync("api/MapSet/GetmapSets/").Result;
			mapSetList = JsonConvert.DeserializeObject<List<mapSet>>(resp.Content.ReadAsStringAsync().Result);

			mapSetDataGrid.ItemsSource = mapSetList;
		}

		private void SelectButton_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				MapSetList = mapSetDataGrid.SelectedItems.Cast<mapSet>().ToList();

				if (mapSetDataGrid.SelectedItem != null)
				{
					var row = (mapSet)mapSetDataGrid.SelectedItem;

					if (row != null)
					{
						mapSetId = row.Id;
					}
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.ToString());
			}

			Result = ResultMode.Ok;
			this.Close();
		}

		private void MapSetDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			try
			{
				MapSetList = mapSetDataGrid.SelectedItems.Cast<mapSet>().ToList();

				if (mapSetDataGrid.SelectedItem != null)
				{
					var row = (mapSet)mapSetDataGrid.SelectedItem;

					if (row != null)
					{
						mapSetId = row.Id;
					}
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.ToString());
			}

			Result = ResultMode.Ok;
			this.Close();
		}

		private void CancelButton_OnClick(object sender, RoutedEventArgs e)
		{
			Result = ResultMode.Cancel;
			this.Close();
		}
	}
}
