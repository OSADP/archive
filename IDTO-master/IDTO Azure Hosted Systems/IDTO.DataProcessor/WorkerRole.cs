using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using IDTO.Data;
using IDTO.DataProcessor.Common;
using IDTO.Common;
using IDTO.DataProcessor.TConnectMonitor;
using IDTO.DataProcessor.TravelerMonitor;
using IDTO.DataProcessor.VehicleLocationMonitor;
using IDTO.Entity.Models;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using IDTO.Common.Storage;
using Ninject;
using Repository;
using Repository.Providers.EntityFramework;
using LogLevel = Microsoft.WindowsAzure.Diagnostics.LogLevel;
using IDTO.BusScheduleInterface;

namespace IDTO.DataProcessor
{
    public class WorkerRole : RoleEntryPoint
    {

        const int DefaultWorkerSleepTime = 300;

        /// <summary>
        /// Instance for outputting trace data messages.
        /// </summary>
        public static IIdtoDiagnostics Diagnostics;
        private const string WadConnectionString = "Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString";

        private bool _wereRunning = true;
        private TConnectMonitorManager _tConnectMonitorMgr;
        private TravelerMonitorManager _travelerMonitorMgr;
        private VehicleLocationMonitorManager _vehicleLocMonitorMgr;

        // This is  list of configuration settings (in ServiceConfiguration.cscfg) that if changed
        // should not result in the role being recycled.  This is implemented in the RoleEnvironmentChanging
        // and RoleEnvironmentChanged callbacks below
        private static string[] _exemptConfigurationItems = new[] { "ConfigTrace", "MainTrace" };

        public static IKernel Kernel = new StandardKernel();

        public override void Run()
        {
            try
            {
                // This is a sample worker implementation. Replace with your logic.
                Trace.TraceInformation("IDTO.DataProcessor entry point called", "Information");

                SetupIoCBindings();

                bool runTConnectMonitor = Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("RunTConnectMonitor"));
                bool runVehicleLocationMonitor = Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("RunVehicleLocationMonitor"));
                bool runTravelerMonitor = Convert.ToBoolean(RoleEnvironment.GetConfigurationSettingValue("RunTravelerMonitor"));

                if (runTConnectMonitor)
                {
                    Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Information, TraceEventId.TraceGeneral, "Starting TConnect Processor Manager");
                    _tConnectMonitorMgr = new TConnectMonitorManager(GetBusSchedules());
                    _tConnectMonitorMgr.ProcessWorker.SecondsBetweenIterations = GetSleepTimeForWorker("TConnectMonitor");
                    _tConnectMonitorMgr.Start();
                }

                if(runTravelerMonitor)
                {

                    _travelerMonitorMgr = new TravelerMonitorManager();
                    _travelerMonitorMgr.ProcessWorker.SecondsBetweenIterations = GetSleepTimeForWorker("TravelerMonitor");
                    _travelerMonitorMgr.Start();
                }
           
                if (runVehicleLocationMonitor)
                {
                    Diagnostics.WriteMainDiagnosticInfo(TraceEventType.Information, TraceEventId.TraceGeneral, "Starting Vehicle Location Processor Manager");
                    _vehicleLocMonitorMgr = new VehicleLocationMonitorManager();
                    _vehicleLocMonitorMgr.ProcessWorker.SecondsBetweenIterations = GetSleepTimeForWorker("VehicleLocationMonitor");
                    _vehicleLocMonitorMgr.Start();
                }

                while (_wereRunning)
                {
                    if (runTConnectMonitor && !_tConnectMonitorMgr.IsRunning())
                    {
                        _tConnectMonitorMgr.Start();
                    }
                    if (runVehicleLocationMonitor && !_vehicleLocMonitorMgr.IsRunning())
                    {
                        _vehicleLocMonitorMgr.Start();
                    }
                    Thread.Sleep(10000);
                    //Trace.WriteLine("Working", "Information");
                }
            }
            catch(Exception ex)
            {
                //todo
                var s = ((System.Data.Entity.Validation.DbEntityValidationException)ex).EntityValidationErrors;
            }
        }

        public override void OnStop()
        {
            _wereRunning = false;
            _tConnectMonitorMgr.Stop();
            base.OnStop();
        }

        private int GetSleepTimeForWorker(string workerName)
        {
            try
            {
                string sleepTimeInSecondsAsAString = RoleEnvironment.GetConfigurationSettingValue(workerName + "SleepTime");

                int sleepTimeInSeconds;
                if (int.TryParse(sleepTimeInSecondsAsAString, out sleepTimeInSeconds))
                {
                    return sleepTimeInSeconds;
                }
            }
            catch (Exception)
            {
                Diagnostics.WriteConfigDiagnosticInfo(TraceEventType.Warning, TraceEventId.TraceUnexpected, "Unable to retreive SleepTime value for worker " + workerName);
            }
            return DefaultWorkerSleepTime;
        }


        private List<IBusSchedule> GetBusSchedules()
        {
            List<IBusSchedule> busSchedules = new List<IBusSchedule>();
            //TODO add code to get bus schedule interface 
            //busSchedules.Add(Kernel.Get<IBusSchedule>("COMMON NAME"));

            return busSchedules;
        }
        private static void SetupIoCBindings()
        {
            //Get connection string
            string connectionString = RoleEnvironment.GetConfigurationSettingValue("IDTOContext");

            Kernel.Load("IOCConfig.xml");

            //This binding means that whenever Ninject encounters a dependency on IUnitOfWork, it will resolve an instance of UnitOfWork and inject it using the connection string as an arg. 
            //Kernel.Bind<IUnitOfWork>().To<UnitOfWork>().WithConstructorArgument("connectionString", connectionString);
            Kernel.Bind<IDbContext>().To<IDTOContext>().WithConstructorArgument("connectionString", connectionString);
           
            // Setup the binding for the EmailService
            //Kernel.Bind<IEmailService>().To<EmailService>()
            //      .WithConstructorArgument("host", RoleEnvironment.GetConfigurationSettingValue("smtpRelay"))
            //      .WithConstructorArgument("port", 25)
            //      .WithConstructorArgument("senderAddress", RoleEnvironment.GetConfigurationSettingValue("smtpSenderEmail"))
            //      .WithConstructorArgument("smtpUserName", RoleEnvironment.GetConfigurationSettingValue("smtpUserName"))
            //      .WithConstructorArgument("smtpPassword", RoleEnvironment.GetConfigurationSettingValue("smtpPassword"));
            
            
            //string storageAccountstring = RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString");
           // var storageAccount2 = CloudStorageAccount.Parse(storageAccountstring);
            // retrieve a reference to the messages queue
            var setting = CloudConfigurationManager.GetSetting("StorageConnectionString");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(setting);

            //This binding means that whenever Ninject encounters a dependency on IAzureTable, it will resolve an instance of AzureTable and inject it using the constructor arguments. 
            Kernel.Bind<IAzureTable<ProbeSnapshotEntry>>().To<AzureTable<ProbeSnapshotEntry>>()
                  .WithConstructorArgument("account", storageAccount);

        }

        public override bool OnStart()
        {
            SetUpBindingForDiagnostics();
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            DiagnosticsConfig_OnStart();

            //needed to avoid runtime exception:"SetConfigurationSettingPublisher needs to be called before FromConfigurationSetting can be used"
           // CloudStorageAccount.SetConfigurationSettingPublisher(
           //    (configName, configSettingPublisher) =>
           //    {
           //        var connectionString =
           //            RoleEnvironment.GetConfigurationSettingValue(configName);
           //        configSettingPublisher(connectionString);
           //    }
           //);
            return base.OnStart();
        }

        public void SetUpBindingForDiagnostics()
        {
            //This binding means that whenever Ninject encounters a dependency on IUBIDiagnostics, it will resolve an instance of UBIDiagnostics and inject it. 
            Kernel.Bind<IIdtoDiagnostics>().To<IdtoDiagnostics>();

            Diagnostics = Kernel.Get<IIdtoDiagnostics>();
        }


        #region DiagnosticsSetup


        private void DiagnosticsConfig_OnStart()
        {
            Diagnostics.WriteConfigDiagnosticInfo(TraceEventType.Verbose, TraceEventId.TraceFunctionEntry, "Entering WorkerRole OnStart");

            // Set up for change notifications if any of our configuration values change at run-time.
            // First Changing is called - this determines whether or not the role is recycled based on what
            // is changing.  Then Changed is called to read the new values.
            RoleEnvironment.Changing += RoleEnvironmentChanging;
            RoleEnvironment.Changed += RoleEnvironmentChanged;

            this.ConfigureDiagnosticMonitor();

            Diagnostics.WriteConfigDiagnosticInfo(TraceEventType.Verbose, TraceEventId.TraceGeneral, "Setting up Diagnostics Monitor - transfer Verbose logs every minute.");

        }
        /// <summary>
        /// HasNonExemptConfigurationChanges - Check if config changes contain any that aren't on our "exempt from recycle" list
        /// Returns TRUE if there is at least one config change that isn't on our list.
        /// </summary>
        /// <param name="chgs">Collection of changes from RoleEnvironmentChanging or RoleEnvironmentChanged</param>
        /// <returns></returns>
        private bool HasNonExemptConfigurationChanges(ReadOnlyCollection<RoleEnvironmentChange> chgs)
        {
            Func<RoleEnvironmentConfigurationSettingChange, bool> changeIsNonExempt =
                    x => !_exemptConfigurationItems.Contains(x.ConfigurationSettingName);

            var environmentChanges = chgs.OfType<RoleEnvironmentConfigurationSettingChange>();

            return environmentChanges.Any(changeIsNonExempt);
        }

        /// <summary>
        /// RoleEventChanging - Called when a change is about to be applied to the role.  Determines whether or not to recycle the role instance.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">A list of what is changing</param>
        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            // Note: e.Cancel == true -> Azure should recycle the role.  If all the changes are in our "exempt" list,
            // we don't need to recycle the role.

            e.Cancel = HasNonExemptConfigurationChanges(e.Changes);

            // Note that we use Trace.WriteLine here rather than going through the Diagnostics class so that we will always log
            // this, even when the switch for whether to log or not is being changed.

            if (!e.Cancel)
            {
                Trace.WriteLine("Processor.WorkerRole::RoleEnvironmentChanging - role is not recycling, getting new switch values from config file.");
            }
            else
            {
                Trace.WriteLine("Processor.WorkerRole::RoleEnvironmentChanging - recycling role instance due to non-exempt configuration changes.");
            }
        }

        /// <summary>
        /// RoleEnvironmentChanged - Called after a change has been applied to the role.  
        /// NOTE: This is called AFTER RoleEnvironmentChanging is called.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">List of what has changed</param>
        private void RoleEnvironmentChanged(object sender, RoleEnvironmentChangedEventArgs e)
        {
            // Refresh the diagnostic switches from the role config values.
            // This allows for run-time changing of the values of the switches without recycling (i.e. rebooting)
            // the role so we can turn on or off more verbose diagnostic output based on the switches we've
            // defined in ServiceConfiguration.cscfg.
            Diagnostics.GetTraceSwitchValuesFromRoleConfiguration();

            // Log the change to the logs - using Trace.WriteLine to circumvent the switches so that if the switch was
            // turned off or to a low setting like Critical, the message showing this will still go into the logs.
            //TODO: Figure out how to do this with after the change to the IUBIDiagnostics
            //Trace.WriteLine("Processor.WorkerRole::RoleEnvironmentChanged - Diagnostics switch values changed.  ConfigTrace = " +
            //                    Diagnostics.ConfigTrace.Switch.Level.ToString() + " MainTrace = " +
            //                    Diagnostics.MainTrace.Switch.Level.ToString());
        }

        /// <summary> 
        /// Performs initial configurarion for Windows Azure Diagnostics for the instance.
        /// </summary> 
        private void ConfigureDiagnosticMonitor()
        {
            DiagnosticMonitorConfiguration diagnosticMonitorConfiguration = DiagnosticMonitor.GetDefaultInitialConfiguration();

            diagnosticMonitorConfiguration.DiagnosticInfrastructureLogs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;
            diagnosticMonitorConfiguration.DiagnosticInfrastructureLogs.ScheduledTransferPeriod = TimeSpan.FromMinutes(1d);

            diagnosticMonitorConfiguration.Directories.ScheduledTransferPeriod = TimeSpan.FromMinutes(1d);
            diagnosticMonitorConfiguration.Directories.BufferQuotaInMB = 100;

            diagnosticMonitorConfiguration.Logs.ScheduledTransferPeriod = TimeSpan.FromMinutes(1d);
            diagnosticMonitorConfiguration.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;

            diagnosticMonitorConfiguration.WindowsEventLog.DataSources.Add("Application!*");
            diagnosticMonitorConfiguration.WindowsEventLog.DataSources.Add("System!*");
            diagnosticMonitorConfiguration.WindowsEventLog.ScheduledTransferPeriod = TimeSpan.FromMinutes(1d);

            //PerformanceCounterConfiguration performanceCounterConfiguration = new PerformanceCounterConfiguration();
            //performanceCounterConfiguration.CounterSpecifier = @"\Processor(_Total)\% Processor Time";
            //performanceCounterConfiguration.SampleRate = System.TimeSpan.FromSeconds(10d);
            //diagnosticMonitorConfiguration.PerformanceCounters.DataSources.Add(performanceCounterConfiguration);

            //PerformanceCounterConfiguration performanceCounterConfigurationMem = new PerformanceCounterConfiguration();
            //performanceCounterConfigurationMem.CounterSpecifier = @"\Memory\% Committed Bytes In Use";
            //performanceCounterConfigurationMem.SampleRate = System.TimeSpan.FromSeconds(60d);
            //diagnosticMonitorConfiguration.PerformanceCounters.DataSources.Add(performanceCounterConfigurationMem);

            //diagnosticMonitorConfiguration.PerformanceCounters.ScheduledTransferPeriod = TimeSpan.FromMinutes(1d);

            DiagnosticMonitor.Start(WadConnectionString, diagnosticMonitorConfiguration);
        }


        #endregion

    }
}
