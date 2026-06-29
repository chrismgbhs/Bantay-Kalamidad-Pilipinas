using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Bantay_Kalamidad_Pilipinas.View;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    public class admindashboard_donation_mainlayout_viewModel : ObservableObject
    {
        private const string DonationsTab = "Donations";
        private const string PledgesTab = "Pledges";
        private const string LogisticsTab = "Logistics";
        private const string InventoryTab = "Inventory";
        private const string DistributionTab = "Distribution";
        private const string WasteTab = "Waste";

        private string _activeTab;
        private object _currentAdminDonationView;
        private IAdminDonationModuleViewModel _currentModuleViewModel;
        private string _selectedDonationModuleTitle;
        private string _currentMode;

        public object CurrentAdminDonationView
        {
            get => _currentAdminDonationView;
            set
            {
                if (_currentAdminDonationView != value)
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
                if (_selectedDonationModuleTitle != value)
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
                if (_currentMode != value)
                {
                    _currentMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public TextDecorationCollection DonationsUnderline => _activeTab == DonationsTab ? TextDecorations.Underline : null;
        public TextDecorationCollection PledgesUnderline => _activeTab == PledgesTab ? TextDecorations.Underline : null;
        public TextDecorationCollection LogisticsUnderline => _activeTab == LogisticsTab ? TextDecorations.Underline : null;
        public TextDecorationCollection InventoryUnderline => _activeTab == InventoryTab ? TextDecorations.Underline : null;
        public TextDecorationCollection DistributionUnderline => _activeTab == DistributionTab ? TextDecorations.Underline : null;
        public TextDecorationCollection WasteUnderline => _activeTab == WasteTab ? TextDecorations.Underline : null;

        public FontWeight DonationsFontWeight => _activeTab == DonationsTab ? FontWeights.Black : FontWeights.Bold;
        public FontWeight PledgesFontWeight => _activeTab == PledgesTab ? FontWeights.Black : FontWeights.Bold;
        public FontWeight LogisticsFontWeight => _activeTab == LogisticsTab ? FontWeights.Black : FontWeights.Bold;
        public FontWeight InventoryFontWeight => _activeTab == InventoryTab ? FontWeights.Black : FontWeights.Bold;
        public FontWeight DistributionFontWeight => _activeTab == DistributionTab ? FontWeights.Black : FontWeights.Bold;
        public FontWeight WasteFontWeight => _activeTab == WasteTab ? FontWeights.Black : FontWeights.Bold;

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
            ShowInventoryCommand = new RelayCommand(ShowInventory);
            ShowDistributionCommand = new RelayCommand(ShowDistribution);
            ShowWasteCommand = new RelayCommand(ShowWaste);

            AddModeCommand = new RelayCommand(EnterAddMode);
            AddItemModeCommand = new RelayCommand(EnterAddItemMode);
            ManageModeCommand = new RelayCommand(EnterManageMode);
            ViewModeCommand = new RelayCommand(EnterViewMode);

            BackToAdminCommand = new RelayCommand(BackToAdminMenu);
            LogoutCommand = new RelayCommand(Logout);

            ShowDonations();
        }

        private void SetActiveTab(string activeTab)
        {
            _activeTab = activeTab;

            OnPropertyChanged(nameof(DonationsUnderline));
            OnPropertyChanged(nameof(PledgesUnderline));
            OnPropertyChanged(nameof(LogisticsUnderline));
            OnPropertyChanged(nameof(InventoryUnderline));
            OnPropertyChanged(nameof(DistributionUnderline));
            OnPropertyChanged(nameof(WasteUnderline));

            OnPropertyChanged(nameof(DonationsFontWeight));
            OnPropertyChanged(nameof(PledgesFontWeight));
            OnPropertyChanged(nameof(LogisticsFontWeight));
            OnPropertyChanged(nameof(InventoryFontWeight));
            OnPropertyChanged(nameof(DistributionFontWeight));
            OnPropertyChanged(nameof(WasteFontWeight));
        }

        private void ShowDonations()
        {
            SelectedDonationModuleTitle = "Donations";
            var viewModel = new admindashboard_donation_Donations_viewModel();
            var view = new admindashboard_donation_Donations_view { DataContext = viewModel };
            LoadModule(view, viewModel);
            SetActiveTab(DonationsTab);
        }

        private void ShowPledges()
        {
            SelectedDonationModuleTitle = "Pledges";
            var viewModel = new admindashboard_donation_Pledges_viewModel();
            var view = new admindashboard_donation_Pledges_view { DataContext = viewModel };
            LoadModule(view, viewModel);
            SetActiveTab(PledgesTab);
        }

        private void ShowLogistics()
        {
            SelectedDonationModuleTitle = "Logistics";
            var viewModel = new admindashboard_donation_Logistics_viewModel();
            var view = new admindashboard_donation_Logistics_view { DataContext = viewModel };
            LoadModule(view, viewModel);
            SetActiveTab(LogisticsTab);
        }

        private void ShowInventory()
        {
            SelectedDonationModuleTitle = "Inventory";
            var viewModel = new admindashboard_donation_Inventory_viewModel();
            var view = new admindashboard_donation_Inventory_view { DataContext = viewModel };
            LoadModule(view, viewModel);
            SetActiveTab(InventoryTab);
        }

        private void ShowDistribution()
        {
            SelectedDonationModuleTitle = "Distribution";
            var viewModel = new admindashboard_donation_Distribution_viewModel();
            var view = new admindashboard_donation_Distribution_view { DataContext = viewModel };
            LoadModule(view, viewModel);
            SetActiveTab(DistributionTab);
        }

        private void ShowWaste()
        {
            SelectedDonationModuleTitle = "Waste";
            var viewModel = new admindashboard_donation_Waste_viewModel();
            var view = new admindashboard_donation_Waste_view { DataContext = viewModel };
            LoadModule(view, viewModel);
            SetActiveTab(WasteTab);
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