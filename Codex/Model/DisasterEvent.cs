using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bantay_Kalamidad_Pilipinas.Model
{
    internal class DisasterEvent
    {
        public string Name { get; set; }
        public string EventID { get; set; }

        // XAML DisplayMemberPath="Event_Name" — alias for Name
        public string Event_Name => Name;

        public DisasterEvent(string name, string eventID)
        {
            Name = name;
            EventID = eventID;
        }
    }
}