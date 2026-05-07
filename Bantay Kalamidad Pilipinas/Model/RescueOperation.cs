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
        public string OperationId;
        public string Event;
        public string Location;
        public string DateStarted;
        public string Status;

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
