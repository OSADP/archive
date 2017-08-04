using System;
using System.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;


namespace IDTO.Common
{
    /// <summary>
    /// Helper class for diagnostic output from the Processor Worker Role.
    /// </summary>
    public class IdtoDiagnostics : IIdtoDiagnostics
    {
        // Trace sources - Think of these as multiple output channels. So write to the ConfigTrace when you're 
        // logging information about configuration, write to the MainTrace when you're logging information about
        // the main role activities themselves (e.g. page load, etc.).  You can add more sources if you want - they
        // give you more ability to control what is and isn't logged.  For instance, rather than a single MainTrace,
        // you might have a source per major component of your main app - Home page, administration, profile management,
        // etc.  Have a switch per Source so you can adjust the verbosity of logging per component.
        /// <summary>
        /// TraceSource used output information regarding configuration settings
        /// </summary>
        public TraceSource ConfigTrace;
        /// <summary>
        /// TraceSource used to output information related to the Main Services activities
        /// </summary>
        public TraceSource MainTrace;
        // Add additional sources here

        /// <summary>
        /// Trace switch to control level of output for the ConfigTrace.
        /// </summary>
        /// <value>
        /// The config trace switch.
        /// </value>
        public SourceSwitch ConfigTraceSwitch { get; set; }
        /// <summary>
        /// Trace switch to control level of output for the MainTrace.
        /// </summary>
        /// <value>
        /// The main trace switch.
        /// </value>
        public SourceSwitch MainTraceSwitch { get; set; }
        // Add additional switches 1:1 with trace sources here; hook them up in the ctor

        /// <summary>
        ///Given a string representation of a tracing level, e.g. from the config file, 
        /// translate it to the SourceLevels enum needed.
        /// </summary>
        /// <param name="str">The string value of the tracing level.</param>
        /// <returns></returns>
        public SourceLevels SourceLevelFromString(string str)
        {
            SourceLevels lvl;

            try
            {
                lvl = (SourceLevels)Enum.Parse(typeof(SourceLevels), str, true);
            }
            catch (System.ArgumentException)
            {
                // Invalid value - just default to off.
                lvl = SourceLevels.Off;
            }

            return lvl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UbiDiagnostics"/> class.
        /// </summary>
        public IdtoDiagnostics()
        {            
            ConfigTraceSwitch = new SourceSwitch("ConfigTrace", "Verbose");
            MainTraceSwitch = new SourceSwitch("MainTrace", "Verbose");
            ConfigTrace = new TraceSource("ConfigTrace");
            ConfigTrace.Switch = ConfigTraceSwitch;
            MainTrace = new TraceSource("MainTrace");
            MainTrace.Switch = MainTraceSwitch;
            GetTraceSwitchValuesFromRoleConfiguration();
        }

        /// <summary>
        /// Gets the trace switch values from role configuration.
        /// </summary>
        public void GetTraceSwitchValuesFromRoleConfiguration()
        {
            if (RoleEnvironment.IsAvailable)
            {
                ConfigTraceSwitch.Level = SourceLevelFromString(RoleEnvironment.GetConfigurationSettingValue("ConfigTrace"));
                MainTraceSwitch.Level = SourceLevelFromString(RoleEnvironment.GetConfigurationSettingValue("MainTrace"));
            }
            // Uses Trace.WriteLine so that the information goes through regardless of the switch values
            Trace.WriteLine("GetTraceSwitchValuesFromRoleConfiguration - Trace switch values set:  Config=" + ConfigTraceSwitch.Level.ToString() +
                "  Main=" + MainTraceSwitch.Level.ToString());
        }

        /// <summary>
        /// Writes the diagnostic info.
        /// </summary>
        /// <param name="src">The TraceSource to write to.</param>
        /// <param name="evtType">Type of the evt.</param>
        /// <param name="evtId">The evt ID.</param>
        /// <param name="msg">The MSG.</param>
        public void WriteDiagnosticInfo(TraceSource src, TraceEventType evtType, TraceEventId evtId, string msg)
        {
            if (src != null)
                src.TraceEvent(evtType, (int)evtId, msg);
        }

        /// <summary>
        /// Writes the diagnostic info.
        /// </summary>
        /// <param name="src">The SRC.</param>
        /// <param name="evtType">Type of the evt.</param>
        /// <param name="evtId">The evt ID.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public void WriteDiagnosticInfo(TraceSource src, TraceEventType evtType, TraceEventId evtId, string format, params object[] args)
        {
            if (src != null)
                src.TraceEvent(evtType, (int)evtId, format, args);
        }



        /// <summary>
        /// Shortcut method for logging to the MainTrace source
        /// </summary>
        /// <param name="evtType">Type of the evt.</param>
        /// <param name="evtId">The evt ID.</param>
        /// <param name="msg">The MSG.</param>
        public void WriteMainDiagnosticInfo(TraceEventType evtType, TraceEventId evtId, string msg)
        {
            WriteDiagnosticInfo(MainTrace, evtType, evtId, msg);
        }

        /// <summary>
        /// Shortcut method for logging to the MainTrace source with string format
        /// </summary>
        /// <param name="evtType">Type of the evt.</param>
        /// <param name="evtId">The evt ID.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        public void WriteMainDiagnosticInfo(TraceEventType evtType, TraceEventId evtId, string format, params object[] args)
        {
            WriteDiagnosticInfo(MainTrace, evtType, evtId, format, args);
        }

        public void WriteConfigDiagnosticInfo(TraceEventType evtType, TraceEventId evtId, string msg)
        {
            WriteDiagnosticInfo(ConfigTrace, evtType, evtId, msg);
        }

        public void WriteConfigDiagnosticInfo(TraceEventType evtType, TraceEventId evtId, string format, params object[] args)
        {
            WriteDiagnosticInfo(ConfigTrace, evtType, evtId, format, args);
        }
    }
}
