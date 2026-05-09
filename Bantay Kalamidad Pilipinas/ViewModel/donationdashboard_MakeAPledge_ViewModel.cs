using Bantay_Kalamidad_Pilipinas.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;
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
                    MessageBox.Show(SelectedDisasterEvent.Name);
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
            SubmitPledgeCommand = new RelayCommand(SubmitPledge);
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

        public async void SubmitPledge()
        {
            if (SelectedDisasterEvent == null)
            {
                MessageBox.Show("Please select a disaster event.");
                return;
            }

            else
            {
                if (PledgeItems[0].ItemName == "Add item name..." || PledgeItems[0].Quantity <= 0 || string.IsNullOrEmpty(PledgeItems[0].Unit))
                {
                    MessageBox.Show("Please fill in the pledge item details.");
                    return;
                }

                else
                {
                    await DatabaseManager.AddPledge(donation_login_ViewModel.CurrentUser.Username, PledgeItems[0].ItemName, PledgeItems[0].Quantity, PledgeItems[0].Unit, ExpectedDeliveryDate.Date, SelectedDisasterEvent.EventID);
                }
            }

        }

        public void InitializeDisasterEvents()
        {
            // Clear existing events
            DisasterEvents.Clear();
            string query = "SELECT * FROM [Disaster Event]";
            DatabaseManager.GetTableData(query, null, out var data);
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
