using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Repository;
namespace IDTO.Entity.Models
{
    public class WebApiGetTripUsage : EntityBase
    {
        //primary key
        public int Id { get; set; }

        [Required]
        public string FromPlace { get; set; }
        [Required]
        public string ToPlace { get; set; }
        [Required]
        public float FromLatitude { get; set; }
        [Required]
        public float FromLongitude { get; set; }
        [Required]
        public float ToLatitude { get; set; }
        [Required]
        public float ToLongitude { get; set; }
        [Required]
        public DateTime SearchDate { get; set; }
        [Required]
        public string Platform { get; set; }
        [Required]
        public int NumberOfResults { get; set; }
        [Required]
        public int ExecutionTimeSeconds { get; set; }
        [Required]
        public float MaxWalkMeters { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
    }
}
