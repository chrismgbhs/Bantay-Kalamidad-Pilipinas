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
            string query = "SELECT * FROM dbo.GetRescueOperationsByUsername(@username)";
            var parameters = new[] { new SqlParameter("@Username", rescue_login_ViewModel.CurrentUser.Username) };
            if (DatabaseManager.GetTableData(query, parameters, out DataTable data))
            {
                var operations = data.AsEnumerable().Select(row => new RescueOperation(
                    row["Operation_ID"].ToString(),
                    row["Event_Name"].ToString(),
                    row["Province"].ToString(),
                    row["Date_Started"].ToString(),
                    row["Rescue_Status"].ToString()
                ));

                foreach (var op in operations)
                {
                    //MessageBox.Show(op.Location);
                    MyRescueOperations.Add(op);
                } 
            }

            else
            {
                MessageBox.Show("No available rescue operations.");
            }
        }
    }
}
