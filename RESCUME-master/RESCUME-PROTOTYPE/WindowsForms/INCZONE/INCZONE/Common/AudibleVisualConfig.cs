using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INCZONE.Model;

namespace INCZONE.Common
{
    public class AudibleVisualConfig
    {

        public AudibleVisualConfig()
        {

        }

        public AudibleVisualConfig(AudibleVisualAlarm entity)
        {
            this.AlarmConfiguration_Id = entity.AlarmConfiguration_Id;
            this.AlarmLevel_Id = entity.AlarmLevel_Id;
            this.Duration = entity.Duration;
            this.Frequency = entity.Frequency;
            this.Persistance = entity.Persistance;
            this.Id = entity.Id;
            this.RadioActive = entity.RadioActive;
        }


        public int Id { get; set; }
        public string Duration { get; set; }
        public string Frequency { get; set; }
        public int Persistance { get; set; }
        public bool RadioActive { get; set; }
        public int AlarmLevel_Id { get; set; }
        public int AlarmConfiguration_Id { get; set; }
    }
}
