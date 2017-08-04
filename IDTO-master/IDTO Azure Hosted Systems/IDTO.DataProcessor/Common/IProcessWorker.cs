using IDTO.Common;
namespace IDTO.DataProcessor.Common
{
    public interface IProcessWorker
    {
        IIdtoDiagnostics Diagnostics { get; set; } 

        bool StopProcessing { get; set; }
        int SecondsBetweenIterations { get; set; }

        void Run();


    }
}
