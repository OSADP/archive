using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository;

namespace IDTO.Entity.Models
{
    public class LastVehiclePosition : EntityBase
    {
        //Primary Key
        public int Id { get; set; }
        [Required]
        public string VehicleName { get; set; }
        [Required]
        public DateTime PositionTimestamp { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        [Required]
        public double Speed { get; set; }
        [Required]
        public short Heading { get; set; }
        [Required]
        public int Accuracy { get; set; }
    }
}
