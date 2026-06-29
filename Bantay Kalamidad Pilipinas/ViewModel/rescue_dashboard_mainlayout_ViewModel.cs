using Bantay_Kalamidad_Pilipinas.Model;
using Bantay_Kalamidad_Pilipinas.View;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class rescue_dashboard_mainlayout_ViewModel : ObservableObject
    {
        private const string MyRescueOperationsTab = "MyRescueOperations";
        private const string MyTeamTab = "MyTeam";
        private const string RescueLocationsTab = "RescueLocations";
        private const string AnnouncementsTab = "Announcements";

        private string _activeTab;
        private string _welcomeText;
        private string _ActiveDisasterEvent;
        private int _AssignedOperationsCount;
        private int _CurrentOperationsCount;
        private ObservableCollection<RescueActiveDisasterEvent> _DisasterEvent;
        private ObservableCollection<AssignedOperation> _AssignedOperations;
        private ObservableCollection<CurrentRescueOperationSummary> _CurrentRescueOperations;
        private object _currentRescueDashboardView;

        public string WelcomeText
        {
            get { return _welcomeText; }
            set
            {
                _welcomeText = value;
                OnPropertyChanged(nameof(WelcomeText));
            }
        }

        public TextDecorationCollection MyRescueOperationsUnderline
        {
            get { return _activeTab == MyRescueOperationsTab ? TextDecorations.Underline : null; }
        }

        public TextDecorationCollection MyTeamUnderline
        {
            get { return _activeTab == MyTeamTab ? TextDecorations.Underline : null; }
        }

        public TextDecorationCollection RescueLocationsUnderline
        {
            get { return _activeTab == RescueLocationsTab ? TextDecorations.Underline : null; }
        }

        public TextDecorationCollection AnnouncementsUnderline
        {
            get { return _activeTab == AnnouncementsTab ? TextDecorations.Underline : null; }
        }

        public FontWeight MyRescueOperationsFontWeight
        {
            get { return _activeTab == MyRescueOperationsTab ? FontWeights.Black : FontWeights.Bold; }
        }

        public FontWeight MyTeamFontWeight
        {
            get { return _activeTab == MyTeamTab ? FontWeights.Black : FontWeights.Bold; }
        }

        public FontWeight RescueLocationsFontWeight
        {
            get { return _activeTab == RescueLocationsTab ? FontWeights.Black : FontWeights.Bold; }
        }

        public FontWeight AnnouncementsFontWeight
        {
            get { return _activeTab == AnnouncementsTab ? FontWeights.Black : FontWeights.Bold; }
        }

        public int CurrentOperationsCount
        {
            get { return _CurrentOperationsCount; }
            set
            {
                _CurrentOperationsCount = value;
                OnPropertyChanged(nameof(CurrentOperationsCount));
            }
        }

        public int AssignedOperationsCount
        {
            get { return _AssignedOperationsCount; }
            set
            {
                _AssignedOperationsCount = value;
                OnPropertyChanged(nameof(AssignedOperationsCount));
            }
        }

        public string ActiveDisasterEvent
        {
            get { return _ActiveDisasterEvent; }
            set
            {
                _ActiveDisasterEvent = value;
                OnPropertyChanged(nameof(ActiveDisasterEvent));
            }
        }

        public ObservableCollection<RescueActiveDisasterEvent> DisasterEvent
        {
            get => _DisasterEvent;
            set
            {
                _DisasterEvent = value;
                OnPropertyChanged(nameof(DisasterEvent));
            }
        }

        public ObservableCollection<AssignedOperation> AssignedOperations
        {
            get => _AssignedOperations;
            set
            {
                _AssignedOperations = value;
                OnPropertyChanged(nameof(AssignedOperations));
            }
        }

        public ObservableCollection<CurrentRescueOperationSummary> CurrentRescueOperations
        {
            get => _CurrentRescueOperations;
            set
            {
                _CurrentRescueOperations = value;
                OnPropertyChanged(nameof(CurrentRescueOperations));
            }
        }

        public object CurrentRescueDashboardView
        {
            get => _currentRescueDashboardView;
            set
            {
                _currentRescueDashboardView = value;
                OnPropertyChanged(nameof(CurrentRescueDashboardView));
            }
        }

        public ICommand ShowMyRescueOperationsCommand { get; }
        public ICommand ShowMyTeamCommand { get; }
        public ICommand ShowRescueLocationsCommand { get; }
        public ICommand ShowAnnouncementsCommand { get; }
        public ICommand LogoutCommand { get; }

        public rescue_dashboard_mainlayout_ViewModel()
        {
            WelcomeText = "Welcome, Rescuer!";

            LogoutCommand = new RelayCommand(Logout);

            ShowMyRescueOperationsCommand = new RelayCommand(ShowMyRescueOperations);
            ShowMyTeamCommand = new RelayCommand(ShowMyTeam);
            ShowRescueLocationsCommand = new RelayCommand(ShowRescueLocations);
            ShowAnnouncementsCommand = new RelayCommand(ShowAnnouncements);

            DisasterEvent = new ObservableCollection<RescueActiveDisasterEvent>();
            AssignedOperations = new ObservableCollection<AssignedOperation>();
            CurrentRescueOperations = new ObservableCollection<CurrentRescueOperationSummary>();

            ShowMyRescueOperations();

            InitializeWelcomeText();
            RefreshDashboard();
        }

        private void SetActiveTab(string tabName)
        {
            _activeTab = tabName;

            OnPropertyChanged(nameof(MyRescueOperationsUnderline));
            OnPropertyChanged(nameof(MyTeamUnderline));
            OnPropertyChanged(nameof(RescueLocationsUnderline));
            OnPropertyChanged(nameof(AnnouncementsUnderline));

            OnPropertyChanged(nameof(MyRescueOperationsFontWeight));
            OnPropertyChanged(nameof(MyTeamFontWeight));
            OnPropertyChanged(nameof(RescueLocationsFontWeight));
            OnPropertyChanged(nameof(AnnouncementsFontWeight));
        }

        private void ShowMyRescueOperations()
        {
            CurrentRescueDashboardView = new rescuedashboard_MyRescueOperations_view();
            SetActiveTab(MyRescueOperationsTab);
        }

        private void ShowMyTeam()
        {
            CurrentRescueDashboardView = new rescuedashboard_MyTeam_view();
            SetActiveTab(MyTeamTab);
        }

        private void ShowRescueLocations()
        {
            CurrentRescueDashboardView = new rescuedashboard_RescueLocations_view();
            SetActiveTab(RescueLocationsTab);
        }

        private void ShowAnnouncements()
        {
            CurrentRescueDashboardView = new rescuedashboard_Announcements_view();
            SetActiveTab(AnnouncementsTab);
        }

        public void RefreshDashboard()
        {
            if (rescue_login_ViewModel.CurrentUser == null ||
                string.IsNullOrWhiteSpace(rescue_login_ViewModel.CurrentUser.Username))
            {
                InitializeWelcomeText();
                return;
            }

            InitializeWelcomeText();
            InitializeActiveDisasterEvent();
            InitializeAssignedOperationsCount();
            InitializeCurrentOperationsCount();
            InitializeAssignedOperationsCardList();
            InitializeCurrentRescueOperationsCardList();
        }

        public void InitializeWelcomeText()
        {
            try
            {
                if (rescue_login_ViewModel.CurrentUser == null ||
                    string.IsNullOrWhiteSpace(rescue_login_ViewModel.CurrentUser.Username))
                {
                    WelcomeText = "Welcome, Rescuer!";
                    return;
                }

                string username = rescue_login_ViewModel.CurrentUser.Username;

                string query = @"
                    SELECT TOP 1
                        v.Volunteer_Name
                    FROM [Users] u
                    LEFT JOIN [Volunteer] v
                        ON u.User_ID = v.User_ID
                    WHERE u.Username = @Username;";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@Username", username)
                };

                if (DatabaseManager.GetTableData(query, parameters, out DataTable data) &&
                    data.Rows.Count > 0 &&
                    data.Rows[0]["Volunteer_Name"] != DBNull.Value)
                {
                    string volunteerName = data.Rows[0]["Volunteer_Name"].ToString();

                    if (!string.IsNullOrWhiteSpace(volunteerName))
                    {
                        WelcomeText = "Welcome, " + volunteerName + "!";
                        return;
                    }
                }

                WelcomeText = "Welcome, " + username + "!";
            }
            catch
            {
                WelcomeText = "Welcome, Rescuer!";
            }
        }

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
                    row["Start_Date"] == DBNull.Value
                        ? ""
                        : Convert.ToDateTime(row["Start_Date"]).ToString("yyyy-MM-dd")
                ));

                foreach (var disasterEvent in events)
                {
                    DisasterEvent.Add(disasterEvent);
                }

                ActiveDisasterEvent = DisasterEvent.Count > 0
                    ? DisasterEvent[0].ActiveDisasterEvent
                    : null;
            }
        }

        public void InitializeAssignedOperationsCount()
        {
            if (rescue_login_ViewModel.CurrentUser == null ||
                string.IsNullOrWhiteSpace(rescue_login_ViewModel.CurrentUser.Username))
            {
                AssignedOperationsCount = 0;
                return;
            }

            string query = "SELECT * FROM dbo.CountRescueOperationsByUsername(@Username)";

            SqlParameter[] parameters =
            {
                new SqlParameter("@Username", rescue_login_ViewModel.CurrentUser.Username)
            };

            DatabaseManager.GetTableData(query, parameters, out DataTable resultTable);

            AssignedOperationsCount = resultTable.Rows.Count > 0
                ? Convert.ToInt32(resultTable.Rows[0]["OperationCount"])
                : 0;
        }

        public void InitializeCurrentOperationsCount()
        {
            if (rescue_login_ViewModel.CurrentUser == null ||
                string.IsNullOrWhiteSpace(rescue_login_ViewModel.CurrentUser.Username))
            {
                CurrentOperationsCount = 0;
                return;
            }

            string query = "SELECT * FROM dbo.GetCurrentOperationsCountByUsername(@Username)";

            SqlParameter[] parameters =
            {
                new SqlParameter("@Username", rescue_login_ViewModel.CurrentUser.Username)
            };

            DatabaseManager.GetTableData(query, parameters, out DataTable opsTable);

            CurrentOperationsCount = opsTable.Rows.Count > 0
                ? Convert.ToInt32(opsTable.Rows[0]["OperationCount"])
                : 0;
        }

        public void InitializeAssignedOperationsCardList()
        {
            AssignedOperations.Clear();

            if (rescue_login_ViewModel.CurrentUser == null ||
                string.IsNullOrWhiteSpace(rescue_login_ViewModel.CurrentUser.Username))
            {
                return;
            }

            string query = @"
                SELECT
                    oa.Role,
                    l.Barangay + ', ' + l.City AS Location,
                    ro.Operation_ID,
                    oa.Operation_Status,
                    ro.Date_Started
                FROM [Operation Assignment] oa
                JOIN [Volunteer] v
                    ON oa.Volunteer_ID = v.Volunteer_ID
                JOIN [Users] u
                    ON v.User_ID = u.User_ID
                JOIN [Rescue Operation] ro
                    ON oa.Operation_ID = ro.Operation_ID
                JOIN [Location] l
                    ON ro.Location_ID = l.Location_ID
                WHERE u.Username = @Username
                ORDER BY ro.Date_Started DESC;";

            SqlParameter[] parameters =
            {
                new SqlParameter("@Username", rescue_login_ViewModel.CurrentUser.Username)
            };

            if (DatabaseManager.GetTableData(query, parameters, out DataTable data))
            {
                foreach (DataRow row in data.Rows)
                {
                    AssignedOperations.Add(new AssignedOperation(
                        row["Role"].ToString(),
                        row["Location"].ToString(),
                        row["Operation_ID"].ToString(),
                        row["Operation_Status"].ToString()));
                }
            }
        }

        public void InitializeCurrentRescueOperationsCardList()
        {
            CurrentRescueOperations.Clear();

            if (rescue_login_ViewModel.CurrentUser == null ||
                string.IsNullOrWhiteSpace(rescue_login_ViewModel.CurrentUser.Username))
            {
                return;
            }

            string query = @"
                SELECT DISTINCT
                    ro.Operation_ID,
                    ro.Rescue_Status,
                    l.Barangay + ', ' + l.City AS Location,
                    ro.Date_Started
                FROM [Rescue Operation] ro
                JOIN [Operation Assignment] oa
                    ON oa.Operation_ID = ro.Operation_ID
                JOIN [Volunteer] v
                    ON oa.Volunteer_ID = v.Volunteer_ID
                JOIN [Users] u
                    ON v.User_ID = u.User_ID
                JOIN [Location] l
                    ON ro.Location_ID = l.Location_ID
                WHERE u.Username = @Username
                ORDER BY ro.Date_Started DESC;";

            SqlParameter[] parameters =
            {
                new SqlParameter("@Username", rescue_login_ViewModel.CurrentUser.Username)
            };

            if (DatabaseManager.GetTableData(query, parameters, out DataTable data))
            {
                foreach (DataRow row in data.Rows)
                {
                    CurrentRescueOperations.Add(new CurrentRescueOperationSummary(
                        row["Operation_ID"].ToString(),
                        row["Rescue_Status"].ToString(),
                        row["Location"].ToString(),
                        row["Date_Started"] == DBNull.Value
                            ? ""
                            : Convert.ToDateTime(row["Date_Started"]).ToString("yyyy-MM-dd")));
                }
            }
        }

        public void Logout()
        {
            _ = GoogleAuthHelper.SignOutAsync();

            rescue_login_ViewModel.CurrentUser = new UserModel();

            Window currentWindow = Application.Current.MainWindow;

            var mainWindow = new MainWindow();
            mainWindow.Content = new rescue_login_view();

            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();

            currentWindow?.Close();
        }
    }
}