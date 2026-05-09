using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class donationdashboard_mainlayout_ViewModel : ObservableObject
    {
        private object _currentDonationDashboardView;
        public object CurrentDonationDashboardView
        {
            get => _currentDonationDashboardView;
            set { _currentDonationDashboardView = value; OnPropertyChanged(nameof(CurrentDonationDashboardView)); }
        }
        public ICommand ShowMyDonationsCommand { get; set; }
        
        public donationdashboard_mainlayout_ViewModel() 
        { 
            ShowMyDonationsCommand = new RelayCommand(() => CurrentDonationDashboardView = new View.donationdashboard_MyDonations_view());
        }
    }
}
