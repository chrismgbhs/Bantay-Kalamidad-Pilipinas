using Bantay_Kalamidad_Pilipinas.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    public class admindashboard_donation_Donations_viewModel : ObservableObject, IAdminDonationModuleViewModel
    {
        private ObservableCollection<AdminDonations> _donations;
        private AdminDonations _selectedDonations;
        private AdminDonations _selectedDonatedItems;
        private string _selectedDonationsFilter;
        private string _selectedDonatedItemsFilter;
        private string _donationsSearchText;
        private string _donatedItemsSearchText;
        private bool _isTableReadOnly = true;
        private Visibility _actionButtonsVisibility = Visibility.Collapsed;

        public ObservableCollection<AdminDonations> Donations
        {
            get => _donations;
            set
            {
                if (_donations != value)
                {
                    _donations = value;
                    OnPropertyChanged();
                }
            }
        }

        public AdminDonations SelectedDonations
        {
            get => _selectedDonations;
            set
            {
                if (_selectedDonations != value)
                {
                    _selectedDonations = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedDonationsFilter
        {
            get => _selectedDonationsFilter;
            set
            {
                if (_selectedDonationsFilter != value)
                {
                    _selectedDonationsFilter = value;
                    OnPropertyChanged();
                    _ = LoadDonationsAsync();
                }
            }
        }

        public string DonationsSearchText
        {
            get => _donationsSearchText;
            set
            {
                if (_donationsSearchText != value)
                {
                    _donationsSearchText = value;
                    OnPropertyChanged();
                    _ = LoadDonationsAsync();
                }
            }
        }
        public AdminDonations SelectedDonatedItems
        {
            get => _selectedDonatedItems;
            set
            {
                if (_selectedDonatedItems != value)
                {
                    _selectedDonatedItems = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedDonatedItemsFilter
        {
            get => _selectedDonatedItemsFilter;
            set
            {
                if (_selectedDonatedItemsFilter != value)
                {
                    _selectedDonatedItemsFilter = value;
                    OnPropertyChanged();
                    _ = LoadDonatedItemsAsync();
                }
            }
        }

        public string DonatedItemsSeacrhText
        {
            get => _donatedItemsSearchText;
            set
            {
                if (_donatedItemsSearchText != value)
                {
                    _donatedItemsSearchText = value;
                    OnPropertyChanged();
                    _ = LoadDonatedItemsAsync();
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

        public admindashboard_donation_Donations_viewModel()
        {
            Donations = new ObservableCollection<AdminDonations>();
            SelectedDonationsFilter = "All Donations";

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            UpdateCommand = new AsyncRelayCommand(UpdateAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);

            _ = LoadDonationsAsync();
            _ = LoadDonatedItemsAsync();


        }
        public void EnterAddMode()
        {
            IsTableReadOnly = false;
            ActionButtonsVisibility = Visibility.Visible;

            AdminDonations newDonations = new AdminDonations
            {
                DonationId = string.Empty,
                Donor = string.Empty,
                Event = string.Empty,
                DateReceived = DateTime.MinValue,
            };

            Donations.Add(newDonations);
            SelectedDonations = newDonations;
        }

        public void EnterAddItemMode()
        {
            IsTableReadOnly = false;
            ActionButtonsVisibility = Visibility.Visible;

            AdminDonations newDonatedItems = new AdminDonations
            {
                DonatedItemID = string.Empty,
                DonationId = string.Empty,
                ItemName = string.Empty,
                Quantity = 0,
            };

            Donations.Add(newDonatedItems);
            SelectedDonations = newDonatedItems;
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

        private async Task LoadDonationsAsync()
        {
            try
            {
                ObservableCollection<AdminDonations> loadedDonations =
                    await DatabaseManager.GetAdminDonationsAsync(
                        SelectedDonationsFilter,
                        DonationsSearchText);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Donations = loadedDonations;
                });
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Could not load donations from the database.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An unexpected error occurred while loading donations.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedDonations == null || SelectedDonatedItems == null)
            {
                MessageBox.Show("Please select a row to save.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //if (!SelectedDonations.IsNew)
            //{
            //    MessageBox.Show("This row already exists. Use UPDATE instead.",
            //        "Save Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    return;
            //}

            try
            {
                if (!string.IsNullOrWhiteSpace(SelectedDonations.DonationId))
                {
                    if (!ValidateDonations(SelectedDonations))
                        return;
                    await DatabaseManager.AddAdminDonationsAsync(SelectedDonations);

                    MessageBox.Show("Donation saved successfully.",
                         "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadDonationsAsync();
                }
                else if (!string.IsNullOrWhiteSpace(SelectedDonatedItems.DonatedItemID))
                {
                    if (!ValidateDonatedItems(SelectedDonatedItems))
                        return;
                    await DatabaseManager.AddAdminDonatedItemsAsync(SelectedDonatedItems);

                    MessageBox.Show("Donated Item saved successfully.",
                        "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadDonatedItemsAsync();
                }
                else
                {
                    MessageBox.Show("No valid Donation or Donated Item to save.",
                        "Save Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                MessageBox.Show("A record with this ID already exists.",
                    "Duplicate ID", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show(
                    "Could not save record.\n\nThe Donation ID, Donor, or Item does not exist in the database.",
                    "Foreign Key Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not save record.\n\n" + ex.Message,
                    "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateAsync()
        {
            if (SelectedDonations == null || SelectedDonatedItems == null)
            {
                MessageBox.Show("Please select a row to update.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //if (SelectedAssignment.IsNew)
            //{
            //    MessageBox.Show("This is a new row. Use SAVE instead.",
            //        "Update Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    return;
            //}

            try
            {
                if (!string.IsNullOrWhiteSpace(SelectedDonations.DonationId))
                {
                    if (!ValidateDonations(SelectedDonations))
                        return;

                    await DatabaseManager.UpdateAdminDonationsAsync(SelectedDonations);

                    MessageBox.Show("Donation updated successfully.",
                        "Updated", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadDonationsAsync();
                }
                else if (!string.IsNullOrWhiteSpace(SelectedDonations.DonatedItemID))
                {
                    if (!ValidateDonatedItems(SelectedDonatedItems))
                        return;

                    await DatabaseManager.UpdateAdminDonatedItemsAsync(SelectedDonations);

                    MessageBox.Show("Donated item updated successfully.",
                        "Updated", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadDonatedItemsAsync();
                }
                else
                {
                    MessageBox.Show("No valid Donation or Donated Item to update.",
                        "Update Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show(
                    "Could not update record.\n\nThe Donation ID, Donor, or Item does not exist in the database.",
                    "Foreign Key Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not update record.\n\n" + ex.Message,
                    "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedDonations == null)
            {
                MessageBox.Show("Please select a row to delete.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string recordType = !string.IsNullOrWhiteSpace(SelectedDonations.DonationId)
                ? "donation"
                : !string.IsNullOrWhiteSpace(SelectedDonations.DonatedItemID)
                    ? "donated item"
                    : string.Empty;

            if (string.IsNullOrEmpty(recordType))
            {
                MessageBox.Show("No valid Donation or Donated Item selected.",
                    "Delete Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                $"Are you sure you want to delete this {recordType}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                if (recordType == "donation")
                {
                    await DatabaseManager.DeleteAdminDonationsAsync(SelectedDonations.DonationId);

                    MessageBox.Show("Donation deleted successfully.",
                        "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadDonationsAsync();
                }
                else if (recordType == "donated item")
                {
                    await DatabaseManager.DeleteAdminDonatedItemAsync(SelectedDonations.DonatedItemID);

                    MessageBox.Show("Donated item deleted successfully.",
                        "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadDonatedItemsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not delete {recordType}.\n\n" + ex.Message,
                    "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private bool ValidateDonations(AdminDonations donations)
        {
            if (string.IsNullOrWhiteSpace(donations.DonationId))
            {
                MessageBox.Show("Donation ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (donations.DonationId.Length > 10)
            {
                MessageBox.Show("Donation ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(donations.DonorId))
            {
                MessageBox.Show("Donor ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (donations.DonorId.Length > 10)
            {
                MessageBox.Show("Donor ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(donations.Donor))
            {
                MessageBox.Show("Donor name or Donor ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(donations.Event))
            {
                MessageBox.Show("Event ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (donations.Event.Length > 10)
            {
                MessageBox.Show("Event ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!donations.DateReceived.HasValue)
            {
                MessageBox.Show("Received Date is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private async Task LoadDonatedItemsAsync()
        {
            try
            {
                ObservableCollection<AdminDonations> loadedDonatedItems =
                    await DatabaseManager.GetAdminDonatedItemsAsync(
                        SelectedDonatedItemsFilter,
                        DonatedItemsSeacrhText);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Donations = loadedDonatedItems;
                });
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Could not load donated items from the database.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An unexpected error occurred while loading donated items.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool ValidateDonatedItems(AdminDonations donatedItems)
        {
            if (string.IsNullOrWhiteSpace(donatedItems.DonatedItemID))
            {
                MessageBox.Show("Donated Item ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (donatedItems.DonatedItemID.Length > 10)
            {
                MessageBox.Show("Donated Item ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(donatedItems.DonationId))
            {
                MessageBox.Show("Donation ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (donatedItems.DonationId.Length > 10)
            {
                MessageBox.Show("Donation ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(donatedItems.ItemName))
            {
                MessageBox.Show("Item Name is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (donatedItems.ItemName.Length > 10)
            {
                MessageBox.Show("Item Name must be 255 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (donatedItems.Quantity <= 0)
            {
                MessageBox.Show("Quantity is required and must be greater than zero.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

    }

}
