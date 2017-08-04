/*!
    @file         InfloMapViewer/MainWindow.cs
    @author       Luke Kucalaba, Joshua Branch

    @copyright
    Copyright (c) 2013 Battelle Memorial Institute. All rights reserved.

    @par
    Unauthorized use or duplication may violate state, federal and/or
    international laws including the Copyright Laws of the United States
    and of other international jurisdictions.

    @par
    @verbatim
    Battelle Memorial Institute
    505 King Avenue
    Columbus, Ohio  43201
    @endverbatim

    @brief
    TBD

    @details
    TBD
*/
using System;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Maps.MapControl.WPF;
using InfloCommon.InfloDb;

namespace InfloMapViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private struct MileMarker
        {
            public double mmNumber_mi;
            public double lat_deg;
            public double lon_deg;
            public double lat_rad;
            public double lon_rad;
            public byte utmZone;
            public byte utmBand;
            public double utmEasting_m;
            public double utmNorthing_m;
        };
        
        private struct FrameDatums
        {
            public IList<TME_CVData_Input> rConnectedVehicleDatums;
            public IList<TMEOutput_QWARNMessage_CV> rQWarnMessageDatums;
            public IList<TMEOutput_SPDHARMMessage_CV> rSpdHarmMessageDatums;
        };

        private static InfloDbContext srDbContext;
        private System.Threading.Thread mrDatabaseThread;
        private System.Threading.AutoResetEvent mrDatabaseThreadExitEvent;
        private FrameDatums mrNextFrameDatums = new FrameDatums();

        // BEGIN locked/shared data members
        private FrameDatums mrCurrFrameDatums = new FrameDatums();
        // END locked/shared data members

        private System.Windows.Threading.DispatcherTimer mrDatabaseTimer;
        private System.Windows.Threading.DispatcherTimer mrRenderTimer;
        private BCS.Geo.Utm2Ellipsoid mrUtm2Ellipsoid;
        private int mrFirstActiveElementIndex;
        private bool mIsMapShadowElementsEnabled = false;

        private string mBingMapMode = "AerialWithLabels";

        private IDictionary<string, UIElement> mrMapActiveElements = 
            new Dictionary<string, UIElement>();

        private IDictionary<string, UIElement> mrMapShadowElements = 
            new Dictionary<string, UIElement>();

        private IDictionary<string, UIElement> mrMapHeadingArrows =
            new Dictionary<string, UIElement>();

        private IDictionary<string, InfloCommon.InfloDb.TME_CVData_Input> mrConnectedVehicles = 
            new Dictionary<string, InfloCommon.InfloDb.TME_CVData_Input>();

        private IDictionary<string, SortedList<double,MileMarker> > mrMileMarkers = 
            new Dictionary<string, SortedList<double,MileMarker> >();

        private const double MAX_SHADOW_ELEMENT_OPACITY = 0.5;
        private const double SHADOW_ELEMENT_FADE_DURATION_sec = 2.0;

        private void connectToDatabase()
        {
            string strDatabaseConnectionString =
                System.Configuration.ConfigurationManager.AppSettings["InfloDatabaseConnectionString"];
            
            if (strDatabaseConnectionString == null)
            {
                Trace.TraceError("Unable to retrieve database connection string");
            }
            else if (strDatabaseConnectionString.Length <= 0)
            {
                Trace.TraceError("Database connection string empty");
            }
            else  //connect to the database
            {
                Trace.TraceInformation("[INFO] Retrieved InfloDatabaseConnectionString for BsmWorkerRole\n{0}", 
                    strDatabaseConnectionString);

                srDbContext = new InfloDbContext(strDatabaseConnectionString);
            }

            string strShowMapShadowElements =
                System.Configuration.ConfigurationManager.AppSettings["ShowMapShadowElements"];

            mIsMapShadowElementsEnabled = Boolean.Parse(strShowMapShadowElements);
            return;
        }

        public void initializeFromDatabase()
        {
            // Load mile markers data structure
            {
                var rRoadwayMileMarkerGroups =
                    srDbContext.Configuration_RoadwayMileMarkers
                        .GroupBy(x => x.RoadwayId).ToList();

                foreach(var rGroup in rRoadwayMileMarkerGroups)
                {
                    string strRoadwayId = rGroup.FirstOrDefault().RoadwayId;
                    
                    SortedList<double,MileMarker> rSortedMileMarkers = 
                        new SortedList<double,MileMarker>();

                    foreach(var rRoadwayMileMarker in rGroup)
                    {
                        const double DegToRad = (Math.PI/180.0);

                        MileMarker mileMarker = new MileMarker();
                        mileMarker.mmNumber_mi = rRoadwayMileMarker.MMNumber;
                        mileMarker.lat_deg = rRoadwayMileMarker.Latitude1;
                        mileMarker.lon_deg = rRoadwayMileMarker.Longitude1;
                        mileMarker.lat_rad = (mileMarker.lat_deg * DegToRad);
                        mileMarker.lon_rad = (mileMarker.lon_deg * DegToRad);
                        mileMarker.utmZone = 0;
                        mileMarker.utmBand = 0; 
                        mileMarker.utmEasting_m = 0; 
                        mileMarker.utmNorthing_m = 0;

                        {
                            byte utmZone;
                            byte utmBand;
                            double utmEasting_m;
                            double utmNorthing_m;

                            if(mrUtm2Ellipsoid.LatLon2Utm(
                                mileMarker.lat_rad, 
                                mileMarker.lon_rad, 
                                out utmZone, 
                                out utmBand, 
                                out utmEasting_m, 
                                out utmNorthing_m))
                            {
                                mileMarker.utmZone = utmZone;
                                mileMarker.utmBand = utmBand;
                                mileMarker.utmEasting_m = utmEasting_m;
                                mileMarker.utmNorthing_m = utmNorthing_m;
                            }
                        }

                        rSortedMileMarkers.Add(mileMarker.mmNumber_mi, mileMarker);
                    }
                    
                    mrMileMarkers.Add(new KeyValuePair<string,SortedList<double,MileMarker>> (strRoadwayId, rSortedMileMarkers));
                }
            }

            // Render roadways as polylines
            {
                foreach(KeyValuePair<string, SortedList<double,MileMarker> > rRoadwayMileMarkers in mrMileMarkers)
                {
                    string strRoadwayId = rRoadwayMileMarkers.Key;

                    LocationCollection rLocations = new LocationCollection();

                    SortedList<double,MileMarker> rSortedMileMarkers = rRoadwayMileMarkers.Value;

                    foreach(KeyValuePair<double,MileMarker> rKeyValuePair in rSortedMileMarkers)
                    {
                        MileMarker rMileMarker = rKeyValuePair.Value;
                        rLocations.Add(new Location(rMileMarker.lat_deg, rMileMarker.lon_deg));
                    }

                    var rRoadway = 
                        srDbContext.Configuration_Roadway
                            .Where(x => x.RoadwayId == strRoadwayId)
                            .FirstOrDefault();
                    {
                        MapPolyline polyline = new MapPolyline();
                        
                        {
                            LinearGradientBrush rBrush = new System.Windows.Media.LinearGradientBrush();
                            rBrush.StartPoint = new Point(0,0);
                            rBrush.EndPoint = new Point(1,1);
                            rBrush.GradientStops.Add(new GradientStop(Colors.SlateGray, 0.0));
                            rBrush.GradientStops.Add(new GradientStop(Colors.Blue, 1.0));
                            polyline.Stroke = rBrush;
                        }

                        //polyline.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue);
                        polyline.StrokeThickness = 23;
                        polyline.StrokeStartLineCap = PenLineCap.Round;
                        polyline.StrokeEndLineCap = PenLineCap.Round;
                        polyline.StrokeLineJoin = PenLineJoin.Round;
                        polyline.Opacity = 0.50;
                        polyline.Locations = rLocations;

                        string strGrade = (rRoadway.Grade == null) ? "Unknown" : ((double)rRoadway.Grade).ToString("f3");
                        string strBeginMM = rRoadway.BeginMM.ToString("f5");
                        string strEndMM = rRoadway.EndMM.ToString("f5");
                        string strLowerHeading = (rRoadway.LowerHeading == null) ? "Unknown" : ((double)rRoadway.LowerHeading).ToString("f1");
                        string strUpperHeading = (rRoadway.UpperHeading == null) ? "Unknown" : ((double)rRoadway.UpperHeading).ToString("f1");
                        string strRecurringCongestionMMLocation = (rRoadway.RecurringCongestionMMLocation == null) ? "Unknown" : ((double)rRoadway.RecurringCongestionMMLocation).ToString("f5");

                        polyline.ToolTip =
                            String.Format(
                                "[Roadway]\nRoadwayId={0}\nName={1}\nDirection={2}\nGrade={3}\nBeginMM={4}\nEndMM={5}\nMMIncreasingDirection={6}\nLowerHeading={7}\nUpperHeading={8}\nRecurringCongestionMMLocation={9}",
                                rRoadway.RoadwayId,        
                                rRoadway.Name,
                                rRoadway.Direction,
                                strGrade,
                                strBeginMM,
                                strEndMM,
                                rRoadway.MMIncreasingDirection,
                                strLowerHeading,
                                strUpperHeading,
                                strRecurringCongestionMMLocation
                            );

                        this.BingMap.Children.Add(polyline);
                    }
                }
            }
            
            // Render georeferenced roadway links
            {
                var rRoadwayLinkGroups =
                    srDbContext.Configuration_RoadwayLinks
                        .GroupBy(x => x.RoadwayId).ToList();

                foreach(var rGroup in rRoadwayLinkGroups)
                {
                    foreach(var rRoadwayLink in rGroup)
                    {
                        LocationCollection rLocationCollection;  // O
                        if(this.georeferenceRoadwayStretch(out rLocationCollection, rRoadwayLink.RoadwayId, rRoadwayLink.BeginMM, rRoadwayLink.EndMM))
                        {
                            MapPolyline polyline = new MapPolyline();
                            
                            {
                                LinearGradientBrush rBrush = new System.Windows.Media.LinearGradientBrush();
                                rBrush.StartPoint = new Point(0,0);
                                rBrush.EndPoint = new Point(1,1);
                                rBrush.GradientStops.Add(new GradientStop(Colors.Peru, 0.0));
                                rBrush.GradientStops.Add(new GradientStop(Colors.Sienna, 1.0));
                                polyline.Stroke = rBrush;
                            }

                            //polyline.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkGreen);
                            polyline.StrokeThickness = 13;
                            polyline.StrokeStartLineCap = PenLineCap.Round;
                            polyline.StrokeEndLineCap = PenLineCap.Round;
                            polyline.StrokeLineJoin = PenLineJoin.Round;
                            polyline.Opacity = 0.40;
                            polyline.Locations = rLocationCollection;
                            
                            string strBeginMM = rRoadwayLink.BeginMM.ToString("f5");
                            string strEndMM = rRoadwayLink.EndMM.ToString("f5");
                            string strNumberLanes = (rRoadwayLink.NumberLanes == null) ? "Unknown" : ((short)rRoadwayLink.NumberLanes).ToString();
                            string strSpeedLimit = (rRoadwayLink.SpeedLimit == null) ? "Unknown" : ((short)rRoadwayLink.SpeedLimit).ToString();
                            string strNumberDetectorStations = (rRoadwayLink.NumberDetectorStations == null) ? "Unknown" : ((short)rRoadwayLink.NumberDetectorStations).ToString();

                            polyline.ToolTip =
                                String.Format(
                                    "[RoadwayLink]\nRoadwayId={0}\nLinkId={1}\nBeginMM={2}\nEndMM={3}\nStartCrossStreetName={4}\nEndCrossStreetName={5}\nUpstreamLinkId={6}\nDownstreamLinkId={7}\nNumberLanes={8}\nSpeedLimit={9}\nNumberDetectorStations={10}\nDetectorStations={11}\nESS={12}\nVSLSignId={13}\nDMSId={14}\nRSEId={15}\nDirection={16}",
                                    rRoadwayLink.RoadwayId,        
                                    rRoadwayLink.LinkId,
                                    strBeginMM,
                                    strEndMM,
                                    rRoadwayLink.StartCrossStreetName,
                                    rRoadwayLink.EndCrossStreetName,
                                    rRoadwayLink.UpstreamLinkId,
                                    rRoadwayLink.DownstreamLinkId,
                                    strNumberLanes,
                                    strSpeedLimit,
                                    strNumberDetectorStations,
                                    rRoadwayLink.DetectorStations,
                                    rRoadwayLink.ESS,
                                    rRoadwayLink.VSLSignId,
                                    rRoadwayLink.DMSId,
                                    rRoadwayLink.RSEId,
                                    rRoadwayLink.Direction
                                );
                            
                            this.BingMap.Children.Add(polyline);

                            {
                                foreach(Location rLocation in rLocationCollection)
                                {
                                    MapPolyline divider = new MapPolyline();
                                    divider.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
                                    divider.StrokeThickness = 13;
                                    divider.StrokeStartLineCap = PenLineCap.Round;  //PenLineCap.Triangle;
                                    divider.StrokeEndLineCap = PenLineCap.Round;  //PenLineCap.Triangle;
                                    divider.StrokeLineJoin = PenLineJoin.Round;  //PenLineJoin.Miter;
                                    divider.Opacity = 0.40;
                                    divider.Locations = new LocationCollection() { rLocation, rLocation };  //1-segment line with zero length
                                    this.BingMap.Children.Add(divider);
                                }
                            }
                        }
                    }
                }
            }
            
            // Render georeferenced roadway sublinks
            {
                var rRoadwaySubLinkGroups =
                    srDbContext.Configuration_RoadwaySubLinks
                        //.Select(x >= x.OrderBy(y >= y.RoadwayId))
                        //.GroupBy(x => x.RoadwayId)
                        //.Select(g => g.OrderBy(x => x.SubLinkId))
                        //.GroupBy(r => new {r.RoadwayId, r.SubLinkId}).ToList()
                        //.Select(g => g.OrderBy(x => x.RoadwayId)).ToList()
                        //.GroupBy(r => new { RoadwayId = r.RoadwayId, SubLinkId = r.SubLinkId} ).ToList()
                        //.GroupBy(g => g.Key).ToList()
                        //.GroupBy(r => r.RoadwayId).ToList()
                        //.SelectMany(g => g.OrderBy(x => x.RoadwayId)).ToList()
                        //.Select(g => new { GroupName = g.Key, Members = g }).ToList()
                        //.Select(g => g.OrderBy(x => x.Key.RoadwayId)).ToList()
                        //.OrderBy(g => g.Key)
                        //.Select(g => g.Key.RoadawayId)
                        //.SelectMany(g => new { RoadwayId = g.Key.RoadwayId })
                        //.Select(g => new { RoadwayId = g.Key.RoadwayId }).ToList()
                        //.OrderBy(x => x.SubLinkId)
                        //.Select(x => x)
                        //.GroupBy(r => r.RoadwayId).ToList().ToList();
                        //.Select(x => x)
                        //.GroupBy(r => new {r.RoadwayId, r.SubLinkId}).ToList()
                        //.Selet
                        //.GroupBy(r => new {r.RoadwayId, r.SubLinkId}).ToList()
                        //.GroupBy(r => r.RoadwayId).ToList()
                        //.Select(x => x)
                        //.GroupBy(r => r.RoadwayId).ToList()
                        //.Select(g => g.OrderBy(x => x.RoadwayId)).ToList()
                        //.OrderBy(x => x.RoadwayId)
                        //.Select(x => x).ToList()
                        //.GroupBy(x => new { x.RoadwayId, x.SubLinkId })
                        //.Select(g => g.OrderBy(x => new { x.RoadwayId, x.SubLinkId }))
                        //.GroupBy(x => x.RoadwayId)
                        //.Select(g => g.OrderBy(x => x.SubLinkId))
                        //.Select(g => g.OrderBy(x => x.SubLinkId)).ToList()
                        ;

                //foreach(var rGroup in rRoadwaySubLinkGroups)
                {
                    //foreach(var rRoadwaySubLink in rGroup)
                    foreach(var rRoadwaySubLink in rRoadwaySubLinkGroups)
                    {
                        LocationCollection rLocationCollection;  // O
                        if(this.georeferenceRoadwayStretch(out rLocationCollection, rRoadwaySubLink.RoadwayId, rRoadwaySubLink.BeginMM, rRoadwaySubLink.EndMM))
                        {
                            MapPolyline polyline = new MapPolyline();

                            {
                                LinearGradientBrush rBrush = new System.Windows.Media.LinearGradientBrush();
                                rBrush.StartPoint = new Point(0,0);
                                rBrush.EndPoint = new Point(1,1);
                                rBrush.GradientStops.Add(new GradientStop(Colors.Gold, 0.0));
                                rBrush.GradientStops.Add(new GradientStop(Colors.Goldenrod, 1.0));
                                polyline.Stroke = rBrush;
                            }
                            
                            //polyline.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGreen);
                            polyline.StrokeThickness = 3;
                            polyline.StrokeStartLineCap = PenLineCap.Round;
                            polyline.StrokeEndLineCap = PenLineCap.Round;
                            polyline.StrokeLineJoin = PenLineJoin.Round;
                            polyline.Opacity = 0.40;
                            polyline.Locations = rLocationCollection;
                            
                            string strBeginMM = rRoadwaySubLink.BeginMM.ToString("f5");
                            string strEndMM = rRoadwaySubLink.EndMM.ToString("f5");
                            string strSpeedLimit = (rRoadwaySubLink.SpeedLimit == null) ? "Unknown" : ((short)rRoadwaySubLink.SpeedLimit).ToString();
                            string strNumberLanes = (rRoadwaySubLink.NumberLanes == null) ? "Unknown" : ((short)rRoadwaySubLink.NumberLanes).ToString();
                            
                            /*
                            System.Console.WriteLine(
                                String.Format(
                                    "\n[RoadwaySubLink]\nRoadwayId={0}\nSubLinkId={1}\nBeginMM={2}\nEndMM={3}\nSpeedLimit={4}\nNumberLanes={5}\nDownstreamSubLinkId={6}\nUpstreamSubLinkId={7}\nVSLSignId={8}\nDMSId={9}\nRSEId={10}\nDirection={11}",
                                    rRoadwaySubLink.RoadwayId,        
                                    rRoadwaySubLink.SubLinkId,
                                    strBeginMM,
                                    strEndMM,
                                    strSpeedLimit,
                                    strNumberLanes,
                                    rRoadwaySubLink.DownstreamSubLinkId,
                                    rRoadwaySubLink.UpstreamSubLinkId,
                                    rRoadwaySubLink.VSLSignID,
                                    rRoadwaySubLink.DMSID,
                                    rRoadwaySubLink.RSEID,
                                    rRoadwaySubLink.Direction
                                )
                            );
                            */

                            polyline.ToolTip =
                                String.Format(
                                    "[RoadwaySubLink]\nRoadwayId={0}\nSubLinkId={1}\nBeginMM={2}\nEndMM={3}\nSpeedLimit={4}\nNumberLanes={5}\nDownstreamSubLinkId={6}\nUpstreamSubLinkId={7}\nVSLSignId={8}\nDMSId={9}\nRSEId={10}\nDirection={11}",
                                    rRoadwaySubLink.RoadwayId,        
                                    rRoadwaySubLink.SubLinkId,
                                    strBeginMM,
                                    strEndMM,
                                    strSpeedLimit,
                                    strNumberLanes,
                                    rRoadwaySubLink.DownstreamSubLinkId,
                                    rRoadwaySubLink.UpstreamSubLinkId,
                                    rRoadwaySubLink.VSLSignID,
                                    rRoadwaySubLink.DMSID,
                                    rRoadwaySubLink.RSEID,
                                    rRoadwaySubLink.Direction
                                );

                            this.BingMap.Children.Add(polyline);

                            {
                                foreach(Location rLocation in rLocationCollection)
                                {
                                    MapPolyline divider = new MapPolyline();
                                    divider.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
                                    divider.StrokeThickness = 3;
                                    divider.StrokeStartLineCap = PenLineCap.Round;  //PenLineCap.Triangle;
                                    divider.StrokeEndLineCap = PenLineCap.Round;  //PenLineCap.Triangle;
                                    divider.StrokeLineJoin = PenLineJoin.Round;  //PenLineJoin.Miter;
                                    divider.Opacity = 0.40;
                                    divider.Locations = new LocationCollection() { rLocation, rLocation };  //1-segment line with zero length
                                    this.BingMap.Children.Add(divider);
                                }
                            }
                        }
                    }
                }
            }

            // Render mile markers as zero-length polyline (1 line segment)
            {
                foreach(KeyValuePair<string, SortedList<double,MileMarker> > rRoadwayMileMarkers in mrMileMarkers)
                {
                    SortedList<double,MileMarker> rSortedMileMarkers = rRoadwayMileMarkers.Value;

                    foreach(KeyValuePair<double,MileMarker> rKeyValuePair in rSortedMileMarkers)
                    {
                        MileMarker rMileMarker = rKeyValuePair.Value;
                        Location rLocation = new Location(rMileMarker.lat_deg, rMileMarker.lon_deg);
                        
                        {
                            MapPolyline polyline = new MapPolyline();

                            {
                                RadialGradientBrush rBrush = new System.Windows.Media.RadialGradientBrush();
                                rBrush.Center = new Point(0.5, 0.5);
                                rBrush.RadiusX = 0.5; 
                                rBrush.RadiusY = 0.5;
                                rBrush.GradientStops.Add(new GradientStop(Colors.DarkBlue, 0.0));
                                rBrush.GradientStops.Add(new GradientStop(Colors.MidnightBlue, 1.0));
                                polyline.Stroke = rBrush;
                            }

                            //polyline.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DarkBlue);
                            polyline.StrokeThickness = 23;
                            polyline.StrokeStartLineCap = PenLineCap.Round;
                            polyline.StrokeEndLineCap = PenLineCap.Round;
                            polyline.StrokeLineJoin = PenLineJoin.Round;
                            polyline.Opacity = 0.40;
                            polyline.Locations = new LocationCollection() { rLocation, rLocation };  //(1 line segment)
                            
                            string strMMNumber = rMileMarker.mmNumber_mi.ToString("f5");
                            string strLatitude1_deg = rMileMarker.lat_deg.ToString("f7");
                            string strLongitude1_deg = rMileMarker.lon_deg.ToString("f7");
                            string strLatitude1_rad = rMileMarker.lat_rad.ToString("f9");
                            string strLongitude1_rad = rMileMarker.lon_rad.ToString("f9");
                            //string strLatitude2 = rRoadwayMileMarker.Latitude2.ToString("f7");
                            //string strLongitude2 = rRoadwayMileMarker.Longitude2.ToString("f7");

                            string strUtmPosition = 
                                        String.Format("\nUTM=[{0}{1}:{2:f3}mE,{3:f3}mN]",
                                            rMileMarker.utmZone,
                                            ((char)rMileMarker.utmBand).ToString(),
                                            rMileMarker.utmEasting_m,
                                            rMileMarker.utmNorthing_m
                                        );

                            polyline.ToolTip =
                                String.Format(
                                    //"[MileMarker]\nRoadwayId={0}\nMMNumber={1}\nLatitude1={2}\nLongitude1={3}\nLatitude2={4}\nLongitude2={5}{6}",
                                    "[MileMarker]\nRoadwayId={0}\nMMNumber={1}\nLatitude={2}deg\nLongitude={3}deg\nLatitude={4}rad\nLongitude={5}rad{6}",
                                    rRoadwayMileMarkers.Key,        
                                    strMMNumber,
                                    strLatitude1_deg,
                                    strLongitude1_deg,
                                    strLatitude1_rad,
                                    strLongitude1_rad,
                                    //strLatitude2,
                                    //strLongitude2,
                                    strUtmPosition
                                );
                            
                            this.BingMap.Children.Add(polyline);
                        }
                    }
                }
            }

            return;
        }

        /*
        private void ToolTipHandler(object sender, ToolTipEventArgs e)
        {
            // To stop the tooltip from appearing, mark the event as handled
            //e.Handled = true; 

            FrameworkElement source = (e.Source as FrameworkElement); 
            if(source != null)
            {
                MessageBox.Show(source.ToolTip.ToString()); // or whatever you like
            }
        }
        */

        public MainWindow()
        {
            try
            {
                System.Data.Entity.Database.SetInitializer<InfloDbContext>(null);

                // Create/initialize the PROJ geoconversion library
                mrUtm2Ellipsoid = new BCS.Geo.Utm2Ellipsoid("WGS84");

                this.InitializeComponent();

                //EventManager.RegisterClassHandler(typeof(FrameworkElement), FrameworkElement.ToolTipOpeningEvent, new ToolTipEventHandler(ToolTipHandler));
            }
            catch(Exception e)
            {
                MessageBox.Show(String.Format("Exception occurred when attempting to initialize the main window:\n{0}", e.ToString()), "InfloMapViewer - Initialization");
                return;
            }

            try
            {
                this.connectToDatabase();
            }
            catch(Exception e)
            {
                MessageBox.Show(String.Format("Exception occurred when attempting to connect to the INFLO database:\n{0}", e.ToString()), "InfloMapViewer - Initialization");
                return;
            }

            try
            {
                this.initializeFromDatabase();
            }
            catch(Exception e)
            {
                MessageBox.Show(String.Format("Exception occurred when attempting to initialize/read from the INFLO database:\n{0}", e.ToString()), "InfloMapViewer - Initialization");
                return;
            }

            mrFirstActiveElementIndex = this.BingMap.Children.Count;

            {
                System.Windows.Threading.DispatcherTimer rDispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                rDispatcherTimer.Tick += new EventHandler(databaseTimer_Tick);
                rDispatcherTimer.Interval = new TimeSpan(0,0,0,1,0);  //1Hz
                rDispatcherTimer.Start();
                mrDatabaseTimer = rDispatcherTimer;
            }
            
            {
                System.Windows.Threading.DispatcherTimer rDispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                rDispatcherTimer.Tick += new EventHandler(renderTimer_Tick);
                rDispatcherTimer.Interval = new TimeSpan(0,0,0,0,100);  //10Hz
                rDispatcherTimer.Start();
                mrRenderTimer = rDispatcherTimer;
            }

            {
                mrDatabaseThreadExitEvent = new System.Threading.AutoResetEvent(false);
                mrDatabaseThread = new System.Threading.Thread(new System.Threading.ThreadStart(OnStart_DatabaseThread));
                mrDatabaseThread.Start();
            }

            return;
        }

        private void OnStart_DatabaseThread()
        {
            while(!mrDatabaseThreadExitEvent.WaitOne(500))  //essentially sleep 500ms every cycle (check for thread exit event)
            {
                DateTime ageCutoffDateTime = DateTime.UtcNow.Subtract(new TimeSpan(0,5,0));  //filter out BSMs older than 5 minutes
                DateTime futureCutoffDateTime = DateTime.UtcNow.Add(new TimeSpan(0,5,0));  //filter out BSMs that are timestamped after 5 mins into the future
                
                mrNextFrameDatums = new FrameDatums();

                mrNextFrameDatums.rConnectedVehicleDatums = 
                    srDbContext.TME_CVData_Input
                        .Where(x => x.DateGenerated > ageCutoffDateTime)
                        .Where(x => x.DateGenerated < futureCutoffDateTime)
                        .GroupBy(x => x.NomadicDeviceID).ToList()
                        .Select(g => g.OrderByDescending(x => x.DateGenerated).FirstOrDefault()).ToList();
                
                mrNextFrameDatums.rQWarnMessageDatums = 
                    srDbContext.TMEOutput_QWARNMessage_CV
                        .Where(x => x.DateGenerated > ageCutoffDateTime)
                        .Where(x => x.DateGenerated < futureCutoffDateTime)
                        .GroupBy(x => x.RoadwayID).ToList()
                        .Select(g => g.OrderByDescending(x => x.DateGenerated).FirstOrDefault()).ToList();

                mrNextFrameDatums.rSpdHarmMessageDatums =
                    srDbContext.TMEOutput_SPDHARMMessage_CV
                        .Where(x => x.DateGenerated > ageCutoffDateTime)
                        .Where(x => x.DateGenerated < futureCutoffDateTime)
                        .GroupBy(x => x.RoadwayId).ToList()
                        .Select(g => g.OrderByDescending(x => x.DateGenerated).FirstOrDefault()).ToList();

                lock(mrDatabaseThread)
                {
                    mrCurrFrameDatums = mrNextFrameDatums;
                }

                //System.Console.WriteLine("{0}:OnStart_DatabaseThread()",System.DateTime.Now.Ticks);
            }

            return;
        }
        
        private void Window_Closed(object sender, EventArgs e)
        {
            mrDatabaseThreadExitEvent.Set();
            mrDatabaseThread.Join();  //infinite timeout
            mrDatabaseThread = null;
            return;
        }

        private void databaseTimer_Tick(object sender, EventArgs e)
        {
            SortedSet<string> rActiveElements = new SortedSet<string>();
            
            lock(mrDatabaseThread)
            {
                //if(rConnectedVehicles != null)
                if(mrCurrFrameDatums.rConnectedVehicleDatums != null)
                {
                    //foreach(var cv in rConnectedVehicles)
                    foreach(var cv in mrCurrFrameDatums.rConnectedVehicleDatums)
                    {
                        Pushpin rCarPushpin;
                        Pushpin rCarPushpinShadow = null;  //if enabled
                        MapPolyline rCarHeadingArrow;
                        
                        bool wasCreated = false;

                        if(mrMapActiveElements.ContainsKey(cv.NomadicDeviceID))
                        {
                            rCarPushpin = (mrMapActiveElements[cv.NomadicDeviceID] as Pushpin);
                            rCarHeadingArrow = (mrMapHeadingArrows[cv.NomadicDeviceID] as MapPolyline);
                            
                            if(mIsMapShadowElementsEnabled)
                            {
                                rCarPushpinShadow = (mrMapShadowElements[cv.NomadicDeviceID] as Pushpin);
                            }
                        }
                        else  //create a new CV marker
                        {
                            wasCreated = true;
                            ControlTemplate rBestTemplate = this.getBestSizedCarPushpinTemplate();

                            {
                                rCarPushpin = new Pushpin();
                                rCarPushpin.Height = 64;
                                rCarPushpin.Width = 64;
                                rCarPushpin.PositionOrigin =  PositionOrigin.Center;
                                rCarPushpin.Template = rBestTemplate;
                            }

                            if(mIsMapShadowElementsEnabled)
                            {
                                rCarPushpinShadow = new Pushpin();
                                rCarPushpinShadow.Height = 64;
                                rCarPushpinShadow.Width = 64;
                                rCarPushpinShadow.PositionOrigin =  PositionOrigin.Center;
                                rCarPushpinShadow.Template = rBestTemplate;
                            }

                            {
                                rCarHeadingArrow = new MapPolyline();

                                rCarHeadingArrow.Stroke = new System.Windows.Media.SolidColorBrush(Colors.Black);
                                rCarHeadingArrow.StrokeThickness = 1;
                                rCarHeadingArrow.StrokeDashArray = new DoubleCollection(){1,2};
                                rCarHeadingArrow.StrokeDashCap = PenLineCap.Round;
                                rCarHeadingArrow.StrokeDashOffset = 0;
                                rCarHeadingArrow.StrokeStartLineCap = PenLineCap.Round;
                                rCarHeadingArrow.StrokeEndLineCap = PenLineCap.Round;
                                rCarHeadingArrow.StrokeLineJoin = PenLineJoin.Round;
                                rCarHeadingArrow.Opacity = 0.75;
                            }
                        }
                        
                        rCarPushpin.Heading = (double)(cv.Heading ?? 0);
                        //rCarPushpin.Heading = normalizeAngle0to2PI((double)(cv.Heading ?? 0) );

                        rCarPushpin.Location = new Location();
                        rCarPushpin.Location.Latitude = (cv.Latitude ?? 0);
                        rCarPushpin.Location.Longitude = (cv.Longitude ?? 0);
                        
                        if(mrMileMarkers.ContainsKey(cv.RoadwayId) &&
                            ((cv.Latitude == null) || (cv.Longitude == null) || (cv.Latitude == 0) || (cv.Longitude == 0)))
                        {
                            LocationCollection rLocationCollection;  // O
                            if(this.georeferenceRoadwayStretch(out rLocationCollection, cv.RoadwayId, cv.MMLocation, cv.MMLocation))
                            {
                                rCarPushpin.Location.Latitude = rLocationCollection.First().Latitude;
                                rCarPushpin.Location.Longitude = rLocationCollection.First().Longitude;
                                //cv.Latitude = rLocationCollection.First().Latitude;
                                //cv.Longitude = rLocationCollection.First().Longitude;

                                if((cv.Heading <= 0) || (cv.Heading >= 360))
                                {
                                    double heading_deg;  // O
                                    if(this.georeferenceHeading(out heading_deg, cv.RoadwayId, cv.MMLocation))
                                    {
                                        rCarPushpin.Heading = heading_deg;
                                        //cv.Heading = (short)heading_deg;
                                    }
                                }
                            }
                        }
                        
                        {
                            LocationCollection rHeadingArrowLocations = new LocationCollection();

                            // Compute heading arrow endpoint geodetic location
                            double speed_mph = (cv.Speed ?? 0);
                            if(speed_mph == 0)
                            {
                                rHeadingArrowLocations.Add(new Location(rCarPushpin.Location));
                            }
                            else
                            {
                                double elapsedTime_ms = 1000.0;  //DateTime.UtcNow.Subtract(cv.DateGenerated ?? DateTime.UtcNow).TotalMilliseconds;

                                // 20mi       1hr    1609.34m
                                //  1hr    3600sec         1mi    
                                double METERS_IN_ONE_MILE = 1609.34;
                                double SECONDS_IN_ONE_HOUR = 3600.0;
                                double MphToMps = (METERS_IN_ONE_MILE / SECONDS_IN_ONE_HOUR);

                                double distH0_m = (speed_mph * MphToMps) * (elapsedTime_ms / 1000.0);

                                const double DegToRad = (Math.PI/180.0);
                                double heading_rad = (rCarPushpin.Heading * DegToRad);

                                double distX0_m = distH0_m * Math.Cos((Math.PI/2.0) - heading_rad);
                                double distY0_m = distH0_m * Math.Sin((Math.PI/2.0) - heading_rad);

                                {
                                    double lat_rad = (rCarPushpin.Location.Latitude * DegToRad);
                                    double lon_rad = (rCarPushpin.Location.Longitude * DegToRad);
                                    byte utmZone;
                                    byte utmBand;
                                    double utmEasting_m;
                                    double utmNorthing_m;

                                    if(mrUtm2Ellipsoid.LatLon2Utm(
                                        lat_rad, 
                                        lon_rad, 
                                        out utmZone, 
                                        out utmBand, 
                                        out utmEasting_m, 
                                        out utmNorthing_m))
                                    {
                                        utmEasting_m += distX0_m;
                                        utmNorthing_m += distY0_m;

                                        if(mrUtm2Ellipsoid.Utm2LatLon(
                                            utmZone,
                                            utmBand,
                                            utmEasting_m,
                                            utmNorthing_m,
                                            out lat_rad, 
                                            out lon_rad))
                                        {
                                            const double RadToDeg = (180.0/Math.PI);
                                            double lat_deg = (lat_rad * RadToDeg);
                                            double lon_deg = (lon_rad * RadToDeg);
                                            rHeadingArrowLocations.Add(new Location(lat_deg, lon_deg));
                                        }
                                    }
                                }
                            }
                            
                            rHeadingArrowLocations.Add(new Location(rCarPushpin.Location));  //arrow goes from target to car (makes dashes look better)
                            rCarHeadingArrow.Locations = rHeadingArrowLocations;
                        }
                        
                        string strCVMessageIdentifier = (cv.CVMessageIdentifier.ToString());
                        string strDateGenerated = (cv.DateGenerated == null) ? "Unknown" : cv.DateGenerated.ToString();
                        string strSpeed = (cv.Speed == null) ? "Unknown" : ((double)cv.Speed).ToString("f1");
                        string strHeading = (cv.Heading == null) ? "Unknown" : ((short)cv.Heading).ToString();
                        string strLatitude = (cv.Latitude == null) ? "Unknown" : ((double)cv.Latitude).ToString("f5");
                        string strLongitude = (cv.Longitude == null) ? "Unknown" : ((double)cv.Longitude).ToString("f5");
                        string strMMLocation = cv.MMLocation.ToString("f3");
                        string strCVQueuedState = (cv.CVQueuedState == null) ? "Unknown" : ((bool)cv.CVQueuedState).ToString();
                        string strCoefficientOfFriction = (cv.CoefficientOfFriction == null) ? "Unknown" : ((double)cv.CoefficientOfFriction).ToString("f5");
                        string strTemperature = (cv.Temperature == null) ? "Unknown" : ((short)cv.Temperature).ToString();
                        
                        {
                            //ToolTip rToolTip = new ToolTip();
                            //rToolTip.Template = this.Resources["CustomTooltipTemplate"] as ControlTemplate;
                            //rToolTip.Name = "ConnectedVehicle";

                            rCarPushpin.ToolTip = 
                                String.Format(
                                    "[ConnectedVehicle]\nCVMessageIdentifier={0}\nNomadicDeviceID={1}\nDateGenerated={2}\nSpeed={3}mph\nHeading={4}deg\nLatitude={5}deg\nLongitude={6}deg\nMMLocation={7}mi\nCVQueuedState={8}\nCoefficientOfFriction={9}\nTemperature={10}F\nRoadwayId={11}", 
                                    strCVMessageIdentifier,
                                    cv.NomadicDeviceID,
                                    strDateGenerated,
                                    strSpeed,
                                    strHeading,
                                    strLatitude,
                                    strLongitude,
                                    strMMLocation,
                                    strCVQueuedState,
                                    strCoefficientOfFriction,
                                    strTemperature,
                                    cv.RoadwayId
                                );

                            //rCarPushpin.ToolTip = rToolTip;
                        }

                        if(cv.CVQueuedState == null)  //unknown
                        {
                            rCarPushpin.Background = new System.Windows.Media.SolidColorBrush(Color.FromRgb(128,128,128));
                        }
                        else if((bool)cv.CVQueuedState)  //queued
                        {
                            //rCarPushpin.Background = new System.Windows.Media.SolidColorBrush(Color.FromRgb(255,64,64));

                            {
                                RadialGradientBrush rBrush = new System.Windows.Media.RadialGradientBrush();
                                rBrush.Center = new Point(0.5, 0.5);
                                rBrush.RadiusX = 0.5; 
                                rBrush.RadiusY = 0.5;
                                rBrush.GradientStops.Add(new GradientStop(Color.FromRgb(255,64,64), 0.0));
                                rBrush.GradientStops.Add(new GradientStop(Color.FromRgb(64,0,0), 1.0));
                                rCarPushpin.Background = rBrush;
                            }
                        }
                        else  //not queued
                        {
                            //rCarPushpin.Background = new System.Windows.Media.SolidColorBrush(Color.FromRgb(0,255,128));
                            
                            {
                                RadialGradientBrush rBrush = new System.Windows.Media.RadialGradientBrush();
                                rBrush.Center = new Point(0.5, 0.5);
                                rBrush.RadiusX = 0.5; 
                                rBrush.RadiusY = 0.5;
                                rBrush.GradientStops.Add(new GradientStop(Color.FromRgb(0,255,128), 0.0));
                                rBrush.GradientStops.Add(new GradientStop(Color.FromRgb(0,64,0), 1.0));
                                rCarPushpin.Background = rBrush;
                            }
                        }
                    
                        if(mIsMapShadowElementsEnabled)
                        {
                            rCarPushpinShadow.Heading = rCarPushpin.Heading;
                            rCarPushpinShadow.Location = new Location();
                            rCarPushpinShadow.Location.Latitude = rCarPushpin.Location.Latitude;
                            rCarPushpinShadow.Location.Longitude = rCarPushpin.Location.Longitude;
                            rCarPushpinShadow.Background = rCarPushpin.Background;
                            rCarPushpinShadow.Opacity = MAX_SHADOW_ELEMENT_OPACITY;
                        }
                        
                        mrConnectedVehicles[cv.NomadicDeviceID] = cv;

                        rActiveElements.Add(cv.NomadicDeviceID);

                        if(wasCreated)
                        {
                            mrMapHeadingArrows.Add(cv.NomadicDeviceID, rCarHeadingArrow);
                            this.BingMap.Children.Add(rCarHeadingArrow);
                            
                            mrMapActiveElements.Add(cv.NomadicDeviceID, rCarPushpin);
                            this.BingMap.Children.Add(rCarPushpin);
                            
                            if(mIsMapShadowElementsEnabled)
                            {
                                // Insert shadow elements behind active elements but in front of roadway elements
                                mrMapShadowElements.Add(cv.NomadicDeviceID, rCarPushpinShadow);
                                this.BingMap.Children.Insert(mrFirstActiveElementIndex, rCarPushpinShadow);
                            }
                        }
                    }
                }
            }

            lock(mrDatabaseThread)
            {
                //if(rQWarnMessageGroups != null)
                //if(rQWarnMessages != null)
                if(mrCurrFrameDatums.rQWarnMessageDatums != null)
                {
                    //foreach(var rGroup in rQWarnMessageGroups)
                    {
                        //foreach(var rQWarnAlert in rGroup)
                        //foreach(var rQWarnAlert in rQWarnMessages)
                        foreach(var rQWarnAlert in mrCurrFrameDatums.rQWarnMessageDatums)
                        {
                            MapPolyline rPolyline = null;
                            bool wasCreated = false;

                            string strQWarnId = String.Format("q{0}", rQWarnAlert.RoadwayID);
                        
                            LocationCollection rLocationCollection;  // O
                            if(this.georeferenceRoadwayStretch(out rLocationCollection, rQWarnAlert.RoadwayID, rQWarnAlert.BOQMMLocation, rQWarnAlert.FOQMMLocation))
                            {
                                if(mrMapActiveElements.ContainsKey(strQWarnId))
                                {
                                    rPolyline = (mrMapActiveElements[strQWarnId] as MapPolyline);
                                }
                                else  //create a new queue symbol
                                {
                                    wasCreated = true;
                                    rPolyline = new MapPolyline();

                                    {
                                        LinearGradientBrush rBrush = new System.Windows.Media.LinearGradientBrush();
                                        rBrush.StartPoint = new Point(0,0);
                                        rBrush.EndPoint = new Point(1,1);
                                        rBrush.GradientStops.Add(new GradientStop(Colors.Magenta, 0.0));
                                        rBrush.GradientStops.Add(new GradientStop(Colors.Red, 1.0));
                                        rPolyline.Stroke = rBrush;
                                    }

                                    //rPolyline.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Magenta);
                                    rPolyline.StrokeThickness = 53;
                                    rPolyline.StrokeStartLineCap = PenLineCap.Flat;
                                    rPolyline.StrokeEndLineCap = PenLineCap.Flat;
                                    rPolyline.StrokeLineJoin = PenLineJoin.Round;
                                    rPolyline.Opacity = 0.50;
                                }
                            }
                            
                            if(rPolyline != null)
                            {
                                rPolyline.Locations = rLocationCollection;

                                string strDateGenerated = (rQWarnAlert.DateGenerated == null) ? "Unknown" : rQWarnAlert.DateGenerated.ToString();
                                string strBOQMMLocation = rQWarnAlert.BOQMMLocation.ToString("f3");
                                string strFOQMMLocation = rQWarnAlert.FOQMMLocation.ToString("f3");
                                string strSpeedInQueue = (rQWarnAlert.SpeedInQueue == null) ? "Unknown" : ((double)rQWarnAlert.SpeedInQueue).ToString("f1");
                                string strRateOfQueueGrowth = (rQWarnAlert.RateOfQueueGrowth == null) ? "Unknown" : ((short)rQWarnAlert.RateOfQueueGrowth).ToString();
                                string strValidityDuration = (rQWarnAlert.ValidityDuration == null) ? "Unknown" : ((int)rQWarnAlert.ValidityDuration).ToString();
                        
                                rPolyline.ToolTip = 
                                    String.Format(
                                        "[QWarnAlert]\nDateGenerated={0}\nRoadwayID={1}\nBOQMMLocation={2}\nFOQMMLocation={3}\nSpeedInQueue={4}\nRateOfQueueGrowth={5}\nValidityDuration={6}", 
                                        strDateGenerated,
                                        rQWarnAlert.RoadwayID,
                                        strBOQMMLocation,
                                        strFOQMMLocation,
                                        strSpeedInQueue,
                                        strRateOfQueueGrowth,
                                        strValidityDuration
                                    );

                                rActiveElements.Add(strQWarnId);
                            }
                        
                            if(wasCreated)
                            {
                                mrMapActiveElements.Add(strQWarnId, rPolyline);
                                this.BingMap.Children.Insert(0, rPolyline);  //always send to back
                            }
                        }
                    }
                }
            }

            lock(mrDatabaseThread)
            {
                //if(rSpdHarmMessageGroups != null)
                //if(rSpdHarmMessages != null)
                if(mrCurrFrameDatums.rSpdHarmMessageDatums != null)
                {
                    //foreach(var rGroup in rSpdHarmMessageGroups)
                    {
                        //foreach(var rSpdHarmAlert in rGroup)
                        //foreach(var rSpdHarmAlert in rSpdHarmMessages)
                        foreach(var rSpdHarmAlert in mrCurrFrameDatums.rSpdHarmMessageDatums)
                        {
                            MapPolyline rPolyline = null;
                            bool wasCreated = false;

                            string strSpdHarmId = String.Format("s{0}", rSpdHarmAlert.RoadwayId);
                        
                            double start_mi = rSpdHarmAlert.BeginMM;
                            double end_mi = (rSpdHarmAlert.EndMM ?? start_mi);

                            LocationCollection rLocationCollection;  // O
                            if(this.georeferenceRoadwayStretch(out rLocationCollection, rSpdHarmAlert.RoadwayId, start_mi, end_mi))
                            {
                                if(mrMapActiveElements.ContainsKey(strSpdHarmId))
                                {
                                    rPolyline = (mrMapActiveElements[strSpdHarmId] as MapPolyline);
                                }
                                else  //create a new queue symbol
                                {
                                    wasCreated = true;
                                    rPolyline = new MapPolyline();

                                    {
                                        LinearGradientBrush rBrush = new System.Windows.Media.LinearGradientBrush();
                                        rBrush.StartPoint = new Point(0,0);
                                        rBrush.EndPoint = new Point(1,1);
                                        rBrush.GradientStops.Add(new GradientStop(Colors.Cyan, 0.0));
                                        rBrush.GradientStops.Add(new GradientStop(Colors.DodgerBlue, 1.0));
                                        rPolyline.Stroke = rBrush;
                                    }

                                    //rPolyline.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Cyan);
                                    rPolyline.StrokeThickness = 53;
                                    rPolyline.StrokeStartLineCap = PenLineCap.Flat;
                                    rPolyline.StrokeEndLineCap = PenLineCap.Flat;
                                    rPolyline.StrokeLineJoin = PenLineJoin.Round;
                                    rPolyline.Opacity = 0.50;
                                }
                            }

                            if(rPolyline != null)
                            {
                                rPolyline.Locations = rLocationCollection;

                                string strDateGenerated = (rSpdHarmAlert.DateGenerated == null) ? "Unknown" : rSpdHarmAlert.DateGenerated.ToString();
                                string strRecommendedSpeed = ((double)rSpdHarmAlert.RecommendedSpeed).ToString("f1");
                                string strBeginMM = rSpdHarmAlert.BeginMM.ToString("f3");
                                string strEndMM = (rSpdHarmAlert.EndMM == null) ? "Unknown" : ((double)rSpdHarmAlert.EndMM).ToString("f3");
                                string strValidityDuration = (rSpdHarmAlert.ValidityDuration == null) ? "Unknown" : ((int)rSpdHarmAlert.ValidityDuration).ToString();

                                rPolyline.ToolTip = 
                                    String.Format(
                                        "[SpdHarmAlert]\nDateGenerated={0}\nRoadwayID={1}\nRecommendedSpeed={2}mph\nBeginMM={3}\nEndMM={4}\nJustification={5}\nValidityDuration={6}", 
                                        strDateGenerated,
                                        rSpdHarmAlert.RoadwayId,
                                        strRecommendedSpeed,
                                        strBeginMM,
                                        strEndMM,
                                        rSpdHarmAlert.Justification,
                                        strValidityDuration
                                    );
                                
                                rActiveElements.Add(strSpdHarmId);
                            }
                        
                            if(wasCreated)
                            {
                                mrMapActiveElements.Add(strSpdHarmId, rPolyline);
                                this.BingMap.Children.Insert(0, rPolyline);  //always send to back
                            }
                        }
                    }
                }
            }

            // Purge all expired map active elements out of the Bing map
            {
                Dictionary<string, UIElement> rPurgeList = new Dictionary<string,UIElement>();

                foreach(KeyValuePair<string, UIElement> rElement in mrMapActiveElements)
                {
                    if(!rActiveElements.Contains(rElement.Key))
                    {
                        rPurgeList.Add(rElement.Key, rElement.Value);
                    }
                }
                
                foreach(KeyValuePair<string, UIElement> rElement in rPurgeList)
                {
                    this.BingMap.Children.Remove(rElement.Value);
                    mrMapActiveElements.Remove(rElement.Key);
                }
            }

            // Purge all expired map heading arrows out of the Bing map
            {
                Dictionary<string, UIElement> rPurgeList = new Dictionary<string,UIElement>();

                foreach(KeyValuePair<string, UIElement> rElement in mrMapHeadingArrows)
                {
                    if(!rActiveElements.Contains(rElement.Key))
                    {
                        rPurgeList.Add(rElement.Key, rElement.Value);
                    }
                }
                
                foreach(KeyValuePair<string, UIElement> rElement in rPurgeList)
                {
                    this.BingMap.Children.Remove(rElement.Value);
                    mrMapHeadingArrows.Remove(rElement.Key);
                }
            }
            
            // Purge all expired map shadow elements out of the Bing map
            if(mIsMapShadowElementsEnabled)
            {
                Dictionary<string, UIElement> rPurgeList = new Dictionary<string,UIElement>();

                foreach(KeyValuePair<string, UIElement> rElement in mrMapShadowElements)
                {
                    if(!rActiveElements.Contains(rElement.Key))
                    {
                        rPurgeList.Add(rElement.Key, rElement.Value);
                    }
                }
                
                foreach(KeyValuePair<string, UIElement> rElement in rPurgeList)
                {
                    this.BingMap.Children.Remove(rElement.Value);
                    mrMapShadowElements.Remove(rElement.Key);
                }
            }

            // Purge all expired connected vehicles out of the CV data map
            {
                Dictionary<string, InfloCommon.InfloDb.TME_CVData_Input> rPurgeList = 
                    new Dictionary<string,InfloCommon.InfloDb.TME_CVData_Input>();
                
                foreach(KeyValuePair<string, InfloCommon.InfloDb.TME_CVData_Input> rConnectedVehicle in mrConnectedVehicles)
                {
                    if(!rActiveElements.Contains(rConnectedVehicle.Key))
                    {
                        rPurgeList.Add(rConnectedVehicle.Key, rConnectedVehicle.Value);
                    }
                }
                
                foreach(KeyValuePair<string, InfloCommon.InfloDb.TME_CVData_Input> rConnectedVehicle in rPurgeList)
                {
                    mrConnectedVehicles.Remove(rConnectedVehicle.Key);
                }
            }

            return;
        }
        
        private void renderTimer_Tick(object sender, EventArgs e)
        {
            if(mIsMapShadowElementsEnabled)
            {
                foreach(KeyValuePair<string, UIElement> rElement in mrMapShadowElements)
                {
                    Pushpin rCarPushpinShadow = (rElement.Value as Pushpin);

                    InfloCommon.InfloDb.TME_CVData_Input cv = mrConnectedVehicles[rElement.Key];
                    double elapsedTime_ms = DateTime.UtcNow.Subtract(cv.DateGenerated ?? DateTime.UtcNow).TotalMilliseconds;

                    // 20mi       1hr    1609.34m
                    //  1hr    3600sec         1mi    
                    double METERS_IN_ONE_MILE = 1609.34;
                    double SECONDS_IN_ONE_HOUR = 3600.0;
                    double MphToMps = (METERS_IN_ONE_MILE / SECONDS_IN_ONE_HOUR);

                    double distH0_m = ((cv.Speed ?? 0) * MphToMps) * (elapsedTime_ms / 1000.0);

                    const double DegToRad = (Math.PI/180.0);
                    double heading_rad = ((cv.Heading ?? 0) * DegToRad);

                    double distX0_m = distH0_m * Math.Cos((Math.PI/2.0) - heading_rad);
                    double distY0_m = distH0_m * Math.Sin((Math.PI/2.0) - heading_rad);

                    {
                        double lat_rad = ((cv.Latitude ?? 0) * DegToRad);
                        double lon_rad = ((cv.Longitude ?? 0) * DegToRad);
                        byte utmZone;
                        byte utmBand;
                        double utmEasting_m;
                        double utmNorthing_m;

                        if(mrUtm2Ellipsoid.LatLon2Utm(
                            lat_rad, 
                            lon_rad, 
                            out utmZone, 
                            out utmBand, 
                            out utmEasting_m, 
                            out utmNorthing_m))
                        {
                            utmEasting_m += distX0_m;
                            utmNorthing_m += distY0_m;

                            if(mrUtm2Ellipsoid.Utm2LatLon(
                                utmZone,
                                utmBand,
                                utmEasting_m,
                                utmNorthing_m,
                                out lat_rad, 
                                out lon_rad))
                            {
                                const double RadToDeg = (180.0/Math.PI);
                                double lat_deg = (lat_rad * RadToDeg);
                                double lon_deg = (lon_rad * RadToDeg);
                                rCarPushpinShadow.Location = new Location(lat_deg, lon_deg);
                            }
                        }
                    }

                    //rCarPushpinShadow.Opacity = 
                }
            }

            return;
        }
        
        private static double normalizeAngle0to2PI(double angle)
        {
            const double TWO_PI = (2.0 * Math.PI);
            return 
                (angle < 0) ? 
                    (TWO_PI - (-angle % TWO_PI)) :
                    (angle % TWO_PI);
        }

        private bool georeferenceHeading(out double heading_deg, string strRoadwayId, double start_mi)
        {
            if(mrMileMarkers.ContainsKey(strRoadwayId))
            {
                SortedList<double,MileMarker> rSortedMileMarkers = mrMileMarkers[strRoadwayId];

                // Ensure there is at least a linear route (requires minimum two mile marker points)
                if(rSortedMileMarkers.Count() >= 2)
                {
                    KeyValuePair<double,MileMarker> rKeyValuePairPreStart = rSortedMileMarkers.First();
                    KeyValuePair<double,MileMarker> rKeyValuePairPostStart = rSortedMileMarkers.First();
                    
                    foreach(KeyValuePair<double,MileMarker> rKeyValuePair in rSortedMileMarkers)
                    {
                        double mmNumber_mi = rKeyValuePair.Key;
                        MileMarker rMileMarker = rKeyValuePair.Value;
                        
                        if(mmNumber_mi < start_mi)  //find the lowerbound mile marker that contains the start_mi
                        {
                            rKeyValuePairPreStart = rKeyValuePair;
                        }
                        else  //find the upperbound mile marker that contains the start_mi
                        {
                            rKeyValuePairPostStart = rKeyValuePair;
                            break;
                        }
                    }
                    
                    double distX0_m = (rKeyValuePairPostStart.Value.utmEasting_m - rKeyValuePairPreStart.Value.utmEasting_m);
                    double distY0_m = (rKeyValuePairPostStart.Value.utmNorthing_m - rKeyValuePairPreStart.Value.utmNorthing_m);
                    
                    double heading_rad = ((Math.PI/2.0) - Math.Atan2(distY0_m, distX0_m));

                    const double RadToDeg = (180.0/Math.PI);
                    heading_deg = (normalizeAngle0to2PI(heading_rad) * RadToDeg);

                    return true;
                }
            }

            heading_deg = 0;
            return false;
        }

        private bool georeferenceRoadwayStretch(out LocationCollection rOutputLocations, string strRoadwayId, double start_mi, double end_mi)
        {
            rOutputLocations = new LocationCollection();

            if(mrMileMarkers.ContainsKey(strRoadwayId))
            {
                SortedList<double,MileMarker> rSortedMileMarkers = mrMileMarkers[strRoadwayId];

                // Ensure there is at least a linear route (requires minimum two mile marker points)
                if(rSortedMileMarkers.Count() >= 2)
                {
                    // Ensure start_mi is before end_mi
                    if(end_mi < start_mi)
                    {
                        double temp = start_mi;
                        start_mi = end_mi;
                        end_mi = temp;
                    }

                    KeyValuePair<double,MileMarker> rKeyValuePairPreStart = rSortedMileMarkers.First();
                    KeyValuePair<double,MileMarker> rKeyValuePairPostStart = rSortedMileMarkers.First();
                    KeyValuePair<double,MileMarker> rKeyValuePairPreEnd = rSortedMileMarkers.First();
                    KeyValuePair<double,MileMarker> rKeyValuePairPostEnd = rSortedMileMarkers.Last();

                    LocationCollection rMiddleRoutePoints = new LocationCollection();
                    bool hasPreStart = false;
                    bool foundPostStart = false;
                    bool hasPostEnd = false;

                    foreach(KeyValuePair<double,MileMarker> rKeyValuePair in rSortedMileMarkers)
                    {
                        double mmNumber_mi = rKeyValuePair.Key;
                        MileMarker rMileMarker = rKeyValuePair.Value;
                        
                        if(mmNumber_mi < start_mi)  //find the lowerbound mile marker that contains the start_mi
                        {
                            rKeyValuePairPreStart = rKeyValuePair;
                            rKeyValuePairPreEnd = rKeyValuePair;
                            hasPreStart = true;
                        }
                        else  //roadway stretch starts right on a mile marker, or we found the pre-start mile marker
                        {
                            if(hasPreStart && !foundPostStart)
                            {
                                rKeyValuePairPostStart = rKeyValuePair;
                                foundPostStart = true;  //got the mile marker after pre-start
                            }
                        
                            if(mmNumber_mi > end_mi)  //find the upperbound mile marker that contains the end_mi
                            {
                                rKeyValuePairPostEnd = rKeyValuePair;
                                hasPostEnd = true;
                                break;  //exit iteration early to ensure the roadway stretch is minimally contained by mile marker points
                            }
                            else  //this is a middle point within the route
                            {
                                rMiddleRoutePoints.Add(new Location(rMileMarker.lat_deg, rMileMarker.lon_deg));
                            }
                            
                            rKeyValuePairPreEnd = rKeyValuePair;
                        }
                    }

                    if(hasPreStart && foundPostStart)
                    {
                        double distX0_m = (rKeyValuePairPostStart.Value.utmEasting_m - rKeyValuePairPreStart.Value.utmEasting_m);
                        double distY0_m = (rKeyValuePairPostStart.Value.utmNorthing_m - rKeyValuePairPreStart.Value.utmNorthing_m);
                        double distH0_m = Math.Sqrt((distX0_m*distX0_m) + (distY0_m*distY0_m));
                        double distH0_mi = (rKeyValuePairPostStart.Value.mmNumber_mi - rKeyValuePairPreStart.Value.mmNumber_mi);
                        double distH1_pct = ((start_mi - rKeyValuePairPreStart.Value.mmNumber_mi) / distH0_mi);
                        //double distH1_m = (distH0_m * distH1_pct);
                        double distX1_m = (distX0_m * distH1_pct);
                        double distY1_m = (distY0_m * distH1_pct);
                        
                        {
                            const double RadToDeg = (180.0/Math.PI);
                            byte utmZone = rKeyValuePairPreStart.Value.utmZone;
                            byte utmBand = rKeyValuePairPreStart.Value.utmBand;
                            double utmEasting_m = (rKeyValuePairPreStart.Value.utmEasting_m + distX1_m);
                            double utmNorthing_m = (rKeyValuePairPreStart.Value.utmNorthing_m + distY1_m);
                            double lat_rad;  // O
                            double lon_rad;  // O

                            if(mrUtm2Ellipsoid.Utm2LatLon(
                                utmZone,
                                utmBand,
                                utmEasting_m,
                                utmNorthing_m,
                                out lat_rad, 
                                out lon_rad))
                            {
                                double lat_deg = (lat_rad * RadToDeg);
                                double lon_deg = (lon_rad * RadToDeg);
                                rOutputLocations.Add(new Location(lat_deg, lon_deg));
                            }
                        }
                    }

                    foreach(Location rLocation in rMiddleRoutePoints)
                    {
                        rOutputLocations.Add(rLocation);
                    }

                    if(hasPostEnd)
                    {
                        double distX0_m = (rKeyValuePairPostEnd.Value.utmEasting_m - rKeyValuePairPreEnd.Value.utmEasting_m);
                        double distY0_m = (rKeyValuePairPostEnd.Value.utmNorthing_m - rKeyValuePairPreEnd.Value.utmNorthing_m);
                        double distH0_m = Math.Sqrt((distX0_m*distX0_m) + (distY0_m*distY0_m));
                        double distH0_mi = (rKeyValuePairPostEnd.Value.mmNumber_mi - rKeyValuePairPreEnd.Value.mmNumber_mi);
                        double distH1_pct = ((end_mi - rKeyValuePairPreEnd.Value.mmNumber_mi) / distH0_mi);
                        //double distH1_m = (distH0_m * distH1_pct);
                        double distX1_m = (distX0_m * distH1_pct);
                        double distY1_m = (distY0_m * distH1_pct);
                        
                        {
                            const double RadToDeg = (180.0/Math.PI);
                            byte utmZone = rKeyValuePairPreEnd.Value.utmZone;
                            byte utmBand = rKeyValuePairPreEnd.Value.utmBand;
                            double utmEasting_m = (rKeyValuePairPreEnd.Value.utmEasting_m + distX1_m);
                            double utmNorthing_m = (rKeyValuePairPreEnd.Value.utmNorthing_m + distY1_m);
                            double lat_rad;  // O
                            double lon_rad;  // O

                            if(mrUtm2Ellipsoid.Utm2LatLon(
                                utmZone,
                                utmBand,
                                utmEasting_m,
                                utmNorthing_m,
                                out lat_rad, 
                                out lon_rad))
                            {
                                double lat_deg = (lat_rad * RadToDeg);
                                double lon_deg = (lon_rad * RadToDeg);
                                rOutputLocations.Add(new Location(lat_deg, lon_deg));
                            }
                        }
                    }

                    if(rOutputLocations.Count() > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private ControlTemplate getBestSizedCarPushpinTemplate()
        {
            string strBestSizedTemplate;
            
            if(this.BingMap.ZoomLevel >= 20)
            {
                strBestSizedTemplate = "CarPushpinTemplate64";
            }
            else if(this.BingMap.ZoomLevel >= 18)
            {
                strBestSizedTemplate = "CarPushpinTemplate32";
            }
            else if(this.BingMap.ZoomLevel >= 16)
            {
                strBestSizedTemplate = "CarPushpinTemplate24";
            }
            else if(this.BingMap.ZoomLevel >= 14)
            {
                strBestSizedTemplate = "CarPushpinTemplate16";
            }
            else
            {
                strBestSizedTemplate = "CarPushpinTemplate8";
            }
 
            return (this.Resources[strBestSizedTemplate] as ControlTemplate);
        }

        private void BingMap_ViewChangeOnFrame(object sender, MapEventArgs e)
        {
            ControlTemplate rBestTemplate = this.getBestSizedCarPushpinTemplate();

            foreach(UIElement rElement in this.BingMap.Children)
            {
                if(rElement is Pushpin)
                {
                    Pushpin rPin = (rElement as Pushpin);

                    if((rPin.Template != null) && !rPin.Template.Equals(rBestTemplate))
                    {
                        rPin.Template = rBestTemplate;
                    }
                }
            }

            return;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.M:
                {
                    if(mBingMapMode == "AerialWithLabels")
                    {
                        mBingMapMode = "Aerial";
                        this.BingMap.Mode = new AerialMode(false);
                    }
                    else if(mBingMapMode == "Aerial")
                    {
                        mBingMapMode = "Road";
                        this.BingMap.Mode = new RoadMode();
                    }
                    else if(mBingMapMode == "Road")
                    {
                        mBingMapMode = "AerialWithLabels";
                        this.BingMap.Mode = new AerialMode(true);
                    }
                    else
                    {
                        mBingMapMode = "Road";
                        this.BingMap.Mode = new RoadMode();
                    }
                } break;
            }

            return;
        }
    }
}
