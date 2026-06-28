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

        // ------------------------------------------------------------------
        // The properties below back rescuedashboard_mainlayout_view.xaml's
        // three info cards. Same situation as the donation dashboard: the
        // XAML was written against these exact names, but the ViewModel
        // never defined most of them, so WPF's bindings failed silently —
        // nothing displayed even though AssignedOperationsCount and
        // CurrentOperationsCount (above) were being computed correctly the
        // whole time. Neither of those two is actually referenced anywhere
        // in this XAML, which is why the rescue dashboard showed nothing
        // at all for any of its three cards.
        // ------------------------------------------------------------------

        // "Active Disaster Event" card — bound via an ItemsControl, so this
        // needs to be a collection of RescueActiveDisasterEvent, not a
        // single string. The ItemsControl's ItemsSource binds to
        // "DisasterEvent" (XAML name, not to be confused with the
        // Model.DisasterEvent class or this VM's ActiveDisasterEvent string
        // above, which nothing in the XAML actually reads).
        private ObservableCollection<RescueActiveDisasterEvent> _DisasterEvent;
        public ObservableCollection<RescueActiveDisasterEvent> DisasterEvent
        {
            get => _DisasterEvent;
            set { _DisasterEvent = value; OnPropertyChanged(nameof(DisasterEvent)); }
        }

        // "My Assigned Operations" card — now a scrollable list (every
        // operation this volunteer is assigned to), not just the latest one.
        private ObservableCollection<AssignedOperation> _AssignedOperations;
        public ObservableCollection<AssignedOperation> AssignedOperations
        {
            get => _AssignedOperations;
            set { _AssignedOperations = value; OnPropertyChanged(nameof(AssignedOperations)); }
        }

        // "Current Rescue Operations" card — now a scrollable list (one row
        // per operation), not a single aggregated breakdown string.
        private ObservableCollection<CurrentRescueOperationSummary> _CurrentRescueOperations;
        public ObservableCollection<CurrentRescueOperationSummary> CurrentRescueOperations
        {
            get => _CurrentRescueOperations;
            set { _CurrentRescueOperations = value; OnPropertyChanged(nameof(CurrentRescueOperations)); }
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

            DisasterEvent = new ObservableCollection<RescueActiveDisasterEvent>();
            AssignedOperations = new ObservableCollection<AssignedOperation>();
            CurrentRescueOperations = new ObservableCollection<CurrentRescueOperationSummary>();

            // default view on load
            CurrentRescueDashboardView = new View.rescuedashboard_MyRescueOperations_view();
            RefreshDashboard();
        }

        /// <summary>
        /// Re-runs every dashboard query. Safe to call multiple times.
        /// </summary>
        public void RefreshDashboard()
        {
            if (rescue_login_ViewModel.CurrentUser == null || string.IsNullOrWhiteSpace(rescue_login_ViewModel.CurrentUser.Username))
            {
                return;
            }

            InitializeActiveDisasterEvent();
            InitializeAssignedOperationsCount();
            InitializeCurrentOperationsCount();
            InitializeAssignedOperationsCardList();
            InitializeCurrentRescueOperationsCardList();
        }

        /// <summary>
        /// Populates the DisasterEvent collection (the "Active Disaster Event"
        /// card's ItemsControl source) with every disaster event that still
        /// has an open rescue operation against it.
        /// </summary>
        public void InitializeActiveDisasterEvent()
        {
            DisasterEvent.Clear();
            string query = "SELECT * FROM dbo.GetActiveDisasterEvent()";

            if (DatabaseManager.GetTableData(query, null, out DataTable data))
            {
                var events = data.AsEnumerable().Select(row => new RescueActiveDisasterEvent(
                    row["Event_Name"].ToString(),
                    row["Status"].ToString(),
                    row["Event_ID"].ToString(),
                    row["Start_Date"] == DBNull.Value ? "" : Convert.ToDateTime(row["Start_Date"]).ToString("yyyy-MM-dd")
                ));

                foreach (var disasterEvent in events)
                {
                    DisasterEvent.Add(disasterEvent);
                }

                // Keep the legacy single-string property in sync too, in case
                // anything else in the app still reads it directly.
                ActiveDisasterEvent = DisasterEvent.Count > 0 ? DisasterEvent[0].ActiveDisasterEvent : null;
            }
            // No active events is a normal state (e.g. no ongoing disasters
            // right now) — not an error, so no MessageBox here.
        }

        /// <summary>
        /// This method initializes the AssignedOperationsCount property by querying the database for the count of operations assigned to the current user.
        /// </summary>
        public void InitializeAssignedOperationsCount()
        {
            string query = "SELECT * FROM dbo.CountRescueOperationsByUsername(@Username)";
            var parameters = new[] { new SqlParameter("@Username", rescue_login_ViewModel.CurrentUser.Username) };

            DatabaseManager.GetTableData(query, parameters, out DataTable resultTable);

            AssignedOperationsCount = resultTable.Rows.Count > 0
                ? Convert.ToInt32(resultTable.Rows[0]["OperationCount"])
                : 0;
        }

        /// <summary>
        /// This method initializes the CurrentOperationsCount property by querying the database for the count of active operations assigned to the current user.
        /// </summary>
        public void InitializeCurrentOperationsCount()
        {
            string query = "SELECT * FROM dbo.GetCurrentOperationsCountByUsername(@Username)";
            var parameters = new[] { new SqlParameter("@Username", rescue_login_ViewModel.CurrentUser.Username) };

            DatabaseManager.GetTableData(query, parameters, out DataTable opsTable);

            CurrentOperationsCount = opsTable.Rows.Count > 0
                ? Convert.ToInt32(opsTable.Rows[0]["OperationCount"])
                : 0;
        }

        /// <summary>
        /// Populates the "My Assigned Operations" card: every operation this
        /// volunteer is currently assigned to, newest first. The card scrolls
        /// in the XAML, so there's no row limit here.
        /// </summary>
        public void InitializeAssignedOperationsCardList()
        {
            AssignedOperations.Clear();
            string safeUsername = "'" + rescue_login_ViewModel.CurrentUser.Username.Replace("'", "''") + "'";

            string query = $@"
                SELECT
                    oa.Role,
                    l.Barangay + ', ' + l.City AS Location,
                    ro.Operation_ID,
                    oa.Operation_Status,
                    ro.Date_Started
                FROM [Operation Assignment] oa
                JOIN [Volunteer] v ON oa.Volunteer_ID = v.Volunteer_ID
                JOIN [Users] u ON v.User_ID = u.User_ID
                JOIN [Rescue Operation] ro ON oa.Operation_ID = ro.Operation_ID
                JOIN [Location] l ON ro.Location_ID = l.Location_ID
                WHERE u.Username = {safeUsername}
                ORDER BY ro.Date_Started DESC";

            DatabaseManager.GetTableDataWithCustomizedQuery(query, out DataTable data);

            foreach (DataRow row in data.Rows)
            {
                AssignedOperations.Add(new AssignedOperation(
                    row["Role"].ToString(),
                    row["Location"].ToString(),
                    row["Operation_ID"].ToString(),
                    row["Operation_Status"].ToString()));
            }
            // An empty list here (no assigned operations yet) is a normal
            // state for a new volunteer — the XAML's scrollable card just
            // renders empty, no error needed.
        }

        /// <summary>
        /// Populates the "Current Rescue Operations" card: every operation
        /// this volunteer is part of, one row per operation, newest first.
        /// Replaces the old single aggregated "breakdown string" approach
        /// now that the card scrolls — a real list is more useful than a
        /// summary once you can see more than one row at a time.
        /// </summary>
        public void InitializeCurrentRescueOperationsCardList()
        {
            CurrentRescueOperations.Clear();
            string safeUsername = "'" + rescue_login_ViewModel.CurrentUser.Username.Replace("'", "''") + "'";

            string query = $@"
                SELECT DISTINCT
                    ro.Operation_ID,
                    ro.Rescue_Status,
                    l.Barangay + ', ' + l.City AS Location,
                    ro.Date_Started
                FROM [Rescue Operation] ro
                JOIN [Operation Assignment] oa ON oa.Operation_ID = ro.Operation_ID
                JOIN [Volunteer] v ON oa.Volunteer_ID = v.Volunteer_ID
                JOIN [Users] u ON v.User_ID = u.User_ID
                JOIN [Location] l ON ro.Location_ID = l.Location_ID
                WHERE u.Username = {safeUsername}
                ORDER BY ro.Date_Started DESC";

            DatabaseManager.GetTableDataWithCustomizedQuery(query, out DataTable data);

            foreach (DataRow row in data.Rows)
            {
                CurrentRescueOperations.Add(new CurrentRescueOperationSummary(
                    row["Operation_ID"].ToString(),
                    row["Rescue_Status"].ToString(),
                    row["Location"].ToString(),
                    row["Date_Started"] == DBNull.Value ? "" : Convert.ToDateTime(row["Date_Started"]).ToString("yyyy-MM-dd")));
            }
            // Empty list = no operations yet — normal, not an error.
        }

        /// <summary>
        /// This method handles the logout functionality by creating a new instance of the MainWindow, setting its content to the rescue_login_view, closing the current main window, and showing the new main window. This effectively logs the user out and returns them to the login screen.
        /// </summary>
        public void Logout()
        {
            // Clears the cached Google credential so the next "Sign in with
            // Google" click (on either portal) re-prompts instead of
            // silently reusing this session's Google account.
            _ = GoogleAuthHelper.SignOutAsync();

            var mainWindow = new MainWindow();
            mainWindow.Content = new View.rescue_login_view();
            Application.Current.MainWindow.Close();
            mainWindow.Show();
            Application.Current.MainWindow = mainWindow;
        }
    }
}