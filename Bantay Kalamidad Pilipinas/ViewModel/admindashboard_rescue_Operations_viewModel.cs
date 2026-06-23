using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bantay_Kalamidad_Pilipinas.Model;
using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    public class admindashboard_rescue_Operations_viewModel : ObservableObject, IAdminRescueModuleViewModel
    {
        private ObservableCollection<AdminRescueOperation> _operations;
        private AdminRescueOperation _selectedOperation;
        private string _selectedOperationFilter;
        private string _operationSearchText;
        private bool _isTableReadOnly = true;
        private Visibility _actionButtonsVisibility = Visibility.Collapsed;

        public ObservableCollection<AdminRescueOperation> Operations
        {
            get => _operations;
            set
            {
                if (_operations != value)
                {
                    _operations = value;
                    OnPropertyChanged();
                }
            }
        }

        public AdminRescueOperation SelectedOperation
        {
            get => _selectedOperation;
            set
            {
                if (_selectedOperation != value)
                {
                    _selectedOperation = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedOperationFilter
        {
            get => _selectedOperationFilter;
            set
            {
                if (_selectedOperationFilter != value)
                {
                    _selectedOperationFilter = value;
                    OnPropertyChanged();
                    _ = LoadOperationsAsync();
                }
            }
        }

        public string OperationSearchText
        {
            get => _operationSearchText;
            set
            {
                if (_operationSearchText != value)
                {
                    _operationSearchText = value;
                    OnPropertyChanged();
                    _ = LoadOperationsAsync();
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

        public admindashboard_rescue_Operations_viewModel()
        {
            Operations = new ObservableCollection<AdminRescueOperation>();
            SelectedOperationFilter = "All Operations";

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            UpdateCommand = new AsyncRelayCommand(UpdateAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);

            _ = LoadOperationsAsync();
        }

        public void EnterAddMode()
        {
            IsTableReadOnly = false;
            ActionButtonsVisibility = Visibility.Visible;

            AdminRescueOperation newOperation = new AdminRescueOperation
            {
                OperationId = string.Empty,
                Event = string.Empty,
                Location = string.Empty,
                DateStarted = DateTime.Today,
                Status = "Pending",
                IsNew = true
            };

            Operations.Add(newOperation);
            SelectedOperation = newOperation;
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

        private async Task LoadOperationsAsync()
        {
            try
            {
                ObservableCollection<AdminRescueOperation> loadedOperations =
                    await DatabaseManager.GetAdminRescueOperationsAsync(
                        SelectedOperationFilter,
                        OperationSearchText);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Operations = loadedOperations;
                });
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Could not load rescue operations from the database.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An unexpected error occurred while loading rescue operations.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedOperation == null)
            {
                MessageBox.Show("Please select the new operation row to save.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!SelectedOperation.IsNew)
            {
                MessageBox.Show("This row already exists. Use UPDATE instead.",
                    "Save Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateOperation(SelectedOperation))
                return;

            try
            {
                await DatabaseManager.AddAdminRescueOperationAsync(SelectedOperation);

                MessageBox.Show("Rescue operation saved successfully.",
                    "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadOperationsAsync();
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                MessageBox.Show("A rescue operation with this ID already exists.",
                    "Duplicate Operation ID", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show(
                    "Could not save rescue operation.\n\nThe Event ID or Location ID does not exist in the database.",
                    "Foreign Key Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Could not save rescue operation.\n\n" + ex.Message,
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred while saving.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateAsync()
        {
            if (SelectedOperation == null)
            {
                MessageBox.Show("Please select a rescue operation to update.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedOperation.IsNew)
            {
                MessageBox.Show("This is a new row. Use SAVE instead.",
                    "Update Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateOperation(SelectedOperation))
                return;

            try
            {
                await DatabaseManager.UpdateAdminRescueOperationAsync(SelectedOperation);

                MessageBox.Show("Rescue operation updated successfully.",
                    "Updated", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadOperationsAsync();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show(
                    "Could not update rescue operation.\n\nThe Event ID or Location ID does not exist in the database.",
                    "Foreign Key Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Could not update rescue operation.\n\n" + ex.Message,
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred while updating.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedOperation == null)
            {
                MessageBox.Show("Please select a rescue operation to delete.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedOperation.IsNew)
            {
                Operations.Remove(SelectedOperation);
                SelectedOperation = null;
                return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                "Are you sure you want to delete this rescue operation?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                await DatabaseManager.DeleteAdminRescueOperationAsync(SelectedOperation.OperationId);

                MessageBox.Show("Rescue operation deleted successfully.",
                    "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadOperationsAsync();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show(
                    "Could not delete rescue operation.\n\nThis operation may already be used in assignments or other records.",
                    "Delete Restricted",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Could not delete rescue operation.\n\n" + ex.Message,
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred while deleting.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateOperation(AdminRescueOperation operation)
        {
            if (string.IsNullOrWhiteSpace(operation.OperationId))
            {
                MessageBox.Show("Operation ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (operation.OperationId.Length > 10)
            {
                MessageBox.Show("Operation ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(operation.Event))
            {
                MessageBox.Show("Event ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (operation.Event.Length > 10)
            {
                MessageBox.Show("Event ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(operation.Location))
            {
                MessageBox.Show("Location ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (operation.Location.Length > 10)
            {
                MessageBox.Show("Location ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (operation.DateStarted == null)
            {
                MessageBox.Show("Date started is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(operation.Status))
            {
                MessageBox.Show("Status is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (operation.Status.Length > 255)
            {
                MessageBox.Show("Status must be 255 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}