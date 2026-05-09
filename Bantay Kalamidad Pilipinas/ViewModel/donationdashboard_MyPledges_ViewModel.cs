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
    internal class donationdashboard_MyPledges_ViewModel : ObservableObject
    {
        private ComboBoxItem _selectedPledgeFilter;
        public ComboBoxItem SelectedPledgeFilter
        {
            get => _selectedPledgeFilter;
            set
            {
                if (_selectedPledgeFilter != value)
                {
                    _selectedPledgeFilter = value;
                    OnPropertyChanged(nameof(SelectedPledgeFilter));
                    //MessageBox.Show($"Selected filter: {SelectedPledgeFilter.Content}");
                    InitializeMyPledges();
                }
            }
        }

        private string _PledgeSearchText;

        public string PledgeSearchText
        {
            get { return _PledgeSearchText; }
            set
            {
                if (_PledgeSearchText != value)
                {
                    _PledgeSearchText = value;
                    OnPropertyChanged(nameof(PledgeSearchText));
                    InitializeMyPledges();
                    //MessageBox.Show($"Search text: {PledgeSearchText}");
                }
            }
        }

        public donationdashboard_MyPledges_ViewModel()
        {
            MyPledges = new ObservableCollection<MyPledge>();
            InitializeMyPledges();
        }

        private ObservableCollection<MyPledge> _MyPledges;
        public ObservableCollection<MyPledge> MyPledges
        {
            get => _MyPledges;
            set { _MyPledges = value; OnPropertyChanged(nameof(MyPledges)); }
        }

        /// <summary>
        /// This method initializes the MyPledges collection by executing a SQL query to retrieve the pledge data for the current user. It joins multiple tables to gather all necessary information about the pledges, including the event name, item name, pickup date, and pickup status. The retrieved data is then converted into Pledge objects and added to the MyPledges collection. If the data retrieval is successful, a success message is displayed; otherwise, an error message is shown.
        /// </summary>
        public void InitializeMyPledges()
        {
            MyPledges.Clear();
            string query = "SELECT * FROM dbo.GetMyPledgesByUsername(@Username, @PledgeStatus, @SearchText)";
            var parameters = new[] { new SqlParameter("@Username", donation_login_ViewModel.CurrentUser.Username),
                                     new SqlParameter("@PledgeStatus", SelectedPledgeFilter?.Content.ToString() ?? "All Pledges"),
                                     new SqlParameter("@SearchText", PledgeSearchText ?? string.Empty) };

            if (SelectedPledgeFilter == null)
            {
                MessageBox.Show("Please select a pledge filter.");
            }

            else
            {

                if (DatabaseManager.GetTableData(query, parameters, out DataTable data))
                {
                    var pledges = data.AsEnumerable().Select(row => new MyPledge(
                        row["Pledge_ID"].ToString(),
                        row["Event_Name"].ToString(),
                        row["Item_Name"].ToString(),
                        row["ExpectedDelivery_Date"].ToString(),
                        row["Pledge_Status"].ToString()
                    ));

                    foreach (var pledge in pledges)
                    {
                        MyPledges.Add(pledge);
                    }
                }

                else
                {
                    MessageBox.Show("No available pledges.");
                }
            }
        }

    }
}
