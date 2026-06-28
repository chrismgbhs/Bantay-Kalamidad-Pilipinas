using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Bantay_Kalamidad_Pilipinas.Model;

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
        private int _pendingIdSequence = -1;

        // Dropdown sources
        private ObservableCollection<DropdownItem> _availableEvents;
        private ObservableCollection<DropdownItem> _availableLocations;
        private DropdownItem _selectedEventDropdown;
        private DropdownItem _selectedLocationDropdown;
        private Visibility _dropdownPanelVisibility = Visibility.Collapsed;

        public ObservableCollection<AdminRescueOperation> Operations
        {
            get => _operations;
            set { if (_operations != value) { _operations = value; OnPropertyChanged(); } }
        }

        public AdminRescueOperation SelectedOperation
        {
            get => _selectedOperation;
            set { if (_selectedOperation != value) { _selectedOperation = value; OnPropertyChanged(); } }
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
            set { if (_isTableReadOnly != value) { _isTableReadOnly = value; OnPropertyChanged(); } }
        }

        public Visibility ActionButtonsVisibility
        {
            get => _actionButtonsVisibility;
            set { if (_actionButtonsVisibility != value) { _actionButtonsVisibility = value; OnPropertyChanged(); } }
        }

        // Dropdowns for Add/Manage mode
        public ObservableCollection<DropdownItem> AvailableEvents
        {
            get => _availableEvents;
            set { if (_availableEvents != value) { _availableEvents = value; OnPropertyChanged(); } }
        }

        public ObservableCollection<DropdownItem> AvailableLocations
        {
            get => _availableLocations;
            set { if (_availableLocations != value) { _availableLocations = value; OnPropertyChanged(); } }
        }

        public DropdownItem SelectedEventDropdown
        {
            get => _selectedEventDropdown;
            set
            {
                if (_selectedEventDropdown != value)
                {
                    _selectedEventDropdown = value;
                    OnPropertyChanged();
                    // Keep selected operation's EventId/Event in sync with dropdown
                    if (SelectedOperation != null && value != null)
                    {
                        SelectedOperation.EventId = value.Id;
                        SelectedOperation.Event = value.Display;
                    }
                }
            }
        }

        public DropdownItem SelectedLocationDropdown
        {
            get => _selectedLocationDropdown;
            set
            {
                if (_selectedLocationDropdown != value)
                {
                    _selectedLocationDropdown = value;
                    OnPropertyChanged();
                    if (SelectedOperation != null && value != null)
                    {
                        SelectedOperation.LocationId = value.Id;
                        SelectedOperation.Location = value.Display;
                    }
                }
            }
        }

        /// <summary>Shows the Event/Location dropdown panel in Add/Manage mode.</summary>
        public Visibility DropdownPanelVisibility
        {
            get => _dropdownPanelVisibility;
            set { if (_dropdownPanelVisibility != value) { _dropdownPanelVisibility = value; OnPropertyChanged(); } }
        }

        public ICommand SaveCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddNewLocationCommand { get; }

        public admindashboard_rescue_Operations_viewModel()
        {
            Operations = new ObservableCollection<AdminRescueOperation>();
            AvailableEvents = new ObservableCollection<DropdownItem>();
            AvailableLocations = new ObservableCollection<DropdownItem>();
            SelectedOperationFilter = "All Operations";

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            UpdateCommand = new AsyncRelayCommand(UpdateAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);
            AddNewLocationCommand = new AsyncRelayCommand(AddNewLocationAsync);

            _ = LoadOperationsAsync();
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

                if (_pendingIdSequence < 0)
                {
                    string firstId = await DatabaseManager.GenerateOperationIdAsync();
                    _pendingIdSequence = int.Parse(firstId.Substring(2)); // "RO" prefix
                }
                else
                {
                    _pendingIdSequence++;
                }

                string newOpId = "RO" + _pendingIdSequence.ToString("D4");

                AdminRescueOperation newOp = new AdminRescueOperation
                {
                    OperationId = newOpId,
                    EventId = string.Empty,
                    Event = string.Empty,
                    LocationId = string.Empty,
                    Location = string.Empty,
                    DateStarted = null,
                    Status = "Pending",
                    IsNew = true
                };

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Operations.Add(newOp);
                    SelectedOperation = newOp;
                    SelectedEventDropdown = null;
                    SelectedLocationDropdown = null;
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
            var events = await DatabaseManager.GetAvailableDisasterEventsAsync();
            var locations = await DatabaseManager.GetAvailableLocationsAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                AvailableEvents.Clear();
                foreach (var e in events)
                    AvailableEvents.Add(new DropdownItem { Id = e.Id, Display = e.Name });

                AvailableLocations.Clear();
                foreach (var l in locations)
                    AvailableLocations.Add(new DropdownItem { Id = l.Id, Display = l.Display });
            });
        }

        private async Task AddNewLocationAsync()
        {
            // Simple input dialog via MessageBox pattern — asks for Barangay, City, Province
            string barangay = ShowInputDialog("Enter Barangay:", "New Location");
            if (string.IsNullOrWhiteSpace(barangay)) return;

            string city = ShowInputDialog("Enter City/Municipality:", "New Location");
            if (string.IsNullOrWhiteSpace(city)) return;

            string province = ShowInputDialog("Enter Province:", "New Location");
            if (string.IsNullOrWhiteSpace(province)) return;

            try
            {
                string newId = await DatabaseManager.AddLocationAsync(barangay, city, province);
                string display = $"{barangay}, {city}";

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var newItem = new DropdownItem { Id = newId, Display = display };
                    AvailableLocations.Add(newItem);
                    SelectedLocationDropdown = newItem;
                });

                MessageBox.Show($"Location added with ID: {newId}",
                    "Location Added", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not add location.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadOperationsAsync()
        {
            try
            {
                var loaded = await DatabaseManager.GetAdminRescueOperationsAsync(
                    SelectedOperationFilter, OperationSearchText);
                Application.Current.Dispatcher.Invoke(() => { Operations = loaded; _pendingIdSequence = -1; });
            }
            catch (SqlException)
            {
                MessageBox.Show("Could not load operations from the database.",
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

            // Sync IDs from dropdowns into the selected operation before validating
            if (SelectedEventDropdown != null)
            {
                SelectedOperation.EventId = SelectedEventDropdown.Id;
                SelectedOperation.Event = SelectedEventDropdown.Display;
            }
            if (SelectedLocationDropdown != null)
            {
                SelectedOperation.LocationId = SelectedLocationDropdown.Id;
                SelectedOperation.Location = SelectedLocationDropdown.Display;
            }

            if (!ValidateOperation(SelectedOperation)) return;

            try
            {
                await DatabaseManager.AddAdminRescueOperationAsync(SelectedOperation);
                MessageBox.Show("Operation saved successfully.", "Saved",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadOperationsAsync();
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                MessageBox.Show("An operation with this ID already exists.",
                    "Duplicate Operation ID", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("The Event ID or Location ID does not exist in the database.",
                    "Foreign Key Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not save operation.\n\n" + ex.Message,
                    "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateAsync()
        {
            if (SelectedOperation == null)
            {
                MessageBox.Show("Please select an operation to update.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedOperation.IsNew)
            {
                MessageBox.Show("This is a new row. Use SAVE instead.",
                    "Update Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Apply dropdown selections if changed
            if (SelectedEventDropdown != null)
            {
                SelectedOperation.EventId = SelectedEventDropdown.Id;
                SelectedOperation.Event = SelectedEventDropdown.Display;
            }
            if (SelectedLocationDropdown != null)
            {
                SelectedOperation.LocationId = SelectedLocationDropdown.Id;
                SelectedOperation.Location = SelectedLocationDropdown.Display;
            }

            if (!ValidateOperation(SelectedOperation)) return;

            try
            {
                await DatabaseManager.UpdateAdminRescueOperationAsync(SelectedOperation);
                MessageBox.Show("Operation updated successfully.", "Updated",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadOperationsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not update operation.\n\n" + ex.Message,
                    "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedOperation == null)
            {
                MessageBox.Show("Please select an operation to delete.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedOperation.IsNew)
            {
                Operations.Remove(SelectedOperation);
                SelectedOperation = null;
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this rescue operation?",
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                await DatabaseManager.DeleteAdminRescueOperationAsync(SelectedOperation.OperationId);
                MessageBox.Show("Operation deleted successfully.", "Deleted",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadOperationsAsync();
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("Cannot delete — this operation has assignments or related records.",
                    "Delete Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not delete operation.\n\n" + ex.Message,
                    "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateOperation(AdminRescueOperation op)
        {
            if (string.IsNullOrWhiteSpace(op.OperationId))
            {
                MessageBox.Show("Operation ID is required.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(op.EventId))
            {
                MessageBox.Show("Please select a Disaster Event.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(op.LocationId))
            {
                MessageBox.Show("Please select a Location.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!op.DateStarted.HasValue)
            {
                MessageBox.Show("Date Started is required.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Simple WPF input dialog — replaces Microsoft.VisualBasic.Interaction.InputBox
        /// so no extra assembly reference is needed.
        /// Returns the entered text, or null/empty if the user cancelled.
        /// </summary>
        private static string ShowInputDialog(string prompt, string title)
        {
            Window dialog = new Window
            {
                Title = title,
                Width = 360,
                Height = 140,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(12) };
            panel.Children.Add(new TextBlock { Text = prompt, Margin = new Thickness(0, 0, 0, 6) });

            TextBox input = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
            panel.Children.Add(input);

            StackPanel buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            string result = string.Empty;

            Button ok = new Button { Content = "OK", Width = 72, Margin = new Thickness(0, 0, 8, 0), IsDefault = true };
            ok.Click += (s, e) => { result = input.Text; dialog.Close(); };

            Button cancel = new Button { Content = "Cancel", Width = 72, IsCancel = true };
            cancel.Click += (s, e) => { result = string.Empty; dialog.Close(); };

            buttons.Children.Add(ok);
            buttons.Children.Add(cancel);
            panel.Children.Add(buttons);

            dialog.Content = panel;
            dialog.ShowDialog();

            return result;
        }
    }

    /// <summary>Generic Id+Display pair for ComboBox dropdowns.</summary>
    public class DropdownItem
    {
        public string Id { get; set; }
        public string Display { get; set; }
        public override string ToString() => Display;
    }
}