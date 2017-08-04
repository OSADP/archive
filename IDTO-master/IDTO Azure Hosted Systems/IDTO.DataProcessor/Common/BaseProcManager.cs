using System.Threading;

namespace IDTO.DataProcessor.Common
{
    public class BaseProcManager
    {
        private IProcessWorker _worker;
        private Thread _workerThread;

        public BaseProcManager(IProcessWorker procWorker)
        {
            _worker = procWorker;
        }

        public IProcessWorker ProcessWorker 
        {
            get { return _worker; }         
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            if (_workerThread != null)
            {
                //Clean up previous instances before trying to restart
                _worker.StopProcessing = true;
                _workerThread.Interrupt();
                _workerThread.Join(1000 * 60);
                if (_workerThread.IsAlive)
                {
                    _workerThread.Abort();
                }
                _workerThread = null;
            }

            string threadName = _worker.GetType() + "Thread";
            _workerThread = new Thread(_worker.Run) { Name = threadName };
            _workerThread.Start();

        }

        public void Stop()
        {
            _worker.StopProcessing = true;

            _workerThread.Join(1000 * 60 * 5);

            if (_workerThread.IsAlive)
            {
                _workerThread.Abort();
            }
            _workerThread = null;
            _worker = null;

        }

        public bool IsRunning()
        {
            if (_workerThread.IsAlive)
                return !_worker.StopProcessing;

            return false;
        }

    }
}
