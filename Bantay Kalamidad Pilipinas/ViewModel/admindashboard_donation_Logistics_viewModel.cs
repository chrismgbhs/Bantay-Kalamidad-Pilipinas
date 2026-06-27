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
    public class admindashboard_donation_Logistics_viewModel : ObservableObject, IAdminDonationModuleViewModel
    {
        private ObservableCollection<AdminLogistics> _logistics;
        private AdminLogistics _selectedDeliveries;
        private AdminLogistics _selectedPickups;
        private string _selectedDeliveriesFilter;
        private string _selectedPickupsFilter;
        private string _deliveriesSearchText;
        private string _pickupsSearchText;
        private bool _isTableReadOnly = true;
        private Visibility _actionButtonsVisibility = Visibility.Collapsed;

        public ObservableCollection<AdminLogistics> Logistics
        {
            get => _logistics;
            set
            {
                if (_logistics != value)
                {
                    _logistics = value;
                    OnPropertyChanged();
                }
            }
        }

        public AdminLogistics SelectedDeliveries
        {
            get => _selectedDeliveries;
            set
            {
                if (_selectedDeliveries != value)
                {
                    _selectedDeliveries = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedDeliveriesFilter
        {
            get => _selectedDeliveriesFilter;
            set
            {
                if (_selectedDeliveriesFilter != value)
                {
                    _selectedDeliveriesFilter = value;
                    OnPropertyChanged();
                    _ = LoadDeliveriesAsync();
                }
            }
        }

        public string DeliveriesSearchText
        {
            get => _deliveriesSearchText;
            set
            {
                if (_deliveriesSearchText != value)
                {
                    _deliveriesSearchText = value;
                    OnPropertyChanged();
                    _ = LoadDeliveriesAsync();
                }
            }
        }
        public AdminLogistics SelectedPickups
        {
            get => _selectedPickups;
            set
            {
                if (_selectedPickups != value)
                {
                    _selectedPickups = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedPickupsFilter
        {
            get => _selectedPickupsFilter;
            set
            {
                if (_selectedPickupsFilter != value)
                {
                    _selectedPickupsFilter = value;
                    OnPropertyChanged();
                    _ = LoadPickupsAsync();
                }
            }
        }

        public string PickupsSeacrhText
        {
            get => _pickupsSearchText;
            set
            {
                if (_pickupsSearchText != value)
                {
                    _pickupsSearchText = value;
                    OnPropertyChanged();
                    _ = LoadPickupsAsync();
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

        public admindashboard_donation_Logistics_viewModel()
        {
            Logistics = new ObservableCollection<AdminLogistics>();
            SelectedDeliveriesFilter = "All Deliveries";
            SelectedPickupsFilter = "All Pickups";


            SaveCommand = new AsyncRelayCommand(SaveAsync);
            UpdateCommand = new AsyncRelayCommand(UpdateAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);

            _ = LoadDeliveriesAsync();
            _ = LoadPickupsAsync();


        }
        public void EnterAddMode()
        {
            IsTableReadOnly = false;
            ActionButtonsVisibility = Visibility.Visible;

            AdminLogistics newDeliveries = new AdminLogistics
            {
                DeliveryId = string.Empty,
                Distribution = string.Empty,
                DeliveryDate = DateTime.MinValue,
                Status = string.Empty,

            };

            Logistics.Add(newDeliveries);
            SelectedDeliveries = newDeliveries;
        }
        public void EnterAddItemMode()
        {
            IsTableReadOnly = false;
            ActionButtonsVisibility = Visibility.Visible;

            AdminLogistics newPickups = new AdminLogistics
            {
                PickupId = string.Empty,
                Donation = string.Empty,
                PickupDate = DateTime.MinValue,
                Status = string.Empty,

            };

            Logistics.Add(newPickups);
            SelectedPickups = newPickups;
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

        private async Task LoadDeliveriesAsync()
        {
            try
            {
                ObservableCollection<AdminLogistics> loadedDeliveries =
                    await DatabaseManager.GetAdminDeliveriesAsync(
                        SelectedDeliveriesFilter,
                        DeliveriesSearchText);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Logistics = loadedDeliveries;
                });
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Could not load deliveries from the database.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An unexpected error occurred while loading deliveries.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedDeliveries == null || SelectedPickups == null)
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
                if (!string.IsNullOrWhiteSpace(SelectedDeliveries.DeliveryId))
                {
                    if (!ValidateDelivery(SelectedDeliveries))
                        return;
                    await DatabaseManager.AddAdminDeliveriesAsync(SelectedDeliveries);

                    MessageBox.Show("Delivery saved successfully.",
                         "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadDeliveriesAsync();
                }
                else if (!string.IsNullOrWhiteSpace(SelectedPickups.PickupId))
                {
                    if (!ValidatePickup(SelectedPickups))
                        return;
                    await DatabaseManager.AddAdminPickupsAsync(SelectedPickups);

                    MessageBox.Show("Pickup saved successfully.",
                        "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadPickupsAsync();
                }
                else
                {
                    MessageBox.Show("No valid Delivery or Pickup to save.",
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
                    "Could not save record.\n\nThe Delivery ID, Pickup ID, or Item does not exist in the database.",
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
            if (SelectedDeliveries == null || SelectedPickups == null)
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
                if (!string.IsNullOrWhiteSpace(SelectedDeliveries.DeliveryId))
                {
                    if (!ValidateDelivery(SelectedDeliveries))
                        return;

                    await DatabaseManager.UpdateAdminDeliveriesAsync(SelectedDeliveries);

                    MessageBox.Show("Deliveries updated successfully.",
                        "Updated", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadDeliveriesAsync();
                }
                else if (!string.IsNullOrWhiteSpace(SelectedPickups.PickupId))
                {
                    if (!ValidatePickup(SelectedPickups))
                        return;

                    await DatabaseManager.UpdateAdminPickupsAsync(SelectedPickups);

                    MessageBox.Show("Pickup updated successfully.",
                        "Updated", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadPickupsAsync();
                }
                else
                {
                    MessageBox.Show("No valid Delivery or Pickup to update.",
                        "Update Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show(
                    "Could not update record.\n\nThe Delivery ID, Pickup ID, or Item does not exist in the database.",
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
            if (SelectedDeliveries == null || SelectedPickups == null)
            {
                MessageBox.Show("Please select a row to delete.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string recordType = !string.IsNullOrWhiteSpace(SelectedDeliveries.DeliveryId)
                ? "delivery"
                : !string.IsNullOrWhiteSpace(SelectedPickups.PickupId)
                    ? "pickup"
                    : string.Empty;

            if (string.IsNullOrEmpty(recordType))
            {
                MessageBox.Show("No valid Delivery or Pickup selected.",
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
                if (recordType == "delivery")
                {
                    await DatabaseManager.DeleteAdminDeliveriesAsync(SelectedDeliveries.DeliveryId);

                    MessageBox.Show("Delivery deleted successfully.",
                        "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadDeliveriesAsync();
                }
                else if (recordType == "pickup")
                {
                    await DatabaseManager.DeleteAdminPickupsAsync(SelectedPickups.PickupId);

                    MessageBox.Show("Pickup deleted successfully.",
                        "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadPickupsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not delete {recordType}.\n\n" + ex.Message,
                    "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private bool ValidateDelivery(AdminLogistics delivery)
        {
            if (string.IsNullOrWhiteSpace(delivery.DeliveryId))
            {
                MessageBox.Show("Delivery ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (delivery.DeliveryId.Length > 10)
            {
                MessageBox.Show("Delivery ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(delivery.Distribution))
            {
                MessageBox.Show("Distribution ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (delivery.Distribution.Length > 10)
            {
                MessageBox.Show("Distribution ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }


            if (string.IsNullOrWhiteSpace(delivery.Status))
            {
                MessageBox.Show("Delivery Status is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!delivery.DeliveryDate.HasValue)
            {
                MessageBox.Show("Delivery Date is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private async Task LoadPickupsAsync()
        {
            try
            {
                ObservableCollection<AdminLogistics> loadedPickup =
                    await DatabaseManager.GetAdminPickupAsync(
                        SelectedPickupsFilter,
                        PickupsSeacrhText);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Logistics = loadedPickup;
                });
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Could not load pickups from the database.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An unexpected error occurred while loading pickups.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool ValidatePickup(AdminLogistics pickup)
        {
            if (string.IsNullOrWhiteSpace(pickup.PickupId))
            {
                MessageBox.Show("Pickup ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (pickup.PickupId.Length > 10)
            {
                MessageBox.Show("Pickup ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(pickup.Donation))
            {
                MessageBox.Show("Donatiom ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (pickup.Donation.Length > 10)
            {
                MessageBox.Show("Donation ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(pickup.Status))
            {
                MessageBox.Show("Status is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!pickup.PickupDate.HasValue)
            {
                MessageBox.Show("Pickup Date is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }


            return true;
        }
    }
}
