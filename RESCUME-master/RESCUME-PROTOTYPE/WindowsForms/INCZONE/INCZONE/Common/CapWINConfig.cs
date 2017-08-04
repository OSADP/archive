using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INCZONE.Model;

namespace INCZONE.Common
{
    public class CapWINConfig
    {
        public int Id { get; set; }
        public string HostURL { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ComPort { get; set; }
        public string BaudRate { get; set; }
        public int DistanceToIncident { get; set; }
        public int LaneData { get; set; }

        public CapWINConfig() { }

        public CapWINConfig(CapWINConfiguration entity) 
        {
            this.Id = entity.Id;
            this.BaudRate = entity.BaudRate;
            this.ComPort = entity.ComPort;
            this.HostURL = entity.HostURL;
            this.Password = entity.Password;
            this.Username = entity.Username;
            this.DistanceToIncident = entity.DistanceToIncident;
            this.LaneData = entity.LaneData;
        }
    }
}
