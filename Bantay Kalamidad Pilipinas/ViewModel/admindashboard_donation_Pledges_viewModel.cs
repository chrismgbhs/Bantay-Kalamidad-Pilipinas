using Bantay_Kalamidad_Pilipinas.Model;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    public class admindashboard_donation_Pledges_viewModel : ObservableObject, IAdminDonationModuleViewModel
    {
        private ObservableCollection<AdminPledges> _pledges;
        private AdminPledges _selectedPledges;
        private AdminPledges _selectedPledgeItems;
        private string _selectedPledgesFilter;
        private string _selectedPledgeItemsFilter;
        private string _pledgesSearchText;
        private string _pledgeItemsSearchText;
        private bool _isTableReadOnly = true;
        private Visibility _actionButtonsVisibility = Visibility.Collapsed;

        public ObservableCollection<AdminPledges> Pledge
        {
            get => _pledges;
            set
            {
                if (_pledges != value)
                {
                    _pledges = value;
                    OnPropertyChanged();
                }
            }
        }

        public AdminPledges SelectedPledges
        {
            get => _selectedPledges;
            set
            {
                if (_selectedPledges != value)
                {
                    _selectedPledges = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedPledgesFilter
        {
            get => _selectedPledgesFilter;
            set
            {
                if (_selectedPledgesFilter != value)
                {
                    _selectedPledgesFilter = value;
                    OnPropertyChanged();
                    _ = LoadPledgesAsync();
                }
            }
        }

        public string PledgesSearchText
        {
            get => _pledgesSearchText;
            set
            {
                if (_pledgesSearchText != value)
                {
                    _pledgesSearchText = value;
                    OnPropertyChanged();
                    _ = LoadPledgesAsync();
                }
            }
        }
        public AdminPledges SelectedPledgeItems
        {
            get => _selectedPledgeItems;
            set
            {
                if (_selectedPledgeItems != value)
                {
                    _selectedPledgeItems = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedPledgeItemsFilter
        {
            get => _selectedPledgeItemsFilter;
            set
            {
                if (_selectedPledgeItemsFilter != value)
                {
                    _selectedPledgeItemsFilter = value;
                    OnPropertyChanged();
                    _ = LoadPledgesItemsAsync();
                }
            }
        }

        public string PledgeItemsSeacrhText
        {
            get => _pledgeItemsSearchText;
            set
            {
                if (_pledgeItemsSearchText != value)
                {
                    _pledgeItemsSearchText = value;
                    OnPropertyChanged();
                    _ = LoadPledgesItemsAsync();
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

        public admindashboard_donation_Pledges_viewModel()
        {
            Pledge = new ObservableCollection<AdminPledges>();
            SelectedPledgesFilter = "All Pledges";
            SelectedPledgeItemsFilter = "All Items";


            SaveCommand = new AsyncRelayCommand(SaveAsync);
            UpdateCommand = new AsyncRelayCommand(UpdateAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);

            _ = LoadPledgesAsync();
            _ = LoadPledgesItemsAsync();


        }
        public void EnterAddMode()
        {
            IsTableReadOnly = false;
            ActionButtonsVisibility = Visibility.Visible;

            AdminPledges newPledges = new AdminPledges
            {
                PledgeId = string.Empty,
                DonorId = string.Empty,
                DatePledge = DateTime.MinValue,
                PledgeStatus = string.Empty,

            };

            Pledge.Add(newPledges);
            SelectedPledges = newPledges;
        }

        public void EnterAddItemMode()
        {
            IsTableReadOnly = false;
            ActionButtonsVisibility = Visibility.Visible;

            AdminPledges newPledgesItems = new AdminPledges
            {
                PledgeItemId = string.Empty,
                PledgeId = string.Empty,
                ItemName = string.Empty,
                Quantity = 0,
                ExpectedDelivery = DateTime.MinValue,
            };

            Pledge.Add(newPledgesItems);
            SelectedPledgeItems = newPledgesItems;
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

        private async Task LoadPledgesAsync()
        {
            try
            {
                ObservableCollection<AdminPledges> loadedPledges =
                    await DatabaseManager.GetAdminPledgesAsync(
                        SelectedPledgesFilter,
                        PledgesSearchText);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Pledge = loadedPledges;
                });
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Could not load pledges from the database.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An unexpected error occurred while loading pledges.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedPledges == null || SelectedPledgeItems == null)
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
                if (!string.IsNullOrWhiteSpace(SelectedPledges.PledgeId))
                {
                    if (!ValidatePledges(SelectedPledges))
                        return;
                    await DatabaseManager.AddAdminPledgeAsync(SelectedPledges);

                    MessageBox.Show("Pledge saved successfully.",
                         "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadPledgesAsync();
                }
                else if (!string.IsNullOrWhiteSpace(SelectedPledgeItems.PledgeItemId))
                {
                    if (!ValidatePledgeItems(SelectedPledgeItems))
                        return; 
                    await DatabaseManager.AddAdminPledgeItemsAsync(SelectedPledgeItems);

                    MessageBox.Show("Pledge Item saved successfully.",
                        "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadPledgesItemsAsync();
                }
                else
                {
                    MessageBox.Show("No valid Pledge or Pledge Item to save.",
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
                    "Could not save record.\n\nThe Pledge ID, Donor ID, or Item does not exist in the database.",
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
            if (SelectedPledges == null || SelectedPledgeItems == null)
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
                if (!string.IsNullOrWhiteSpace(SelectedPledges.PledgeId))
                {
                    if (!ValidatePledges(SelectedPledges))
                        return;

                    await DatabaseManager.UpdateAdminPledgeAsync(SelectedPledges);

                    MessageBox.Show("Pledge updated successfully.",
                        "Updated", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadPledgesAsync();
                }
                else if (!string.IsNullOrWhiteSpace(SelectedPledgeItems.PledgeItemId))
                {
                    if (!ValidatePledgeItems(SelectedPledgeItems))
                        return;

                    await DatabaseManager.UpdateAdminPledgeItemsAsync(SelectedPledgeItems);

                    MessageBox.Show("Pledge Donated item updated successfully.",
                        "Updated", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadPledgesItemsAsync();
                }
                else
                {
                    MessageBox.Show("No valid Pledge or Pledge Item to update.",
                        "Update Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show(
                    "Could not update record.\n\nThe Pledge ID, Donor ID, or Item does not exist in the database.",
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
            if (SelectedPledges == null || SelectedPledgeItems == null)
            {
                MessageBox.Show("Please select a row to delete.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string recordType = !string.IsNullOrWhiteSpace(SelectedPledges.PledgeId)
                ? "pledge"
                : !string.IsNullOrWhiteSpace(SelectedPledgeItems.PledgeItemId)
                    ? "pledge item"
                    : string.Empty;

            if (string.IsNullOrEmpty(recordType))
            {
                MessageBox.Show("No valid Pledge or Pledge Item selected.",
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
                if (recordType == "pledge")
                {
                    await DatabaseManager.DeleteAdminPledgeAsync(SelectedPledges.PledgeId);

                    MessageBox.Show("Pledge deleted successfully.",
                        "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadPledgesAsync();
                }
                else if (recordType == "pledge item")
                {
                    await DatabaseManager.DeleteAdminPledgeItemAsync(SelectedPledgeItems.PledgeItemId);

                    MessageBox.Show("Pledge item deleted successfully.",
                        "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadPledgesItemsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not delete {recordType}.\n\n" + ex.Message,
                    "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private bool ValidatePledges(AdminPledges pledges)
        {
            if (string.IsNullOrWhiteSpace(pledges.PledgeId))
            {
                MessageBox.Show("Pledge ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (pledges.PledgeId.Length > 10)
            {
                MessageBox.Show("Pledge ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(pledges.DonorId))
            {
                MessageBox.Show("Donor ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (pledges.DonorId.Length > 10)
            {
                MessageBox.Show("Donor ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }


            if (string.IsNullOrWhiteSpace(pledges.PledgeStatus))
            {
                MessageBox.Show("Pledge Status is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!pledges.DatePledge.HasValue)
            {
                MessageBox.Show("Pledge Date is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private async Task LoadPledgesItemsAsync()
        {
            try
            {
                ObservableCollection<AdminPledges> loadedPledgeItems =
                    await DatabaseManager.GetAdminPledgeItemsAsync(
                        SelectedPledgeItemsFilter,
                        PledgeItemsSeacrhText);

                Application.Current.Dispatcher.Invoke(() =>
                {   
                    Pledge = loadedPledgeItems;
                });
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Could not load pledge items from the database.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An unexpected error occurred while loading pledge items.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool ValidatePledgeItems(AdminPledges pledgeItems)
        {
            if (string.IsNullOrWhiteSpace(pledgeItems.PledgeItemId))
            {
                MessageBox.Show("Pledge Item ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (pledgeItems.PledgeItemId.Length > 10)
            {
                MessageBox.Show("Pledge Item ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(pledgeItems.PledgeId))
            {
                MessageBox.Show("Pledge ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (pledgeItems.PledgeId.Length > 10)
            {
                MessageBox.Show("Pledge ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(pledgeItems.ItemName))
            {
                MessageBox.Show("Item Name is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            
            if (pledgeItems.Quantity <= 0)
            {
                MessageBox.Show("Quantity is required and must be greater than zero.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!pledgeItems.ExpectedDelivery.HasValue)
            {
                MessageBox.Show("Expected Delivery Date is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }


            return true;
        }
    }
}
