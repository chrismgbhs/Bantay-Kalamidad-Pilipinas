using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Bantay_Kalamidad_Pilipinas.View;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    public class admindashboard_donation_mainlayout_viewModel : ObservableObject
    {
        private object _currentAdminDonationView;
        private IAdminDonationModuleViewModel _currentModuleViewModel;
        private string _selectedDonationModuleTitle;
        private string _currentMode;

        public object CurrentAdminDonationView
        {
            get => _currentAdminDonationView;
            set
            {
                if(_currentAdminDonationView  != value)
                {
                    _currentAdminDonationView = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedDonationModuleTitle
        {
            get => _selectedDonationModuleTitle;
            set
            {
                if( _selectedDonationModuleTitle != value)
                {
                    _selectedDonationModuleTitle = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentMode
        {
            get => _currentMode;
            set
            {
                if( _currentMode != value)
                {
                    _currentMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand ShowDonationsCommand { get; }
        public ICommand ShowPledgesCommand { get; }
        public ICommand ShowLogisticsCommand { get; }
        public ICommand ShowInventoryCommand { get; }
        public ICommand ShowDistributionCommand { get; }
        public ICommand ShowWasteCommand { get; }

        public ICommand AddModeCommand { get; }
        public ICommand AddItemModeCommand { get; }

        public ICommand ManageModeCommand { get; }
        public ICommand ViewModeCommand { get; }

        public ICommand BackToAdminCommand { get; }
        public ICommand LogoutCommand { get; }

        public admindashboard_donation_mainlayout_viewModel()
        {
            ShowDonationsCommand = new RelayCommand(ShowDonations);
            ShowPledgesCommand = new RelayCommand(ShowPledges);
            ShowLogisticsCommand = new RelayCommand(ShowLogistics);

            AddModeCommand = new RelayCommand(EnterAddMode);
            AddItemModeCommand = new RelayCommand(EnterAddItemMode);
            ManageModeCommand = new RelayCommand(EnterManageMode);
            ViewModeCommand = new RelayCommand(EnterViewMode);

            BackToAdminCommand = new RelayCommand(BackToAdminMenu);
            LogoutCommand = new RelayCommand(Logout);

            ShowDonations();
        }

        private void ShowDonations()
        {
            SelectedDonationModuleTitle = "Donations";

            var viewModel = new admindashboard_donation_Donations_viewModel();
            var view = new admindashboard_donation_Donations_view
            {
                DataContext = viewModel
            };

            
        }

        private void ShowPledges()
        {
            SelectedDonationModuleTitle = "Pledges";
            
            var viewModel = new admindashboard_donation_Pledges_viewModel();
            var view = new admindashboard_donation_Pledges_view
            {
                DataContext = viewModel
            };

        }

        private void ShowLogistics()
        {
            SelectedDonationModuleTitle = "Logistics";

            var viewModel = new admindashboard_donation_Logistics_viewModel();
            var view = new admindashboard_donation_Logistics_view
            {
                DataContext = viewModel
            };

        }

        private void LoadModule(object view, IAdminDonationModuleViewModel viewModel)
        {
            CurrentAdminDonationView = view;
            _currentModuleViewModel = viewModel;

            CurrentMode = "View";
            _currentModuleViewModel.EnterViewMode();
        }

        private void EnterAddMode()
        {
            CurrentMode = "Add";
            _currentModuleViewModel?.EnterAddMode();
        }
        private void EnterAddItemMode()
        {
            CurrentMode = "Add Item";
            _currentModuleViewModel?.EnterAddItemMode();
        }

        private void EnterManageMode()
        {
            CurrentMode = "Manage";
            _currentModuleViewModel?.EnterManageMode();
        }

        private void EnterViewMode()
        {
            CurrentMode = "View";
            _currentModuleViewModel?.EnterViewMode();
        }

        private void BackToAdminMenu()
        {
            Window currentWindow = Application.Current.MainWindow;

            var menuHost = new MainWindow();
            menuHost.Content = new admin_menu_view();
            menuHost.Show();

            Application.Current.MainWindow = menuHost;
            currentWindow?.Close();
        }

        private void Logout()
        {
            Window currentWindow = Application.Current.MainWindow;

            var loginHost = new MainWindow();
            loginHost.Content = new admin_login_view();
            loginHost.Show();

            Application.Current.MainWindow = loginHost;
            currentWindow?.Close();
        }
    }
}
