using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShuttle.API.Model
{
    public class Driver
    {
        public int DriverId { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public byte[] Picture { get; set; }

        public int CarrierId { get; set; }

        public Carrier Carrier { get; set; }

        public double RatingAvg { get; set; }

        public int TotalRides { get; set; }

        public int? VehicleId { get; set; }         // Not used, only for compatibility

        public virtual ICollection<Vehicle> Vehicles { get; set; }    

        public ICollection<Ride> Rides { get; set; }

    }
}
