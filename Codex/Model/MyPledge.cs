using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bantay_Kalamidad_Pilipinas.Model
{
    internal class MyPledge
    {
        public string PledgeId { get; set; }
        public string Event { get; set; }
        public string Item { get; set; }
        public string ExpectedDate { get; set; }            
        public string Status { get; set; }

        public MyPledge(string pledgeID, string eventName, string itemName, string expectedDate, string status)
        {
            PledgeId = pledgeID;
            Event = eventName;
            Item = itemName;
            ExpectedDate = expectedDate;
            Status = status;
        }
    }
}
