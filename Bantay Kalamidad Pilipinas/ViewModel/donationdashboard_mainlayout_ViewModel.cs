using Bantay_Kalamidad_Pilipinas.Model;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Bantay_Kalamidad_Pilipinas.View;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class donationdashboard_mainlayout_ViewModel : ObservableObject
    {
        private const string MyDonationsTab = "MyDonations";
        private const string PledgeTab = "Pledge";
        private const string PickupDeliveryTab = "PickupDelivery";
        private const string AboutUsTab = "AboutUs";
        private const string ContactUsTab = "ContactUs";

        private string _activeTab;

        private string _WelcomeText = "Welcome, Donor!";
        public string WelcomeText
        {
            get { return _WelcomeText; }
            set
            {
                _WelcomeText = value;
                OnPropertyChanged(nameof(WelcomeText));
            }
        }

        public TextDecorationCollection MyDonationsUnderline
        {
            get { return _activeTab == MyDonationsTab ? TextDecorations.Underline : null; }
        }

        public TextDecorationCollection PledgeUnderline
        {
            get { return _activeTab == PledgeTab ? TextDecorations.Underline : null; }
        }

        public TextDecorationCollection PickupDeliveryUnderline
        {
            get { return _activeTab == PickupDeliveryTab ? TextDecorations.Underline : null; }
        }

        public TextDecorationCollection AboutUsUnderline
        {
            get { return _activeTab == AboutUsTab ? TextDecorations.Underline : null; }
        }

        public TextDecorationCollection ContactUsUnderline
        {
            get { return _activeTab == ContactUsTab ? TextDecorations.Underline : null; }
        }

        public FontWeight MyDonationsFontWeight
        {
            get { return _activeTab == MyDonationsTab ? FontWeights.Black : FontWeights.Bold; }
        }

        public FontWeight PledgeFontWeight
        {
            get { return _activeTab == PledgeTab ? FontWeights.Black : FontWeights.Bold; }
        }

        public FontWeight PickupDeliveryFontWeight
        {
            get { return _activeTab == PickupDeliveryTab ? FontWeights.Black : FontWeights.Bold; }
        }

        public FontWeight AboutUsFontWeight
        {
            get { return _activeTab == AboutUsTab ? FontWeights.Black : FontWeights.Bold; }
        }

        public FontWeight ContactUsFontWeight
        {
            get { return _activeTab == ContactUsTab ? FontWeights.Black : FontWeights.Bold; }
        }

        private int _TotalDonationsCount;
        public int TotalDonationsCount
        {
            get { return _TotalDonationsCount; }
            set { _TotalDonationsCount = value; OnPropertyChanged(nameof(TotalDonationsCount)); }
        }

        private int _ActivePledgesCount;
        public int ActivePledgesCount
        {
            get { return _ActivePledgesCount; }
            set { _ActivePledgesCount = value; OnPropertyChanged(nameof(ActivePledgesCount)); }
        }

        private int _ScheduledPickupsCount;
        public int ScheduledPickupsCount
        {
            get { return _ScheduledPickupsCount; }
            set { _ScheduledPickupsCount = value; OnPropertyChanged(nameof(ScheduledPickupsCount)); }
        }

        private int _TotalItemCount;
        public int TotalItemCount
        {
            get => _TotalItemCount;
            set { _TotalItemCount = value; OnPropertyChanged(nameof(TotalItemCount)); }
        }

        private string _LastDonationDate;
        public string LastDonationDate
        {
            get => _LastDonationDate;
            set { _LastDonationDate = value; OnPropertyChanged(nameof(LastDonationDate)); }
        }

        private string _ImpactSummary;
        public string ImpactSummary
        {
            get => _ImpactSummary;
            set { _ImpactSummary = value; OnPropertyChanged(nameof(ImpactSummary)); }
        }

        private string _NextPendingItem;
        public string NextPendingItem
        {
            get => _NextPendingItem;
            set { _NextPendingItem = value; OnPropertyChanged(nameof(NextPendingItem)); }
        }

        private string _PledgeCompletionRate;
        public string PledgeCompletionRate
        {
            get => _PledgeCompletionRate;
            set { _PledgeCompletionRate = value; OnPropertyChanged(nameof(PledgeCompletionRate)); }
        }

        private string _NextPickupDateTime;
        public string NextPickupDateTime
        {
            get => _NextPickupDateTime;
            set { _NextPickupDateTime = value; OnPropertyChanged(nameof(NextPickupDateTime)); }
        }

        private string _PickupLocation;
        public string PickupLocation
        {
            get => _PickupLocation;
            set { _PickupLocation = value; OnPropertyChanged(nameof(PickupLocation)); }
        }

        private string _NextDropOffDateTime = "Not yet scheduled";
        public string NextDropOffDateTime
        {
            get => _NextDropOffDateTime;
            set { _NextDropOffDateTime = value; OnPropertyChanged(nameof(NextDropOffDateTime)); }
        }

        private string _DestinationHub = "—";
        public string DestinationHub
        {
            get => _DestinationHub;
            set { _DestinationHub = value; OnPropertyChanged(nameof(DestinationHub)); }
        }

        private string _DeliveryStatus = "—";
        public string DeliveryStatus
        {
            get => _DeliveryStatus;
            set { _DeliveryStatus = value; OnPropertyChanged(nameof(DeliveryStatus)); }
        }

        private object _currentDonationDashboardView;
        public object CurrentDonationDashboardView
        {
            get => _currentDonationDashboardView;
            set { _currentDonationDashboardView = value; OnPropertyChanged(nameof(CurrentDonationDashboardView)); }
        }

        private bool _isPledgeMenuOpen;
        public bool IsPledgeMenuOpen
        {
            get => _isPledgeMenuOpen;
            set
            {
                _isPledgeMenuOpen = value;
                OnPropertyChanged(nameof(IsPledgeMenuOpen));
            }
        }

        public ICommand ShowMyDonationsCommand { get; set; }
        public ICommand TogglePledgeMenuCommand { get; set; }
        public ICommand ShowMyPledgesCommand { get; set; }
        public ICommand ShowPickupDeliveryCommand { get; set; }
        public ICommand ShowAboutUsCommand { get; set; }
        public ICommand ShowContactUsCommand { get; set; }
        public ICommand ShowMakePledgeCommand { get; set; }
        public ICommand LogoutCommand { get; }

        public static event Action DonationDataChanged;

        public donationdashboard_mainlayout_ViewModel()
        {
            ShowMyDonationsCommand = new RelayCommand(ShowMyDonations);
            ShowMyPledgesCommand = new RelayCommand(ShowMyPledges);
            ShowPickupDeliveryCommand = new RelayCommand(ShowPickupDelivery);
            ShowAboutUsCommand = new RelayCommand(ShowAboutUs);
            ShowContactUsCommand = new RelayCommand(ShowContactUs);
            ShowMakePledgeCommand = new RelayCommand(ShowMakePledge);
            TogglePledgeMenuCommand = new RelayCommand(TogglePledgeMenu);
            LogoutCommand = new RelayCommand(Logout);

            DonationDataChanged += OnDonationDataChanged;

            ShowMyDonations();

            InitializeWelcomeText();
            RefreshCounters();
        }

        private void SetActiveTab(string tabName)
        {
            _activeTab = tabName;

            OnPropertyChanged(nameof(MyDonationsUnderline));
            OnPropertyChanged(nameof(PledgeUnderline));
            OnPropertyChanged(nameof(PickupDeliveryUnderline));
            OnPropertyChanged(nameof(AboutUsUnderline));
            OnPropertyChanged(nameof(ContactUsUnderline));

            OnPropertyChanged(nameof(MyDonationsFontWeight));
            OnPropertyChanged(nameof(PledgeFontWeight));
            OnPropertyChanged(nameof(PickupDeliveryFontWeight));
            OnPropertyChanged(nameof(AboutUsFontWeight));
            OnPropertyChanged(nameof(ContactUsFontWeight));
        }

        private void ShowMyDonations()
        {
            CurrentDonationDashboardView = new donationdashboard_MyDonations_view();
            IsPledgeMenuOpen = false;
            SetActiveTab(MyDonationsTab);
        }

        private void ShowMyPledges()
        {
            CurrentDonationDashboardView = new donationdashboard_MyPledges_view();
            IsPledgeMenuOpen = false;
            SetActiveTab(PledgeTab);
        }

        private void ShowMakePledge()
        {
            CurrentDonationDashboardView = new donationdashboard_MakeAPledge_view();
            IsPledgeMenuOpen = false;
            SetActiveTab(PledgeTab);
        }

        private void ShowPickupDelivery()
        {
            CurrentDonationDashboardView = new donationdashboard_Pickup_Delivery_view();
            IsPledgeMenuOpen = false;
            SetActiveTab(PickupDeliveryTab);
        }

        private void ShowAboutUs()
        {
            CurrentDonationDashboardView = new donationdashboard_AboutUs_view();
            IsPledgeMenuOpen = false;
            SetActiveTab(AboutUsTab);
        }

        private void ShowContactUs()
        {
            CurrentDonationDashboardView = new donationdashboard_ContactUs_view();
            IsPledgeMenuOpen = false;
            SetActiveTab(ContactUsTab);
        }

        public static void NotifyDonationDataChanged()
        {
            DonationDataChanged?.Invoke();
        }

        private void OnDonationDataChanged()
        {
            InitializeWelcomeText();
            RefreshCounters();
        }

        public void InitializeWelcomeText()
        {
            try
            {
                if (donation_login_ViewModel.CurrentUser == null ||
                    string.IsNullOrWhiteSpace(donation_login_ViewModel.CurrentUser.Username))
                {
                    WelcomeText = "Welcome, Donor!";
                    return;
                }

                string username = donation_login_ViewModel.CurrentUser.Username;

                string query = @"
                    SELECT TOP 1
                        d.Donor_Name
                    FROM [Users] u
                    LEFT JOIN [Donor] d
                        ON u.User_ID = d.User_ID
                    WHERE u.Username = @Username;";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@Username", username)
                };

                if (DatabaseManager.GetTableData(query, parameters, out DataTable data) &&
                    data.Rows.Count > 0 &&
                    data.Rows[0]["Donor_Name"] != DBNull.Value)
                {
                    string donorName = data.Rows[0]["Donor_Name"].ToString();

                    if (!string.IsNullOrWhiteSpace(donorName))
                    {
                        WelcomeText = "Welcome, " + donorName + "!";
                        return;
                    }
                }

                WelcomeText = "Welcome, " + username + "!";
            }
            catch
            {
                WelcomeText = "Welcome, Donor!";
            }
        }

        public void RefreshCounters()
        {
            if (donation_login_ViewModel.CurrentUser == null ||
                string.IsNullOrWhiteSpace(donation_login_ViewModel.CurrentUser.Username))
            {
                InitializeEmptyDashboardCards();
                return;
            }

            InitializeWelcomeText();
            InitializeTotalDonationsCount();
            InitializeActivePledgesCount();
            InitializeScheduledPickupsCount();
            InitializeDonationCardDetails();
            InitializePledgeCardDetails();
            InitializePickupCardDetails();
        }

        private void InitializeEmptyDashboardCards()
        {
            TotalDonationsCount = 0;
            ActivePledgesCount = 0;
            ScheduledPickupsCount = 0;

            TotalItemCount = 0;
            LastDonationDate = "No donations yet";
            ImpactSummary = "Your contributions will appear here once a donation is recorded.";

            NextPendingItem = "None pending";
            PledgeCompletionRate = "No pledges yet";

            NextPickupDateTime = "None scheduled";
            PickupLocation = "Not tracked yet";

            NextDropOffDateTime = "Not yet scheduled";
            DestinationHub = "—";
            DeliveryStatus = "—";
        }

        public void Logout()
        {
            DonationDataChanged -= OnDonationDataChanged;

            _ = GoogleAuthHelper.SignOutAsync();

            donation_login_ViewModel.CurrentUser = new UserModel();

            Window currentWindow = Application.Current.MainWindow;

            var mainWindow = new MainWindow();
            mainWindow.Content = new donation_login_view();

            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();

            currentWindow?.Close();
        }

        public void InitializeTotalDonationsCount()
        {
            string query = "SELECT * FROM dbo.TotalDonationsCount(@Username)";
            var parameters = new[]
            {
                new SqlParameter("@Username", donation_login_ViewModel.CurrentUser.Username)
            };

            DatabaseManager.GetTableData(query, parameters, out DataTable data);

            TotalDonationsCount = data.Rows.Count > 0
                ? Convert.ToInt32(data.Rows[0]["DonationsCount"])
                : 0;
        }

        public void InitializeActivePledgesCount()
        {
            string query = "SELECT * FROM dbo.GetActivePledgesCount(@Username)";
            var parameters = new[]
            {
                new SqlParameter("@Username", donation_login_ViewModel.CurrentUser.Username)
            };

            DatabaseManager.GetTableData(query, parameters, out DataTable resultTable);

            ActivePledgesCount = resultTable.Rows.Count > 0
                ? Convert.ToInt32(resultTable.Rows[0]["PledgesCount"])
                : 0;
        }

        public void InitializeScheduledPickupsCount()
        {
            string query = "SELECT * FROM dbo.GetScheduledPickupCountsByUsername(@Username)";
            var parameters = new[]
            {
                new SqlParameter("@Username", donation_login_ViewModel.CurrentUser.Username)
            };

            DatabaseManager.GetTableData(query, parameters, out DataTable opsTable);

            ScheduledPickupsCount = opsTable.Rows.Count > 0
                ? Convert.ToInt32(opsTable.Rows[0]["PickupSchedulesCount"])
                : 0;
        }

        public void InitializeDonationCardDetails()
        {
            string safeUsername = "'" + donation_login_ViewModel.CurrentUser.Username.Replace("'", "''") + "'";

            string query = $@"
                SELECT
                    ISNULL(SUM(di.Quantity_Received), 0) AS TotalItems,
                    MAX(dn.Date_Received) AS LastDate
                FROM [Donation] dn
                JOIN [Donor] d ON dn.Donor_ID = d.Donor_ID
                JOIN [Users] u ON d.User_ID = u.User_ID
                LEFT JOIN [Donated Items] di ON di.Donation_ID = dn.Donation_ID
                WHERE u.Username = {safeUsername}";

            DatabaseManager.GetTableDataWithCustomizedQuery(query, out DataTable data);

            if (data.Rows.Count > 0 && data.Rows[0]["LastDate"] != DBNull.Value)
            {
                TotalItemCount = Convert.ToInt32(data.Rows[0]["TotalItems"]);
                LastDonationDate = Convert.ToDateTime(data.Rows[0]["LastDate"]).ToString("yyyy-MM-dd");
                ImpactSummary = $"You've contributed {TotalItemCount} item{(TotalItemCount == 1 ? "" : "s")} so far. Thank you!";
            }
            else
            {
                TotalItemCount = 0;
                LastDonationDate = "No donations yet";
                ImpactSummary = "Your contributions will appear here once a donation is recorded.";
            }
        }

        public void InitializePledgeCardDetails()
        {
            string safeUsername = "'" + donation_login_ViewModel.CurrentUser.Username.Replace("'", "''") + "'";

            string nextPendingQuery = $@"
                SELECT TOP 1 pi.Item_Name, pi.ExpectedDelivery_Date
                FROM [Pledge] p
                JOIN [Donor] d ON p.Donor_ID = d.Donor_ID
                JOIN [Users] u ON d.User_ID = u.User_ID
                JOIN [Pledge Item] pi ON pi.Pledge_ID = p.Pledge_ID
                WHERE u.Username = {safeUsername} AND p.Pledge_Status = 'Pending'
                ORDER BY pi.ExpectedDelivery_Date ASC";

            DatabaseManager.GetTableDataWithCustomizedQuery(nextPendingQuery, out DataTable pendingData);

            NextPendingItem = pendingData.Rows.Count > 0
                ? $"{pendingData.Rows[0]["Item_Name"]} (due {Convert.ToDateTime(pendingData.Rows[0]["ExpectedDelivery_Date"]).ToString("yyyy-MM-dd")})"
                : "None pending";

            string completionQuery = $@"
                SELECT
                    COUNT(*) AS TotalPledges,
                    SUM(CASE WHEN p.Pledge_Status = 'Fulfilled' THEN 1 ELSE 0 END) AS FulfilledPledges
                FROM [Pledge] p
                JOIN [Donor] d ON p.Donor_ID = d.Donor_ID
                JOIN [Users] u ON d.User_ID = u.User_ID
                WHERE u.Username = {safeUsername}";

            DatabaseManager.GetTableDataWithCustomizedQuery(completionQuery, out DataTable completionData);

            if (completionData.Rows.Count > 0 &&
                Convert.ToInt32(completionData.Rows[0]["TotalPledges"]) > 0)
            {
                int total = Convert.ToInt32(completionData.Rows[0]["TotalPledges"]);
                int fulfilled = completionData.Rows[0]["FulfilledPledges"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(completionData.Rows[0]["FulfilledPledges"]);

                double rate = (double)fulfilled / total * 100.0;
                PledgeCompletionRate = $"{rate:0}% ({fulfilled}/{total})";
            }
            else
            {
                PledgeCompletionRate = "No pledges yet";
            }
        }

        public void InitializePickupCardDetails()
        {
            string safeUsername = "'" + donation_login_ViewModel.CurrentUser.Username.Replace("'", "''") + "'";

            string query = $@"
                SELECT TOP 1 ps.Pickup_Date
                FROM [Pickup Schedule] ps
                JOIN [Donation] dn ON ps.Donation_ID = dn.Donation_ID
                JOIN [Donor] d ON dn.Donor_ID = d.Donor_ID
                JOIN [Users] u ON d.User_ID = u.User_ID
                WHERE u.Username = {safeUsername} AND ps.Pickup_Status = 'Pending'
                ORDER BY ps.Pickup_Date ASC";

            DatabaseManager.GetTableDataWithCustomizedQuery(query, out DataTable data);

            NextPickupDateTime = data.Rows.Count > 0
                ? Convert.ToDateTime(data.Rows[0]["Pickup_Date"]).ToString("yyyy-MM-dd")
                : "None scheduled";

            PickupLocation = "Not tracked yet";
        }

        private void TogglePledgeMenu()
        {
            IsPledgeMenuOpen = !IsPledgeMenuOpen;
        }
    }
}