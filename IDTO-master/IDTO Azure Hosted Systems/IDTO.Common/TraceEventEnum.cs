namespace IDTO.Common
{
    // Event types
    /// <summary>
    /// Enumeration of the TraceEventId
    /// </summary>
    public enum TraceEventId
    {
        /// <summary>
        /// General Trace message
        /// </summary>
        TraceGeneral = 0,           // General type
        /// <summary>
        /// Trace message for function entry
        /// </summary>
        TraceFunctionEntry = 1,
        /// <summary>
        /// Trace message for function exit
        /// </summary>
        TraceFunctionExit = 2,      // Note that can see just entry / exit by querying eventId lt 3
        /// <summary>
        /// Trace message for Exceptions
        /// </summary>
        TraceException = 100,
        /// <summary>
        /// Trace message for unexpected results or behaviors
        /// </summary>
        TraceUnexpected = 200,
        /// <summary>
        /// Trace message for tracking execution flow
        /// </summary>
        TraceFlow = 300             // Can see entry / exit plus errors with eventId le 200
    };


}
