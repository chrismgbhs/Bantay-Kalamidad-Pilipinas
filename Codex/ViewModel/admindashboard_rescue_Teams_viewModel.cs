using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bantay_Kalamidad_Pilipinas.Model;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    public class admindashboard_rescue_Teams_viewModel : ObservableObject, IAdminRescueModuleViewModel
    {
        private ObservableCollection<AdminRescueTeam> _teams;
        private AdminRescueTeam _selectedTeam;
        private string _selectedTeamFilter;
        private string _teamSearchText;
        private bool _isTableReadOnly = true;
        private Visibility _actionButtonsVisibility = Visibility.Collapsed;

        public ObservableCollection<AdminRescueTeam> Teams
        {
            get => _teams;
            set
            {
                if (_teams != value)
                {
                    _teams = value;
                    OnPropertyChanged();
                }
            }
        }

        public AdminRescueTeam SelectedTeam
        {
            get => _selectedTeam;
            set
            {
                if (_selectedTeam != value)
                {
                    _selectedTeam = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedTeamFilter
        {
            get => _selectedTeamFilter;
            set
            {
                if (_selectedTeamFilter != value)
                {
                    _selectedTeamFilter = value;
                    OnPropertyChanged();
                    _ = LoadTeamsAsync();
                }
            }
        }

        public string TeamSearchText
        {
            get => _teamSearchText;
            set
            {
                if (_teamSearchText != value)
                {
                    _teamSearchText = value;
                    OnPropertyChanged();
                    _ = LoadTeamsAsync();
                }
            }
        }

        public bool IsTableReadOnly
        {
            get => _isTableReadOnly;
            set
            {
                if (_isTableReadOnly != value)
                {
                    _isTableReadOnly = value;
                    OnPropertyChanged();
                }
            }
        }

        public Visibility ActionButtonsVisibility
        {
            get => _actionButtonsVisibility;
            set
            {
                if (_actionButtonsVisibility != value)
                {
                    _actionButtonsVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        public admindashboard_rescue_Teams_viewModel()
        {
            Teams = new ObservableCollection<AdminRescueTeam>();
            SelectedTeamFilter = "All Teams";

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            UpdateCommand = new AsyncRelayCommand(UpdateAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);

            _ = LoadTeamsAsync();
        }

        public void EnterAddMode()
        {
            // Teams are derived from Assignments — they cannot be directly added.
            MessageBox.Show(
                "Teams are automatically derived from operation assignments and cannot be added manually.\n\n" +
                "To add a team member, add an Assignment instead.",
                "Read-Only",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        public void EnterManageMode()
        {
            // Teams are read-only — editing is done through Assignments.
            MessageBox.Show(
                "Teams are automatically derived from operation assignments and cannot be edited directly.\n\n" +
                "To change team composition, update Assignments instead.",
                "Read-Only",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        public void EnterViewMode()
        {
            IsTableReadOnly = true;
            ActionButtonsVisibility = Visibility.Collapsed;
        }

        private async Task LoadTeamsAsync()
        {
            try
            {
                ObservableCollection<AdminRescueTeam> loadedTeams =
                    await DatabaseManager.GetAdminRescueTeamsAsync(
                        SelectedTeamFilter,
                        TeamSearchText);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Teams = loadedTeams;
                });
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Could not load rescue teams from the database.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An unexpected error occurred while loading teams.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedTeam == null)
            {
                MessageBox.Show("Please select the new team row to save.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!SelectedTeam.IsNew)
            {
                MessageBox.Show("This row already exists. Use UPDATE instead.",
                    "Save Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateTeam(SelectedTeam))
                return;

            try
            {
                await DatabaseManager.AddAdminRescueTeamAsync(SelectedTeam);

                MessageBox.Show("Team saved successfully.",
                    "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadTeamsAsync();
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                MessageBox.Show("A team/operation with this ID already exists.",
                    "Duplicate Operation ID", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show(
                    "Could not save team.\n\nThe Event ID or Location ID does not exist.",
                    "Foreign Key Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not save team.\n\n" + ex.Message,
                    "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateAsync()
        {
            if (SelectedTeam == null)
            {
                MessageBox.Show("Please select a team to update.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedTeam.IsNew)
            {
                MessageBox.Show("This is a new row. Use SAVE instead.",
                    "Update Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateTeam(SelectedTeam))
                return;

            try
            {
                await DatabaseManager.UpdateAdminRescueTeamAsync(SelectedTeam);

                MessageBox.Show("Team updated successfully.",
                    "Updated", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadTeamsAsync();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show(
                    "Could not update team.\n\nThe Event ID or Location ID does not exist.",
                    "Foreign Key Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not update team.\n\n" + ex.Message,
                    "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedTeam == null)
            {
                MessageBox.Show("Please select a team to delete.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedTeam.IsNew)
            {
                Teams.Remove(SelectedTeam);
                SelectedTeam = null;
                return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                "Are you sure you want to delete this team/operation?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                await DatabaseManager.DeleteAdminRescueTeamAsync(SelectedTeam.TeamOperation);

                MessageBox.Show("Team deleted successfully.",
                    "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadTeamsAsync();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show(
                    "Could not delete team.\n\nThis operation may already have assignments or related records.",
                    "Delete Restricted",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not delete team.\n\n" + ex.Message,
                    "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateTeam(AdminRescueTeam team)
        {
            if (string.IsNullOrWhiteSpace(team.TeamOperation))
            {
                MessageBox.Show("Team/Operation ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (team.TeamOperation.Length > 10)
            {
                MessageBox.Show("Team/Operation ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(team.Event))
            {
                MessageBox.Show("Event ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (team.Event.Length > 10)
            {
                MessageBox.Show("Event ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(team.Location))
            {
                MessageBox.Show("Location ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (team.Location.Length > 10)
            {
                MessageBox.Show("Location ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(team.Status))
            {
                MessageBox.Show("Status is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (team.Status.Length > 255)
            {
                MessageBox.Show("Status must be 255 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}