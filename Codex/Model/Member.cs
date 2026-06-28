using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bantay_Kalamidad_Pilipinas.Model
{
    internal class Member
    {
        public string Volunteer { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }

        // Matches the XAML's PriorityBinding, which tries Contact,
        // ContactNumber, Contact_Number, VolunteerContact in that order —
        // Contact_Number is the one that actually matches a real column
        // (Volunteer.Contact_Number in the schema), so that's the name used
        // here. The PriorityBinding will pick this one up automatically.
        public string Contact_Number { get; set; }

        public Member(string volunteer, string role, string status, string contactNumber)
        {
            Volunteer = volunteer;
            Role = role;
            Status = status;
            Contact_Number = contactNumber;
        }
    }
}