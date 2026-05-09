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
                    //MessageBox.Show($"Selected filter: {SelectedDonationFilter.Content}");
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
                    //MessageBox.Show($"Search text: {DonationSearchText}");
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
        }

        /// <summary>
        /// This method initializes the MyDonations collection by executing a SQL query to retrieve the donation data for the current user. It joins multiple tables to gather all necessary information about the donations, including the event name, item name, pickup date, and pickup status. The retrieved data is then converted into Donation objects and added to the MyDonations collection. If the data retrieval is successful, a success message is displayed; otherwise, an error message is shown.
        /// </summary>
        public void InitializeMyDonations()
        {
            MyDonations.Clear();
            string query = "SELECT * FROM dbo.GetMyDonations(@Username, @PickupStatus, @SearchText)";
            var parameters = new[] { new SqlParameter("@Username", donation_login_ViewModel.CurrentUser.Username),
                                     new SqlParameter("@PickupStatus", SelectedDonationFilter?.Content.ToString() ?? "All Donations"),
                                     new SqlParameter("@SearchText", DonationSearchText ?? string.Empty) };

            if (SelectedDonationFilter == null)
            {
                MessageBox.Show("Please select a donation filter.");
            }

            else
            {

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

                else
                {
                    MessageBox.Show("No available donations.");
                }
            }
        }
    }
}
