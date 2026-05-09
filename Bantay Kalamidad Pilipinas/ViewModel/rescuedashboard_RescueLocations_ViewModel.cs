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
    internal class rescuedashboard_RescueLocations_ViewModel : ObservableObject
    {
        private ObservableCollection<RescueLocation> _RescueLocations;

        public ObservableCollection<RescueLocation> RescueLocations
        {
            get => _RescueLocations;
            set { _RescueLocations = value; OnPropertyChanged(nameof(RescueLocations)); }
        }

        public rescuedashboard_RescueLocations_ViewModel()
        {
            RescueLocations = new ObservableCollection<RescueLocation>();
            InitializeRescueLocations();
        }

        /// <summary>
        /// This method retrieves rescue location data from the database based on the current user's assigned rescue operations and populates the RescueLocations collection. It executes a SQL query that joins multiple tables to gather the necessary information, including barangay, city, province, evacuation center name, and rescue status. If the data retrieval is successful, it creates RescueLocation objects and adds them to the collection; otherwise, it displays an error message.
        /// </summary>
        public void InitializeRescueLocations()
        {
            string query = "SELECT * FROM dbo.GetRescueLocationsByUsername(@Username)";
            var parameters = new[] { new SqlParameter("@Username", rescue_login_ViewModel.CurrentUser.Username) };

            if (DatabaseManager.GetTableData(query, parameters, out DataTable data))
            {
                var operations = data.AsEnumerable().Select(row => new RescueLocation(
                    row["Barangay"].ToString(),
                    row["City"].ToString(),
                    row["Province"].ToString(),
                    row["Center_Name"].ToString(),
                    row["Rescue_Status"].ToString()
                ));

                foreach (var op in operations)
                {
                    //MessageBox.Show(op.Location);
                    RescueLocations.Add(op);
                }
            }

            else
            {
                MessageBox.Show("Failed to load rescue locations.");
            }
        }
    }
}
