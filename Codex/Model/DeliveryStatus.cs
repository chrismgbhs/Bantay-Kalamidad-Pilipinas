using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bantay_Kalamidad_Pilipinas.Model
{
    internal class DeliveryStatus
    {
        public string DonationId { get; set; }
        public string DeliveryDate { get; set; }
        public string Status { get; set; }

        public DeliveryStatus(string donationID, string deliveryDate, string status)
        {
            DonationId = donationID;
            DeliveryDate = deliveryDate;
            Status = status;
        }
    }
}
