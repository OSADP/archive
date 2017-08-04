using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Collections;


namespace INCZONE.VITAL
{
    public class VITALModule
    {
//        private vitalVehicleInterface vehicle = new ChevyCruze();
        private vitalVehicleInterface vehicle = new ChevyCaprice();

        // Constructors
        public VITALModule()
        {
        }

        // Connect to Module and send initialization commands
        public void Connect(string port)
        {
            vehicle.Connect(port);
        }

        // Disconnect from the module
        public void Disconnect()
        {
            vehicle.Disconnect();
        }

        // Activate Horn and Lights alarm
        public bool ActivateAlarms()
        {
            Console.Out.WriteLine("Activating Vehicle Alarms");
            return vehicle.activateAlarms();
        }

        // Deactivate Horn and Lights alarm
        public void DeactivateAlarms()
        {
            vehicle.deactivateAlarms();
        }

        public bool IsConnected()
        {
            return vehicle.isConnected();
        }

        public void setVehicle(String newVehicle)
        {
            Console.Out.WriteLine("Setting Vehicle: " + newVehicle);
            if (newVehicle.ToLower().Equals("caprice"))
            {
                Console.Out.WriteLine("Changing Vehicle to Caprice");
                vehicle = new ChevyCaprice();
            }
            if (newVehicle.ToLower().Equals("cruze"))
            {
                Console.Out.WriteLine("Changing Vehicle to Cruze");
                vehicle = new ChevyCruze();
            }
        }

    }
}
