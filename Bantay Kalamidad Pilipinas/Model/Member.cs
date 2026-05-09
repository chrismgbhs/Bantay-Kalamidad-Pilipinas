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

        public Member(string volunteer, string role, string status)
        {
            Volunteer = volunteer;
            Role = role;
            Status = status;
        }
    }
}
