using Bantay_Kalamidad_Pilipinas.View;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    public class admindashboard_rescue_mainlayout_viewModel : ObservableObject
    {
        private const string RescuersTab = "Rescuers";
        private const string DisasterEventsTab = "DisasterEvents";
        private const string OperationsTab = "Operations";
        private const string AssignmentsTab = "Assignments";
        private const string TeamsTab = "Teams";

        private string _activeTab;
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

        public TextDecorationCollection RescuersUnderline => _activeTab == RescuersTab ? TextDecorations.Underline : null;
        public TextDecorationCollection DisasterEventsUnderline => _activeTab == DisasterEventsTab ? TextDecorations.Underline : null;
        public TextDecorationCollection OperationsUnderline => _activeTab == OperationsTab ? TextDecorations.Underline : null;
        public TextDecorationCollection AssignmentsUnderline => _activeTab == AssignmentsTab ? TextDecorations.Underline : null;
        public TextDecorationCollection TeamsUnderline => _activeTab == TeamsTab ? TextDecorations.Underline : null;

        public FontWeight RescuersFontWeight => _activeTab == RescuersTab ? FontWeights.Black : FontWeights.Bold;
        public FontWeight DisasterEventsFontWeight => _activeTab == DisasterEventsTab ? FontWeights.Black : FontWeights.Bold;
        public FontWeight OperationsFontWeight => _activeTab == OperationsTab ? FontWeights.Black : FontWeights.Bold;
        public FontWeight AssignmentsFontWeight => _activeTab == AssignmentsTab ? FontWeights.Black : FontWeights.Bold;
        public FontWeight TeamsFontWeight => _activeTab == TeamsTab ? FontWeights.Black : FontWeights.Bold;

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

        private void SetActiveTab(string activeTab)
        {
            _activeTab = activeTab;

            OnPropertyChanged(nameof(RescuersUnderline));
            OnPropertyChanged(nameof(DisasterEventsUnderline));
            OnPropertyChanged(nameof(OperationsUnderline));
            OnPropertyChanged(nameof(AssignmentsUnderline));
            OnPropertyChanged(nameof(TeamsUnderline));

            OnPropertyChanged(nameof(RescuersFontWeight));
            OnPropertyChanged(nameof(DisasterEventsFontWeight));
            OnPropertyChanged(nameof(OperationsFontWeight));
            OnPropertyChanged(nameof(AssignmentsFontWeight));
            OnPropertyChanged(nameof(TeamsFontWeight));
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
            SetActiveTab(RescuersTab);
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
            SetActiveTab(DisasterEventsTab);
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
            SetActiveTab(OperationsTab);
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
            SetActiveTab(AssignmentsTab);
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
            SetActiveTab(TeamsTab);
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