using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

using Excel = Microsoft.Office.Interop.Excel;

using PAI.CTIP.Optimization.Model;
using PAI.CTIP.Optimization.Model.Orders;
using PAI.CTIP.Optimization.Services;
using PAI.CTIP.Optimization.Model.Equipment;
using PAI.CTIP.Optimization.Reporting.Services;
using PAI.Core;

using System.Threading;
using System.Diagnostics;
using System.Net;
using Ninject;
using PAI.CTIP.Optimization.Reporting.Model;


namespace Vesco
{
    public partial class Vesco : Form
    {
        private static JobHelper _helper = null;

        public static JobHelper Helper
        {
            get { return _helper ?? (_helper = new JobHelper()); }
        }


        public static IKernel Kernel { get; set; }

        private static void Initialize()
        {
            IKernel kernel = new StandardKernel(new MyModule());
            Kernel = kernel;
        }

        public static TInterface GetService<TInterface>()
        {
            try
            {
                var result = Kernel.Get<TInterface>();
                return result;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        
        int driverRowCount;
        int locationRowCount;
        int routeStopRowCount;

        List<Location> locations = new List<Location>();
        List<Location> badLocations = new List<Location>();

        List<SpecialLocation> locationTimePad = new List<SpecialLocation>();

        List<Driver> drivers = new List<Driver>();
        List<Driver> badDrivers = new List<Driver>();
        List<Location> badDriverLocations = new List<Location>();

        List<Job> jobs = new List<Job>();
        List<Job> badJobs = new List<Job>();
        List<Job> badJobsWithTbaLoc = new List<Job>();
        List<String> badAssJobs = new List<String>();
        List<Location> badJobLocations = new List<Location>();
        String importFileName;
        String carrier;

        private Boolean isReOptimized;
        private List<VescoOrder> vescoOrders;
        private String generatedLocalFile;
        private String generatedLocalFileName;
        private String ftpFileName;
        private String timeStamp;
        private DateTime runDate;
        private String TIME_FORMAT = "hh:mm tt";
        private Boolean shouldGenStats;

        public Form SouthWestForm { get; set; }

        public Vesco()
        {
            InitializeComponent();
        }
        
        public Vesco(List<String> _badOrders, String _importFileName)
        {
            InitializeComponent();
            badAssJobs = _badOrders;
            importFileName = _importFileName;
        }

        public Vesco(Boolean _isReOptimized, List<VescoOrder> _vescoOrders, List<String> _badOrders, String _importFileName, String _carrier)
        {
            InitializeComponent();

            Dictionary<string, string> Properties = Util.GetProperties(Application.StartupPath + "\\settings.properties");
            this.shouldGenStats = Convert.ToBoolean(Properties["stats"]);

            this.isReOptimized = _isReOptimized;
            this.vescoOrders = _vescoOrders;
            this.badAssJobs = _badOrders;
            this.importFileName = _importFileName;
            this.carrier = _carrier;
        }

        public Vesco(Boolean _isReOptimized, List<VescoOrder> _vescoOrders, List<String> _badOrders, String _importFileName, String _carrier, DateTime _runDate)
        {
            InitializeComponent();

            Dictionary<string, string> Properties = Util.GetProperties(Application.StartupPath + "\\settings.properties");
            this.shouldGenStats = Convert.ToBoolean(Properties["stats"]);

            this.runDate = _runDate;
            this.isReOptimized = _isReOptimized;
            this.vescoOrders = _vescoOrders;
            this.badAssJobs = _badOrders;
            this.importFileName = _importFileName;
            this.carrier = _carrier;
        }


        public void performSteps()
        {
            this.performStepOne();
            this.performStepTwo();
            this.performStepThree();
        }
        
        private void performStepOne()
        {
            Initialize();

            Excel.Application excelObj = new Excel.Application();
            Excel.Workbook workbook = null;
            int badRecordIndex = 0;

            try
            {
                this.Cursor = Cursors.WaitCursor;

                reset();

                workbook = excelObj.Workbooks.Open(Application.StartupPath + "\\Templates\\Vesco.xlsx");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error: Could not load Vesco file." + Environment.NewLine +
                    "Location: " + badRecordIndex + "." + Environment.NewLine +
                    "Original error: " + ex.Message);
            }
            finally
            {
                if (workbook != null)
                {
                    Marshal.ReleaseComObject(workbook);
                }
                if (excelObj != null)
                {
                    excelObj.Quit();
                    Marshal.ReleaseComObject(excelObj);
                }
            }

            loadFile();
        }

        private void loadFile()
        {
            Excel.Application excelObj = new Excel.Application();
            Excel.Workbook workbook = null;
            Excel.Worksheet locationWorkSheet = null;
            Excel.Worksheet routeStopWorkSheet = null;
            Excel.Worksheet driverWorkSheet = null;

            Excel.Range locationRange = null;
            Excel.Range driverRange = null;
            Excel.Range routeStopRange = null;

            try
            {

                workbook = excelObj.Workbooks.Open(Application.StartupPath + "\\Templates\\Vesco.xlsx");

                // Locations
                locationWorkSheet = (Excel.Worksheet)workbook.Sheets.get_Item("Location");
                locationRange = locationWorkSheet.UsedRange;
                locationRowCount = locationRange.Rows.Count;
                label5.Text = (locationRowCount - 1).ToString();

                // Drivers
                driverWorkSheet = (Excel.Worksheet)workbook.Sheets.get_Item("Driver");
                driverRange = driverWorkSheet.UsedRange;
                driverRowCount = driverRange.Rows.Count;
                label4.Text = (driverRowCount - 1).ToString();

                // RouteStops
                routeStopWorkSheet = (Excel.Worksheet)workbook.Sheets.get_Item("RouteStop");
                routeStopRange = routeStopWorkSheet.UsedRange;
                routeStopRowCount = routeStopRange.Rows.Count;
                label6.Text = (routeStopRowCount - 1).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error: Could not load Vesco file." + Environment.NewLine +
                    "Original error: " + ex.Message);
            }
            finally
            {
                if (locationRange != null)
                {
                    Marshal.ReleaseComObject(locationRange);
                }
                if (locationWorkSheet != null)
                {
                    Marshal.ReleaseComObject(locationWorkSheet);
                }
                if (driverRange != null)
                {
                    Marshal.ReleaseComObject(driverRange);
                }
                if (driverWorkSheet != null)
                {
                    Marshal.ReleaseComObject(driverWorkSheet);
                }
                if (routeStopRange != null)
                {
                    Marshal.ReleaseComObject(routeStopRange);
                }
                if (routeStopWorkSheet != null)
                {
                    Marshal.ReleaseComObject(routeStopWorkSheet);
                }
                if (workbook != null)
                {
                    Marshal.ReleaseComObject(workbook);
                }
                if (excelObj != null)
                {
                    excelObj.Quit();
                    Marshal.ReleaseComObject(excelObj);
                }
            }
        }

        void reset()
        {
            locations = new List<Location>();
            drivers = new List<Driver>();
            jobs = new List<Job>();
        }

        private void performStepTwo()
        {
            importLocations();
            importDrivers();
            importRouteStops();
        }

        void importLocations()
        {
            Excel.Application locationApp = new Excel.Application();
            Excel.Workbook locationBook = null;
            Excel.Worksheet locationWorkSheet = null;
            Excel.Range locationRange = null;
            Location loc;
            Location badLoc;

            try
            {
                locationApp = new Excel.Application();
                locationBook = locationApp.Workbooks.Open(Application.StartupPath + "\\Templates\\Vesco.xlsx");
                locationWorkSheet = (Excel.Worksheet)locationBook.Sheets.get_Item("Location");
                locationRange = locationWorkSheet.UsedRange;
                locationRowCount = locationRange.Rows.Count;

                for (int i = 2; i <= locationRowCount; i++)  // Start at the second row and skip the header
                {
                    try
                    {
                        loc = new Location();
                        if (locationRange[i, 1].Value2 == null)
                        {
                            break;  //effin blank record
                        }
                        loc.DisplayName = locationRange[i, 1].Value2.ToString();
                        loc.Street = locationRange[i, 2].Value2.ToString();
                        loc.Zip = locationRange[i, 3].Value2.ToString();
                        loc.Latitude = Convert.ToDouble(locationRange[i, 4].Value2);
                        loc.Longitude = Convert.ToDouble(locationRange[i, 5].Value2);
                        loc.LiveLoadPad = Convert.ToInt16(locationRange[i, 6].Value2);
                        loc.OrderType = (locationRange[i, 7].Value2 == null ? 3 : Convert.ToInt16(locationRange[i, 7].Value2));
                        loc.LongHaul = Convert.ToBoolean(locationRange[i, 8].Value2);
                        loc.ShortHaul = Convert.ToBoolean(locationRange[i, 9].Value2);
                        locations.Add(loc);
                        label10.Text = locations.Count.ToString();
                    }
                    catch (Exception ex)
                    {
                        Console.Out.WriteLine(ex.Message);
                        badLoc = new Location();
                        badLoc.DisplayName = "Location #" + i.ToString() + " is invalid.";
                        badLocations.Add(badLoc);
                        Console.Out.WriteLine(badLoc.DisplayName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Location Import Error: Could not import the Vesco file. Original error: " + ex.Message);
            }
            finally
            {
                if (locationRange != null)
                {
                    Marshal.ReleaseComObject(locationRange);
                }
                if (locationWorkSheet != null)
                {
                    Marshal.ReleaseComObject(locationWorkSheet);
                }
                if (locationBook != null)
                {
                    locationBook.Close(false);
                    Marshal.ReleaseComObject(locationBook);
                }
                if (locationApp != null)
                {
                    locationApp.Quit();
                    Marshal.ReleaseComObject(locationApp);
                }
            }
        }

        void importDrivers()
        {
            Excel.Application driverApp = new Excel.Application();
            Excel.Workbook driverBook = null;
            Excel.Worksheet driverWorkSheet = null;
            Excel.Range driverRange = null;

            object misValue = System.Reflection.Missing.Value;

            Driver driver;
            String tempLocName;
            Double earliestStartTime;
            DateTime conv;

            try
            {
                driverApp = new Excel.Application();
                driverBook = driverApp.Workbooks.Open(Application.StartupPath + "\\Templates\\Vesco.xlsx");
                driverWorkSheet = (Excel.Worksheet)driverBook.Sheets.get_Item("Driver");
                driverRange = driverWorkSheet.UsedRange;

                // Sort based upon the driving hours, available hours, and rating
/*                driverRange.Sort(
                    driverRange.Columns[2, Type.Missing], // driving hours
                    Excel.XlSortOrder.xlDescending, // descending
                    driverRange.Columns[3, Type.Missing], // available hours
                    Type.Missing, // ??
                    Excel.XlSortOrder.xlDescending, // descending
                    driverRange.Columns[7, Type.Missing], // rating
                    Excel.XlSortOrder.xlAscending, // ascending
                    Excel.XlYesNoGuess.xlYes, Type.Missing, Type.Missing,
                    Excel.XlSortOrientation.xlSortColumns,
                    Excel.XlSortMethod.xlPinYin,
                    Excel.XlSortDataOption.xlSortNormal,
                    Excel.XlSortDataOption.xlSortNormal,
                    Excel.XlSortDataOption.xlSortNormal);
*/
                driverRowCount = driverRange.Rows.Count;
                Location badDriverLocation;

                for (int i = 2; i <= driverRowCount; i++) // Start at the second row and skip the header
                {
                    driver = new Driver();
                    driver.DisplayName = driverRange[i, 1].Value2.ToString();
                    driver.AvailableDrivingHours = Convert.ToDouble(driverRange[i, 2].Value2);
                    driver.AvailableDutyHours = Convert.ToDouble(driverRange[i, 3].Value2);

                    earliestStartTime = Convert.ToDouble(driverRange[i, 4].Value2);
                    conv = DateTime.FromOADate(earliestStartTime);
                    driver.EarliestStartTime = new TimeSpan(conv.Hour, conv.Minute, conv.Second);

                    tempLocName = driverRange[i, 5].Value2.ToString();
                    driver.StartingLocation = locations.Find(
                        delegate(Location lc)
                        {
                            return lc.DisplayName == tempLocName;
                        }
                    );

                    driver.IsHazmatEligible = Convert.ToBoolean(driverRange[i, 6].Value);

                    //Where is Overweight?
                    //driver.IsOverweightEligible = Convert.ToBoolean(driverRange[i, 7].Value);
                    
                    driver.OrderType = Convert.ToInt16(driverRange[i, 9].Value2);
                    driver.IsLongHaulEligible = Convert.ToBoolean(driverRange[i, 10].Value);
                    driver.IsShortHaulEligible = Convert.ToBoolean(driverRange[i, 11].Value);


                    if (driver.StartingLocation == null)
                    {
                        badDrivers.Add(driver);

                        badDriverLocation = new Location();
                        badDriverLocation.DisplayName = tempLocName;
                        badDriverLocations.Add(badDriverLocation);
                        
                        Console.Out.WriteLine("Warning, " + tempLocName + " is not a valid Starting Locaiton for driver " + driver.DisplayName);
                    }
                    else
                    {
                        drivers.Add(driver);
                    }
                    label11.Text = drivers.Count.ToString();
                }

                driverBook.Close(false, misValue, misValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Driver Import Error: Could not import the Vesco file. Original error: " + ex.Message);
            }
            finally
            {
                if (driverRange != null)
                {
                    Marshal.ReleaseComObject(driverRange);
                }
                if (driverWorkSheet != null)
                {
                    Marshal.ReleaseComObject(driverWorkSheet);
                }
                if (driverBook != null)
                {
//                    driverBook.Close(false);
                    Marshal.ReleaseComObject(driverBook);
                }
                if (driverApp != null)
                {
                    driverApp.Quit();
                    Marshal.ReleaseComObject(driverApp);
                }
            }
            label16.Text = badDrivers.Count.ToString();
        }

        void importRouteStops()
        {
            Excel.Application routeStopApp = new Excel.Application();
            Excel.Workbook routeStopBook = null;
            Excel.Worksheet routeStopWorkSheet = null;
            Excel.Range routeStopRange = null;

            try
            {
                routeStopApp = new Excel.Application();
                routeStopBook = routeStopApp.Workbooks.Open(Application.StartupPath + "\\Templates\\Vesco.xlsx");
                routeStopWorkSheet = (Excel.Worksheet)routeStopBook.Sheets.get_Item("RouteStop");
                routeStopRange = routeStopWorkSheet.UsedRange;
                routeStopRowCount = routeStopRange.Rows.Count;
                int routeStopColCount = routeStopRange.Columns.Count;

                Job job = new Job();
                List<RouteStop> routeStop = new List<RouteStop>();
                int invalidRouteStops = 0;
                String tempLocName;
                String temLocAddress = null;
                String tempCurrJobName;
                String tempPrevJobName;
                StopAction tempStopAction;
                List<StopAction> stopActions = StopActions.Actions.Cast<StopAction>().ToList();
                Boolean isBadJob = false;
                Boolean isBadJobWithTbaLoc = false;
                Location tempLoc;
                Location badJobLocation;
                int tempHighestRank = 3;
                
                try
                {
                    for (int i = 2; i <= routeStopRowCount; i++) // Start at the second row and skip the header
                    {

                        tempCurrJobName = routeStopRange[i, 1].Value2.ToString();
                        tempPrevJobName = routeStopRange[i - 1, 1].Value2.ToString();
                        // Create New Job if the current record doesn't have the same Job Number as the last record
                        if (tempCurrJobName != tempPrevJobName)
                        {
                            job = new Job();
                            routeStop = new List<RouteStop>();
                            job.DisplayName = routeStopRange[i, 1].Value2.ToString();
                            job.Id = jobs.Count + 1;
                            job.EquipmentConfiguration = new EquipmentConfiguration();
                            job.IsHazmat = Convert.ToBoolean(routeStopRange[i, 9].Value);

                            tempHighestRank = 3;
                            job.OrderType = tempHighestRank;
                        }

                        tempLocName = routeStopRange[i, 4].Value2.ToString();
                        if (routeStopRange[i, 5].Value2 != null)
                        {
                            temLocAddress = routeStopRange[i, 5].Value2.ToString();

                            tempLoc = locations.Find(
                                delegate(Location lc)
                                {
                                    return lc.DisplayName == tempLocName && lc.Street == temLocAddress;
                                }
                            );
                        }
                        else
                        {
                            tempLoc = locations.Find(
                                delegate(Location lc)
                                {
                                    return lc.DisplayName == tempLocName;
                                }
                            );
                        }

                        if (tempLoc == null)
                        {
                            Console.Out.WriteLine("     Warning, " + tempLocName + "," + temLocAddress + " is not a valid Starting Locaiton");
                            badJobLocation = new Location();
                            badJobLocation.DisplayName = tempLocName;
                            badJobLocation.Street = temLocAddress;
                            badJobLocations.Add(badJobLocation);
                            isBadJob = true;
                        }
                        else if ("TBA".Equals(tempLoc.DisplayName))
                        {
                            Console.Out.WriteLine("  Warning, " + tempLocName + " is TBA");
                            isBadJobWithTbaLoc = true;

                        }
                        else {
                            if (tempHighestRank > tempLoc.OrderType)
                            {
                                job.OrderType = tempLoc.OrderType;
                                tempHighestRank = tempLoc.OrderType;
                            }
                            job.IsLongHaul = tempLoc.LongHaul;
                            job.IsShortHaul = tempLoc.ShortHaul;
                        }

                        string stopAction = routeStopRange[i, 3].Value2.ToString();
                        string tempSaId = (stopAction.Split('-')).First();

                        tempStopAction = stopActions.Find(
                            delegate(StopAction sa)
                            {
                                return sa.Id == Convert.ToInt16(tempSaId);
                            }
                        );

                        int tempStopDelay = 0;
                        if (routeStopRange[i, 6].Value2 != null)
                        {
                            tempStopDelay = Convert.ToInt16(routeStopRange[i, 6].Value2);
                        }

                        Double windowStartTime = 0.0;
                        TimeSpan windowStartTimeSpan = Helper.GetTimeSpan(0, 0);
                        DateTime windowStartTimeConv;
                        if (routeStopRange[i, 7].Value2 != null)
                        {
                            windowStartTime = Convert.ToDouble(routeStopRange[i, 7].Value2);
                            windowStartTimeConv = DateTime.FromOADate(windowStartTime);
                            windowStartTimeSpan = new TimeSpan(
                                windowStartTimeConv.Hour, windowStartTimeConv.Minute, windowStartTimeConv.Second);
                        }

                        Double windowEndTime = 0.0;
                        TimeSpan windowEndTimeSpan = Helper.GetTimeSpan(24, 0);
                        DateTime windowEndTimeConv;
                        if (routeStopRange[i, 8].Value2 != null)
                        {
                            windowEndTime = Convert.ToDouble(routeStopRange[i, 8].Value2);
                            windowEndTimeConv = DateTime.FromOADate(windowEndTime);
                            windowEndTimeSpan = new TimeSpan(
                                windowEndTimeConv.Hour, windowEndTimeConv.Minute, windowEndTimeConv.Second);
                        }
                            
                        // Add Route Stops
                        routeStop.Add(
                            Helper.CreateRouteStop(
                                job,
                                tempStopAction,
                                tempLoc,
                                Helper.GetTimeSpan(tempStopDelay),
                                windowStartTimeSpan,
                                windowEndTimeSpan
                            )
                        );

                        // A new job is coming up or this is the last record in the collection, add this job to the jobs list
                        if (i == routeStopRowCount || routeStopRange[i, 1].Value2.ToString() != routeStopRange[i + 1, 1].Value2.ToString())
                        {
                            job.RouteStops = routeStop;
                            if (isBadJob)
                            {
                                badJobs.Add(job);
                                isBadJob = false;
                                label8.Text = badJobs.Count.ToString();
                            }
                            else if (isBadJobWithTbaLoc)
                            {
                                badJobsWithTbaLoc.Add(job);
                                isBadJobWithTbaLoc = false;
                            }
                            else
                            {
                                jobs.Add(job);
                                label9.Text = jobs.Count.ToString(); 
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.Message);
                    Console.Out.WriteLine(ex.StackTrace);

                    invalidRouteStops++;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Route Stop Import Error: Could not import the Vesco file. Original error: " + ex.Message);
            }
            finally
            {
                if (routeStopRange != null)
                {
                    Marshal.ReleaseComObject(routeStopRange);
                }
                if (routeStopWorkSheet != null)
                {
                    Marshal.ReleaseComObject(routeStopWorkSheet);
                }
                if (routeStopBook != null)
                {
                    Marshal.ReleaseComObject(routeStopBook);
                }
                if (routeStopApp != null)
                {
                    routeStopApp.Quit();
                    Marshal.ReleaseComObject(routeStopApp);
                }
            }
        }

        private void performStepThree()
        {
            // initialize the Optimization service
            var optimizer = GetService<IDrayageOptimizer>();
            optimizer.Initialize();

            if (jobs.Count > 0)
            {
                // build the solution
//                var solution = optimizer.BuildSolution(drivers, drivers[0], jobs);

                /*************************  New PAI Code ********************/
                List<Job> sanitizedJobs = new List<Job>();
                var routeSanitizer = GetService<IRouteSanitizer>();

                // sanitize all the jobs
                foreach (var tempJob in jobs)
                {
                    routeSanitizer.PrepareJob(tempJob);
                    sanitizedJobs.Add(tempJob);
                }

                Console.WriteLine();
                Console.WriteLine(string.Format("Number of drivers->{0}", drivers.Count));
                foreach (var d in drivers) 
                {
                    //Spit out the Drivers
                    Console.WriteLine(string.Format(
                        "\tdriver->{0}|AvDrHrs->{1}|AvDuHrs->{2}|ErlStTm->{3}|StLoc->{4}|GeoCode->({9},{10})|Haz->{5}|LngHaul->{6}|ShrtHaul->{7}|OrderType->{8}", 
                        d.DisplayName, 
                        d.AvailableDrivingHours, 
                        d.AvailableDutyHours,
                        (runDate + d.EarliestStartTime).ToString(TIME_FORMAT), 
                        d.StartingLocation.DisplayName,
                        d.IsHazmatEligible, 
                        d.IsLongHaulEligible, 
                        d.IsShortHaulEligible, 
                        d.OrderType,
                        d.StartingLocation.Longitude,
                        d.StartingLocation.Latitude));
                }
                Console.WriteLine();

                Console.WriteLine();
                Console.WriteLine(string.Format("Number of sanitized jobs->{0}", sanitizedJobs.Count));
                foreach (var j in sanitizedJobs)
                {
                    //Spit out the Jobs
                    Console.WriteLine(string.Format(
                        "\tJob->{0}|Haz->{1}|LngHaul->{2}|ShrtHaul->{3}|Priority->{4}|OrderType->{5}",
                        j.DisplayName,
                        j.IsHazmat,
                        j.IsLongHaul,
                        j.IsShortHaul,
                        j.Priority,
                        j.OrderType));

                    foreach(var rs in j.RouteStops)
                    {
                        //Spit out the RouteStops
                        Console.WriteLine(string.Format(
                            "\t\tStopAct->{0}|LocName->{1}|GeoCode->({6},{7})|StopDel->{3}|WinSt->{4}|WinEnd->{5}",
                            rs.StopAction.Name,
                            rs.Location.DisplayName,
                            rs.Location.Street,
                            rs.StopDelay,
                            rs.WindowStart,
                            rs.WindowEnd,
                            rs.Location.Longitude,
                            rs.Location.Latitude));
                    }
                }
                Console.WriteLine();

                var solution = optimizer.BuildSolution(
                    drivers, 
                    new Driver()
                    {
                        DisplayName = "Placeholder Driver",
                        StartingLocation = drivers.First().StartingLocation
                    }, 
                    sanitizedJobs
                );

                ReportingService reportingService;
                SolutionPerformanceStatistics solutionPerformance = new SolutionPerformanceStatistics();
                string solutionPerformanceReport;

                if (this.shouldGenStats)
                {
                    // generate the statistics
                    reportingService = (ReportingService)GetService<IReportingService>();
                    solutionPerformance = reportingService.GetSolutionPerformanceStatistics(solution);
                    solutionPerformanceReport = reportingService.GetSolutionPerformanceStatisticsReport(solution);
                }

                StringBuilder sbFtpData = new StringBuilder();
                timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
                String prePend = "O_";
                if (isReOptimized)
                {
                    prePend = "R_";
                }

                generatedLocalFileName = prePend + carrier + "_" + timeStamp + ".csv";
                generatedLocalFile = Application.StartupPath + "\\Templates\\Plans\\" + generatedLocalFileName;
                //            File.Delete(optimizedFileName);

                using (StreamWriter outfile = new StreamWriter(generatedLocalFile))
                {
                    outfile.WriteLine("Optimization plan for file(s)," + importFileName);
                    outfile.WriteLine("Route solutions created:," + solution.RouteSolutions.Count);
                    outfile.WriteLine("Total Time:," +
                        Math.Truncate(solution.RouteStatistics.TotalTime.TotalHours) + " hours and " +
                        solution.RouteStatistics.TotalTime.Minutes + " minutes");

                    if (AssociatedConverter.totalOrdersCount > 0)
                    {
                        outfile.WriteLine("Total Jobs:," + AssociatedConverter.totalOrdersCount);
                    }
                    if (SouthwestConverter.totalOrdersCount > 0)
                    {
                        outfile.WriteLine("Total Jobs:," + SouthwestConverter.totalOrdersCount);
                    }
                    if (badAssJobs.Count() > 0)
                    {
                        outfile.Write(badAssJobs.Count() + " job(s) that couldn't be mapped:  ");
                        foreach (var ba in badAssJobs)
                        {
                            outfile.Write("," + ba);
                        }
                        outfile.WriteLine();
                    }
                    if (badJobs.Count() > 0)
                    {
                        outfile.Write(badJobs.Count() + " job(s) with invalid locations:  ");
                        foreach (var j in badJobs)
                        {
                            outfile.Write("," + j.DisplayName);
                        }
                        outfile.WriteLine();
                    }
                    if (badJobLocations.Count() > 0)
                    {
                        outfile.Write(badJobLocations.Count() + " invalid locations:  ");
                        foreach (var j in badJobLocations)
                        {
                            outfile.Write("," + j.DisplayName + " " + j.Street);
                        }
                        outfile.WriteLine();
                    }
                    if (badJobsWithTbaLoc.Count() > 0)
                    {
                        outfile.Write(badJobsWithTbaLoc.Count() + " job(s) with TBA locations:  ");
                        foreach (var bj in badJobsWithTbaLoc)
                        {
                            outfile.Write("," + bj.DisplayName);
                        }
                        outfile.WriteLine();
                    }
                    if (badDrivers.Count() > 0)
                    {
                        outfile.Write(badDrivers.Count() + " drivers(s) with invalid locations:  ");
                        foreach (var d in badDrivers)
                        {
                            outfile.Write("," + d.DisplayName);
                        }
                        outfile.WriteLine();
                    }
                    if (solution.UnassignedJobNodes.Count > 0)
                    {
                        outfile.Write(solution.UnassignedJobNodes.Count + " unassigned job(s):  ");
                        foreach (var unassignedJobNode in solution.UnassignedJobNodes)
                        {
                            outfile.Write("," + unassignedJobNode);
                        }
                        outfile.WriteLine();
                    }

                    // Give a summary of all the drivers
                    foreach (var routeSolution in solution.RouteSolutions)
                    {
                        outfile.Write("Driver: " + routeSolution.DriverNode.Driver.DisplayName +
                            " has " +
                            routeSolution.Nodes.Count + " jobs assigned,");

                        foreach (var nds in routeSolution.Nodes)
                        {
                            outfile.Write(nds + ",");
                        }
                        outfile.WriteLine();
                    }

                    // count, get, and display all the drivers with 0 jobs assigned
                    StringBuilder zeroJobDrivers = new StringBuilder();
                    var assignedDrivers = solution.RouteSolutions.Select(x => x.DriverNode.Driver);
                    var zeroJDCounter = drivers.Count - solution.RouteSolutions.Select(x => x.DriverNode).Distinct().Count();
                    var unassignedDrivers =
                        drivers
                            .Where(x => !assignedDrivers.Contains(x))
                            .Select(y => y.DisplayName)
                            .Aggregate(string.Empty, (current,
                                unassignedDriver) => current + (", " + unassignedDriver));
                    if (unassignedDrivers.StartsWith(", "))
                        unassignedDrivers = unassignedDrivers.Substring(2);
                    zeroJobDrivers.Append(unassignedDrivers);
                    outfile.WriteLine(zeroJDCounter + " drivers with 0 jobs assigned," + zeroJobDrivers + Environment.NewLine);

                    VescoOrder vescoOrder;
                    Boolean lookupVesco;
                    int routeCount = 0;
                    StringBuilder prevFtpLine = new StringBuilder();
                    StringBuilder ftpLine = new StringBuilder();
                    foreach (var routeSolution in solution.RouteSolutions)
                    {
                        routeCount++;

                        TruckPerformanceStatistics driverStatistics = new TruckPerformanceStatistics();
                        if (this.shouldGenStats && solutionPerformance != null)
                        {
                            driverStatistics =
                                solutionPerformance.TruckStatistics.FirstOrDefault(
                                    p => p.Key.DriverNode.Driver.DisplayName == routeSolution.DriverNode.Driver.DisplayName).Value;
                        }

                        outfile.WriteLine();
                        outfile.WriteLine("ROUTE #" + routeCount + " Summary");
                        outfile.WriteLine("Assigned to Driver:," + routeSolution.DriverNode.Driver.DisplayName);
                        outfile.WriteLine(string.Format("Driver Start Time/Location/Ranking:,{0}/{1}/{2}",  
                            (runDate + routeSolution.DriverNode.Driver.EarliestStartTime).ToString(TIME_FORMAT),
                            routeSolution.DriverNode.Driver.StartingLocation.DisplayName,
                            routeSolution.DriverNode.Driver.OrderType));
                        outfile.WriteLine("Travel Distance:," + routeSolution.RouteStatistics.TotalTravelDistance.ToString("#.#") + " miles");
                        outfile.WriteLine("Estimated Total Time:," +
                            routeSolution.RouteStatistics.TotalTime.Hours + " hours and " +
                            routeSolution.RouteStatistics.TotalTime.Minutes + " minutes");
                        outfile.WriteLine("Estimated Travel Time:," +
                            routeSolution.RouteStatistics.TotalTravelTime.Hours + " hours and " +
                            routeSolution.RouteStatistics.TotalTravelTime.Minutes + " minutes");
                        outfile.Write(routeSolution.Nodes.Count + " jobs:,");

                        foreach (var nds in routeSolution.Nodes)
                        {
                            outfile.Write(nds + ",");
                        }

                        outfile.WriteLine(Environment.NewLine + Environment.NewLine +
                            "ROUTE #" + routeCount + " Detail,Driver: " + routeSolution.DriverNode.Driver.DisplayName);
                        outfile.WriteLine("Stop #,ORDER #,ACTION,LOCATION,WINDOW START, WINDOW END," +
                            (this.shouldGenStats ? "LeavePrevStop,Distance,TotTravTime,ETA,Wait,ETD," : "") +
                            "LEG TYPE,HAZARDOUS,ORIGINAL LEG #,CONTAINER,STEAMSHIP LINE,LIVE LOAD,DISPATCH SEQUENCE,OVERSIZED,RANKING");

                        int nodeCount = 0;
                        int stopCount = 0;
                        
                        foreach (var node in routeSolution.AllNodes)
                        {
                            foreach (var routeStop in node.RouteStops)
                            {
//                                Console.WriteLine(string.Format("Name={0},Priority={1}", routeStop.StopAction.Name, node.Priority));
                                outfile.Write(string.Format("{0},", stopCount + 1));
                                if (nodeCount == 0 || nodeCount + 1 == routeSolution.AllNodes.Count)
                                {
                                    outfile.Write(",");
                                    lookupVesco = false;
                                }
                                else
                                {
                                    outfile.Write(node + ",");
                                    lookupVesco = true;
                                }
                                outfile.Write(routeStop.StopAction.Name + ",");
                                outfile.Write(routeStop.Location.DisplayName.Replace(',', ' ') + ",");
                                if (lookupVesco)
                                {
                                    vescoOrder = vescoOrders.Find(
                                        delegate(VescoOrder vo)
                                        {
                                            return vo.Job == node.ToString() &&
                                                vo.Location == routeStop.Location.DisplayName;
                                        }
                                    );
                                    
                                    // just blank out the start time
                                    //if (vescoOrder.WindowStart != null && vescoOrder.WindowEnd != null)
                                    //{
                                    //    if (vescoOrder.WindowStart.Equals(vescoOrder.WindowEnd))
                                    //    {
                                    //        DateTime dt = DateTime.Parse(vescoOrder.WindowEnd, System.Globalization.CultureInfo.CurrentCulture);
                                    //        vescoOrder.WindowEnd = dt.AddHours(1).ToString("HH:mm:ss"); ;
                                    //    }
                                    //}

//                                    outfile.Write(vescoOrder.WindowStart + ",");
//                                    outfile.Write(vescoOrder.WindowEnd + ",");

                                    outfile.Write((runDate + routeStop.WindowStart).ToString(TIME_FORMAT) + ",");
                                    outfile.Write((runDate + routeStop.WindowEnd).ToString(TIME_FORMAT) + ",");

                                    // Write out the statistics
                                    if (this.shouldGenStats)
                                    {
                                        if (stopCount != 0 && !(stopCount >= driverStatistics.RouteSegmentStatistics.Count))
                                        {
                                            var segmentStatistics = driverStatistics.RouteSegmentStatistics[stopCount - 1];
                                            outfile.Write(string.Format("{0},", (runDate + segmentStatistics.StartTime).ToString(TIME_FORMAT)));
                                            outfile.Write(string.Format("{0} mi,", (segmentStatistics.Statistics.TotalTravelDistance != 0 ?
                                                segmentStatistics.Statistics.TotalTravelDistance.ToString("#.#") : "0")));
                                            outfile.Write(string.Format("{0} min,", (segmentStatistics.Statistics.TotalTravelTime.TotalMinutes != 0 ?
                                                segmentStatistics.Statistics.TotalTravelTime.TotalMinutes.ToString("#.#") : "0")));
                                            outfile.Write(string.Format("{0},", (runDate + segmentStatistics.StartTime.Add(
                                                segmentStatistics.Statistics.TotalTravelTime).Add(
                                                segmentStatistics.Statistics.TotalWaitTime)).ToString(TIME_FORMAT)));
                                            if (routeStop.StopDelay.HasValue)
                                            {
                                                //  Not sure why I'm getting this from the routeStop and not the vescoOrder
                                                outfile.Write(string.Format("{0} min,", routeStop.StopDelay.Value.TotalMinutes));
                                            }
                                            outfile.Write(string.Format("{0},", (runDate + segmentStatistics.EndTime).ToString(TIME_FORMAT)));
                                        }
                                    }

                                    outfile.Write(vescoOrder.OriginalLegType + ",");
                                    outfile.Write(vescoOrder.Hazardous + ",");
                                    outfile.Write(vescoOrder.OriginalLegNumber + ",");
                                    outfile.Write(vescoOrder.ContainerNumber + ",");
                                    outfile.Write(vescoOrder.SteamshipLine + ",");
                                    outfile.Write(vescoOrder.LiveLoad + ",");
                                    outfile.Write(vescoOrder.DispatcherSequence + ",");
                                    outfile.Write(vescoOrder.Overweight + ",");

                                    var currentJob = jobs.FirstOrDefault(j => j.DisplayName == node.ToString());
                                    outfile.Write(currentJob.OrderType + ",");

                                    //for FtpFile
                                    ftpLine.Append(vescoOrder.OriginalLegNumber + ",");
                                    ftpLine.Append(routeSolution.DriverNode.Driver.DisplayName + ",");
                                    ftpLine.Append(node + ",");
                                    ftpLine.Append(vescoOrder.DispatcherSequence);

                                    if (!ftpLine.ToString().Equals(prevFtpLine.ToString()))
                                    {
                                        sbFtpData.Append(ftpLine + Environment.NewLine);
                                        prevFtpLine.Clear();
                                        prevFtpLine.Append(ftpLine);
                                    }
                                    ftpLine.Clear();
                                }
                                else
                                {
                                    outfile.Write(",,");
                                    if (shouldGenStats)
                                    {
                                        if (stopCount == 0)
                                        {
                                            var segmentStatistics = driverStatistics.RouteSegmentStatistics[0]; // First One
                                            outfile.Write(string.Format(",,,,,{0},", (runDate + segmentStatistics.StartTime).ToString(TIME_FORMAT)));
                                        }
                                        else
                                        {
                                            var segmentStatistics = driverStatistics.RouteSegmentStatistics[driverStatistics.RouteSegmentStatistics.Count - 1]; // Last One
                                            outfile.Write(string.Format("{0},", (runDate + segmentStatistics.StartTime).ToString(TIME_FORMAT)));
                                            outfile.Write(string.Format("{0} mi,", (segmentStatistics.Statistics.TotalTravelDistance != 0 ?
                                                segmentStatistics.Statistics.TotalTravelDistance.ToString("#.#") : "0")));
                                            outfile.Write(string.Format("{0} min,", (segmentStatistics.Statistics.TotalTravelTime.TotalMinutes != 0 ?
                                                segmentStatistics.Statistics.TotalTravelTime.TotalMinutes.ToString("#.#") : "0")));
                                            outfile.Write(string.Format("{0},", (runDate + segmentStatistics.StartTime.Add(
                                                segmentStatistics.Statistics.TotalTravelTime).Add(
                                                segmentStatistics.Statistics.TotalWaitTime)).ToString(TIME_FORMAT)));
                                        }
                                    }
                                }

                                outfile.WriteLine();
                                stopCount++;
                            }
                            nodeCount++;
                        }
                        outfile.WriteLine();
                        Console.WriteLine("\n");
                    }

                    // For the FTP File
                    ftpFileName = "F" + prePend + carrier + "_" + timeStamp + ".csv";
                    using (StreamWriter ftpFile = new StreamWriter(
                        Application.StartupPath + "\\Templates\\" + ftpFileName))
                    {
                        ftpFile.WriteLine(sbFtpData);
                    }
                }

                this.ftpFileToLeidos();
                this.ftpPlanToLeidos();
                this.ftpFileToTrinium();
            }
            else
            {
                String s = "There are no route stops to optimize.";
                MessageBox.Show(s);
                label18.Text = s;
                this.Cursor = Cursors.Default;
            }

        }
        
        private void Vesco_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "EXCEL.EXE";
            startInfo.Arguments = "\"" + generatedLocalFile + "\""; //extra escaped double quotes are needed for file paths with spaces in them
            Process.Start(startInfo);
        }

        public void ftpFileToLeidos()
        {
            /// Set the file name and path
            String localFilePath = Application.StartupPath + "\\Templates\\";

            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://72.44.215.114/out/" + ftpFileName);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential("FRATIS", "OptimizeMyLoad123!@#");

            // Copy the contents of the file to the request stream.
            StreamReader sourceStream = new StreamReader(localFilePath + ftpFileName);
            byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            sourceStream.Close();
            request.ContentLength = fileContents.Length;

            try
            {
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                label7.Text = ftpFileName;
                label18.Text = "FTP File to Leidos success.";
            }
            catch (WebException we)
            {
                Console.WriteLine(we.Message);
                MessageBox.Show("Cannot connect to the FTP Leidos server.  Please, make sure you are connected to the internet and try again.");
                label7.Text = ftpFileName;
                label18.Text = "FTP File to Leidos failed.";
            }
        }

        public void ftpPlanToLeidos()
        {
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://72.44.215.114/out/" + generatedLocalFileName);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential("FRATIS", "OptimizeMyLoad123!@#");

            // Copy the contents of the file to the request stream.
            StreamReader sourceStream = new StreamReader(generatedLocalFile);
            byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            sourceStream.Close();
            request.ContentLength = fileContents.Length;

            try
            {
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                label7.Text = ftpFileName;
                label18.Text = "FTP File to Leidos success.";

            }
            catch (WebException we)
            {
                Console.WriteLine(we.Message);
                MessageBox.Show("Cannot connect to the FTP Leidos server.  Please, make sure you are connected to the internet and try again.");
                label7.Text = ftpFileName;
                label18.Text = "FTP File to Leidos failed.";
            }
        }

        public void ftpFileToTrinium()
        {
            /// Set the file name and path
            String localFilePath = Application.StartupPath + "\\Templates\\";

            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://72.44.215.114/in/" + ftpFileName);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = new NetworkCredential("FRATIS", "OptimizeMyLoad123!@#");

            // Copy the contents of the file to the request stream.
            StreamReader sourceStream = new StreamReader(localFilePath + ftpFileName);
            byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            sourceStream.Close();
            request.ContentLength = fileContents.Length;

            try
            {
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                label7.Text = ftpFileName;
                label18.Text = "FTP File to Trinium success.";
            }
            catch (WebException we)
            {
                Console.WriteLine(we.Message);
                MessageBox.Show("Cannot connect to the FTP Trinium server.  Please, make sure you are connected to the internet and try again.");
                label7.Text = ftpFileName;
                label18.Text = "FTP File to Trinium failed.";
            }
            finally
            {
                button1.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> Properties = Util.GetProperties(Application.StartupPath + "\\settings.properties");
            Properties["carrier"] = "0";
            Util.saveProperties(Properties);
        }
    }
}
