using Bantay_Kalamidad_Pilipinas.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
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

        /// <summary>
        /// This method initializes the MyTeam collection by fetching the operations assigned to the current user and then retrieving the team members for each operation.
        /// </summary>
        public void InitializeMyTeam()
        {
            MyTeam.Clear();
            string query = "SELECT * FROM dbo.GetMyTeamByUsername(@Username)";
            var parameters = new[] { new SqlParameter("@Username", rescue_login_ViewModel.CurrentUser.Username) };

            if (DatabaseManager.GetTableData(query, parameters, out DataTable opsTable))
            {
                var operationIDs = opsTable.AsEnumerable()
                                           .Select(row => row["Operation_ID"].ToString())
                                           .ToList();

                foreach (var opID in operationIDs)
                {
                    string teamQuery = "SELECT * FROM dbo.GetMyTeamByOperationID(@OperationID)";
                    var teamParameters = new[] { new SqlParameter("@OperationID", opID) };
                    if (DatabaseManager.GetTableData(teamQuery, teamParameters, out DataTable teamTable))
                    {
                        foreach (DataRow row in teamTable.Rows)
                        {
                            var member = new Member(
                                row["Volunteer_Name"].ToString(),
                                row["Role"].ToString(),
                                row["Operation_Status"].ToString(),
                                row["Contact_Number"].ToString());
                            MyTeam.Add(member);
                        }
                    }
                }
            }
            // No "else" MessageBox — a volunteer with no assigned operations
            // (and therefore no team to show) is a normal state, not an error.
        }
    }
}