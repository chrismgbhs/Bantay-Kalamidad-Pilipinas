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
    internal class donationdashboard_mainlayout_ViewModel : ObservableObject
    {
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

        // ------------------------------------------------------------------
        // The properties below back the dashboard cards in
        // donationdashboard_mainlayout_view.xaml. The XAML was written
        // against these exact property names, but the ViewModel never
        // defined them — WPF bindings to a missing property fail silently
        // (no exception, the TextBlock just renders blank), which is why
        // "My Total Donations", "Scheduled Pickups", and most of "Active
        // Pledges" never showed anything even though TotalDonationsCount
        // and ScheduledPickupsCount (above) were being computed correctly
        // the whole time. Only ActivePledgesCount happened to match an
        // existing XAML binding, which is why that was the one number that
        // ever appeared.
        // ------------------------------------------------------------------

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

        // "Scheduled Deliveries" card properties. There is currently no FK
        // path in the schema from a donor's Donation to a Delivery Schedule
        // row (Delivery Schedule hangs off Distribution, the outgoing side
        // — see Day 1 notes on GetDeliveryStatusByUsername). These three
        // stay at a placeholder value honestly instead of querying a path
        // that doesn't exist yet.
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

        public ICommand ShowMyDonationsCommand { get; set; }
        public ICommand TogglePledgeMenuCommand { get; set; }
        public ICommand ShowMyPledgesCommand { get; set; }
        public ICommand ShowPickupDeliveryCommand { get; set; }
        public ICommand ShowAboutUsCommand { get; set; }
        public ICommand ShowContactUsCommand { get; set; }
        public ICommand ShowMakePledgeCommand { get; set; }
        public ICommand LogoutCommand { get; }
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


        public donationdashboard_mainlayout_ViewModel()
        {
            ShowMyDonationsCommand = new RelayCommand(() => CurrentDonationDashboardView = new View.donationdashboard_MyDonations_view());
            ShowMyPledgesCommand = new RelayCommand(() => CurrentDonationDashboardView = new View.donationdashboard_MyPledges_view());
            ShowPickupDeliveryCommand = new RelayCommand(() => CurrentDonationDashboardView = new View.donationdashboard_Pickup_Delivery_view());
            ShowAboutUsCommand = new RelayCommand(() => CurrentDonationDashboardView = new View.donationdashboard_AboutUs_view());
            ShowContactUsCommand = new RelayCommand(() => CurrentDonationDashboardView = new View.donationdashboard_ContactUs_view());
            ShowMakePledgeCommand = new RelayCommand(() => CurrentDonationDashboardView = new View.donationdashboard_MakeAPledge_view());
            TogglePledgeMenuCommand = new RelayCommand(TogglePledgeMenu);
            LogoutCommand = new RelayCommand(Logout);

            // Child ViewModels (MyDonations, MyPledges, MakeAPledge, Pickup&Delivery) are
            // created fresh by the commands above and have no reference back to this layout
            // VM. Without this, the dashboard counters go stale after a pledge is submitted
            // or a donation/pickup status changes elsewhere, until the user logs out and
            // back in. Subscribing to a static event lets any donation-side ViewModel signal
            // "something changed" without needing a direct reference to this instance.
            DonationDataChanged += OnDonationDataChanged;

            RefreshCounters();
        }

        /// <summary>
        /// Raised by any donation-side ViewModel after a write that should be reflected
        /// in the dashboard counters (e.g. submitting a pledge, a pickup being scheduled).
        /// </summary>
        public static event Action DonationDataChanged;

        /// <summary>
        /// Call this from anywhere on the donation side after a successful write
        /// (e.g. DonationDataChanged?.Invoke(); inside SubmitPledge()) to refresh
        /// every open dashboard's counters.
        /// </summary>
        public static void NotifyDonationDataChanged()
        {
            DonationDataChanged?.Invoke();
        }

        private void OnDonationDataChanged()
        {
            RefreshCounters();
        }

        /// <summary>
        /// Re-runs all three counter queries. Safe to call multiple times.
        /// No-ops if there is no logged-in donor yet (defensive — guards against
        /// the static event firing before CurrentUser is populated).
        /// </summary>
        public void RefreshCounters()
        {
            if (donation_login_ViewModel.CurrentUser == null || string.IsNullOrWhiteSpace(donation_login_ViewModel.CurrentUser.Username))
            {
                return;
            }

            InitializeTotalDonationsCount();
            InitializeActivePledgesCount();
            InitializeScheduledPickupsCount();
            InitializeDonationCardDetails();
            InitializePledgeCardDetails();
            InitializePickupCardDetails();
        }

        public void Logout()
        {
            DonationDataChanged -= OnDonationDataChanged;

            // Clears the cached Google credential so the next "Sign in with
            // Google" click (on either portal) re-prompts instead of
            // silently reusing this session's Google account. Fire-and-forget
            // is fine here — it's a fast local file delete, and even if it's
            // still finishing when the new window appears, the worst case is
            // a fraction-of-a-second-stale cache, not a broken logout.
            _ = GoogleAuthHelper.SignOutAsync();

            var mainWindow = new MainWindow();
            mainWindow.Content = new View.donation_login_view();
            Application.Current.MainWindow.Close();
            mainWindow.Show();
            Application.Current.MainWindow = mainWindow;
        }

        /// <summary>
        /// This method initializes the TotalDonationsCount property by querying the database for the total count of donations made by the current user. It retrieves the count and assigns it to the TotalDonationsCount property.
        /// </summary>
        public void InitializeTotalDonationsCount()
        {
            string query = "SELECT * FROM dbo.TotalDonationsCount(@Username)";
            var parameters = new[] { new SqlParameter("@Username", donation_login_ViewModel.CurrentUser.Username) };

            // GetTableData returns false both on a real DB error AND when the
            // query legitimately produces zero rows (see DatabaseManager.GetTableData,
            // which returns data.Rows.Count > 0). A donor with zero donations is a
            // normal, expected state — not an error — so we don't show a MessageBox
            // here; TotalDonationsCount correctly stays at 0.
            DatabaseManager.GetTableData(query, parameters, out DataTable data);

            TotalDonationsCount = data.Rows.Count > 0
                ? Convert.ToInt32(data.Rows[0]["DonationsCount"])
                : 0;
        }

        /// <summary>
        /// This method initializes the ActivePledgesCount property by querying the database for the count of active pledges made by the current user.
        /// </summary>
        public void InitializeActivePledgesCount()
        {
            string query = "SELECT * FROM dbo.GetActivePledgesCount(@Username)";
            var parameters = new[] { new SqlParameter("@Username", donation_login_ViewModel.CurrentUser.Username) };

            DatabaseManager.GetTableData(query, parameters, out DataTable resultTable);

            ActivePledgesCount = resultTable.Rows.Count > 0
                ? Convert.ToInt32(resultTable.Rows[0]["PledgesCount"])
                : 0;
        }

        /// <summary>
        /// This method initializes the ScheduledPickupsCount property by querying the database for the count of scheduled pickups for the current user.
        /// </summary>
        public void InitializeScheduledPickupsCount()
        {
            string query = "SELECT * FROM dbo.GetScheduledPickupCountsByUsername(@Username)";
            var parameters = new[] { new SqlParameter("@Username", donation_login_ViewModel.CurrentUser.Username) };

            DatabaseManager.GetTableData(query, parameters, out DataTable opsTable);

            ScheduledPickupsCount = opsTable.Rows.Count > 0
                ? Convert.ToInt32(opsTable.Rows[0]["PickupSchedulesCount"])
                : 0;
        }

        /// <summary>
        /// Populates the "My Total Donations" card: total donated item count
        /// across all of this donor's Donation rows, the date of their most
        /// recent donation, and a one-line impact summary string.
        /// </summary>
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

        /// <summary>
        /// Populates the "Active Pledges" card's two extra fields: the next
        /// pending pledge item (soonest expected delivery date among
        /// 'Pending' pledges) and a fulfilled-vs-total completion rate.
        /// </summary>
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

            if (completionData.Rows.Count > 0 && Convert.ToInt32(completionData.Rows[0]["TotalPledges"]) > 0)
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

        /// <summary>
        /// Populates the "Scheduled Pickups" card: date/time of the donor's
        /// soonest pending pickup. Pickup Location is left as a placeholder
        /// — Pickup Schedule has no Location/Center column in the current
        /// schema, so there is nothing real to query for it yet.
        /// </summary>
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

            // Pickup Schedule has no location column in the current schema —
            // there is nothing real to show here yet. Left honest rather than
            // fabricated.
            PickupLocation = "Not tracked yet";
        }

        private void TogglePledgeMenu()
        {
            IsPledgeMenuOpen = !IsPledgeMenuOpen;
        }
    }
}