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
    public class admindashboard_rescue_Assignments_viewModel : ObservableObject, IAdminRescueModuleViewModel
    {
        private ObservableCollection<AdminOperationAssignment> _assignments;
        private AdminOperationAssignment _selectedAssignment;
        private string _selectedAssignmentFilter;
        private string _assignmentSearchText;
        private bool _isTableReadOnly = true;
        private Visibility _actionButtonsVisibility = Visibility.Collapsed;

        private ObservableCollection<DropdownItem> _availableOperations;
        private ObservableCollection<DropdownItem> _availableRescuers;
        private DropdownItem _selectedOperationDropdown;
        private DropdownItem _selectedRescuerDropdown;
        private Visibility _dropdownPanelVisibility = Visibility.Collapsed;

        public ObservableCollection<AdminOperationAssignment> Assignments
        {
            get => _assignments;
            set { if (_assignments != value) { _assignments = value; OnPropertyChanged(); } }
        }

        public AdminOperationAssignment SelectedAssignment
        {
            get => _selectedAssignment;
            set { if (_selectedAssignment != value) { _selectedAssignment = value; OnPropertyChanged(); } }
        }

        public string SelectedAssignmentFilter
        {
            get => _selectedAssignmentFilter;
            set
            {
                if (_selectedAssignmentFilter != value)
                {
                    _selectedAssignmentFilter = value;
                    OnPropertyChanged();
                    _ = LoadAssignmentsAsync();
                }
            }
        }

        public string AssignmentSearchText
        {
            get => _assignmentSearchText;
            set
            {
                if (_assignmentSearchText != value)
                {
                    _assignmentSearchText = value;
                    OnPropertyChanged();
                    _ = LoadAssignmentsAsync();
                }
            }
        }

        public bool IsTableReadOnly
        {
            get => _isTableReadOnly;
            set { if (_isTableReadOnly != value) { _isTableReadOnly = value; OnPropertyChanged(); } }
        }

        public Visibility ActionButtonsVisibility
        {
            get => _actionButtonsVisibility;
            set { if (_actionButtonsVisibility != value) { _actionButtonsVisibility = value; OnPropertyChanged(); } }
        }

        public ObservableCollection<DropdownItem> AvailableOperations
        {
            get => _availableOperations;
            set { if (_availableOperations != value) { _availableOperations = value; OnPropertyChanged(); } }
        }

        public ObservableCollection<DropdownItem> AvailableRescuers
        {
            get => _availableRescuers;
            set { if (_availableRescuers != value) { _availableRescuers = value; OnPropertyChanged(); } }
        }

        public DropdownItem SelectedOperationDropdown
        {
            get => _selectedOperationDropdown;
            set
            {
                if (_selectedOperationDropdown != value)
                {
                    _selectedOperationDropdown = value;
                    OnPropertyChanged();
                    if (SelectedAssignment != null && value != null)
                        SelectedAssignment.Operation = value.Id; // store the ID, show the display name
                }
            }
        }

        public DropdownItem SelectedRescuerDropdown
        {
            get => _selectedRescuerDropdown;
            set
            {
                if (_selectedRescuerDropdown != value)
                {
                    _selectedRescuerDropdown = value;
                    OnPropertyChanged();
                    if (SelectedAssignment != null && value != null)
                    {
                        SelectedAssignment.VolunteerId = value.Id;
                        SelectedAssignment.RescuerName = value.Display;
                    }
                }
            }
        }

        public Visibility DropdownPanelVisibility
        {
            get => _dropdownPanelVisibility;
            set { if (_dropdownPanelVisibility != value) { _dropdownPanelVisibility = value; OnPropertyChanged(); } }
        }

        public ICommand SaveCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        public admindashboard_rescue_Assignments_viewModel()
        {
            Assignments = new ObservableCollection<AdminOperationAssignment>();
            AvailableOperations = new ObservableCollection<DropdownItem>();
            AvailableRescuers = new ObservableCollection<DropdownItem>();
            SelectedAssignmentFilter = "All Assignments";

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            UpdateCommand = new AsyncRelayCommand(UpdateAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);

            _ = LoadAssignmentsAsync();
        }

        public void EnterAddMode()
        {
            IsTableReadOnly = false;
            ActionButtonsVisibility = Visibility.Visible;
            DropdownPanelVisibility = Visibility.Visible;
            _ = PrepareAddModeAsync();
        }

        private async Task PrepareAddModeAsync()
        {
            try
            {
                await LoadDropdownsAsync();
                string newId = await DatabaseManager.GenerateAssignmentIdAsync();

                var newAssignment = new AdminOperationAssignment
                {
                    AssignmentId = newId,
                    Operation = string.Empty,
                    VolunteerId = string.Empty,
                    RescuerName = string.Empty,
                    Role = string.Empty,
                    Status = "Pending",
                    IsNew = true
                };

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Assignments.Add(newAssignment);
                    SelectedAssignment = newAssignment;
                    SelectedOperationDropdown = null;
                    SelectedRescuerDropdown = null;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not prepare add mode.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void EnterManageMode()
        {
            IsTableReadOnly = false;
            ActionButtonsVisibility = Visibility.Visible;
            DropdownPanelVisibility = Visibility.Visible;
            _ = LoadDropdownsAsync();
        }

        public void EnterViewMode()
        {
            IsTableReadOnly = true;
            ActionButtonsVisibility = Visibility.Collapsed;
            DropdownPanelVisibility = Visibility.Collapsed;
        }

        private async Task LoadDropdownsAsync()
        {
            var operations = await DatabaseManager.GetAvailableOperationsAsync();
            var rescuers = await DatabaseManager.GetAvailableRescuersAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                AvailableOperations.Clear();
                foreach (var op in operations)
                    AvailableOperations.Add(new DropdownItem { Id = op.Id, Display = op.Display });

                AvailableRescuers.Clear();
                foreach (var r in rescuers)
                    AvailableRescuers.Add(new DropdownItem { Id = r.Id, Display = r.Name });
            });
        }

        private async Task LoadAssignmentsAsync()
        {
            try
            {
                var loaded = await DatabaseManager.GetAdminOperationAssignmentsAsync(
                    SelectedAssignmentFilter, AssignmentSearchText);
                Application.Current.Dispatcher.Invoke(() => { Assignments = loaded; });
            }
            catch (SqlException)
            {
                MessageBox.Show("Could not load assignments from the database.",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedAssignment == null)
            {
                MessageBox.Show("Please select the new assignment row to save.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!SelectedAssignment.IsNew)
            {
                MessageBox.Show("This row already exists. Use UPDATE instead.",
                    "Save Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Sync dropdown selections into the model
            if (SelectedOperationDropdown != null)
                SelectedAssignment.Operation = SelectedOperationDropdown.Id;
            if (SelectedRescuerDropdown != null)
            {
                SelectedAssignment.VolunteerId = SelectedRescuerDropdown.Id;
                SelectedAssignment.RescuerName = SelectedRescuerDropdown.Display;
            }

            if (!ValidateAssignment(SelectedAssignment)) return;

            try
            {
                await DatabaseManager.AddAdminOperationAssignmentAsync(SelectedAssignment);
                MessageBox.Show("Assignment saved successfully.", "Saved",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAssignmentsAsync();
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                MessageBox.Show("An assignment with this ID already exists.",
                    "Duplicate ID", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("The Operation ID or Rescuer does not exist in the database.",
                    "Foreign Key Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not save assignment.\n\n" + ex.Message,
                    "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateAsync()
        {
            if (SelectedAssignment == null)
            {
                MessageBox.Show("Please select an assignment to update.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedAssignment.IsNew)
            {
                MessageBox.Show("This is a new row. Use SAVE instead.",
                    "Update Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedOperationDropdown != null)
                SelectedAssignment.Operation = SelectedOperationDropdown.Id;
            if (SelectedRescuerDropdown != null)
            {
                SelectedAssignment.VolunteerId = SelectedRescuerDropdown.Id;
                SelectedAssignment.RescuerName = SelectedRescuerDropdown.Display;
            }

            if (!ValidateAssignment(SelectedAssignment)) return;

            try
            {
                await DatabaseManager.UpdateAdminOperationAssignmentAsync(SelectedAssignment);
                MessageBox.Show("Assignment updated successfully.", "Updated",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAssignmentsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not update assignment.\n\n" + ex.Message,
                    "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedAssignment == null)
            {
                MessageBox.Show("Please select an assignment to delete.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedAssignment.IsNew)
            {
                Assignments.Remove(SelectedAssignment);
                SelectedAssignment = null;
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this assignment?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                await DatabaseManager.DeleteAdminOperationAssignmentAsync(SelectedAssignment.AssignmentId);
                MessageBox.Show("Assignment deleted successfully.", "Deleted",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAssignmentsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not delete assignment.\n\n" + ex.Message,
                    "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateAssignment(AdminOperationAssignment a)
        {
            if (string.IsNullOrWhiteSpace(a.AssignmentId))
            {
                MessageBox.Show("Assignment ID is required.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(a.Operation))
            {
                MessageBox.Show("Please select an Operation.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(a.VolunteerId) && string.IsNullOrWhiteSpace(a.RescuerName))
            {
                MessageBox.Show("Please select a Rescuer.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(a.Role))
            {
                MessageBox.Show("Role is required.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}