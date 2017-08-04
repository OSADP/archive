using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using INCZONE.Model;

namespace INCZONE.Common
{
    public class DSRCConfig
    {
        public int Id { get; set; }
        public int ACM { get; set; }
        public int BSM { get; set; }
        public int EVA { get; set; }
        public int TIM { get; set; }

        public DSRCConfig()
        {
        }

        public DSRCConfig(DSRCConfiguration entity)
        {
            this.ACM = entity.ACM;
            this.BSM = entity.BSM;
            this.EVA = entity.EVA;
            this.Id = entity.Id;
            this.TIM = entity.TIM;
        }
    }
}
