using Bantay_Kalamidad_Pilipinas.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class donationdashboard_MakeAPledge_ViewModel : ObservableObject
    {
        private ObservableCollection<DisasterEvent> _DisasterEvents { get; set; }

        public ObservableCollection<DisasterEvent> DisasterEvents
        {
            get => _DisasterEvents;
            set { _DisasterEvents = value; OnPropertyChanged(nameof(DisasterEvents)); }
        }

        private ObservableCollection<Pledge> _PledgeItems { get; set; }

        public ObservableCollection<Pledge> PledgeItems
        {
            get => _PledgeItems;
            set { _PledgeItems = value; OnPropertyChanged(nameof(PledgeItems)); }
        }

        private DateTime _ExpectedDeliveryDate = DateTime.Today;

        public DateTime ExpectedDeliveryDate
        {
            get => _ExpectedDeliveryDate;
            set { _ExpectedDeliveryDate = value; OnPropertyChanged(nameof(ExpectedDeliveryDate)); }
        }

        private Pledge _selectedPledge;
        public Pledge SelectedPledge
        {
            get => _selectedPledge;
            set
            {
                _selectedPledge = value;
                OnPropertyChanged(nameof(SelectedPledge));
            }
        }

        private DisasterEvent _SelectedDisasterEvent;
        public DisasterEvent SelectedDisasterEvent
        {
            get => _SelectedDisasterEvent;
            set
            {
                if (_SelectedDisasterEvent != value)
                {
                    _SelectedDisasterEvent = value;
                    OnPropertyChanged(nameof(SelectedDisasterEvent));
                }
            }
        }

        public ICommand SubmitPledgeCommand { get; set; }
        public ICommand AddItemCommand { get; }

        public ICommand RemovePledgeItemCommand { get; set; }

        public donationdashboard_MakeAPledge_ViewModel()
        {
            DisasterEvents = new ObservableCollection<DisasterEvent>();
            PledgeItems = new ObservableCollection<Pledge>();
            RemovePledgeItemCommand = new RelayCommand(RemovePledgeItem);
            SubmitPledgeCommand = new RelayCommand(async () => await SubmitPledge());
            AddItemCommand = new RelayCommand(AddItem);
            InitializeDisasterEvents();
        }

        public void RemovePledgeItem()
        {
            PledgeItems.Clear();
        }

        public void AddItem()
        {
            if (PledgeItems.Count == 0)
            {
                PledgeItems.Clear();
                PledgeItems.Add(new Pledge { ItemName = "Add item name...", Quantity = 0, Unit = "" });
            }
        }

        public async Task SubmitPledge()
        {
            if (SelectedDisasterEvent == null)
            {
                MessageBox.Show("Please select a disaster event.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PledgeItems == null || PledgeItems.Count == 0)
            {
                MessageBox.Show("Please add at least one pledge item.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var item = PledgeItems[0];

            if (string.IsNullOrWhiteSpace(item.ItemName) || item.ItemName == "Add item name..." ||
                item.Quantity <= 0 || string.IsNullOrWhiteSpace(item.Unit))
            {
                MessageBox.Show("Please fill in the pledge item details.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ExpectedDeliveryDate.Date < DateTime.Today)
            {
                MessageBox.Show("Expected delivery date cannot be in the past.", "Invalid Date", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await DatabaseManager.AddPledge(
                donation_login_ViewModel.CurrentUser.Username,
                item.ItemName,
                item.Quantity,
                item.Unit,
                ExpectedDeliveryDate.Date,
                SelectedDisasterEvent.EventID);

            // Let any open dashboard know its counters are now stale.
            donationdashboard_mainlayout_ViewModel.NotifyDonationDataChanged();

            // Reset the form so the same item can't be accidentally re-submitted.
            PledgeItems.Clear();
            SelectedDisasterEvent = null;
            ExpectedDeliveryDate = DateTime.Today;
        }

        public void InitializeDisasterEvents()
        {
            DisasterEvents.Clear();
            string query = "SELECT * FROM [Disaster Event]";

            if (!DatabaseManager.GetTableData(query, null, out var data))
            {
                MessageBox.Show("No disaster events are available right now.", "Make A Pledge", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var events = data.AsEnumerable().Select(row => new DisasterEvent(
                row["Event_Name"].ToString(),
                row["Event_ID"].ToString()
            ));

            foreach (var disasterEvent in events)
            {
                DisasterEvents.Add(disasterEvent);
            }
        }
    }
}