using Bantay_Kalamidad_Pilipinas.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class rescue_dashboard_mainlayout_ViewModel : ObservableObject
    {
        private string _ActiveDisasterEvent;
        private int _AssignedOperationsCount;
        private int _CurrentOperationsCount;

        public int CurrentOperationsCount
        {
            get { return _CurrentOperationsCount; }
            set { _CurrentOperationsCount = value; OnPropertyChanged(nameof(CurrentOperationsCount)); }
        }

        public int AssignedOperationsCount
        {
            get { return _AssignedOperationsCount; }
            set { _AssignedOperationsCount = value; OnPropertyChanged(nameof(AssignedOperationsCount)); }
        }
        public string ActiveDisasterEvent
        {
            get { return _ActiveDisasterEvent; }
            set { _ActiveDisasterEvent = value; OnPropertyChanged(nameof(ActiveDisasterEvent)); }
        }

        private object _currentRescueDashboardView;
        public object CurrentRescueDashboardView
        {
            get => _currentRescueDashboardView;
            set { _currentRescueDashboardView = value; OnPropertyChanged(nameof(CurrentRescueDashboardView)); }
        }

        public ICommand ShowMyRescueOperationsCommand { get; }
        public ICommand ShowMyTeamCommand { get; }
        public ICommand ShowRescueLocationsCommand { get; }
        public ICommand ShowAnnouncementsCommand { get; }
        public ICommand LogoutCommand { get; }

        public rescue_dashboard_mainlayout_ViewModel()
        {
            LogoutCommand = new RelayCommand(Logout);
            ShowMyRescueOperationsCommand = new RelayCommand(() => CurrentRescueDashboardView = new View.rescuedashboard_MyRescueOperations_view());
            ShowMyTeamCommand = new RelayCommand(() => CurrentRescueDashboardView = new View.rescuedashboard_MyTeam_view());
            ShowRescueLocationsCommand = new RelayCommand(() => CurrentRescueDashboardView = new View.rescuedashboard_RescueLocations_view());
            ShowAnnouncementsCommand = new RelayCommand(() => CurrentRescueDashboardView = new View.rescuedashboard_Announcements_view());

            // default view on load
            CurrentRescueDashboardView = new View.rescuedashboard_MyRescueOperations_view();
            InitializeActiveDisasterEvent(); 
            InitializeAssignedOperationsCount();
            InitializeCurrentOperationsCount();
        }

        /// <summary>
        /// This method initializes the ActiveDisasterEvent property by querying the database for all disaster events associated with ongoing rescue operations. It retrieves the event names and assigns them to the ActiveDisasterEvent property. If the query fails, it displays an error message.
        /// </summary>
        public void InitializeActiveDisasterEvent()
        {
            string query = "SELECT * FROM dbo.GetActiveDisasterEvent()";
            if (DatabaseManager.GetTableData(query, null, out DataTable data))
            {
                var disasterEvents = data.AsEnumerable().Select(row => new DisasterEvent(
                    row["Event_Name"].ToString(),
                    "Filler"
                ));

                foreach (var disasterEvent in disasterEvents)
                {
                    MessageBox.Show(disasterEvent.Name);
                    ActiveDisasterEvent = disasterEvent.Name;
                }
            }
        }

        /// <summary>
        /// This method initializes the AssignedOperationsCount property by querying the database for the count of operations assigned to the current user. It retrieves the count and assigns it to the AssignedOperationsCount property. If the query fails, it displays an error message.
        /// </summary>
        public void InitializeAssignedOperationsCount()
        {
            string query = "SELECT * FROM dbo.CountRescueOperationsByUsername(@Username)";
            var parameters = new[] { new SqlParameter("@Username", rescue_login_ViewModel.CurrentUser.Username) };
            if (DatabaseManager.GetTableData(query, parameters, out DataTable resultTable))
            {
                if (resultTable.Rows.Count > 0)
                {
                    AssignedOperationsCount = Convert.ToInt32(resultTable.Rows[0]["OperationCount"]);
                }
                else
                {
                    AssignedOperationsCount = 0;
                }
            }
            else
            {
                MessageBox.Show("Failed to load assigned operations count.");
            }
        }

        /// <summary>
        /// This method initializes the CurrentOperationsCount property by querying the database for the count of active operations assigned to the current user. It retrieves the count and assigns it to the CurrentOperationsCount property. If the query fails, it displays an error message.
        /// </summary>
        public void InitializeCurrentOperationsCount() 
        {
            string assignedOpsQuery = "SELECT * FROM dbo.GetCurrentOperationsCountByUsername(@Username)";
            var parameters = new[] { new SqlParameter("@Username", rescue_login_ViewModel.CurrentUser.Username) };
            if (DatabaseManager.GetTableData(assignedOpsQuery, parameters, out DataTable opsTable))
            {
                if (opsTable.Rows.Count > 0)
                {
                    CurrentOperationsCount = Convert.ToInt32(opsTable.Rows[0]["OperationCount"]);
                }
                else
                {
                    CurrentOperationsCount = 0;
                }
            }
            else
            {
                MessageBox.Show("Failed to load current operations count.");
            }
        }

        /// <summary>
        /// This method handles the logout functionality by creating a new instance of the MainWindow, setting its content to the rescue_login_view, closing the current main window, and showing the new main window. This effectively logs the user out and returns them to the login screen.
        /// </summary>
        public void Logout()
        {
            var mainWindow = new MainWindow();
            mainWindow.Content = new View.rescue_login_view();
            Application.Current.MainWindow.Close();
            mainWindow.Show();
        }
    }
}
