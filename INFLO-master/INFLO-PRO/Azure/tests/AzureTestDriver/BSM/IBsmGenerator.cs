using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureTestDriver.BSM
{
    /// <summary>
    /// Defines the requirements of a BSM Generator, which will generate IBsmMessages at a given interval
    /// </summary>
    public interface IBsmGenerator
    {
        /// <summary>
        /// Event thrown when a new BSM Message is generated
        /// </summary>
        event BsmMessageGeneratedEventHandler MessageGenerated;

        /// <summary>
        /// Time interval between generated messages, in Milliseconds.
        /// </summary>
        uint GenerateInterval { get; set; }

        /// <summary>
        /// Starts the generator
        /// </summary>
        void Start();
        /// <summary>
        /// Stops the generator
        /// </summary>
        void Stop();
    }


    /// <summary>
    /// Event Handler for when a new BSM Message is generated.
    /// </summary>
    /// <param name="sender">Object generating the message</param>
    /// <param name="e">Event arguments including IBsmMessage</param>
    public delegate void BsmMessageGeneratedEventHandler(object sender, BsmMessageGeneratedEventArgs e);
    /// <summary>
    /// Event Arguments for when a new BSM Message is generated.
    /// </summary>
    public class BsmMessageGeneratedEventArgs : EventArgs
    {
        /// <summary>
        /// The BSM Message created by the generator
        /// </summary>
        public IBsmMessage Message { get; private set; }

        public BsmMessageGeneratedEventArgs(IBsmMessage message)
        {
            Message = message;
        }
    }
}
