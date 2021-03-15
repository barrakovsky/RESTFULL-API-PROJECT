using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Restfull_API_Project.Models
{
    public class Image
    {
        //Image id
        public int Id { get; set; }

        //Image url
        public string ImageUrl { get; set; }

        //Vehicle Id that this specific image belongs to
        public int VehicleId { get; set; }

        //Image array - byte array of the image
        [NotMapped]
        public byte[] ImageArray { get; set; }
    }
}
