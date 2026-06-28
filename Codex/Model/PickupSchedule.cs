using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bantay_Kalamidad_Pilipinas.Model
{
    internal class PickupSchedule
    {
        public string DonationId { get; set; }
        public string PickupDate { get; set; }
        public string Status { get; set; }

        public PickupSchedule(string donationID, string pickupDate, string status)
        {
            DonationId = donationID;
            PickupDate = pickupDate;
            Status = status;
        }
    }
}
