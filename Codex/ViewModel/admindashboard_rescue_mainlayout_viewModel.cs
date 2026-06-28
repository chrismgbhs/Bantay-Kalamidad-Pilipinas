using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bantay_Kalamidad_Pilipinas.View;
using System.Windows;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    public class admindashboard_rescue_mainlayout_viewModel : ObservableObject
    {
        private object _currentAdminRescueView;
        private IAdminRescueModuleViewModel _currentModuleViewModel;
        private string _selectedRescueModuleTitle;
        private string _currentMode;

        public object CurrentAdminRescueView
        {
            get => _currentAdminRescueView;
            set
            {
                if (_currentAdminRescueView != value)
                {
                    _currentAdminRescueView = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedRescueModuleTitle
        {
            get => _selectedRescueModuleTitle;
            set
            {
                if (_selectedRescueModuleTitle != value)
                {
                    _selectedRescueModuleTitle = value;
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

        public ICommand ShowRescuersCommand { get; }
        public ICommand ShowDisasterEventsCommand { get; }
        public ICommand ShowOperationsCommand { get; }
        public ICommand ShowAssignmentsCommand { get; }
        public ICommand ShowTeamsCommand { get; }

        public ICommand AddModeCommand { get; }
        public ICommand ManageModeCommand { get; }
        public ICommand ViewModeCommand { get; }

        public ICommand BackToAdminMenuCommand { get; }
        public ICommand LogoutCommand { get; }

        public admindashboard_rescue_mainlayout_viewModel()
        {
            ShowRescuersCommand = new RelayCommand(ShowRescuers);
            ShowDisasterEventsCommand = new RelayCommand(ShowDisasterEvents);
            ShowOperationsCommand = new RelayCommand(ShowOperations);
            ShowAssignmentsCommand = new RelayCommand(ShowAssignments);
            ShowTeamsCommand = new RelayCommand(ShowTeams);

            AddModeCommand = new RelayCommand(EnterAddMode);
            ManageModeCommand = new RelayCommand(EnterManageMode);
            ViewModeCommand = new RelayCommand(EnterViewMode);

            BackToAdminMenuCommand = new RelayCommand(BackToAdminMenu);
            LogoutCommand = new RelayCommand(Logout);

            ShowRescuers();
        }

        private void ShowRescuers()
        {
            SelectedRescueModuleTitle = "Rescuers";

            var viewModel = new admindashboard_rescue_Rescuers_viewModel();
            var view = new admindashboard_rescue_Rescuers_view
            {
                DataContext = viewModel
            };

            LoadModule(view, viewModel);
        }

        private void ShowDisasterEvents()
        {
            SelectedRescueModuleTitle = "Disaster Events";

            var viewModel = new admindashboard_rescue_DisasterEvents_viewModel();
            var view = new admindashboard_rescue_DisasterEvents_view
            {
                DataContext = viewModel
            };

            LoadModule(view, viewModel);
        }

        private void ShowOperations()
        {
            SelectedRescueModuleTitle = "Operations";

            var viewModel = new admindashboard_rescue_Operations_viewModel();
            var view = new admindashboard_rescue_Operations_view
            {
                DataContext = viewModel
            };

            LoadModule(view, viewModel);
        }

        private void ShowAssignments()
        {
            SelectedRescueModuleTitle = "Assignments";

            var viewModel = new admindashboard_rescue_Assignments_viewModel();
            var view = new admindashboard_rescue_Assignments_view
            {
                DataContext = viewModel
            };

            LoadModule(view, viewModel);
        }

        private void ShowTeams()
        {
            SelectedRescueModuleTitle = "Teams";

            var viewModel = new admindashboard_rescue_Teams_viewModel();
            var view = new admindashboard_rescue_Teams_view
            {
                DataContext = viewModel
            };

            LoadModule(view, viewModel);
        }

        private void LoadModule(object view, IAdminRescueModuleViewModel viewModel)
        {
            CurrentAdminRescueView = view;
            _currentModuleViewModel = viewModel;

            CurrentMode = "View";
            _currentModuleViewModel.EnterViewMode();
        }

        private void EnterAddMode()
        {
            CurrentMode = "Add";
            _currentModuleViewModel?.EnterAddMode();
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