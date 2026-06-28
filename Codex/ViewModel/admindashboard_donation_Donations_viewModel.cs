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
        private ObservableCollection<AdminDonations> _donatedItems;
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

        // Pending ID counters to prevent duplicates when adding multiple rows
        private int _pendingDonationIdSeq = -1;
        private int _pendingDonatedItemIdSeq = -1;

        // Dropdown sources for add mode
        private ObservableCollection<DropdownItem> _availableDonors;
        private ObservableCollection<DropdownItem> _availableEvents;
        private DropdownItem _selectedDonorDropdown;
        private DropdownItem _selectedEventDropdown;
        private Visibility _dropdownPanelVisibility = Visibility.Collapsed;

        public ObservableCollection<DropdownItem> AvailableDonors
        {
            get => _availableDonors;
            set { _availableDonors = value; OnPropertyChanged(); }
        }
        public ObservableCollection<DropdownItem> AvailableEvents
        {
            get => _availableEvents;
            set { _availableEvents = value; OnPropertyChanged(); }
        }
        public DropdownItem SelectedDonorDropdown
        {
            get => _selectedDonorDropdown;
            set
            {
                _selectedDonorDropdown = value; OnPropertyChanged();
                if (SelectedDonations != null && value != null)
                {
                    SelectedDonations.DonorId = value.Id;
                    SelectedDonations.Donor = value.Display;
                }
            }
        }
        public DropdownItem SelectedEventDropdown
        {
            get => _selectedEventDropdown;
            set
            {
                _selectedEventDropdown = value; OnPropertyChanged();
                if (SelectedDonations != null && value != null)
                {
                    SelectedDonations.EventId = value.Id;
                    SelectedDonations.Event = value.Display;
                }
            }
        }
        public Visibility DropdownPanelVisibility
        {
            get => _dropdownPanelVisibility;
            set { _dropdownPanelVisibility = value; OnPropertyChanged(); }
        }

        // ---------------------------------------------------------------
        // XAML alias properties — the XAML binds to these names, which
        // differ from the original backing-property names. These delegate
        // to the existing properties so no existing logic changes.
        // ---------------------------------------------------------------

        // First grid (Donations) aliases
        public ObservableCollection<AdminDonations> DonatedItems
        {
            get => _donatedItems;
            set { _donatedItems = value; OnPropertyChanged(); }
        }

        public AdminDonations SelectedDonation
        {
            get => SelectedDonations;
            set => SelectedDonations = value;
        }

        public string SelectedDonationFilter
        {
            get => SelectedDonationsFilter;
            set => SelectedDonationsFilter = value;
        }

        public string DonationSearchText
        {
            get => DonationsSearchText;
            set => DonationsSearchText = value;
        }

        // Second grid (Donated Items) aliases
        public AdminDonations SelectedDonatedItem
        {
            get => SelectedDonatedItems;
            set => SelectedDonatedItems = value;
        }

        public string SelectedDonatedItemFilter
        {
            get => SelectedDonatedItemsFilter;
            set => SelectedDonatedItemsFilter = value;
        }

        public string DonatedItemSearchText
        {
            get => DonatedItemsSeacrhText;
            set => DonatedItemsSeacrhText = value;
        }

        public ICommand SaveCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddToDistributionCommand { get; }

        public admindashboard_donation_Donations_viewModel()
        {
            Donations = new ObservableCollection<AdminDonations>();
            DonatedItems = new ObservableCollection<AdminDonations>();
            AvailableDonors = new ObservableCollection<DropdownItem>();
            AvailableEvents = new ObservableCollection<DropdownItem>();
            SelectedDonationsFilter = "All Donations";

            // Items 2 & 4: admin cannot add or edit donations — values come from donors
            SaveCommand = new RelayCommand(() =>
                MessageBox.Show("Adding donations is not allowed. Donations are submitted by donors.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information));
            UpdateCommand = new RelayCommand(() =>
                MessageBox.Show("Editing donations is not allowed. Donation data comes from donors.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information));
            DeleteCommand = new RelayCommand(() =>
                MessageBox.Show("Deleting donations is not allowed.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information));

            // Item 3: distribute selected donation
            AddToDistributionCommand = new AsyncRelayCommand(AddToDistributionAsync);

            _ = LoadDonationsAsync();
            _ = LoadDonatedItemsAsync();
        }

        // Item 2: block add
        public void EnterAddMode() =>
            MessageBox.Show("Adding donations is not allowed. Donations are submitted by donors.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information);

        public void EnterAddItemMode() =>
            MessageBox.Show("Donated items cannot be added by admin — they come from donor submissions.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information);

        // Item 4: block manage
        public void EnterManageMode() =>
            MessageBox.Show("Editing donations is not allowed. Donation data comes from donors.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information);

        public void EnterViewMode()
        {
            IsTableReadOnly = true;
            ActionButtonsVisibility = Visibility.Collapsed;
            DropdownPanelVisibility = Visibility.Collapsed;
        }

        // Item 3: add selected donation to a distribution
        private async Task AddToDistributionAsync()
        {
            if (SelectedDonations == null || string.IsNullOrWhiteSpace(SelectedDonations.DonationId))
            {
                MessageBox.Show("Please select a donation first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Load available distributions for selection
            List<(string Id, string Display)> distributions;
            try
            {
                distributions = await DatabaseManager.GetAvailableDistributionsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load distributions.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (distributions.Count == 0)
            {
                MessageBox.Show("No distributions exist yet. Please create a distribution first.", "No Distributions", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Show a selection dialog with a ComboBox
            string selectedDistId = ShowDistributionPickerDialog(distributions);
            if (string.IsNullOrWhiteSpace(selectedDistId)) return;

            try
            {
                await DatabaseManager.LinkDonationToDistributionAsync(SelectedDonations.DonationId, selectedDistId);
                MessageBox.Show($"Donation {SelectedDonations.DonationId} linked to Distribution {selectedDistId}.",
                    "Linked", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not link donation to distribution.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string ShowDistributionPickerDialog(List<(string Id, string Display)> distributions)
        {
            var dialog = new System.Windows.Window
            {
                Title = "Select Distribution",
                Width = 400,
                Height = 160,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                ResizeMode = System.Windows.ResizeMode.NoResize
            };
            var panel = new System.Windows.Controls.StackPanel { Margin = new System.Windows.Thickness(12) };
            panel.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = "Select a Distribution to link this donation to:",
                Margin = new System.Windows.Thickness(0, 0, 0, 6)
            });

            var combo = new System.Windows.Controls.ComboBox { Margin = new System.Windows.Thickness(0, 0, 0, 10) };
            foreach (var (id, display) in distributions)
                combo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = display, Tag = id });
            combo.SelectedIndex = 0;
            panel.Children.Add(combo);

            var btns = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right
            };

            string result = string.Empty;
            var ok = new System.Windows.Controls.Button { Content = "OK", Width = 72, Margin = new System.Windows.Thickness(0, 0, 8, 0), IsDefault = true };
            ok.Click += (s, e) =>
            {
                var item = combo.SelectedItem as System.Windows.Controls.ComboBoxItem;
                result = item?.Tag?.ToString() ?? string.Empty;
                dialog.Close();
            };
            var cancel = new System.Windows.Controls.Button { Content = "Cancel", Width = 72, IsCancel = true };
            cancel.Click += (s, e) => { result = string.Empty; dialog.Close(); };
            btns.Children.Add(ok);
            btns.Children.Add(cancel);
            panel.Children.Add(btns);
            dialog.Content = panel;
            dialog.ShowDialog();
            return result;
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

        private async Task LoadDonationDropdownsAsync()
        {
            var donors = await DatabaseManager.GetAvailableDonorsAsync();
            var events = await DatabaseManager.GetAvailableDisasterEventsAsync();
            Application.Current.Dispatcher.Invoke(() =>
            {
                AvailableDonors.Clear();
                foreach (var d in donors)
                    AvailableDonors.Add(new DropdownItem { Id = d.Id, Display = d.Name });
                AvailableEvents.Clear();
                foreach (var e in events)
                    AvailableEvents.Add(new DropdownItem { Id = e.Id, Display = e.Name });
            });
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
                    _pendingDonationIdSeq = -1;
                    _pendingDonatedItemIdSeq = -1;
                    DropdownPanelVisibility = Visibility.Collapsed;
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
                    DonatedItems = loadedDonatedItems;
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