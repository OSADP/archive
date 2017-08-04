using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INCZONE.Model;

namespace INCZONE.Common
{
    public class DGPSConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string HostIP { get; set; }
        public string HostPort { get; set; }
        public int RefreshRate { get; set; }
        public bool IsDefault { get; set; }
        public int LocationRefreshRate { get; set; }

        public DGPSConfig() { }

        public DGPSConfig(DGPSConfiguration entity) 
        { 
            this.HostIP = entity.HostIP;
            this.HostPort = entity.HostPort;
            this.Id = entity.Id;
            this.IsDefault = entity.IsDefault;
            this.LocationRefreshRate = (int)entity.LocationRefreshRate;
            this.Name = entity.Name;
            this.Password = entity.Password;
            this.RefreshRate = entity.RefreshRate;
            this.Username = entity.Username;
        }
    }
}
