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
            InitializeTotalDonationsCount();
            InitializeActivePledgesCount();
            InitializeScheduledPickupsCount();
        }

        public void Logout()
        {
            var mainWindow = new MainWindow();
            mainWindow.Content = new View.donation_login_view();
            Application.Current.MainWindow.Close();
            mainWindow.Show();
        }

        /// <summary>
        /// This method initializes the TotalDonationsCount property by querying the database for the total count of donations made by the current user. It retrieves the count and assigns it to the TotalDonationsCount property. If the query fails, it displays an error message.
        /// </summary>
        public void InitializeTotalDonationsCount()
        {
            string query = "SELECT * FROM dbo.TotalDonationsCount(@Username)";
            var parameters = new[] { new SqlParameter("@Username", donation_login_ViewModel.CurrentUser.Username) };
            if (DatabaseManager.GetTableData(query, parameters, out DataTable data))
            {
                if (data.Rows.Count > 0)
                {
                    TotalDonationsCount = Convert.ToInt32(data.Rows[0]["DonationsCount"]);
                }
                else
                {
                    TotalDonationsCount = 0;
                }
            }
            else
            {
                MessageBox.Show("Failed to load total donations count.");
            }
        }

        /// <summary>
        /// This method initializes the ActivePledgesCount property by querying the database for the count of active pledges made by the current user. It retrieves the count and assigns it to the ActivePledgesCount property. If the query fails, it displays an error message.
        /// </summary>
        public void InitializeActivePledgesCount()
        {
            string query = "SELECT * FROM dbo.GetActivePledgesCount(@Username)";
            var parameters = new[] { new SqlParameter("@Username", donation_login_ViewModel.CurrentUser.Username) };
            if (DatabaseManager.GetTableData(query, parameters, out DataTable resultTable))
            {
                if (resultTable.Rows.Count > 0)
                {
                    ActivePledgesCount = Convert.ToInt32(resultTable.Rows[0]["PledgesCount"]);
                }
                else
                {
                    ActivePledgesCount = 0;
                }
            }
            else
            {
                MessageBox.Show("Failed to load active pledges count.");
            }
        }

        /// <summary>
        /// This method initializes the ScheduledPickupsCount property by querying the database for the count of scheduled pickups assigned to the current user. It retrieves the count and assigns it to the ScheduledPickupsCount property. If the query fails, it displays an error message.
        /// </summary>
        public void InitializeScheduledPickupsCount()
        {
            string assignedOpsQuery = "SELECT * FROM dbo.GetScheduledPickupCountsByUsername(@Username)";
            var parameters = new[] { new SqlParameter("@Username", donation_login_ViewModel.CurrentUser.Username) };
            if (DatabaseManager.GetTableData(assignedOpsQuery, parameters, out DataTable opsTable))
            {
                if (opsTable.Rows.Count > 0)
                {
                    ScheduledPickupsCount = Convert.ToInt32(opsTable.Rows[0]["PickupSchedulesCount"]);
                }
                else
                {
                    ScheduledPickupsCount = 0;
                }
            }
            else
            {
                MessageBox.Show("Failed to load scheduled pickups count.");
            }
        }

        private void TogglePledgeMenu()
        {
            IsPledgeMenuOpen = !IsPledgeMenuOpen;
        }
    }
}
