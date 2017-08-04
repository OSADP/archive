// This file has been autogenerated from a class added in the UI designer.

using System;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using IDTO.Common;
using IDTO.Common.Models;

using MonoTouch.MapKit;
using MonoTouch.CoreLocation;

namespace IDTO.iPhone
{
	public partial class TripDetailsMapViewController : IDTOViewController
	{
		public Itinerary ItineraryToShow{ get; set;}
		public Trip TripToShow{ get; set; }


		private Dictionary<int,MKPolyline> polylineDictionary;
		private Dictionary<int, From> fromNameDictionary;
		private Dictionary<int, To> toNameDictionary;
		private Dictionary<int, String> modeDictionary;
		public TripDetailsMapViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			mapView.Delegate = new MapViewDelegate ();

			this.horizScrollView.PageChanged = ((int pageNumber) => {
				updateMapFromDictionary(pageNumber);
			
			});

			polylineDictionary = new Dictionary<int, MKPolyline> ();
			fromNameDictionary = new Dictionary<int, From> ();
			toNameDictionary = new Dictionary<int,To> ();
			modeDictionary = new Dictionary<int, string> ();


			if (ItineraryToShow != null) {
				SetupItinerary ();
			} else if (TripToShow != null) {
				SetupTrip ();
			}

			setupConnectorView ();
		}

		void SetupItinerary ()
		{
			setupHorizontalSlider (ItineraryToShow.legs.Count);
			List<Leg> LegsToShow = ItineraryToShow.legs;
			List<CLLocationCoordinate2D> points = new List<CLLocationCoordinate2D> ();
			int index = 1;
			foreach (var leg in LegsToShow) {
				List<CLLocationCoordinate2D> legpoints = new List<CLLocationCoordinate2D> ();
				foreach (var coord in leg.googlePoints) {
					CLLocationCoordinate2D newCoord = new CLLocationCoordinate2D (coord.Latitude, coord.Longitude);
					points.Add (newCoord);
					legpoints.Add (newCoord);
				}
				var legpolyline = MKPolyline.FromCoordinates (legpoints.ToArray ());
				polylineDictionary.Add (index, legpolyline);
				fromNameDictionary.Add (index, leg.from);
				toNameDictionary.Add (index, leg.to);
				modeDictionary.Add (index, leg.mode);
				index++;
			}
			var polyline = MKPolyline.FromCoordinates (points.ToArray ());
			polylineDictionary.Add (0, polyline);
			fromNameDictionary.Add (0, ItineraryToShow.legs [0].from);
			toNameDictionary.Add (0, ItineraryToShow.legs [ItineraryToShow.legs.Count - 1].to);
			modeDictionary.Add (0, "none");
			updateMapFromDictionary (0);
		}

		void SetupTrip ()
		{
			setupHorizontalSlider (TripToShow.Steps.Count);
			List<Step> LegsToShow = TripToShow.Steps;

			List<CLLocationCoordinate2D> points = new List<CLLocationCoordinate2D> ();
			int index = 1;
			foreach (var leg in LegsToShow) {
				List<CLLocationCoordinate2D> legpoints = new List<CLLocationCoordinate2D> ();
				foreach (var coord in leg.googlePoints) {
					CLLocationCoordinate2D newCoord = new CLLocationCoordinate2D (coord.Latitude, coord.Longitude);
					points.Add (newCoord);
					legpoints.Add (newCoord);
				}
				var legpolyline = MKPolyline.FromCoordinates (legpoints.ToArray ());
				polylineDictionary.Add (index, legpolyline);

				From fromPlace = new From ();
				fromPlace.name = leg.FromName;
				fromPlace.lat = leg.googlePoints [0].Latitude;
				fromPlace.lon = leg.googlePoints [0].Longitude;

				To toPlace = new To ();
				toPlace.name = leg.ToName;
				toPlace.lat = leg.googlePoints [leg.googlePoints.Count - 1].Latitude;
				toPlace.lon = leg.googlePoints [leg.googlePoints.Count - 1].Longitude;

				fromNameDictionary.Add (index, fromPlace);
				toNameDictionary.Add (index, toPlace);
				modeDictionary.Add (index, ModeType.IdToString(leg.ModeId));
				index++;
			}
			var polyline = MKPolyline.FromCoordinates (points.ToArray ());
			polylineDictionary.Add (0, polyline);

			From tripFrom = new From ();
			tripFrom.name = TripToShow.Steps[0].FromName;
			tripFrom.lat = TripToShow.Steps[0].googlePoints [0].Latitude;
			tripFrom.lon = TripToShow.Steps[0].googlePoints [0].Longitude;

			To tripTo = new To ();
			tripTo.name = TripToShow.Steps[TripToShow.Steps.Count-1].FromName;
			int googlePointsCount = TripToShow.Steps [TripToShow.Steps.Count - 1].googlePoints.Count;
			tripTo.lat = TripToShow.Steps[TripToShow.Steps.Count-1].googlePoints [googlePointsCount-1].Latitude;
			tripTo.lon = TripToShow.Steps[TripToShow.Steps.Count-1].googlePoints [googlePointsCount-1].Longitude;

			fromNameDictionary.Add (0, tripFrom);
			toNameDictionary.Add (0, tripTo);
			modeDictionary.Add (0, "none");
			updateMapFromDictionary (0);
		}

		private void updateMapFromDictionary(int index)
		{
			if(mapView.Overlays!=null)
				mapView.RemoveOverlays (mapView.Overlays);
	
			if(mapView.Annotations!=null)
				mapView.RemoveAnnotations (mapView.Annotations);

			From fromPlace = fromNameDictionary [index];

			mapView.AddAnnotation(new MKStartPointAnnotation(){
				Title = fromPlace.name,
				Coordinate = new CLLocationCoordinate2D(fromPlace.lat,fromPlace.lon),
				Mode = modeDictionary[index]
			});


			To toPlace = toNameDictionary [index];

			mapView.AddAnnotation(new MKEndPointAnnotation(){
				Title = toPlace.name,
				Coordinate = new CLLocationCoordinate2D(toPlace.lat,toPlace.lon)
			});

			MKPolyline polyline = polylineDictionary [index];
			mapView.AddOverlay (polyline);

			MKPolygon polygon = MKPolygon.FromPoints (polyline.Points);
			mapView.SetVisibleMapRect (polygon.BoundingMapRect, new UIEdgeInsets (20, 10, 10, 10), true);
		}

		private void setupHorizontalSlider(int legStepCount)
		{
			List<String> items = new List<string> ();
			items.Add ("Overview Map");
			for (int i = 1; i < legStepCount; i++) {
				String stepString = "Step " + i.ToString ();
				items.Add (stepString);
			}
				
			this.horizScrollView.AddItems (items);
		}

		private void setupConnectorView ()
		{
			this.AddConnectorView (horizScrollView);
			ExtendToView = mapView;
		}
	}
}