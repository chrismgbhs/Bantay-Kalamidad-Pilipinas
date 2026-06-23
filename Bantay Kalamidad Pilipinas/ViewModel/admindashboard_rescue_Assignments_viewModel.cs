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

        public ObservableCollection<AdminOperationAssignment> Assignments
        {
            get => _assignments;
            set
            {
                if (_assignments != value)
                {
                    _assignments = value;
                    OnPropertyChanged();
                }
            }
        }

        public AdminOperationAssignment SelectedAssignment
        {
            get => _selectedAssignment;
            set
            {
                if (_selectedAssignment != value)
                {
                    _selectedAssignment = value;
                    OnPropertyChanged();
                }
            }
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

        public admindashboard_rescue_Assignments_viewModel()
        {
            Assignments = new ObservableCollection<AdminOperationAssignment>();
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

            AdminOperationAssignment newAssignment = new AdminOperationAssignment
            {
                AssignmentId = string.Empty,
                Operation = string.Empty,
                RescuerName = string.Empty,
                VolunteerId = string.Empty,
                Role = string.Empty,
                Status = "Pending",
                IsNew = true
            };

            Assignments.Add(newAssignment);
            SelectedAssignment = newAssignment;
        }

        public void EnterManageMode()
        {
            IsTableReadOnly = false;
            ActionButtonsVisibility = Visibility.Visible;
        }

        public void EnterViewMode()
        {
            IsTableReadOnly = true;
            ActionButtonsVisibility = Visibility.Collapsed;
        }

        private async Task LoadAssignmentsAsync()
        {
            try
            {
                ObservableCollection<AdminOperationAssignment> loadedAssignments =
                    await DatabaseManager.GetAdminOperationAssignmentsAsync(
                        SelectedAssignmentFilter,
                        AssignmentSearchText);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Assignments = loadedAssignments;
                });
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Could not load operation assignments from the database.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An unexpected error occurred while loading assignments.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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

            if (!ValidateAssignment(SelectedAssignment))
                return;

            try
            {
                await DatabaseManager.AddAdminOperationAssignmentAsync(SelectedAssignment);

                MessageBox.Show("Assignment saved successfully.",
                    "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadAssignmentsAsync();
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                MessageBox.Show("An assignment with this ID already exists.",
                    "Duplicate Assignment ID", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show(
                    "Could not save assignment.\n\nThe Operation ID or Rescuer does not exist in the database.",
                    "Foreign Key Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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

            if (!ValidateAssignment(SelectedAssignment))
                return;

            try
            {
                await DatabaseManager.UpdateAdminOperationAssignmentAsync(SelectedAssignment);

                MessageBox.Show("Assignment updated successfully.",
                    "Updated", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadAssignmentsAsync();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show(
                    "Could not update assignment.\n\nThe Operation ID or Rescuer does not exist in the database.",
                    "Foreign Key Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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

            MessageBoxResult confirm = MessageBox.Show(
                "Are you sure you want to delete this assignment?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                await DatabaseManager.DeleteAdminOperationAssignmentAsync(SelectedAssignment.AssignmentId);

                MessageBox.Show("Assignment deleted successfully.",
                    "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadAssignmentsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not delete assignment.\n\n" + ex.Message,
                    "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateAssignment(AdminOperationAssignment assignment)
        {
            if (string.IsNullOrWhiteSpace(assignment.AssignmentId))
            {
                MessageBox.Show("Assignment ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (assignment.AssignmentId.Length > 10)
            {
                MessageBox.Show("Assignment ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(assignment.Operation))
            {
                MessageBox.Show("Operation ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (assignment.Operation.Length > 10)
            {
                MessageBox.Show("Operation ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(assignment.RescuerName))
            {
                MessageBox.Show("Rescuer name or Volunteer ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(assignment.Role))
            {
                MessageBox.Show("Role is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (assignment.Role.Length > 255)
            {
                MessageBox.Show("Role must be 255 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(assignment.Status))
            {
                MessageBox.Show("Status is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (assignment.Status.Length > 255)
            {
                MessageBox.Show("Status must be 255 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}