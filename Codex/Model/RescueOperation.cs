using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Bantay_Kalamidad_Pilipinas.Model
{
    internal class RescueOperation
    {
        public string OperationId { get; set; }
        public string Event { get; set; }
        public string Location { get; set; }
        public string DateStarted { get; set; }
        public string Status { get; set; }

        public RescueOperation(string operationID, string eventName, string location, string dateStarted, string status)
        {
            OperationId = operationID;
            Event = eventName;
            Location = location;
            DateStarted = dateStarted;
            Status = status;
        }
    }
}
