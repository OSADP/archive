using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using MapEdit.Data.Models;
using System.Net;


namespace MapEdit
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region Data Members

		List<GpsData> pointDataList;

		private MapLayer mGpsDataLayer = new MapLayer();
		private MapLayer mPolygonLayer = new MapLayer();

		Pushpin _selectedPin;
		private ControlTemplate _selectedTemplate;
		private ControlTemplate _defaultTemplate;
		private ControlTemplate _firstPinTemplate;
		private ControlTemplate _lastPinTemplate;

		private Dictionary<Track, Track> _tracks = new Dictionary<Track, Track>();
		private Track _selectedTrack;

		private bool _pinMoving;

		private static string BASEADDRESS = "http://inczonemap.cloudapp.net/";
		//private const string BASEADDRESS = "http://localhost:65130/";

		private HttpClient client = new HttpClient();

		private double _offestDistanceKillometers = 0.0036576;

		private string _mapsetName;
		private string _mapsetDescription;

		private FilterWindow window = new FilterWindow { WindowStartupLocation = WindowStartupLocation.CenterOwner };

		#endregion

		#region Constructor
		public MainWindow()
		{
			InitializeComponent();

			window.ValueChanged += window_ValueChanged;
			//window.Owner = this;

			pointDataList = new List<GpsData>();

			myMap.Children.Add(mPolygonLayer);
			myMap.Children.Add(mGpsDataLayer);
			myMap.MouseMove += myMap_MouseMove;
			myMap.MouseDown += myMap_MouseDown;
			_selectedTemplate = (ControlTemplate)FindResource("SelectedPinTemplate");
			_defaultTemplate = (ControlTemplate)FindResource("DefaultPinTemplate");
			_firstPinTemplate = (ControlTemplate)FindResource("FirstPinTemplate");
			_lastPinTemplate = (ControlTemplate)FindResource("LastPinTemplate");

			//client.Timeout = TimeSpan.FromMinutes(4);

			client.BaseAddress = new Uri(BASEADDRESS);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		}

		#endregion

		#region Map Interaction

		void myMap_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (_selectedPin != null && _pinMoving)
			{
				e.Handled = true;

				// Determin the location to place the pushpin at on the map.

				//Get the mouse click coordinates
				Point mousePosition = e.GetPosition(myMap);
				//Convert the mouse coordinates to a locatoin on the map
				Location pinLocation = myMap.ViewportPointToLocation(mousePosition);

				// The pushpin to add to the map.

				_selectedPin.Location = pinLocation;

				var gpsData = _selectedPin.Content as GpsData ?? new GpsData();

				gpsData.Latitude = pinLocation.Latitude;
				gpsData.Longitude = pinLocation.Longitude;

				_selectedPin.Content = gpsData;

				var locCol = new LocationCollection();
				foreach (var pushpin in _selectedTrack.Pins)
				{
					locCol.Add(new Location(pushpin.Location.Latitude, pushpin.Location.Longitude));
				}
				_selectedTrack.Polyline.Stroke = new SolidColorBrush(Colors.Red);
				_selectedTrack.Polyline.StrokeThickness = 4;
				_selectedTrack.Polyline.Locations = locCol;

				refreshMap();
			}

			_pinMoving = false;
		}

		void myMap_MouseMove(object sender, MouseEventArgs e)
		{
			if (_pinMoving)
			{
				e.Handled = true;

				// Determin the location to place the pushpin at on the map.

				//Get the mouse click coordinates
				Point mousePosition = e.GetPosition(myMap);
				//Convert the mouse coordinates to a locatoin on the map
				Location pinLocation = myMap.ViewportPointToLocation(mousePosition);

				// The pushpin to add to the map.

				if (_selectedPin != null) _selectedPin.Location = pinLocation;

				var locCol = new LocationCollection();
				foreach (var pushpin in _selectedTrack.Pins)
				{
					locCol.Add(new Location(pushpin.Location.Latitude, pushpin.Location.Longitude));
				}
				_selectedTrack.Polyline.Stroke = new SolidColorBrush(Colors.Red);
				_selectedTrack.Polyline.StrokeThickness = 4;
				_selectedTrack.Polyline.Locations = locCol;

				refreshMap();
			}
		}

		void pin_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (_selectedPin != null) _selectedPin.Template = _defaultTemplate;

			_selectedPin = sender as Pushpin;
			if (_selectedPin == null || _selectedPin.Content == null) return;

			// find the Track that owns this pin
			foreach (var track in _tracks)
			{
				foreach (var pin in track.Value.Pins)
				{
					if (pin == _selectedPin)
						_selectedTrack = track.Value;
				}
			}

			_selectedPin.Template = _selectedTemplate;

			var data = _selectedPin.Content as GpsData;
			if (data == null) return;


			Time.Text = data.Time;
			Lat.Text = data.Latitude.ToString();
			Lon.Text = data.Longitude.ToString();
			Quality.Text = data.Quality.ToString();
			SatCount.Text = data.SatCount.ToString();
			Hdop.Text = data.Hdop.ToString();
			Altitude.Text = data.Altitude.ToString();
			DgpsStationId.Text = data.DgpsStationId;
			DgpsAge.Text = data.LastDgps;
			laneOrder.Text = data.LaneOrder.ToString();
			postedSpeed.Text = data.PostedSpeed.ToString();
			laneDirection.Text = data.LaneDirection;
			laneType.Text = data.LaneType;

			for (int i = 0; i < _selectedTrack.Pins.Count; i++)
			{
				if (_selectedTrack != null && i < _selectedTrack.Pins.Count - 1)
				{
					if (_selectedTrack.Pins[i] == _selectedPin)
					{
						var currentPointData = _selectedTrack.Pins[i].Content as GpsData;
						var nextPointData = _selectedTrack.Pins[i + 1].Content as GpsData;
					}
				}
			}

			refreshMap();
		}

		private void pingDoubleClickHandler(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			bool shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
			bool controlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

			if (controlPressed)
			{
				deletePointMenuItem_Click(sender, e);
				return;
			}

			if (shiftPressed)
			{
				addPointMenuItem_Click(sender, e);
				return;
			}

			movePointMenuItem_Click(sender, e);
		}

		private void refreshMap()
		{
			mGpsDataLayer.Children.Clear();

			foreach (var track in _tracks)
			{
				for (int i = 0; i < track.Value.Pins.Count-1; i++)
				{
					var currentPointData = track.Value.Pins[i].Content as GpsData ?? new GpsData();
					var nextPointData = track.Value.Pins[i+1].Content as GpsData ?? new GpsData();
					var bearing = Bearing2(currentPointData, nextPointData);
					var degrees = ((bearing * 180) / Math.PI) - 90;
					track.Value.Pins[i].Template = _firstPinTemplate;
					track.Value.Pins[i].LayoutTransform = new RotateTransform() { Angle = degrees, CenterX = 6, CenterY = 6 };
				}

				track.Value.Pins[track.Value.Pins.Count-1].Template = _lastPinTemplate;


				//if (track.Value.LastPushpin != null) track.Value.LastPushpin.Template = _lastPinTemplate;
				//if (track.Value.FirstPushpin != null)
				//{
				//	track.Value.FirstPushpin.Template = _firstPinTemplate;

				//	var nextPin = track.Value.Pins[1];
				//	var currentPointData = track.Value.FirstPushpin.Content as GpsData ?? new GpsData();
				//	var nextPointData = nextPin.Content as GpsData ?? new GpsData();
				//	var bearing = Bearing2(currentPointData, nextPointData);
				//	var degrees = ((bearing*180)/Math.PI) - 90;

				//	track.Value.FirstPushpin.LayoutTransform = new RotateTransform() { Angle = degrees, CenterX = 6, CenterY = 6 };
				//}


				if (track.Value == _selectedTrack)
				{
					track.Value.Polyline.Stroke = new SolidColorBrush(Colors.Blue);
					track.Value.Polyline.StrokeThickness = 4;
				}
				else
				{
					track.Value.Polyline.Stroke = new SolidColorBrush(Colors.Red);
					track.Value.Polyline.StrokeThickness = 4;
				}

				mGpsDataLayer.Children.Add(track.Value.Polyline);

				foreach (var pin in track.Value.Pins)
				{
					mGpsDataLayer.Children.Add(pin);
				}
			}
		}

		private Pushpin MakePin(GpsData data)
		{
			var pin = new Pushpin {Template = _defaultTemplate};
			// Make the main menu.
			var mainMenu = new ContextMenu
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Top
			};

			// Make the top level menu items.
			var LineMenuItem = new MenuItem {Header = "_Line"};

			var PointMenuItem = new MenuItem {Header = "_Point"};

			// Make the Line menu's items.
			var divideMenuItem = new MenuItem {Header = "_Divide"};
			divideMenuItem.Click += divideLineMenuItem_Click;
			var openToolTip = new ToolTip {Content = "Divide the current track segment into two segments"};
			divideMenuItem.ToolTip = openToolTip;

			var removeMenuItem = new MenuItem {Header = "_Remove"};
			removeMenuItem.Click += removeMenuItem_Click;
			var removeToolTip = new ToolTip {Content = "Remove this track segment"};
			removeMenuItem.ToolTip = removeToolTip;

			var offsetLeftMenuItem = new MenuItem {Header = "_OffsetLeft"};
			offsetLeftMenuItem.Click += offsetLeftMenuItem_Click;
			var offsetLeftToolTip = new ToolTip {Content = "Offset this track segment to the LEFT"};
			offsetLeftMenuItem.ToolTip = offsetLeftToolTip;

			var offsetRightMenuItem = new MenuItem {Header = "OffsetRight"};
			offsetRightMenuItem.Click += offsetRighttMenuItem_Click;
			var offsetRightToolTip = new ToolTip {Content = "Offset this track segment to the RIGHT"};
			offsetRightMenuItem.ToolTip = offsetRightToolTip;

			var reverseLineMenuItem = new MenuItem { Header = "ReverseLineDirection" };
			reverseLineMenuItem.Click += reverseLineMenuItem_Click;
			var reverseLineToolTip = new ToolTip { Content = "Reverse the line direction" };
			reverseLineMenuItem.ToolTip = reverseLineToolTip;

			var laneOrderMenuItem = new MenuItem {Header = "_LaneOrder"};
			laneOrderMenuItem.Click += laneOrderMenuItem_Click;
			var laneOrderToolTip = new ToolTip {Content = "Add lane order"};
			laneOrderToolTip.ToolTip = laneOrderToolTip;

			var cleanLineMenuItem = new MenuItem {Header = "_CleanLine"};
			cleanLineMenuItem.Click += cleanLineMenuItem_Click;
			var cleanLineToolTip = new ToolTip {Content = "clean up line"};
			cleanLineMenuItem.ToolTip = cleanLineToolTip;

			// Make the Point menu's items.
			var addPointMenuItem = new MenuItem { Header = "_AddPoint" };
			addPointMenuItem.Click += addPointMenuItem_Click;
			var addPointToolTip = new ToolTip { Content = "Add a NEW point BEFORE the selected point" };
			addPointMenuItem.ToolTip = addPointToolTip;

			var deletePointMenuItem = new MenuItem { Header = "_DeletePoint" };
			deletePointMenuItem.Click += deletePointMenuItem_Click;
			var deletePointToolTip = new ToolTip { Content = "Delete the selected point" };
			deletePointMenuItem.ToolTip = deletePointToolTip;

			var movePointMenuItem = new MenuItem { Header = "_MovePoint" };
			movePointMenuItem.Click += movePointMenuItem_Click;
			var movePointToolTip = new ToolTip { Content = "Move the selected point" };
			movePointMenuItem.ToolTip = movePointToolTip;

			// Wire up methods
			mainMenu.Items.Add(LineMenuItem);
			mainMenu.Items.Add(PointMenuItem);

			LineMenuItem.Items.Add(divideMenuItem);
			LineMenuItem.Items.Add(removeMenuItem);
			LineMenuItem.Items.Add(offsetLeftMenuItem);
			LineMenuItem.Items.Add(offsetRightMenuItem);
			LineMenuItem.Items.Add(reverseLineMenuItem);
			LineMenuItem.Items.Add(cleanLineMenuItem);
			LineMenuItem.Items.Add(laneOrderMenuItem);

			PointMenuItem.Items.Add(addPointMenuItem);
			PointMenuItem.Items.Add(deletePointMenuItem);
			PointMenuItem.Items.Add(movePointMenuItem);

			pin.ContextMenu = mainMenu;
			pin.PositionOrigin = PositionOrigin.Center;
			pin.Location = new Location(data.Latitude, data.Longitude);
			pin.MouseDown += pin_MouseDown;
			pin.MouseDoubleClick += pingDoubleClickHandler;
			pin.Content = data;

			return pin;
		}

		private void addPositionDataToMap(string trackName)
		{
			if (pointDataList == null || pointDataList.Count < 1)
				return;

			var locCol = new LocationCollection();
			var mapZoom = myMap.ZoomLevel;
			GpsData lastPos = null;

			//_tracks.Clear();
			var track = new Track {Name = trackName};
			_tracks.Add(track, track);

			numberOfPoints.Text = pointDataList.Count.ToString();

			bool isFirst = true;
			foreach (GpsData data in pointDataList)
			{
				if (data.Latitude != 0 && data.Longitude != 0)
				{
					var pin = MakePin(data);

					if (isFirst)
					{
						pin.Template = _firstPinTemplate;
					}

					isFirst = false;

					locCol.Add(new Location(data.Latitude, data.Longitude));

					lastPos = data;

					if (track.Pins.Count == 0)
						track.FirstPushpin = pin;

					track.LastPushpin = pin;

					track.Pins.Add(pin);
				}
			}

			if (track.LastPushpin != null) track.LastPushpin.Template = _lastPinTemplate;

			if (lastPos != null)
			{
				track.Polyline.Stroke = new SolidColorBrush(Colors.Red);
				track.Polyline.StrokeThickness = 4;
				track.Polyline.Locations = locCol;


				//mGpsDataLayer.Children.Add(track.Polyline);

				locCol = new LocationCollection();

				//var zoom = Math.Max(mapZoom, 15);
				var zoom = Math.Max(mapZoom, 30);
				myMap.SetView(new Location(lastPos.Latitude, lastPos.Longitude), zoom);
			}

			refreshMap();
		}
		#endregion

		#region Map Context Menu Items
		private void addPointMenuItem_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			var newPinList = new List<Pushpin>();
			for (int i = 0; i < _selectedTrack.Pins.Count; i++)
			{
				if (_selectedTrack != null)
				{
					if (_selectedTrack.Pins[i] == _selectedPin)
					{
						var gpsData = new GpsData
						{
							Latitude = _selectedTrack.Pins[i].Location.Latitude,
							Longitude = _selectedTrack.Pins[i].Location.Longitude
						};
						var newPin = MakePin(gpsData);
						newPinList.Add(newPin);

						_selectedPin = newPin;
						_pinMoving = true;
					}
					newPinList.Add(_selectedTrack.Pins[i]);
				}
			}
			_selectedTrack.Pins = newPinList;

			var locCol = new LocationCollection();
			foreach (var pushpin in _selectedTrack.Pins)
			{
				locCol.Add(new Location(pushpin.Location.Latitude, pushpin.Location.Longitude));
			}
			_selectedTrack.Polyline.Stroke = new SolidColorBrush(Colors.Red);
			_selectedTrack.Polyline.StrokeThickness = 4;
			_selectedTrack.Polyline.Locations = locCol;

			refreshMap();
		}
		private void deletePointMenuItem_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			// TODO: delete point
			for (int i = 0; i < _selectedTrack.Pins.Count; i++)
			{
				if (_selectedTrack != null && i < _selectedTrack.Pins.Count - 1)
				{
					if (_selectedTrack.Pins[i] == _selectedPin)
					{
						_selectedTrack.Pins.RemoveAt(i);
					}
				}
			}

			var locCol = new LocationCollection();
			foreach (var pushpin in _selectedTrack.Pins)
			{
				locCol.Add(new Location(pushpin.Location.Latitude, pushpin.Location.Longitude));
			}
			_selectedTrack.Polyline.Stroke = new SolidColorBrush(Colors.Red);
			_selectedTrack.Polyline.StrokeThickness = 4;
			_selectedTrack.Polyline.Locations = locCol;

			refreshMap();
		}
		private void movePointMenuItem_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			_pinMoving = true;

			refreshMap();
		}
		private void cleanLineMenuItem_Click(object sender, RoutedEventArgs e)
		{
			double bearing = 0;
			double lastBearing = 0;
			var offsetTrackLocations = new LocationCollection();

			for (int i = 0; i < _selectedTrack.Pins.Count; i++)
			{
				if (i < _selectedTrack.Pins.Count - 1)
				{
					var currentPointData = _selectedTrack.Pins[i].Content as GpsData;
					var nextPointData = _selectedTrack.Pins[i + 1].Content as GpsData;
					//bearing = BearingToPoint(currentPointData, nextPointData);
					bearing = Bearing2(currentPointData, nextPointData);
					if (i == 0)
						lastBearing = bearing;

					if (IsReciprocalBearing(bearing, lastBearing))
					{
						var startPoint = Math.Max(0, i - 2);
						var endPoint = Math.Max(0, i - 2);
						Trace.WriteLine(string.Format("recip at {0}", i));
						lastBearing = bearing;

						bool pointsremoved = false;
						while (i < _selectedTrack.Pins.Count - 1 && !pointsremoved)
						{
							currentPointData = _selectedTrack.Pins[i].Content as GpsData;
							nextPointData = _selectedTrack.Pins[i + 1].Content as GpsData;
							bearing = Bearing2(currentPointData, nextPointData);

							if (IsReciprocalBearing(bearing, lastBearing))
							{
								endPoint = Math.Min(_selectedTrack.Pins.Count - 1, i + 2);

								var count = endPoint - startPoint;

								if (count < 50)
								{
									// remove points
									for (int j = endPoint; j > startPoint; j--)
									{
										_selectedTrack.Pins.RemoveAt(j);
									}
								}
								pointsremoved = true;
							}

							lastBearing = bearing;
							i++;
						}
					}

					lastBearing = bearing;
				}
			}

			for (int i = 0; i < _selectedTrack.Pins.Count; i++)
			{
				offsetTrackLocations.Add(new Location(_selectedTrack.Pins[i].Location.Latitude,
					_selectedTrack.Pins[i].Location.Longitude));
			}

			_selectedTrack.Polyline.Stroke = new SolidColorBrush(Colors.Red);
			_selectedTrack.Polyline.StrokeThickness = 1;
			_selectedTrack.Polyline.Locations = offsetTrackLocations;

			refreshMap();
		}
		private void offsetLeftMenuItem_Click(object sender, RoutedEventArgs e)
		{
			OffsetSelectedLine(-(Math.PI / 2));
		}
		private void offsetRighttMenuItem_Click(object sender, RoutedEventArgs e)
		{
			OffsetSelectedLine(Math.PI / 2);
		}
		private void reverseLineMenuItem_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			var locCol = new LocationCollection();

			var newTrack = new Track();

			bool isFirst = true;
			for (int i = _selectedTrack.Pins.Count - 1; i >= 0; i--)
			{
				if (isFirst)
					newTrack.FirstPushpin = _selectedTrack.Pins[i];

				isFirst = false;

				newTrack.LastPushpin = _selectedTrack.Pins[i];
				newTrack.Pins.Add(_selectedTrack.Pins[i]);
				locCol.Add(new Location(_selectedTrack.Pins[i].Location.Latitude, _selectedTrack.Pins[i].Location.Longitude));
				_selectedTrack.Pins.RemoveAt(i);
			}

			_tracks.Remove(_selectedTrack);
			_tracks.Add(newTrack, newTrack);
			_selectedTrack = newTrack;

			_selectedTrack.Polyline.Stroke = new SolidColorBrush(Colors.Red);
			_selectedTrack.Polyline.StrokeThickness = 4;
			_selectedTrack.Polyline.Locations = locCol;

			refreshMap();
		}
		private void OffsetSelectedLine(double relativeBearingRadians)
		{
			// Offset the track to the specified bearing.
			var offsetTrack = new Track();

			var offsetTrackLocations = new LocationCollection();

			double bearing = 0;
			var currentPointData = new GpsData();

			for (int i = 0; i < _selectedTrack.Pins.Count; i++)
			{
				if (i < _selectedTrack.Pins.Count - 1)
				{
					currentPointData = _selectedTrack.Pins[i].Content as GpsData;
					var nextPointData = _selectedTrack.Pins[i + 1].Content as GpsData;
					bearing = Bearing2(currentPointData, nextPointData) + relativeBearingRadians;
				}

				var loc = new GeoLocation
				{
					Latitude = _selectedTrack.Pins[i].Location.Latitude,
					Longitude = _selectedTrack.Pins[i].Location.Longitude
				};

				var newLocation = FindPointAtDistanceFrom(loc, bearing, _offestDistanceKillometers);

				var gpsData = new GpsData(currentPointData) {Latitude = newLocation.Latitude, Longitude = newLocation.Longitude};

				var pin = MakePin(gpsData);

				if (offsetTrack.Pins.Count == 0)
					offsetTrack.FirstPushpin = pin;

				offsetTrack.LastPushpin = pin;

				offsetTrack.Pins.Add(pin);
				offsetTrackLocations.Add(new Location(newLocation.Latitude, newLocation.Longitude));
			}

			offsetTrack.Polyline.Stroke = new SolidColorBrush(Colors.Red);
			offsetTrack.Polyline.StrokeThickness = 1;
			offsetTrack.Polyline.Locations = offsetTrackLocations;

			_tracks.Add(offsetTrack, offsetTrack);

			refreshMap();
		}
		private void removeMenuItem_Click(object sender, RoutedEventArgs e)
		{
			// Remove old track from list and add
			if (_tracks.ContainsKey(_selectedTrack))
				_tracks.Remove(_selectedTrack);

			refreshMap();
		}
		private void divideLineMenuItem_Click(object sender, RoutedEventArgs e)
		{
			// Split the track into two pieces.
			var firstTrack = new Track();
			var secondTrack = new Track();

			var firstTrackLocations = new LocationCollection();
			var secondTrackLocations = new LocationCollection();

			bool foundPin = false;
			for (int i = 0; i < _selectedTrack.Pins.Count; i++)
			{
				if (_selectedTrack.Pins[i] == _selectedPin)
				{
					foundPin = true;
				}
				if (foundPin)
				{
					if (firstTrack.Pins.Count == 0)
						firstTrack.FirstPushpin = _selectedTrack.Pins[i];

					firstTrack.LastPushpin = _selectedTrack.Pins[i];

					firstTrack.Pins.Add(_selectedTrack.Pins[i]);
					firstTrackLocations.Add(new Location(_selectedTrack.Pins[i].Location.Latitude, _selectedTrack.Pins[i].Location.Longitude));
				}
				else
				{
					if (secondTrack.Pins.Count == 0)
						secondTrack.FirstPushpin = _selectedTrack.Pins[i];

					secondTrack.LastPushpin = _selectedTrack.Pins[i];

					secondTrack.Pins.Add(_selectedTrack.Pins[i]);
					secondTrackLocations.Add(new Location(_selectedTrack.Pins[i].Location.Latitude, _selectedTrack.Pins[i].Location.Longitude));
				}
			}

			firstTrack.Polyline.Stroke = new SolidColorBrush(Colors.Red);
			firstTrack.Polyline.StrokeThickness = 1;
			firstTrack.Polyline.Locations = firstTrackLocations;

			secondTrack.Polyline.Stroke = new SolidColorBrush(Colors.Red);
			secondTrack.Polyline.StrokeThickness = 1;
			secondTrack.Polyline.Locations = secondTrackLocations;


			// Remove old track from list and add
			if (_tracks.ContainsKey(_selectedTrack))
				_tracks.Remove(_selectedTrack);

			// Add two new pieces into list.
			_tracks.Add(firstTrack, firstTrack);
			_tracks.Add(secondTrack, secondTrack);

			refreshMap();
		}
		private void laneOrderMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var laneOrderWindow = new LaneDetails();
			if (_selectedTrack.Pins != null && _selectedTrack.Pins.Count > 0)
			{
				var gpsData = _selectedTrack.Pins[0].Content as GpsData;
				if (gpsData != null)
				{
					laneOrderWindow.Order = gpsData.LaneOrder;
					laneOrderWindow.PostedSpeed = Convert.ToInt32(gpsData.PostedSpeed);
					laneOrderWindow.LaneDirection = gpsData.LaneDirection;
					laneOrderWindow.LaneType = gpsData.LaneType;
				}


				laneOrderWindow.ShowDialog();

				for (int i = 0; i < _selectedTrack.Pins.Count; i++)
				{
					var currentPointData = _selectedTrack.Pins[i].Content as GpsData;
					if (currentPointData != null)
					{
						currentPointData.LaneOrder = laneOrderWindow.Order;
						currentPointData.PostedSpeed = laneOrderWindow.PostedSpeed;
						currentPointData.LaneDirection = laneOrderWindow.LaneDirection;
						currentPointData.LaneType = laneOrderWindow.LaneType;
					}
				}
			}
		}
		#endregion

		#region Window Menu Items
		private void viewAerial_Click(object sender, RoutedEventArgs e)
		{
			myMap.Mode = new AerialMode();
			refreshMap();
		}
		private void viewHybrid_Click(object sender, RoutedEventArgs e)
		{
			myMap.Mode = new AerialMode(true);
			refreshMap();
		}
		private void viewMap_Click(object sender, RoutedEventArgs e)
		{
			myMap.Mode = new RoadMode();
			refreshMap();
		}
		private void exitMenuItem_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show("Goodbye!");
			Close();
		}
		private void MenuItem_OpenFromFile_OnClick(object sender, RoutedEventArgs e)
		{
			//MenuItem_ClearTracks_OnClick(sender, e);

			var dlg = new Microsoft.Win32.OpenFileDialog {DefaultExt = ".ubx", Filter = "UBlox log files (.ubx)|*.ubx"};

			var result = dlg.ShowDialog();
			if (result == false)
				return;

			if (result == true)
			{
				var inputfileName = dlg.FileName;
				try
				{
					int totalCount = 0;
					using (var sr = new StreamReader(inputfileName))
					{
						while (sr.ReadLine() != null)
						{
							totalCount++;
						}
					}
					Progress.Maximum = totalCount;

					using (var sr = new StreamReader(inputfileName))
					{
						int counter = 0;
						string line;
						Progress.Visibility = Visibility.Visible;

						pointDataList.Clear();
						while ((line = sr.ReadLine()) != null)
						{
							if (line.StartsWith("$GPGGA"))
							{
								var newPoint = ParseGGA(line);
								pointDataList.Add(newPoint);
							}
							counter++;
							if (counter % 500 == 0)
							{
								Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
									new Action(delegate
									{
										StatusBlock.Text = string.Format("processing line# {0} of {1}", counter, totalCount);
										Progress.Value = counter;
									}));
							}
						}
					}

					StatusBlock.Text = "Complete. ";
					Progress.Value = 0;
					Progress.Visibility = Visibility.Hidden;

					var cleanFileName = dlg.FileName;
					var file = new FileInfo(cleanFileName);
					if (file.Exists)
						cleanFileName = file.Name;
					addPositionDataToMap(cleanFileName);
				}
				catch (Exception ex)
				{
					Console.WriteLine("The file could not be read:");
					Console.WriteLine(ex.Message);
				}
			}
		}
		private void MenuItem_SaveToFile_OnClick(object sender, RoutedEventArgs e)
		{
			var sfd = new CommonSaveFileDialog();
			var result = sfd.ShowDialog();
			if (result == CommonFileDialogResult.Cancel)
				return;

			if (sfd.FileName != null)
			{
				var builder = new StringBuilder();
				foreach (var track in _tracks)
				{
					foreach (var pin in track.Value.Pins)
					{
						var data = pin.Content as GpsData;
						if (data == null) continue;

						var lat = Math.Abs(data.Latitude);
						var lon = Math.Abs(data.Longitude);

						builder.Append(string.Format("$GPGGA,{0},{1:D2}{2:00.00000},{3},{4:D3}{5:00.00000},{6},{7},{8:D2},{9},{10:0.0},M,0,M,{11},{12}*69\r\n",
							data.Time,
							(int)lat,
							((lat - (int)lat) * 60),
							(data.Latitude < 0) ? "S" : "N",
							(int)lon,
							((lon - (int)lon) * 60),
							(data.Longitude < 0) ? "W" : "E",
							data.Quality,
							data.NumberOfSatellites,
							data.Hdop,
							data.Altitude,
							data.LastDgps,
							(data.DgpsStationId == string.Empty) ? "0000" : data.DgpsStationId
							));
					}
				}

				var file = sfd.FileName;

				if (!file.ToUpper().EndsWith("UBX"))
					file = string.Format("{0}.ubx", file);

				using (var sr = new StreamWriter(file))
				{
					sr.Write(builder.ToString());
				}
			}
		}
		private async void MenuItem_OpenFromCloud_OnClick(object sender, RoutedEventArgs e)
		{
			var openCloudMapSetWindow = new OpenCloudMapSet(OpenMode.Open) {WindowStartupLocation = WindowStartupLocation.CenterOwner};
			openCloudMapSetWindow.Owner = this;
			openCloudMapSetWindow.ShowDialog();
			if (openCloudMapSetWindow.Result == ResultMode.Cancel)
				return;

			var mapSetList = openCloudMapSetWindow.MapSetList;
			int counter = 1;
			Progress.Maximum = mapSetList.Count;
			foreach (var mapSet in mapSetList)
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					StatusBlock.Text = string.Format("Opening Mapset # {0} of {1}", counter++, mapSetList.Count);
					Progress.Value = counter;
				});

				await OpenCloudSet(mapSet.Id);				
			}

			refreshMap();
		}
		private async Task OpenCloudSet(Guid Id)
		{
			Guid temp = Id;

			Progress.Maximum = 3;
			Progress.Visibility = Visibility.Visible;

			string getString = "api/MapLink/GetmapLinks/" + temp;
			//var resp = client.GetAsync(getString).Result;
			//List<mapLink> mapLinkList = JsonConvert.DeserializeObject<List<mapLink>>(resp.Content.ReadAsStringAsync().Result);
			var resp = await client.GetAsync(getString);
			string returnString = await resp.Content.ReadAsStringAsync();
			var mapLinkList = JsonConvert.DeserializeObject<List<mapLink>>(returnString);

			getString = "api/MapNode/GetmapNodes/" + temp;
			//resp = client.GetAsync(getString).Result;
			//List<mapNode> mapNodeList = JsonConvert.DeserializeObject<List<mapNode>>(resp.Content.ReadAsStringAsync().Result);
			resp = await client.GetAsync(getString);
			returnString = await resp.Content.ReadAsStringAsync();
			var mapNodeList = JsonConvert.DeserializeObject<List<mapNode>>(returnString);
			numberOfPoints.Text = mapNodeList.Count.ToString();

			Progress.Maximum = mapNodeList.Count;

			var mapZoom = myMap.ZoomLevel;
			GpsData lastPos = null;

			var startNodes = mapLinkList.Where(s => s.startMapNodeId == null);

			long counter = 0;
			foreach (var startNode in startNodes)
			{
				var locCol = new LocationCollection();
				var track = new Track();
				Trace.WriteLine("new track");
				_tracks.Add(track, track);
				//var nextNode = mapLinkList.FirstOrDefault(s => s.startMapNodeId == startNode.endMapNodeId);
				var nextNode = startNode;

				while (nextNode != null)
				{
					counter++;
					if (counter%50 == 0)
					{
						Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
							new Action(delegate
							{
								StatusBlock.Text = string.Format("processing line# {0} of {1}", counter, Progress.Maximum);
								Progress.Value = counter;
							}));
					}

					//add a  pin for each one of these
					if (nextNode.endMapNodeId == null)
						break;

					var node = mapNodeList.FirstOrDefault(s => s.Id == nextNode.endMapNodeId);
					if (node != null)
					{
						var data = new GpsData();
						data.Latitude = node.latitude;
						data.Longitude = node.longitude;
						data.LaneOrder = node.laneOrder;
						data.PostedSpeed = node.postedSpeed;
						data.LaneDirection = node.LaneDirection;
						data.LaneType = node.LaneType;


						lastPos = data;

						var pin = MakePin(data);

						locCol.Add(new Location(data.Latitude, data.Longitude));

						if (track.Pins.Count == 0)
							track.FirstPushpin = pin;

						track.LastPushpin = pin;

						track.Pins.Add(pin);
					}

					nextNode = mapLinkList.FirstOrDefault(s => s.startMapNodeId == nextNode.endMapNodeId);
					//Trace.WriteLine(string.Format("startNode: {0} nextNode: {1}", nextNode.startMapNodeId, nextNode.endMapNodeId));
				}

				track.Polyline.Stroke = new SolidColorBrush(Colors.Red);
				track.Polyline.StrokeThickness = 4;
				track.Polyline.Locations = locCol;
			}
			StatusBlock.Text = "Complete. ";
			Progress.Value = 0;
			Progress.Visibility = Visibility.Hidden;

			var zoom = Math.Max(mapZoom, 15);

			if (lastPos != null)
				myMap.SetView(new Location(lastPos.Latitude, lastPos.Longitude), zoom);
		}
		private async void MenuItem_SaveToCloud_OnClick(object sender, RoutedEventArgs e)
		{
			// TODO: create a popup to ask user for a name for the data set

			var saveCloudMapSetWindow = new SaveCloudMapSet {WindowStartupLocation = WindowStartupLocation.CenterOwner};
			saveCloudMapSetWindow.Owner = this;
			saveCloudMapSetWindow.ShowDialog();

			if (saveCloudMapSetWindow.DialogResult == true && _tracks.Count > 0)
			{
				string tempName = saveCloudMapSetWindow.mapSetName;
				string tempDesc = saveCloudMapSetWindow.mapSetDesc;

				using (var saveClient = new HttpClient())
				{
					ServicePointManager.Expect100Continue = false;
					saveClient.Timeout = TimeSpan.FromMinutes(10);

					saveClient.BaseAddress = new Uri(BASEADDRESS);
					saveClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

					try
					{
						if (saveCloudMapSetWindow.mustDelete)
						{
							Progress.Maximum = 3;
							Progress.Visibility = Visibility.Visible;

							Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
								new Action(delegate
								{
									StatusBlock.Text = string.Format("Deleting Mapset - Task # {0} of {1}", 1, 3);
									Progress.Value = 1;
								}));
							var resp = await saveClient.DeleteAsync("api/MapLink/DeletemapLink/" + saveCloudMapSetWindow.mapSetId);
							//mapLink mapLinkResp = JsonConvert.DeserializeObject<mapLink>(resp.Content.ReadAsStringAsync().Result);
							Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
								new Action(delegate
								{
									StatusBlock.Text = string.Format("Deleting Mapset - Task # {0} of {1}", 2, 3);
									Progress.Value = 2;
								}));
							resp = await saveClient.DeleteAsync("api/MapNode/DeletemapNode/" + saveCloudMapSetWindow.mapSetId);
							//mapNode mapNodeResp = JsonConvert.DeserializeObject<mapNode>(resp.Content.ReadAsStringAsync().Result);
							Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
								new Action(delegate
								{
									StatusBlock.Text = string.Format("Deleting Mapset - Task # {0} of {1}", 3, 3);
									Progress.Value = 3;
								}));
							resp = await saveClient.DeleteAsync("api/MapSet/DeletemapSet/" + saveCloudMapSetWindow.mapSetId);
							//mapSet mapSetResp = JsonConvert.DeserializeObject<mapSet>(resp.Content.ReadAsStringAsync().Result);
							StatusBlock.Text = "Complete. ";
							Progress.Value = 0;
							Progress.Visibility = Visibility.Hidden;
						}
					}
					catch (Exception ex)
					{
						Trace.WriteLine(ex.ToString());
					}

					//post mapset get generated id
					var mapset = new mapSet();

					_mapsetName = tempName;// "myTestName3";
					_mapsetDescription = tempDesc;// "myTestDescription2";

					mapset.Id = Guid.NewGuid();
					mapset.name = _mapsetName;
					mapset.description = _mapsetDescription;


					Progress.Maximum = 3;
					Progress.Visibility = Visibility.Visible;

					Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
						new Action(delegate
						{
							StatusBlock.Text = string.Format("Saving Mapset - Task # {0} of {1}", 1, 3);
							Progress.Value = 1;
						}));
					string jsonMapSetString = JsonConvert.SerializeObject(mapset);

					var content = new StringContent(jsonMapSetString, Encoding.UTF8, "application/json");
					//var resp = client.PostAsync("api/MapSet/PostmapSet", content).Result;
					var response = await saveClient.PostAsync("api/MapSet/PostmapSet", content);
					string returnString = await response.Content.ReadAsStringAsync();

					var mapSetResp = JsonConvert.DeserializeObject<mapSet>(returnString);

					//post list of map nodes

					var mapNodeList = new List<mapNode>();
					var mapLinkList = new List<mapLink>();
					mapLink maplink;

					foreach (var track in _tracks)
					{
						Guid startId = Guid.Empty;
						Guid endId = Guid.Empty;
						Guid currentId = Guid.Empty;

						for (int ii = 0; ii < track.Value.Pins.Count; ii++)
						{
							var mapnode = new mapNode();
							maplink = new mapLink();
							//maplink.startMapNode = new mapNode();
							//maplink.endMapNode = new mapNode();

							var gpsdata = track.Value.Pins[ii].Content as GpsData;
							if (gpsdata == null)
							{
								continue;
							}
							currentId = mapnode.Id = Guid.NewGuid();
							mapnode.elevation = gpsdata.Elevation;
							mapnode.latitude = gpsdata.Latitude;
							mapnode.longitude = gpsdata.Longitude;
							mapnode.mapSetId = mapSetResp.Id;
							mapnode.laneOrder = gpsdata.LaneOrder;
							mapnode.postedSpeed = Convert.ToInt32(gpsdata.PostedSpeed);
							mapnode.LaneDirection = gpsdata.LaneDirection;
							mapnode.LaneType = gpsdata.LaneType;

							mapNodeList.Add(mapnode);

							//add the mapLinks
							maplink.Id = Guid.NewGuid();
							if (startId != Guid.Empty)
								maplink.startMapNodeId = startId;
							if (currentId != Guid.Empty)
								maplink.endMapNodeId = currentId;
							maplink.mapSetId = mapSetResp.Id;
							mapLinkList.Add(maplink);

							startId = currentId;
							//endId = currentId;

						}
						maplink = new mapLink {Id = Guid.NewGuid()};
						//maplink.startMapNode = new mapNode();
						//maplink.endMapNode = new mapNode();

						if (startId != Guid.Empty)
							maplink.startMapNodeId = startId;
						//				maplink.endMapNodeId = Guid.Empty;
						maplink.mapSetId = mapSetResp.Id;
						mapLinkList.Add(maplink);
					}

					Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
						new Action(delegate
						{
							StatusBlock.Text = string.Format("Saving Mapset - Task # {0} of {1}", 2, 3);
							Progress.Value = 2;
						}));

					string jsonNodeString = JsonConvert.SerializeObject(mapNodeList);

					content = new StringContent(jsonNodeString, Encoding.UTF8, "application/json");

					try
					{
						//HttpClient.Timeout
						//Timeout = TimeSpan.FromMilliseconds(1)
						//resp = client.PostAsync("api/MapNode/PostmapNodeList", content).Result;
						response = await saveClient.PostAsync("api/MapNode/PostmapNodeList", content);
					}
					catch (TaskCanceledException tce)
					{
						StatusBlock.Text = "PostmapNodeList Task Canceled Exception";
						Console.WriteLine("TaskCanceledException");
					}
					//catch (Exception ex)
					//{
					//	Trace.WriteLine(ex.ToString());
					//}

					Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
						new Action(delegate
						{
							StatusBlock.Text = string.Format("Saving Mapset - Task # {0} of {1}", 3, 3);
							Progress.Value = 3;
						}));

					try
					{
						string jsonLinkString = JsonConvert.SerializeObject(mapLinkList);

						content = new StringContent(jsonLinkString, Encoding.UTF8, "application/json");
						response = await saveClient.PostAsync("api/MapLink/PostmapLinkList", content);
					}
					catch (Exception ex)
					{
						StatusBlock.Text = "PostmapNodeList Task Canceled Exception";
						Console.WriteLine(ex.Message);
					}
					StatusBlock.Text = "Complete. ";
					Progress.Value = 0;
					Progress.Visibility = Visibility.Hidden;
				}


				#region test

				//get list of map nodes for mapset
				//mapNodeList.Clear();
				//string getString = "api/MapNode/GetmapNodes/" + mapSetResp.Id;

				//resp = client.GetAsync(getString).Result;
				//mapNodeList = JsonConvert.DeserializeObject<List<mapNode>>(resp.Content.ReadAsStringAsync().Result);
				//resp = await client.GetAsync(getString);
				//returnString = await resp.Content.ReadAsStringAsync();
				//mapNodeList = JsonConvert.DeserializeObject<List<mapNode>>(returnString);

				//foreach (var node in mapNodeList)
				//{
				//	//maplink.
				//}

				//string jsonLinkString = JsonConvert.SerializeObject(mapLinkList);

				//mapNode mapnode1 = new mapNode
				//{
				//	latitude = 39.5M,
				//	longitude = -76.3159M,
				//	elevation = 0.0M,
				//	laneWidth = 0,
				//	directionality = 0,
				//	xOffset = 0,
				//	yOffset = 0,
				//	zOffset = 0,
				//	positionalAccuracyP1 = 0,
				//	positionalAccuracyP2 = 0,
				//	positionalAccuracyP3 = 0,
				//	laneOrder = 0,
				//	postedSpeed = 0,
				//	mapSetId = 4,
				//	distance = 0
				//};
				//mapNode mapnode2 = new mapNode
				//{
				//	latitude = 40.5M,
				//	longitude = -75.3159M,
				//	elevation = 0.1M,
				//	laneWidth = 0,
				//	directionality = 0,
				//	xOffset = 0,
				//	yOffset = 0,
				//	zOffset = 0,
				//	positionalAccuracyP1 = 0,
				//	positionalAccuracyP2 = 0,
				//	positionalAccuracyP3 = 0,
				//	laneOrder = 0,
				//	postedSpeed = 0,
				//	mapSetId = 4,
				//	distance = 0
				//};

				//List<mapNode> mapNodeList = new List<mapNode>();
				//mapNodeList.Add(mapnode1);
				//mapNodeList.Add(mapnode2);
				#endregion
			}
			else if (_tracks.Count < 1)
			{
				StatusBlock.Text = "Nothings to save...";
			}
		}
		private async void MenuItem_DeleteFromCloud_OnClick(object sender, RoutedEventArgs e)
		{
			var openCloudMapSetWindow = new OpenCloudMapSet(OpenMode.Delete) {WindowStartupLocation = WindowStartupLocation.CenterOwner};
			openCloudMapSetWindow.Owner = this;
			openCloudMapSetWindow.WindowMode = OpenMode.Delete;
			openCloudMapSetWindow.ShowDialog();

			if (openCloudMapSetWindow.Result == ResultMode.Cancel)
				return;

			var mapSetList = openCloudMapSetWindow.MapSetList;
			
			if (mapSetList.Count < 1)
				return;

			bool? result;
			var DeleteAllMapData = new DeleteAllMapData();
			DeleteAllMapData.Owner = this;
			DeleteAllMapData.ShowDialog();
			result = DeleteAllMapData.DialogResult;

			if (result != null && (bool)result)
			{
				try
				{
					if (mapSetList.Count > 0)
					{
						int counter = 1;
						Progress.Maximum = mapSetList.Count;
						Progress.Visibility = Visibility.Visible;
						foreach (var mapset in mapSetList)
						{
							Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
								new Action(delegate
								{
									StatusBlock.Text = string.Format("Deleting Mapsets # {0} of {1}", counter, mapSetList.Count);
									Progress.Value = counter;
								}));
							var resp = await client.DeleteAsync("api/MapLink/DeletemapLink/" + mapset.Id);
							resp = await client.DeleteAsync("api/MapNode/DeletemapNode/" + mapset.Id);
							resp = await client.DeleteAsync("api/MapSet/DeletemapSet/" + mapset.Id);
							counter++;
						}
						StatusBlock.Text = "Complete. ";
						Progress.Value = 0;
						Progress.Visibility = Visibility.Hidden;
					}
				}
				catch (Exception ex)
				{
					Trace.WriteLine(ex.ToString());
				}
			}
		}
		private void MenuItem_ClearTracks_OnClick(object sender, RoutedEventArgs e)
		{
			_tracks.Clear();
			_selectedPin = null;
			_selectedTrack = null;
			//pointDataList = new List<GpsData>();
			numberOfPoints.Text = "0";
			refreshMap();
		}
		#endregion

		#region ParseGGA
		public GpsData ParseGGA(string ggaSentence)
		{
			var data = new GpsData();

			var NmeaGgaParsingPattern = @"^.*\$(?<source>\w{2})GGA,(?<hh>\d{2})(?<mm>\d{2})(?<ss>\d{2})\.(?<ms>\d+)," +
				@"(?<lat_deg>\d{2})(?<lat_min>\d{2}\.\d+),(?<lat_t>[N,S])," +
				@"(?<long_deg>\d{3})(?<long_min>\d{2}\.\d+),(?<long_t>[E,W]),(?<Quality>[0,1,2])," +
				@"(?<num_sat>\d{2}),(?<hdop>\d+\.\d+),(?<altitude>-?\d*\.\d+),(?<meters>\w{1})," +
				@"(?<last_dgps>\d*)";

			var parsingRegexp = new Regex(NmeaGgaParsingPattern,
						   RegexOptions.Singleline | RegexOptions.Compiled);

			var matchResult = parsingRegexp.Match(ggaSentence);
			if (!matchResult.Success)
				throw new Exception("Invalid GGA data format");

			// Extract data
			var geodataSource = matchResult.Groups["source"].Value;
			var hour = int.Parse(matchResult.Groups["hh"].Value);
			var minutes = int.Parse(matchResult.Groups["mm"].Value);
			var seconds = int.Parse(matchResult.Groups["ss"].Value);
			var milseconds = int.Parse(matchResult.Groups["ms"].Value);
			var latDeg = int.Parse(matchResult.Groups["lat_deg"].Value);
			var latMin = double.Parse(matchResult.Groups["lat_min"].Value);
			var latSign = matchResult.Groups["lat_t"].Value;
			var longDeg = int.Parse(matchResult.Groups["long_deg"].Value);
			var longMin = double.Parse(matchResult.Groups["long_min"].Value);
			var longSign = matchResult.Groups["long_t"].Value;
			data.Quality = int.Parse(matchResult.Groups["Quality"].Value);
			data.SatCount = int.Parse(matchResult.Groups["num_sat"].Value);
			data.Altitude = double.Parse(matchResult.Groups["altitude"].Value);
			var meters = matchResult.Groups["meters"].Value;
			data.LastDgps = matchResult.Groups["last_dgps"].Value;
			data.DgpsStationId = matchResult.Groups["dgps_refstat"].Value;


			data.Time = string.Format("{0:D2}{1:D2}{2:D2}.{3:D2}", hour, minutes, seconds, milseconds);


			double.TryParse(matchResult.Groups["hdop"].Value, out data.Hdop);
			data.Latitude = latDeg + latMin / 60.0;
			if (latSign == "S")
				data.Latitude = data.Latitude * -1;
			data.Longitude = longDeg + longMin / 60.0;
			if (longSign == "W")
				data.Longitude = data.Longitude * -1;

			//Debug.WriteLine(geodataSource);
			//Debug.WriteLine("{0}:{1}:{2}.{3} lat, {4};{5}, {6}, {7};{8}long, {9}, {10} quality, {11} num Satalites", hour, minutes, seconds, milseconds, latDeg, latMin, latT, longDeg, longMin, longt, quality, numSat);
			//			Debug.WriteLine("{0} hdop", hdop);
			//Debug.WriteLine("{0} hdop, {1} altitude, {2} meters, {3} lastDgps", hdop, altitude, meters, lastDgps);

			return data;
		}
		#endregion

		#region Geometry Helper Functions

		public static GeoLocation FindPointAtDistanceFrom(GeoLocation startPoint, double initialBearingRadians, double distanceKilometres)
		{
			const double radiusEarthKilometres = 6371.01;
			var distRatio = distanceKilometres / radiusEarthKilometres;
			var distRatioSine = Math.Sin(distRatio);
			var distRatioCosine = Math.Cos(distRatio);

			var startLatRad = DegreesToRadians(startPoint.Latitude);
			var startLonRad = DegreesToRadians(startPoint.Longitude);

			var startLatCos = Math.Cos(startLatRad);
			var startLatSin = Math.Sin(startLatRad);

			var endLatRads = Math.Asin((startLatSin * distRatioCosine) + (startLatCos * distRatioSine * Math.Cos(initialBearingRadians)));

			var endLonRads = startLonRad
				+ Math.Atan2(
					Math.Sin(initialBearingRadians) * distRatioSine * startLatCos,
					distRatioCosine - startLatSin * Math.Sin(endLatRads));

			return new GeoLocation
			{
				Latitude = RadiansToDegrees(endLatRads),
				Longitude = RadiansToDegrees(endLonRads)
			};
		}

		public struct GeoLocation
		{
			public double Latitude { get; set; }
			public double Longitude { get; set; }
		}

		public double Bearing2(GpsData coordinate1, GpsData coordinate2)
		{
			//Convert input values to radians   
			var lat1 = DegreesToRadians(coordinate1.Latitude);
			var long1 = DegreesToRadians(coordinate1.Longitude);
			var lat2 = DegreesToRadians(coordinate2.Latitude);
			var long2 = DegreesToRadians(coordinate2.Longitude);

			double deltaLong = long2 - long1;

			double y = Math.Sin(deltaLong) * Math.Cos(lat2);
			double x = Math.Cos(lat1) * Math.Sin(lat2) -
					Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(deltaLong);
			double bearing = Math.Atan2(y, x);
			return bearing;
			//return bearing - Math.PI / 2;
		}

		public static Double Bearing(GpsData coordinate1, GpsData coordinate2)
		{
			var latitude1 = coordinate1.Latitude;
			var latitude2 = coordinate2.Latitude;

			var longitudeDifference = (coordinate2.Longitude - coordinate1.Longitude);


			var y = Math.Sin(longitudeDifference) * Math.Cos(latitude2);
			var x = Math.Cos(latitude1) * Math.Sin(latitude2) -

					Math.Sin(latitude1) * Math.Cos(latitude2) * Math.Cos(longitudeDifference);

			return Math.Atan2(y, x) - Math.PI / 2;
		}

		public double BearingToPoint(GpsData firstPoint, GpsData otherPoint)
		{
			var y = Math.Sin(Math.Abs(firstPoint.Longitude - otherPoint.Longitude)) * Math.Cos(otherPoint.Latitude);
			var x = Math.Cos(firstPoint.Latitude) * Math.Sin(otherPoint.Latitude) -
			Math.Sin(firstPoint.Latitude) * Math.Cos(otherPoint.Latitude) * Math.Cos(Math.Abs(firstPoint.Longitude - otherPoint.Longitude));
			var brng = Math.Atan2(y, x);

			return brng - Math.PI / 2;
		}

		public bool IsReciprocalBearing(double bearingOneRadians, double beaingTwoRadians)
		{
			double delta;
			if ((bearingOneRadians > 0 && beaingTwoRadians > 0) || (bearingOneRadians < 0 && beaingTwoRadians < 0))
			{
				delta = Math.Abs(Math.Abs(bearingOneRadians) - Math.Abs(beaingTwoRadians));
			}

			else
			{
				delta = Math.Abs(Math.Abs(bearingOneRadians) + Math.Abs(beaingTwoRadians));
			}

			return (delta > (Math.PI / 2));
		}

		public static double DegreesToRadians(double degrees)
		{
			const double degToRadFactor = Math.PI / 180;
			return degrees * degToRadFactor;
		}

		public static double RadiansToDegrees(double radians)
		{
			const double radToDegFactor = 180 / Math.PI;
			return radians * radToDegFactor;
		}
		#endregion

		private void Offest_OnKeyUp(object sender, KeyEventArgs e)
		{
			double.TryParse(Offest.Text, out _offestDistanceKillometers);
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{

		}

		private void MenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			window.Show();
		}

		void window_ValueChanged(object sender, TextChangedEventArgs e)
		{
			var window = sender as FilterWindow;
			if (window != null)
			{
				var val = window.SelectedValue;
			}
		}
	}
}
