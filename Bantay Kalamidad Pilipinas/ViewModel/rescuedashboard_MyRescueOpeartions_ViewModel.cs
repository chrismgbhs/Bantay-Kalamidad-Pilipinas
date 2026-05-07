using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bantay_Kalamidad_Pilipinas.Model;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class rescuedashboard_MyRescueOpeartions_ViewModel : ObservableObject
    {
        private ObservableCollection<RescueOperation> _MyRescueOperations;

        public ObservableCollection<RescueOperation> MyRescueOperations
        {
            get => _MyRescueOperations;
            set { _MyRescueOperations = value; OnPropertyChanged(nameof(MyRescueOperations)); }
        }

        public rescuedashboard_MyRescueOpeartions_ViewModel()
        {
            MyRescueOperations = new ObservableCollection<RescueOperation>();
            InitializeRescueOperations();
        }

        public void InitializeRescueOperations()
        {
            string query = @"
                SELECT 
                    ro.Operation_ID,
                    de.Event_Name,
                    l.Location_Name,
                    ro.Date_Started,
                    ro.Rescue_Status
                FROM [Rescue Operation] ro
                JOIN [Operation Assignment] oa ON ro.Operation_ID = oa.Operation_ID
                JOIN [Volunteer] v            ON oa.Volunteer_ID = v.Volunteer_ID
                JOIN [Users] u                ON v.User_ID = u.User_ID
                JOIN [Location] l             ON ro.Location_ID = l.Location_ID
                JOIN [Disaster Event] de      ON ro.Event_ID = de.Event_ID
                WHERE u.Username = '" + rescue_login_ViewModel.CurrentUser.Username + @"'";

            if (DatabaseManager.GetTableDataWithCustomizedQuery(query, out DataTable data))
            {
                var operations = data.AsEnumerable().Select(row => new RescueOperation(
                    row["Operation_ID"].ToString(),
                    row["Event_Name"].ToString(),
                    row["Location_Name"].ToString(),
                    row["Date_Started"].ToString(),
                    row["Rescue_Status"].ToString()
                ));

                foreach (var op in operations)
                {
                    MyRescueOperations.Add(op);
                } 
            }

            else
            {
                MessageBox.Show("Failed to load rescue operations.");
            }
        }
    }
}
