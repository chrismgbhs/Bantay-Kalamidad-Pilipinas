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
    public class admindashboard_rescue_Rescuers_viewModel : ObservableObject, IAdminRescueModuleViewModel
    {
        private ObservableCollection<AdminRescuer> _rescuers;
        private AdminRescuer _selectedRescuer;
        private string _selectedRescuerFilter;
        private string _rescuerSearchText;
        private bool _isTableReadOnly = true;
        private Visibility _actionButtonsVisibility = Visibility.Collapsed;

        // Tracks the highest pending sequence number so multiple "Add" clicks
        // before saving don't all get the same ID from the database.
        // Reset to -1 whenever rows are saved or the grid is reloaded.
        private int _pendingIdSequence = -1;
        private int _pendingUserIdSequence = -1; // separate counter for User_ID

        public ObservableCollection<AdminRescuer> Rescuers
        {
            get => _rescuers;
            set
            {
                if (_rescuers != value)
                {
                    _rescuers = value;
                    OnPropertyChanged();
                }
            }
        }

        public AdminRescuer SelectedRescuer
        {
            get => _selectedRescuer;
            set
            {
                if (_selectedRescuer != value)
                {
                    _selectedRescuer = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedRescuerFilter
        {
            get => _selectedRescuerFilter;
            set
            {
                if (_selectedRescuerFilter != value)
                {
                    _selectedRescuerFilter = value;
                    OnPropertyChanged();
                    _ = LoadRescuersAsync();
                }
            }
        }

        public string RescuerSearchText
        {
            get => _rescuerSearchText;
            set
            {
                if (_rescuerSearchText != value)
                {
                    _rescuerSearchText = value;
                    OnPropertyChanged();
                    _ = LoadRescuersAsync();
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

        public admindashboard_rescue_Rescuers_viewModel()
        {
            Rescuers = new ObservableCollection<AdminRescuer>();
            SelectedRescuerFilter = "All Rescuers";

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            UpdateCommand = new AsyncRelayCommand(UpdateAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);

            _ = LoadRescuersAsync();
        }

        public void EnterAddMode()
        {
            IsTableReadOnly = false;
            ActionButtonsVisibility = Visibility.Visible;

            // IDs and Status are auto-generated — admin only provides Name, ContactNumber, and optionally User_ID
            _ = AddNewRescuerRowAsync();
        }

        private async Task AddNewRescuerRowAsync()
        {
            try
            {
                // Rescuer ID counter
                if (_pendingIdSequence < 0)
                {
                    string firstId = await DatabaseManager.GenerateRescuerIdAsync();
                    _pendingIdSequence = int.Parse(firstId.Substring(1));
                }
                else
                {
                    _pendingIdSequence++;
                }

                // User ID counter — separate so both are dedup-safe
                if (_pendingUserIdSequence < 0)
                {
                    string firstUserId = await DatabaseManager.GenerateUserIdAsync();
                    _pendingUserIdSequence = int.Parse(firstUserId);
                }
                else
                {
                    _pendingUserIdSequence++;
                }

                string newRescuerId = "V" + _pendingIdSequence.ToString("D4");
                string newUserId = _pendingUserIdSequence.ToString();

                AdminRescuer newRescuer = new AdminRescuer
                {
                    RescuerId = newRescuerId,
                    Name = string.Empty,
                    ContactNumber = string.Empty,
                    UserId = newUserId,
                    Status = "Active",
                    IsNew = true
                };

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Rescuers.Add(newRescuer);
                    SelectedRescuer = newRescuer;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not generate new rescuer ID.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private async Task LoadRescuersAsync()
        {
            try
            {
                ObservableCollection<AdminRescuer> loadedRescuers =
                    await DatabaseManager.GetAdminRescuersAsync(
                        SelectedRescuerFilter,
                        RescuerSearchText);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Rescuers = loadedRescuers;
                    _pendingIdSequence = -1;
                    _pendingUserIdSequence = -1;
                });
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Could not load rescuers from the database.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An unexpected error occurred while loading rescuers.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedRescuer == null)
            {
                MessageBox.Show("Please select the new rescuer row to save.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!SelectedRescuer.IsNew)
            {
                MessageBox.Show("This row already exists. Use UPDATE instead.",
                    "Save Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateRescuer(SelectedRescuer))
                return;

            try
            {
                await DatabaseManager.AddAdminRescuerAsync(SelectedRescuer);

                MessageBox.Show("Rescuer saved successfully.",
                    "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadRescuersAsync();
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                MessageBox.Show("A rescuer with this ID already exists.",
                    "Duplicate Rescuer ID", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Could not save rescuer.\n\n" + ex.Message,
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
            if (SelectedRescuer == null)
            {
                MessageBox.Show("Please select a rescuer to update.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedRescuer.IsNew)
            {
                MessageBox.Show("This is a new row. Use SAVE instead.",
                    "Update Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateRescuer(SelectedRescuer))
                return;

            try
            {
                await DatabaseManager.UpdateAdminRescuerAsync(SelectedRescuer);

                MessageBox.Show("Rescuer updated successfully.",
                    "Updated", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadRescuersAsync();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Could not update rescuer.\n\n" + ex.Message,
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
            if (SelectedRescuer == null)
            {
                MessageBox.Show("Please select a rescuer to delete.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedRescuer.IsNew)
            {
                Rescuers.Remove(SelectedRescuer);
                SelectedRescuer = null;
                return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                "Are you sure you want to delete this rescuer?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                await DatabaseManager.DeleteAdminRescuerAsync(SelectedRescuer.RescuerId);

                MessageBox.Show("Rescuer deleted successfully.",
                    "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadRescuersAsync();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Could not delete rescuer.\n\nThis rescuer may already be used in assignments.\n\n" + ex.Message,
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred while deleting.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateRescuer(AdminRescuer rescuer)
        {
            if (string.IsNullOrWhiteSpace(rescuer.RescuerId))
            {
                MessageBox.Show("Rescuer ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (rescuer.RescuerId.Length > 10)
            {
                MessageBox.Show("Rescuer ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(rescuer.Name) && rescuer.Name.Length > 255)
            {
                MessageBox.Show("Name must be 255 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(rescuer.ContactNumber) && rescuer.ContactNumber.Length > 50)
            {
                MessageBox.Show("Contact number must be 50 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(rescuer.UserId) && !int.TryParse(rescuer.UserId, out _))
            {
                MessageBox.Show("User ID must be a valid number or left blank.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}