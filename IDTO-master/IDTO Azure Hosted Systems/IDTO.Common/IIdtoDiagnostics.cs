using System.Diagnostics;

namespace  IDTO.Common
{
    public interface IIdtoDiagnostics
    {
        /// <summary>
        /// Trace switch to control level of output for the ConfigTrace.
        /// </summary>
        /// <value>
        /// The config trace switch.
        /// </value>
        SourceSwitch ConfigTraceSwitch { get; set; }

        /// <summary>
        /// Trace switch to control level of output for the MainTrace.
        /// </summary>
        /// <value>
        /// The main trace switch.
        /// </value>
        SourceSwitch MainTraceSwitch { get; set; }

        /// <summary>
        ///Given a string representation of a tracing level, e.g. from the config file, 
        /// translate it to the SourceLevels enum needed.
        /// </summary>
        /// <param name="str">The string value of the tracing level.</param>
        /// <returns></returns>
        SourceLevels SourceLevelFromString(string str);

        /// <summary>
        /// Gets the trace switch values from role configuration.
        /// </summary>
        void GetTraceSwitchValuesFromRoleConfiguration();

        /// <summary>
        /// Writes the diagnostic info.
        /// </summary>
        /// <param name="src">The TraceSource to write to.</param>
        /// <param name="evtType">Type of the evt.</param>
        /// <param name="evtId">The evt ID.</param>
        /// <param name="msg">The MSG.</param>
        void WriteDiagnosticInfo(TraceSource src, TraceEventType evtType, TraceEventId evtId, string msg);

        /// <summary>
        /// Writes the diagnostic info.
        /// </summary>
        /// <param name="src">The SRC.</param>
        /// <param name="evtType">Type of the evt.</param>
        /// <param name="evtId">The evt ID.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        void WriteDiagnosticInfo(TraceSource src, TraceEventType evtType, TraceEventId evtId, string format, params object[] args);

        /// <summary>
        /// Shortcut method for logging to the MainTrace source
        /// </summary>
        /// <param name="evtType">Type of the evt.</param>
        /// <param name="evtId">The evt ID.</param>
        /// <param name="msg">The MSG.</param>
        void WriteMainDiagnosticInfo(TraceEventType evtType, TraceEventId evtId, string msg);

        /// <summary>
        /// Shortcut method for logging to the MainTrace source with string format
        /// </summary>
        /// <param name="evtType">Type of the evt.</param>
        /// <param name="evtId">The evt ID.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        void WriteMainDiagnosticInfo(TraceEventType evtType, TraceEventId evtId, string format, params object[] args);

        /// <summary>
        /// Shortcut method for logging to the MainTrace source
        /// </summary>
        /// <param name="evtType">Type of the evt.</param>
        /// <param name="evtId">The evt ID.</param>
        /// <param name="msg">The MSG.</param>
        void WriteConfigDiagnosticInfo(TraceEventType evtType, TraceEventId evtId, string msg);

        /// <summary>
        /// Shortcut method for logging to the MainTrace source with string format
        /// </summary>
        /// <param name="evtType">Type of the evt.</param>
        /// <param name="evtId">The evt ID.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        void WriteConfigDiagnosticInfo(TraceEventType evtType, TraceEventId evtId, string format, params object[] args);

    }
}