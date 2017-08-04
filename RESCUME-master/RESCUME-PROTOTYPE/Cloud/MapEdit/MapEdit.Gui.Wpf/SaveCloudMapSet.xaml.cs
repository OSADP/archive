using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Newtonsoft.Json;

namespace MapEdit
{
	/// <summary>
	/// Interaction logic for SaveCloudMapSet.xaml
	/// </summary>
	public partial class SaveCloudMapSet : Window
	{
		public string mapSetName = null;
		public string mapSetDesc = null;
        public bool mustDelete = false;
        public Guid mapSetId;
		List<mapSet> mapSetList;
		private const string BASEADDRESS = "http://inczonemap.cloudapp.net/";
		//private const string BASEADDRESS = "http://localhost:65130/";

		private HttpClient client = new HttpClient();

		public SaveCloudMapSet()
		{
			InitializeComponent();

			client.BaseAddress = new Uri(BASEADDRESS);
		}

		private async void saveMapSetToCloudOK_Click(object sender, RoutedEventArgs e)
		{
            //this.DialogResult = false;

			try
			{
				if (mapSetNameTxtBox.Text == "")// || mapSetNameTxtBox.Text != null || mapSetNameTxtBox.Text != string.Empty)
				{					
					warningTxtBox.Content = "You must enter a Map Set Name";
				}
				else
				{
					mapSetName = mapSetNameTxtBox.Text;
					mapSetDesc = mapSetDescTxtBox.Text;

                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromMinutes(10);
						client.BaseAddress = new Uri(BASEADDRESS);

                        var resp = await client.GetAsync("api/MapSet/GetmapSets/");
                        string returnString = await resp.Content.ReadAsStringAsync();
                        mapSetList = JsonConvert.DeserializeObject<List<mapSet>>(returnString);

                        if (mapSetList.Count > 0)
                        {
                            foreach (var mapset in mapSetList)
                            {
                                if (mapset.name == mapSetName)
                                {
                                    bool? result;
                                    MapSetNameExists mapSetNameExists = new MapSetNameExists();
                                    mapSetNameExists.ShowDialog();
                                    result = mapSetNameExists.DialogResult;

                                    if ((bool)result)
                                    {
                                        mustDelete = true;
                                        mapSetId = mapset.Id;
                                        break;
                                    }
                                    else
                                    {
                                        warningTxtBox.Content = "You must enter a new Map Set Name";
                                        break;
                                    }
                                }
                            }
                            this.DialogResult = true;
                            this.Close();
                        }
                        else
                        {
                            this.DialogResult = true;
                            this.Close();
                        }
                    }
			    }
            }
			catch(Exception ex)
			{
				Trace.WriteLine(ex.ToString());
			}
		}

        private void Window_Closed(object sender, EventArgs e)
        {

        }
	}
}
