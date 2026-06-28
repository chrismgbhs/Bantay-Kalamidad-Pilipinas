using Bantay_Kalamidad_Pilipinas.Model;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    public class admindashboard_donation_Pledges_viewModel : ObservableObject, IAdminDonationModuleViewModel
    {
        private ObservableCollection<AdminPledges> _pledges;
        private ObservableCollection<AdminPledges> _pledgeItems;
        private AdminPledges _selectedPledges;
        private AdminPledges _selectedPledgeItems;
        private string _selectedPledgesFilter;
        private string _selectedPledgeItemsFilter;
        private string _pledgesSearchText;
        private string _pledgeItemsSearchText;
        private bool _isTableReadOnly = true;
        private Visibility _actionButtonsVisibility = Visibility.Collapsed;

        // Pending pickup add state
        private ObservableCollection<DropdownItem> _availableDonations;
        private DropdownItem _selectedDonationDropdown;
        private DateTime? _pendingPickupDate;
        private string _pendingPickupStatus = "Scheduled";
        private Visibility _pickupPanelVisibility = Visibility.Collapsed;
        private int _pendingPickupIdSeq = -1;

        public ObservableCollection<AdminPledges> Pledge
        {
            get => _pledges;
            set
            {
                _pledges = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Pledges)); // XAML binds to Pledges — must notify both
            }
        }
        public ObservableCollection<AdminPledges> Pledges
        {
            get => Pledge;
            set => Pledge = value;
        }

        public ObservableCollection<AdminPledges> PledgeItems
        {
            get => _pledgeItems;
            set { _pledgeItems = value; OnPropertyChanged(); }
        }

        public AdminPledges SelectedPledges
        {
            get => _selectedPledges;
            set { _selectedPledges = value; OnPropertyChanged(); }
        }
        public AdminPledges SelectedPledge
        {
            get => SelectedPledges;
            set => SelectedPledges = value;
        }

        public AdminPledges SelectedPledgeItems
        {
            get => _selectedPledgeItems;
            set { _selectedPledgeItems = value; OnPropertyChanged(); }
        }
        public AdminPledges SelectedPledgeItem
        {
            get => SelectedPledgeItems;
            set => SelectedPledgeItems = value;
        }

        public string SelectedPledgesFilter
        {
            get => _selectedPledgesFilter;
            set { _selectedPledgesFilter = value; OnPropertyChanged(); _ = LoadPledgesAsync(); }
        }
        public string SelectedPledgeFilter
        {
            get => SelectedPledgesFilter;
            set => SelectedPledgesFilter = value;
        }

        public string PledgesSearchText
        {
            get => _pledgesSearchText;
            set { _pledgesSearchText = value; OnPropertyChanged(); _ = LoadPledgesAsync(); }
        }
        public string PledgeSearchText
        {
            get => PledgesSearchText;
            set => PledgesSearchText = value;
        }

        public string SelectedPledgeItemsFilter
        {
            get => _selectedPledgeItemsFilter;
            set { _selectedPledgeItemsFilter = value; OnPropertyChanged(); _ = LoadPledgesItemsAsync(); }
        }
        public string SelectedPledgeItemFilter
        {
            get => SelectedPledgeItemsFilter;
            set => SelectedPledgeItemsFilter = value;
        }

        public string PledgeItemsSeacrhText
        {
            get => _pledgeItemsSearchText;
            set { _pledgeItemsSearchText = value; OnPropertyChanged(); _ = LoadPledgesItemsAsync(); }
        }
        public string PledgeItemSearchText
        {
            get => PledgeItemsSeacrhText;
            set => PledgeItemsSeacrhText = value;
        }

        public bool IsTableReadOnly
        {
            get => _isTableReadOnly;
            set { _isTableReadOnly = value; OnPropertyChanged(); }
        }
        public Visibility ActionButtonsVisibility
        {
            get => _actionButtonsVisibility;
            set { _actionButtonsVisibility = value; OnPropertyChanged(); }
        }

        // Pickup panel for fulfilled pledge flow
        public ObservableCollection<DropdownItem> AvailableDonations
        {
            get => _availableDonations;
            set { _availableDonations = value; OnPropertyChanged(); }
        }
        public DropdownItem SelectedDonationDropdown
        {
            get => _selectedDonationDropdown;
            set { _selectedDonationDropdown = value; OnPropertyChanged(); }
        }
        public DateTime? PendingPickupDate
        {
            get => _pendingPickupDate;
            set { _pendingPickupDate = value; OnPropertyChanged(); }
        }
        public string PendingPickupStatus
        {
            get => _pendingPickupStatus;
            set { _pendingPickupStatus = value; OnPropertyChanged(); }
        }
        public Visibility PickupPanelVisibility
        {
            get => _pickupPanelVisibility;
            set { _pickupPanelVisibility = value; OnPropertyChanged(); }
        }

        // Only UpdateCommand is allowed — admin can only change status
        public ICommand SaveCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand FulfillCommand { get; }
        public ICommand ConfirmPickupCommand { get; }
        public ICommand CancelPickupCommand { get; }

        public admindashboard_donation_Pledges_viewModel()
        {
            Pledge = new ObservableCollection<AdminPledges>();
            PledgeItems = new ObservableCollection<AdminPledges>();
            AvailableDonations = new ObservableCollection<DropdownItem>();

            SelectedPledgesFilter = "All Pledges";
            SelectedPledgeItemsFilter = "All Items";

            // Save and Delete are blocked — admin can only update status
            SaveCommand = new RelayCommand(() =>
                MessageBox.Show("Adding pledges is not allowed. Pledges are created by donors.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information));
            DeleteCommand = new RelayCommand(() =>
                MessageBox.Show("Deleting pledges is not allowed.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information));
            UpdateCommand = new AsyncRelayCommand(UpdateStatusAsync);
            FulfillCommand = new AsyncRelayCommand(HandleFulfillAsync);
            ConfirmPickupCommand = new AsyncRelayCommand(ConfirmPickupAddAsync);
            CancelPickupCommand = new RelayCommand(() => PickupPanelVisibility = Visibility.Collapsed);

            _ = LoadPledgesAsync();
            _ = LoadPledgesItemsAsync();
        }

        // Item 8: Add/Manage not allowed
        public void EnterAddMode() =>
            MessageBox.Show("Adding pledges is not allowed. Pledges are created by donors.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information);
        public void EnterAddItemMode() =>
            MessageBox.Show("Adding pledge items is not allowed.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information);
        public void EnterManageMode() =>
            MessageBox.Show("Managing pledges directly is not allowed. Use the status buttons to update.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information);
        public void EnterViewMode()
        {
            IsTableReadOnly = true;
            ActionButtonsVisibility = Visibility.Collapsed;
        }

        private async Task LoadPledgesAsync()
        {
            try
            {
                var loaded = await DatabaseManager.GetAdminPledgesAsync(SelectedPledgesFilter, PledgesSearchText);
                Application.Current.Dispatcher.Invoke(() => Pledge = loaded);
            }
            catch (SqlException)
            {
                MessageBox.Show("Could not load pledges.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading pledges.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadPledgesItemsAsync()
        {
            try
            {
                var loaded = await DatabaseManager.GetAdminPledgeItemsAsync(SelectedPledgeItemsFilter, PledgeItemsSeacrhText);
                Application.Current.Dispatcher.Invoke(() => PledgeItems = loaded);
            }
            catch (SqlException)
            {
                MessageBox.Show("Could not load pledge items.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading pledge items.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Item 6: Only status can be edited
        private async Task UpdateStatusAsync()
        {
            if (SelectedPledges == null)
            {
                MessageBox.Show("Please select a pledge to update its status.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(SelectedPledges.PledgeStatus))
            {
                MessageBox.Show("Please set a status before updating.", "No Status", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                await DatabaseManager.UpdateAdminPledgeStatusAsync(SelectedPledges.PledgeId, SelectedPledges.PledgeStatus);
                MessageBox.Show("Pledge status updated.", "Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadPledgesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not update status.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Item 7: When status is Fulfilled, ask if received or pickup later
        private async Task HandleFulfillAsync()
        {
            if (SelectedPledges == null)
            {
                MessageBox.Show("Please select a pledge first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Has the pledged item already been received?\n\n" +
                "YES — Add directly to Donations now.\n" +
                "NO  — Schedule a pickup for later.",
                "Pledge Fulfilled",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel) return;

            // Mark pledge as Fulfilled first
            try
            {
                await DatabaseManager.UpdateAdminPledgeStatusAsync(SelectedPledges.PledgeId, "Fulfilled");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not update pledge status.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (result == MessageBoxResult.Yes)
            {
                // Add to Donations
                try
                {
                    await DatabaseManager.AddDonationFromPledgeAsync(SelectedPledges.DonorId);
                    MessageBox.Show("Donation added successfully from pledge.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadPledgesAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not add donation.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Schedule a pickup — show the pickup panel
                await LoadDonationsForDropdownAsync();
                PendingPickupDate = null;
                PendingPickupStatus = "Scheduled";
                PickupPanelVisibility = Visibility.Visible;
                await LoadPledgesAsync();
            }
        }

        private async Task LoadDonationsForDropdownAsync()
        {
            using (var conn = new SqlConnection(SQL.connectionString))
            {
                string q = "SELECT Donation_ID FROM Donation WHERE Donor_ID = @donorId ORDER BY Donation_ID DESC";
                using (var cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.Add("@donorId", SqlDbType.VarChar, 10).Value = SelectedPledges?.DonorId ?? "";
                    await conn.OpenAsync();
                    Application.Current.Dispatcher.Invoke(() => AvailableDonations.Clear());
                    using (var reader = await cmd.ExecuteReaderAsync())
                        while (await reader.ReadAsync())
                        {
                            string id = reader["Donation_ID"].ToString();
                            Application.Current.Dispatcher.Invoke(() =>
                                AvailableDonations.Add(new DropdownItem { Id = id, Display = id }));
                        }
                }
            }
        }

        private async Task ConfirmPickupAddAsync()
        {
            if (SelectedDonationDropdown == null)
            {
                MessageBox.Show("Please select a Donation ID.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!PendingPickupDate.HasValue)
            {
                MessageBox.Show("Please select a pickup date.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Generate pickup ID with dup prevention using stored procedure pattern
                if (_pendingPickupIdSeq < 0)
                {
                    string firstId = await DatabaseManager.GeneratePickupIdAsync();
                    _pendingPickupIdSeq = int.Parse(firstId.Substring(2));
                }
                else
                    _pendingPickupIdSeq++;

                string newPickupId = "PU" + _pendingPickupIdSeq.ToString("D4");

                await DatabaseManager.AddPickupScheduleAsync(
                    newPickupId,
                    SelectedDonationDropdown.Id,
                    PendingPickupDate.Value,
                    PendingPickupStatus);

                _pendingPickupIdSeq = -1; // reset after successful save
                PickupPanelVisibility = Visibility.Collapsed;
                MessageBox.Show($"Pickup scheduled with ID: {newPickupId}", "Pickup Scheduled", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not schedule pickup.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}