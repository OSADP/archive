using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AzureTestDriver.BSM
{
    class BsmGeneratorFromFile : IBsmGenerator
    {
        public const uint BSM_DATA_LENGTH = 36;

        public event BsmMessageGeneratedEventHandler MessageGenerated;

        public uint GenerateInterval { get { return generateInterval; } set { generateInterval = value; } }

        private uint generateInterval = 1000;
        private Timer generateTimer;

        private Random generateRandom;

        private List<byte[]> loadedBsms = new List<byte[]>();

        public BsmGeneratorFromFile()
        { 
            generateRandom = new Random((int)DateTime.Now.Ticks);
            System.Threading.Thread.Sleep(1);

            generateTimer = new Timer(generateInterval);
            generateTimer.AutoReset = true;
            generateTimer.Elapsed += generateTimer_Elapsed;

        }

        public void Start()
        {
            loadedBsms.Clear();

            try
            {
                System.IO.DirectoryInfo berFolder = new System.IO.DirectoryInfo(Properties.Settings.Default.BSMBerFileLocation);

                var filesToRead = berFolder.GetFiles().Where(x => x.Extension.Equals(".ber"));

                foreach (var file in filesToRead)
                {
                    try
                    {
                        System.IO.FileStream fs = new System.IO.FileStream(file.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                        byte[] data = new byte[fs.Length];
                        fs.Read(data, 0, (int)fs.Length);

                        fs.Close();

                        loadedBsms.Add(data);
                    }
                    catch { }
                }
            }
            catch { }

            generateTimer.Interval = Math.Max(generateRandom.Next(-(int)generateInterval, (int)generateInterval) + generateInterval, 50);
            generateTimer.Start();
        }
        public void Stop()
        {
            generateTimer.Stop();
        }


        void generateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            /*
             * Set the generator to tick at some time +- 50% of generate interval.  
             * This will help keep the test driver from generating all of the messages at the same exact time.
             */
            generateTimer.Interval = generateRandom.Next(-(int)(generateInterval / 2), (int)(generateInterval / 2)) + generateInterval;

            byte[] messageBytes;

            if (loadedBsms.Count == 0)
            {
                messageBytes = new byte[BSM_DATA_LENGTH];
                generateRandom.NextBytes(messageBytes);
            }
            else
            {
                messageBytes = loadedBsms[generateRandom.Next(loadedBsms.Count)];
            }

            IBsmMessage message = new BsmMessage(messageBytes);

            onMessageGenerated(message);
        }
        private void onMessageGenerated(IBsmMessage message)
        {
            if (MessageGenerated != null)
                MessageGenerated(this, new BsmMessageGeneratedEventArgs(message));
        }

    }
}
