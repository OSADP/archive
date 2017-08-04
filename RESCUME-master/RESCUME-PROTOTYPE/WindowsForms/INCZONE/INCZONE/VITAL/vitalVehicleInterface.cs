using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INCZONE.VITAL
{
    interface vitalVehicleInterface
    {
        bool isConnected();
        bool activateAlarms();
        void deactivateAlarms();
        void Connect(string port);
        void Disconnect();

    }
}
