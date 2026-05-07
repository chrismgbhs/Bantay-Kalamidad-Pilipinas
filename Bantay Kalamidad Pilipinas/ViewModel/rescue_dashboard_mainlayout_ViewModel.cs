using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class rescue_dashboard_mainlayout_ViewModel : ObservableObject
    {
        private object _currentRescueDashboardView;
        public object CurrentRescueDashboardView
        {
            get => _currentRescueDashboardView;
            set { _currentRescueDashboardView = value; OnPropertyChanged(nameof(CurrentRescueDashboardView)); }
        }

        public ICommand ShowMyRescueOperationsCommand { get; }
        public ICommand ShowMyTeamCommand { get; }
        public ICommand ShowRescueLocationsCommand { get; }
        public ICommand ShowAnnouncementsCommand { get; }

        public rescue_dashboard_mainlayout_ViewModel()
        {
            ShowMyRescueOperationsCommand = new RelayCommand(() => CurrentRescueDashboardView = new View.rescuedashboard_MyRescueOperations_view());
            ShowMyTeamCommand = new RelayCommand(() => CurrentRescueDashboardView = new View.rescuedashboard_MyTeam_view());
            ShowRescueLocationsCommand = new RelayCommand(() => CurrentRescueDashboardView = new View.rescuedashboard_RescueLocations_view());
            ShowAnnouncementsCommand = new RelayCommand(() => CurrentRescueDashboardView = new View.rescuedashboard_Announcements_view());

            // default view on load
            CurrentRescueDashboardView = new View.rescuedashboard_MyRescueOperations_view();
        }
    }
}
