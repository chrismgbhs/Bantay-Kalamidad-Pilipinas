using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bantay_Kalamidad_Pilipinas.Model
{
    internal class RescueLocation
    {
        public string Barangay { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string EvacuationArea { get; set; }
        public string RescueStatus { get; set; }

        public RescueLocation(string barangay, string city, string province, string evacuationArea, string rescueStatus)
        {
            Barangay = barangay;
            City = city;
            Province = province;
            EvacuationArea = evacuationArea;
            RescueStatus = rescueStatus;
        }
    }
}
