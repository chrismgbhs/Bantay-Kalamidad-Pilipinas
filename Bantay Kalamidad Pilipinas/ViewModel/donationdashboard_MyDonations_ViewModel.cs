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
using System.Windows.Controls;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class donationdashboard_MyDonations_ViewModel : ObservableObject
    {
        private ComboBoxItem _selectedDonationFilter;
        public ComboBoxItem SelectedDonationFilter
        {
            get => _selectedDonationFilter;
            set
            {
                if (_selectedDonationFilter != value)
                {
                    _selectedDonationFilter = value;
                    OnPropertyChanged(nameof(SelectedDonationFilter));
                    InitializeMyDonations();
                }
            }
        }

        private string _DonationSearchText;

        public string DonationSearchText
        {
            get { return _DonationSearchText; }
            set
            {
                if (_DonationSearchText != value)
                {
                    _DonationSearchText = value;
                    OnPropertyChanged(nameof(DonationSearchText));
                    InitializeMyDonations();
                }
            }
        }

        private ObservableCollection<Donation> _MyDonations;
        public ObservableCollection<Donation> MyDonations
        {
            get => _MyDonations;
            set { _MyDonations = value; OnPropertyChanged(nameof(MyDonations)); }
        }

        public donationdashboard_MyDonations_ViewModel()
        {
            MyDonations = new ObservableCollection<Donation>();
            InitializeMyDonations();
        }

        /// <summary>
        /// This method initializes the MyDonations collection by executing a SQL query to retrieve the donation data for the current user. It joins multiple tables to gather all necessary information about the donations, including the event name, item name, pickup date, and pickup status. The retrieved data is then converted into Donation objects and added to the MyDonations collection.
        /// </summary>
        public void InitializeMyDonations()
        {
            MyDonations.Clear();
            string query = "SELECT * FROM dbo.GetMyDonations(@Username, @PickupStatus, @SearchText)";

            // Default to "All Donations" when no filter has been picked yet (e.g. on
            // first load) instead of blocking the user with a popup before they've
            // had a chance to interact with the dropdown.
            string pickupStatusFilter = SelectedDonationFilter?.Content?.ToString() ?? "All Donations";

            var parameters = new[] {
                new SqlParameter("@Username", donation_login_ViewModel.CurrentUser.Username),
                new SqlParameter("@PickupStatus", pickupStatusFilter),
                new SqlParameter("@SearchText", DonationSearchText ?? string.Empty)
            };

            if (DatabaseManager.GetTableData(query, parameters, out DataTable data))
            {
                var donations = data.AsEnumerable().Select(row => new Donation(
                    row["Donation_ID"].ToString(),
                    row["Event_Name"].ToString(),
                    row["Item_Name"].ToString(),
                    row["Pickup_Date"].ToString(),
                    row["Pickup_Status"].ToString()
                ));

                foreach (var donation in donations)
                {
                    MyDonations.Add(donation);
                }
            }
            // No "else" MessageBox here — an empty result (no donations yet, or no
            // matches for the current filter/search) is a normal state, not an error.
            // The view's empty-state UI (if any) should handle a zero-count collection.
        }
    }
}