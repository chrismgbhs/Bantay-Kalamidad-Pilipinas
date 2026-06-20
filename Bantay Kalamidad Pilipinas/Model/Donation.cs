using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bantay_Kalamidad_Pilipinas.Model
{
    internal class Donation
    {
        public string DonationId { get; set; }
        public string Event { get; set; }
        public string Item { get; set; }
        public string DateReceived { get; set; }    
        public string Status { get; set; }

        public Donation(string donationID, string eventName, string item, string dateReceived, string status)
        {
            DonationId = donationID;
            Event = eventName;
            Item = item;
            DateReceived = dateReceived;
            Status = status;
        }
    }
}
