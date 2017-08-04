using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INCZONE.Model;

namespace INCZONE.Common
{
    public class AlarmConfig
    {

        public AlarmConfig() {}

        public AlarmConfig(AlarmConfiguration entity)
        {
            this.AudibleVisualConfigs = _ConvetAudibleVisualEnitiyList(entity.AudibleVisualAlarms);
            this.Id = entity.Id;
            this.IsDefault = entity.IsDefault;
            this.Name = entity.Name;
            this.VitalConfigs = _ConvetVitalEnitiyList(entity.VehicleAlarms);
        }

        private List<VitalConfig> _ConvetVitalEnitiyList(ICollection<VehicleAlarm> collection)
        {
            List<VitalConfig> list = new List<VitalConfig>();

            foreach (VehicleAlarm entity in collection)
            {
                VitalConfig config = new VitalConfig(entity);
                list.Add(config);
            }

            return list;
        }

        private List<AudibleVisualConfig> _ConvetAudibleVisualEnitiyList(ICollection<AudibleVisualAlarm> collection)
        {
            List<AudibleVisualConfig> list = new List<AudibleVisualConfig>();

            foreach (AudibleVisualAlarm entity in collection)
            {
                AudibleVisualConfig config = new AudibleVisualConfig(entity);
                list.Add(config);
            }

            return list;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public List<AudibleVisualConfig> AudibleVisualConfigs { get; set; }
        public List<VitalConfig> VitalConfigs { get; set; }
    }
}
