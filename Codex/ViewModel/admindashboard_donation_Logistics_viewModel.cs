using Bantay_Kalamidad_Pilipinas.Model;
using System.Data.SqlClient;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    public class admindashboard_donation_Logistics_viewModel : ObservableObject, IAdminDonationModuleViewModel
    {
        // Separate backing collections — NOT the same object
        private ObservableCollection<AdminLogistics> _deliveries;
        private ObservableCollection<AdminLogistics> _pickups;
        private AdminLogistics _selectedDelivery;
        private AdminLogistics _selectedPickup;
        private string _selectedDeliveryFilter;
        private string _selectedPickupFilter;
        private string _deliverySearchText;
        private string _pickupSearchText;
        private bool _isTableReadOnly = true;
        private Visibility _actionButtonsVisibility = Visibility.Collapsed;

        // Dropdown support for adding deliveries
        private ObservableCollection<DropdownItem> _availableDistributions;
        private DropdownItem _selectedDistributionDropdown;
        // Dropdown support for adding pickups
        private ObservableCollection<DropdownItem> _availableDonations;
        private DropdownItem _selectedDonationDropdown;
        private Visibility _dropdownPanelVisibility = Visibility.Collapsed;
        private int _pendingPickupIdSeq = -1;
        private int _pendingDeliveryIdSeq = -1;

        // ── Deliveries ─────────────────────────────────────────────────
        public ObservableCollection<AdminLogistics> Deliveries
        {
            get => _deliveries;
            set { _deliveries = value; OnPropertyChanged(); }
        }
        public AdminLogistics SelectedDelivery
        {
            get => _selectedDelivery;
            set { _selectedDelivery = value; OnPropertyChanged(); }
        }
        // Alias for backward-compat with old VM bindings
        public AdminLogistics SelectedDeliveries { get => SelectedDelivery; set => SelectedDelivery = value; }

        public string SelectedDeliveryFilter
        {
            get => _selectedDeliveryFilter;
            set { _selectedDeliveryFilter = value; OnPropertyChanged(); _ = LoadDeliveriesAsync(); }
        }
        public string SelectedDeliveriesFilter { get => SelectedDeliveryFilter; set => SelectedDeliveryFilter = value; }

        public string DeliverySearchText
        {
            get => _deliverySearchText;
            set { _deliverySearchText = value; OnPropertyChanged(); _ = LoadDeliveriesAsync(); }
        }
        public string DeliveriesSearchText { get => DeliverySearchText; set => DeliverySearchText = value; }

        // ── Pickups ────────────────────────────────────────────────────
        public ObservableCollection<AdminLogistics> Pickups
        {
            get => _pickups;
            set { _pickups = value; OnPropertyChanged(); }
        }
        public AdminLogistics SelectedPickup
        {
            get => _selectedPickup;
            set { _selectedPickup = value; OnPropertyChanged(); }
        }
        public AdminLogistics SelectedPickups { get => SelectedPickup; set => SelectedPickup = value; }

        public string SelectedPickupFilter
        {
            get => _selectedPickupFilter;
            set { _selectedPickupFilter = value; OnPropertyChanged(); _ = LoadPickupsAsync(); }
        }
        public string SelectedPickupsFilter { get => SelectedPickupFilter; set => SelectedPickupFilter = value; }

        public string PickupSearchText
        {
            get => _pickupSearchText;
            set { _pickupSearchText = value; OnPropertyChanged(); _ = LoadPickupsAsync(); }
        }
        public string PickupsSeacrhText { get => PickupSearchText; set => PickupSearchText = value; }

        // ── UI state ───────────────────────────────────────────────────
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

        // ── Add-mode dropdowns ─────────────────────────────────────────
        public ObservableCollection<DropdownItem> AvailableDistributions
        {
            get => _availableDistributions;
            set { _availableDistributions = value; OnPropertyChanged(); }
        }
        public DropdownItem SelectedDistributionDropdown
        {
            get => _selectedDistributionDropdown;
            set
            {
                _selectedDistributionDropdown = value; OnPropertyChanged();
                if (SelectedDelivery != null && value != null)
                    SelectedDelivery.Distribution = value.Id;
            }
        }
        public ObservableCollection<DropdownItem> AvailableDonations
        {
            get => _availableDonations;
            set { _availableDonations = value; OnPropertyChanged(); }
        }
        public DropdownItem SelectedDonationDropdown
        {
            get => _selectedDonationDropdown;
            set
            {
                _selectedDonationDropdown = value; OnPropertyChanged();
                if (SelectedPickup != null && value != null)
                    SelectedPickup.Donation = value.Id;
            }
        }
        public Visibility DropdownPanelVisibility
        {
            get => _dropdownPanelVisibility;
            set { _dropdownPanelVisibility = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        public admindashboard_donation_Logistics_viewModel()
        {
            Deliveries = new ObservableCollection<AdminLogistics>();
            Pickups = new ObservableCollection<AdminLogistics>();
            AvailableDistributions = new ObservableCollection<DropdownItem>();
            AvailableDonations = new ObservableCollection<DropdownItem>();

            SelectedDeliveryFilter = "All Deliveries";
            SelectedPickupFilter = "All Pickups";

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            UpdateCommand = new AsyncRelayCommand(UpdateAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);

            // Load both independently — no shared collection racing
            _ = LoadDeliveriesAsync();
            _ = LoadPickupsAsync();
        }

        private async Task LoadDeliveriesAsync()
        {
            try
            {
                var loaded = await DatabaseManager.GetAdminDeliveriesAsync(
                    SelectedDeliveryFilter, DeliverySearchText);
                Application.Current.Dispatcher.Invoke(() => { Deliveries = loaded; _pendingDeliveryIdSeq = -1; });
            }
            catch (SqlException)
            {
                MessageBox.Show("Could not load deliveries from the database.",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred while loading deliveries.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadPickupsAsync()
        {
            try
            {
                var loaded = await DatabaseManager.GetAdminPickupAsync(
                    SelectedPickupFilter, PickupSearchText);
                Application.Current.Dispatcher.Invoke(() => Pickups = loaded);
            }
            catch (SqlException)
            {
                MessageBox.Show("Could not load pickups from the database.",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred while loading pickups.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDropdownsAsync()
        {
            // Distributions for delivery dropdown
            using (var conn = new System.Data.SqlClient.SqlConnection(SQL.connectionString))
            {
                string q = "SELECT Distribution_ID FROM Distribution ORDER BY Distribution_ID";
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        Application.Current.Dispatcher.Invoke(() => AvailableDistributions.Clear());
                        while (await reader.ReadAsync())
                        {
                            string id = reader["Distribution_ID"].ToString();
                            Application.Current.Dispatcher.Invoke(() =>
                                AvailableDistributions.Add(new DropdownItem { Id = id, Display = id }));
                        }
                    }
                }
            }
            // Donations for pickup dropdown
            using (var conn = new System.Data.SqlClient.SqlConnection(SQL.connectionString))
            {
                string q = "SELECT Donation_ID FROM Donation ORDER BY Donation_ID";
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        Application.Current.Dispatcher.Invoke(() => AvailableDonations.Clear());
                        while (await reader.ReadAsync())
                        {
                            string id = reader["Donation_ID"].ToString();
                            Application.Current.Dispatcher.Invoke(() =>
                                AvailableDonations.Add(new DropdownItem { Id = id, Display = id }));
                        }
                    }
                }
            }
        }

        public void EnterAddMode()
        {
            IsTableReadOnly = false;
            ActionButtonsVisibility = Visibility.Visible;

            var result = MessageBox.Show(
                "What would you like to add?\n\nYES — Add a Delivery\nNO  — Add a Pickup",
                "Select Type",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DropdownPanelVisibility = Visibility.Visible;
                _ = PrepareDeliveryAddAsync();
            }
            else if (result == MessageBoxResult.No)
            {
                DropdownPanelVisibility = Visibility.Visible;
                _ = PreparePickupAddAsync();
            }
            else
            {
                // Cancel — revert state
                IsTableReadOnly = true;
                ActionButtonsVisibility = Visibility.Collapsed;
            }
        }

        private async Task PrepareDeliveryAddAsync()
        {
            await LoadDropdownsAsync();

            if (_pendingDeliveryIdSeq < 0)
            {
                string firstId = await GenerateDeliveryIdAsync();
                _pendingDeliveryIdSeq = int.Parse(firstId.Substring(2)); // skip "DL"
            }
            else
                _pendingDeliveryIdSeq++;

            string newId = "DL" + _pendingDeliveryIdSeq.ToString("D4");
            var row = new AdminLogistics
            {
                DeliveryId = newId,
                Distribution = string.Empty,
                DeliveryDate = null,
                Status = "Pending",
                IsNew = true
            };
            Application.Current.Dispatcher.Invoke(() =>
            {
                Deliveries.Add(row);
                SelectedDelivery = row;
                SelectedDistributionDropdown = null;
            });
        }

        public void EnterAddItemMode()
        {
            IsTableReadOnly = false;
            ActionButtonsVisibility = Visibility.Visible;
            DropdownPanelVisibility = Visibility.Visible;
            _ = PreparePickupAddAsync();
        }

        private async Task PreparePickupAddAsync()
        {
            await LoadDropdownsAsync();
            string newId = await GeneratePickupIdAsync();
            var row = new AdminLogistics
            {
                PickupId = newId,
                Donation = string.Empty,
                PickupDate = null,
                Status = "Scheduled",
                IsNew = true
            };
            Application.Current.Dispatcher.Invoke(() =>
            {
                Pickups.Add(row);
                SelectedPickup = row;
                SelectedDonationDropdown = null;
            });
        }

        private static async Task<string> GenerateDeliveryIdAsync()
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(SQL.connectionString))
            {
                string q = @"SELECT ISNULL(MAX(CAST(SUBSTRING(Delivery_ID, 3, LEN(Delivery_ID)) AS INT)), 0) + 1
                             FROM [Delivery Schedule] WHERE Delivery_ID LIKE 'DV[0-9]%'";
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    int next = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return "DV" + next.ToString("D4");
                }
            }
        }

        private static async Task<string> GeneratePickupIdAsync()
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(SQL.connectionString))
            {
                string q = @"SELECT ISNULL(MAX(CAST(SUBSTRING(Pickup_ID, 3, LEN(Pickup_ID)) AS INT)), 0) + 1
                             FROM [Pickup Schedule] WHERE Pickup_ID LIKE 'PU[0-9]%'";
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, conn))
                {
                    await conn.OpenAsync();
                    int next = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return "PU" + next.ToString("D4");
                }
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
            DropdownPanelVisibility = Visibility.Collapsed;
        }

        private async Task SaveAsync()
        {
            bool saveDelivery = SelectedDelivery != null && SelectedDelivery.IsNew;
            bool savePickup = SelectedPickup != null && SelectedPickup.IsNew;

            if (!saveDelivery && !savePickup)
            {
                MessageBox.Show("Please select the new row to save.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (saveDelivery)
                {
                    if (SelectedDistributionDropdown != null)
                        SelectedDelivery.Distribution = SelectedDistributionDropdown.Id;

                    if (string.IsNullOrWhiteSpace(SelectedDelivery.Distribution))
                    {
                        MessageBox.Show("Please select a Distribution.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (!SelectedDelivery.DeliveryDate.HasValue)
                    {
                        MessageBox.Show("Please select a Delivery Date.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Use usp_AddDeliverySchedule — it auto-generates the DL#### ID
                    await DatabaseManager.AddDeliveryViaSpAsync(
                        SelectedDelivery.Distribution,
                        SelectedDelivery.DeliveryDate.Value,
                        SelectedDelivery.Status ?? "Pending");

                    _pendingDeliveryIdSeq = -1;
                    MessageBox.Show("Delivery saved.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadDeliveriesAsync();
                }

                if (savePickup)
                {
                    if (SelectedDonationDropdown != null)
                        SelectedPickup.Donation = SelectedDonationDropdown.Id;

                    if (string.IsNullOrWhiteSpace(SelectedPickup.Donation))
                    {
                        MessageBox.Show("Please select a Donation.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (!SelectedPickup.PickupDate.HasValue)
                    {
                        MessageBox.Show("Please select a Pickup Date.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Generate PU#### ID with dup prevention
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
                        SelectedPickup.Donation,
                        SelectedPickup.PickupDate.Value,
                        SelectedPickup.Status ?? "Scheduled");

                    // Item 11: if status is Completed, prompt to add to inventory
                    if (SelectedPickup.Status == "Completed")
                        await PromptAddPickupToInventoryAsync(newPickupId);

                    _pendingPickupIdSeq = -1;
                    MessageBox.Show("Pickup saved.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadPickupsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not save.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static async Task PromptAddPickupToInventoryAsync(string pickupId)
        {
            var result = MessageBox.Show(
                "This pickup is marked Completed. Add the received items to Inventory now?",
                "Add to Inventory",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            string itemName = ShowInputDialog("Enter Item Name:", "Add to Inventory");
            if (string.IsNullOrWhiteSpace(itemName)) return;

            string qtyStr = ShowInputDialog("Enter Quantity:", "Add to Inventory");
            if (!int.TryParse(qtyStr, out int qty) || qty <= 0) { MessageBox.Show("Invalid quantity.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            string unit = ShowInputDialog("Enter Unit (e.g. kg, pcs, boxes):", "Add to Inventory");
            string category = ShowInputDialog("Enter Category (e.g. Food, Clothing, Medical):", "Add to Inventory");

            try
            {
                await DatabaseManager.AddPickupToInventoryAsync(pickupId, itemName, qty, unit ?? "pcs", category ?? "General");
                MessageBox.Show("Items added to inventory.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not add to inventory.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string ShowInputDialog(string prompt, string title)
        {
            var dialog = new System.Windows.Window
            {
                Title = title,
                Width = 360,
                Height = 140,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                ResizeMode = System.Windows.ResizeMode.NoResize
            };
            var panel = new System.Windows.Controls.StackPanel { Margin = new System.Windows.Thickness(12) };
            panel.Children.Add(new System.Windows.Controls.TextBlock { Text = prompt, Margin = new System.Windows.Thickness(0, 0, 0, 6) });
            var input = new System.Windows.Controls.TextBox { Margin = new System.Windows.Thickness(0, 0, 0, 10) };
            panel.Children.Add(input);
            var btns = new System.Windows.Controls.StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, HorizontalAlignment = System.Windows.HorizontalAlignment.Right };
            string result = string.Empty;
            var ok = new System.Windows.Controls.Button { Content = "OK", Width = 72, Margin = new System.Windows.Thickness(0, 0, 8, 0), IsDefault = true };
            ok.Click += (s, e) => { result = input.Text; dialog.Close(); };
            var cancel = new System.Windows.Controls.Button { Content = "Cancel", Width = 72, IsCancel = true };
            cancel.Click += (s, e) => { result = string.Empty; dialog.Close(); };
            btns.Children.Add(ok); btns.Children.Add(cancel);
            panel.Children.Add(btns);
            dialog.Content = panel;
            dialog.ShowDialog();
            return result;
        }

        private async Task UpdateAsync()
        {
            try
            {
                if (SelectedDelivery != null && !SelectedDelivery.IsNew)
                {
                    await DatabaseManager.UpdateAdminDeliveriesAsync(SelectedDelivery);
                    await LoadDeliveriesAsync();
                }
                else if (SelectedPickup != null && !SelectedPickup.IsNew)
                {
                    await DatabaseManager.UpdateAdminPickupsAsync(SelectedPickup);
                    await LoadPickupsAsync();
                }
                else
                    MessageBox.Show("Please select an existing row to update.",
                        "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not update.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteAsync()
        {
            try
            {
                if (SelectedDelivery != null)
                {
                    if (SelectedDelivery.IsNew) { Deliveries.Remove(SelectedDelivery); return; }
                    if (MessageBox.Show("Delete this delivery?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
                    await DatabaseManager.DeleteAdminDeliveriesAsync(SelectedDelivery.DeliveryId);
                    await LoadDeliveriesAsync();
                }
                else if (SelectedPickup != null)
                {
                    if (SelectedPickup.IsNew) { Pickups.Remove(SelectedPickup); return; }
                    if (MessageBox.Show("Delete this pickup?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
                    await DatabaseManager.DeleteAdminPickupsAsync(SelectedPickup.PickupId);
                    await LoadPickupsAsync();
                }
                else
                    MessageBox.Show("Please select a row to delete.",
                        "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not delete.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}