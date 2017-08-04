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
using IDTO.Common.Models;
using Android.Graphics;

using Android.Gms.Maps;
using Android.Gms.Maps.Model;

namespace IDTO.Android
{
    public class ItineraryMapView : Java.Lang.Object, GoogleMap.IOnMapLoadedCallback
	{

		private ItineraryMapPresenter presenter;
		private Activity activity;

        private Dictionary<int, PolylineOptions> polylineDictionary;
        private Dictionary<int, From> fromNameDictionary;
        private Dictionary<int, To> toNameDictionary;
        private Dictionary<int, String> modeDictionary;

        private MapFragment mapFragment;
        private ImageButton btnPrevMap;
        private ImageButton btnNextMap;
        private TextView txtMapDesc;

		private int displayingIndex;

		public ItineraryMapView(Activity activity, ItineraryMapPresenter presenter, Itinerary ItineraryToShow) 
        {
            try
            {
                polylineDictionary = new Dictionary<int, PolylineOptions>();
                fromNameDictionary = new Dictionary<int, From>();
                toNameDictionary = new Dictionary<int, To>();
                modeDictionary = new Dictionary<int, string>();

                this.presenter = presenter;
                this.activity = activity;
                this.activity.SetContentView(Resource.Layout.itinerary_map);

				this.mapFragment = this.activity.FragmentManager.FindFragmentById<MapFragment>(Resource.Id.mapfragment);

				

				this.btnPrevMap = this.activity.FindViewById<ImageButton>(Resource.Id.itinerary_map_btnPrevMap);
				this.btnNextMap = this.activity.FindViewById<ImageButton>(Resource.Id.itinerary_map_btnNextMap);
				this.txtMapDesc = this.activity.FindViewById<TextView>(Resource.Id.itinerary_map_txtMapDesc);

				txtMapDesc.Text = "";

				btnPrevMap.Click += (object sender, EventArgs e) => {
					displayingIndex--;
					if(displayingIndex<0)
						displayingIndex =0;
					updateMapFromDictionary(displayingIndex);

				};

				btnNextMap.Click += (object sender, EventArgs e) => {
					displayingIndex++;
					if(displayingIndex > polylineDictionary.Count - 1)
						displayingIndex = polylineDictionary.Count -1;
					updateMapFromDictionary(displayingIndex);

				};

                while(this.mapFragment.Map==null)
                {
                    System.Threading.Thread.Sleep(500);
                }

                DisplayItinerary(ItineraryToShow);

                this.mapFragment.Map.SetOnMapLoadedCallback(this);

            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

		}


		public void DisplayItinerary(Itinerary ItineraryToShow)
		{
            List<Leg> LegsToShow = ItineraryToShow.legs;
            List<LatLng> points = new List<LatLng>();
            int index = 1;
            foreach (var leg in LegsToShow)
            {
                List<LatLng> legpoints = new List<LatLng>();
                foreach (var coord in leg.googlePoints)
                {
                    LatLng newCoord = new LatLng(coord.Latitude, coord.Longitude);
                    points.Add(newCoord);
                    legpoints.Add(newCoord);
                }

				var legPolyline = new PolylineOptions().Visible(true).InvokeColor(Color.Red).InvokeWidth(5);
                legPolyline.Add(legpoints.ToArray());

                polylineDictionary.Add(index, legPolyline);
                fromNameDictionary.Add(index, leg.from);
                toNameDictionary.Add(index, leg.to);
                modeDictionary.Add(index, leg.mode);
                index++;
            }

			PolylineOptions polyline = new PolylineOptions().Visible(true).InvokeColor(Color.Red).InvokeWidth(5);
            polyline.Add(points.ToArray());

            polylineDictionary.Add(0, polyline);
            fromNameDictionary.Add(0, ItineraryToShow.legs[0].from);
            toNameDictionary.Add(0, ItineraryToShow.legs[ItineraryToShow.legs.Count - 1].to);
            modeDictionary.Add(0, "none");
            
		}

        private void updateMapFromDictionary(int index)
        {
			var map = mapFragment.Map;
			if (map != null) {

				map.Clear ();

				displayingIndex = index;

				updatePrevNextButtonVisibility (index);

				From fromPlace = fromNameDictionary [index];
				LatLng fromLatLng = new LatLng (fromPlace.lat, fromPlace.lon);

				if (displayingIndex == 0) {
					txtMapDesc.Text = "Overview Map";
				} else {
					txtMapDesc.Text = "Step " + index.ToString ();
				}


				MarkerOptions fromMarkerOpt = new MarkerOptions ();
				fromMarkerOpt.SetPosition (fromLatLng);
				fromMarkerOpt.SetTitle (fromPlace.name);
				fromMarkerOpt.InvokeIcon (BitmapDescriptorFactory.DefaultMarker (BitmapDescriptorFactory.HueGreen));

				var fromMarker = map.AddMarker (fromMarkerOpt);

				To toPlace = toNameDictionary [index];
				LatLng toLatLng = new LatLng (toPlace.lat, toPlace.lon);
				MarkerOptions toMarkerOpt = new MarkerOptions ();
				toMarkerOpt.SetPosition (toLatLng);
				toMarkerOpt.SetTitle (toPlace.name);
				toMarkerOpt.InvokeIcon (BitmapDescriptorFactory.DefaultMarker (BitmapDescriptorFactory.HueRed));

				var toMarker = map.AddMarker (toMarkerOpt);

				PolylineOptions polylineOptions = polylineDictionary [index];
				var polyline = map.AddPolyline (polylineOptions);

				LatLngBounds bounds = findMapBounds (polylineOptions);
				CameraUpdate cameraUpdate = CameraUpdateFactory.NewLatLngBounds (bounds, 50);

				//map.AnimateCamera(cameraUpdate);
				map.MoveCamera (cameraUpdate);
			}
        }

		private LatLngBounds findMapBounds(PolylineOptions polyOptions)
		{
			double maxLat = -10000;
			double maxLon = -10000;
			double minLat = 10000;
			double minLon = 10000;

			foreach(LatLng latLon in polyOptions.Points)
			{
				if (latLon.Latitude > maxLat)
					maxLat = latLon.Latitude;

				if (latLon.Latitude < minLat)
					minLat = latLon.Latitude;

				if (latLon.Longitude > maxLon)
					maxLon = latLon.Longitude;

				if (latLon.Longitude < minLon)
					minLon = latLon.Longitude;
			}

			LatLng southWestLatLng = new LatLng (minLat, minLon);
			LatLng northEastLatLng = new LatLng (maxLat, maxLon);

			return new LatLngBounds (southWestLatLng, northEastLatLng);

		}
				

		private void updatePrevNextButtonVisibility(int index)
		{
			if(displayingIndex ==0)
				btnPrevMap.Visibility = ViewStates.Invisible;
			else
				btnPrevMap.Visibility = ViewStates.Visible;

			if (displayingIndex == fromNameDictionary.Count - 1)
				btnNextMap.Visibility = ViewStates.Invisible;
			else
				btnNextMap.Visibility = ViewStates.Visible;

		}


        public void OnMapLoaded()
        {
            updateMapFromDictionary(0);
        }

        //public IntPtr Handle
        //{
        //    get { return new IntPtr(); }
        //}

        //public void Dispose()
        //{
            
        //}
    }
}

