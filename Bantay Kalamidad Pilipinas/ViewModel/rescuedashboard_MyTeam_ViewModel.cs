using Bantay_Kalamidad_Pilipinas.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class rescuedashboard_MyTeam_ViewModel : ObservableObject
    {
        private ObservableCollection<Member> _MyTeam;

        public ObservableCollection<Member> MyTeam
        {
            get => _MyTeam;
            set { _MyTeam = value; OnPropertyChanged(nameof(MyTeam)); }
        }

        public rescuedashboard_MyTeam_ViewModel()
        {
            MyTeam = new ObservableCollection<Member>();
            InitializeMyTeam();
        }

        public void InitializeMyTeam()
        {
            // Get all Operation_IDs assigned to the current user
            string assignedOpsQuery = @"
            SELECT oa.Operation_ID
            FROM [Operation Assignment] oa
            JOIN [Volunteer] v ON oa.Volunteer_ID = v.Volunteer_ID
            JOIN [Users] u ON v.User_ID = u.User_ID
            WHERE u.Username = '" + rescue_login_ViewModel.CurrentUser.Username + @"' AND oa.Operation_Status = 'Active'";

            if (DatabaseManager.GetTableDataWithCustomizedQuery(assignedOpsQuery, out DataTable opsTable))
            {
                var operationIDs = opsTable.AsEnumerable()
                                           .Select(row => row["Operation_ID"].ToString())
                                           .ToList();

                foreach (var opID in operationIDs)
                {
                    // For each Operation_ID, get the volunteers assigned
                    string teamQuery = @"
                    SELECT 
                        v.Volunteer_Name,
                        oa.Role,
                        oa.Operation_Status
                    FROM [Operation Assignment] oa
                    JOIN [Volunteer] v ON oa.Volunteer_ID = v.Volunteer_ID
                    JOIN [Users] u ON v.User_ID = u.User_ID
                    WHERE oa.Operation_ID = '" + opID + @"'";

                    if (DatabaseManager.GetTableDataWithCustomizedQuery(teamQuery, out DataTable teamTable))
                    {
                        foreach (DataRow row in teamTable.Rows)
                        {
                            var member = new Member(row["Volunteer_Name"].ToString(), row["Role"].ToString(), row["Operation_Status"].ToString());
                            MyTeam.Add(member);
                            //MessageBox.Show($"Added member: {member.Volunteer}, Role: {member.Role}, Status: {member.Status}");
                        }
                    }
                }
            }

            else
            {
                MessageBox.Show("Failed to load assigned operations.");
            }
        }
    }
}
