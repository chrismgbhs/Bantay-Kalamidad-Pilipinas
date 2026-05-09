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
    internal class donationdashboard_Pickup_Delivery_ViewModel : ObservableObject
    {
        private ObservableCollection<PickupSchedule> _PickupSchedules;
        public ObservableCollection<PickupSchedule> PickupSchedules
        {
            get => _PickupSchedules;
            set { _PickupSchedules = value; OnPropertyChanged(nameof(PickupSchedules)); }
        }

        private ObservableCollection<DeliveryStatus> _DeliveryStatuses;
        public ObservableCollection<DeliveryStatus> DeliveryStatuses
        {
            get => _DeliveryStatuses;
            set { _DeliveryStatuses = value; OnPropertyChanged(nameof(DeliveryStatuses)); }
        }

        public donationdashboard_Pickup_Delivery_ViewModel()
        {
            PickupSchedules = new ObservableCollection<PickupSchedule>();
            DeliveryStatuses = new ObservableCollection<DeliveryStatus>();
            InitializePickupSchedules();
            InitializeDeliveryStatuses();
        }

        /// <summary>
        /// This method initializes the PickupSchedules collection by fetching data from the database based on the current user's username. It executes a SQL query to retrieve pickup schedule information and populates the collection with the results. If no schedules are available, it displays a message box to inform the user.
        /// </summary>
        public void InitializePickupSchedules()
        {
            PickupSchedules.Clear();
            string query = "SELECT * FROM dbo.GetPickupScheduleByUsername(@Username)";
            var parameters = new[] { new SqlParameter("@Username", donation_login_ViewModel.CurrentUser.Username)};

                if (DatabaseManager.GetTableData(query, parameters, out DataTable data))
                {
                    var schedules = data.AsEnumerable().Select(row => new PickupSchedule(
                        row["Donation_ID"].ToString(),
                        Convert.ToDateTime(row["Pickup_Date"]).Date.ToString("yyyy-MM-dd"),
                        row["Pickup_Status"].ToString()
                    ));

                    foreach (var schedule in schedules)
                    {
                        PickupSchedules.Add(schedule);
                    }
                }

                else
                {
                    MessageBox.Show("No available schedule.");
                }
        }

        /// <summary>
        /// This method initializes the DeliveryStatuses collection by fetching data from the database based on the current user's username. It executes a SQL query to retrieve delivery status information and populates the collection with the results. If no delivery statuses are available, it displays a message box to inform the user.
        /// </summary>
        public void InitializeDeliveryStatuses()
        {
            DeliveryStatuses.Clear();
            string query = "SELECT * FROM dbo.GetDeliveryStatusByUsername(@Username)";
            var parameters = new[] { new SqlParameter("@Username", donation_login_ViewModel.CurrentUser.Username) };

            if (DatabaseManager.GetTableData(query, parameters, out DataTable data))
            {
                var statuses = data.AsEnumerable().Select(row => new DeliveryStatus(
                    row["Donation_ID"].ToString(),
                    Convert.ToDateTime(row["Delivery_Date"]).Date.ToString("yyyy-MM-dd"),
                    row["Delivery_Status"].ToString()
                ));

                foreach (var status in statuses)
                {
                    DeliveryStatuses.Add(status);
                }
            }

            else
            {
                MessageBox.Show("No available delivery status.");
            }
        }
    }
}
