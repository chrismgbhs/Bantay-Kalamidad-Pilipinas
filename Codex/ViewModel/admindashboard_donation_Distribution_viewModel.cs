using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Bantay_Kalamidad_Pilipinas.Model;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    public class admindashboard_donation_Distribution_viewModel : ObservableObject, IAdminDonationModuleViewModel
    {
        // ── Distribution (top grid) ──────────────────────────────────────
        private List<AdminDistribution> _distributions;
        private AdminDistribution _selectedDistribution;
        private string _selectedDistributionFilter;
        private string _distributionSearchText;

        // ── Distribution Items (bottom grid) ─────────────────────────────
        private List<AdminDistributionItem> _distributionItems;
        private AdminDistributionItem _selectedDistributionItem;
        private string _selectedDistributionItemFilter;
        private string _distributionItemSearchText;

        // ── Dropdown support for editing ─────────────────────────────────
        private ObservableCollection<DropdownItem> _availableBeneficiaries;
        private ObservableCollection<DropdownItem> _availableEvents;
        private ObservableCollection<DropdownItem> _availableLocations;
        private DropdownItem _selectedBeneficiaryDropdown;
        private DropdownItem _selectedEventDropdown;
        private DropdownItem _selectedLocationDropdown;
        private Visibility _dropdownPanelVisibility = Visibility.Collapsed;
        private Visibility _actionButtonsVisibility = Visibility.Collapsed;
        private string _beneficiaryName;
        private DateTime? _newDistributionDate;

        public string BeneficiaryName
        {
            get => _beneficiaryName;
            set { _beneficiaryName = value; OnPropertyChanged(); }
        }

        public DateTime? NewDistributionDate
        {
            get => _newDistributionDate;
            set { _newDistributionDate = value; OnPropertyChanged(); }
        }

        public List<AdminDistribution> Distributions
        {
            get => _distributions;
            set { _distributions = value; OnPropertyChanged(); }
        }
        public AdminDistribution SelectedDistribution
        {
            get => _selectedDistribution;
            set { _selectedDistribution = value; OnPropertyChanged(); }
        }
        public string SelectedDistributionFilter
        {
            get => _selectedDistributionFilter;
            set { _selectedDistributionFilter = value; OnPropertyChanged(); _ = LoadAsync(); }
        }
        public string DistributionSearchText
        {
            get => _distributionSearchText;
            set { _distributionSearchText = value; OnPropertyChanged(); _ = LoadAsync(); }
        }

        public List<AdminDistributionItem> DistributionItems
        {
            get => _distributionItems;
            set { _distributionItems = value; OnPropertyChanged(); }
        }
        public AdminDistributionItem SelectedDistributionItem
        {
            get => _selectedDistributionItem;
            set { _selectedDistributionItem = value; OnPropertyChanged(); }
        }
        public string SelectedDistributionItemFilter
        {
            get => _selectedDistributionItemFilter;
            set { _selectedDistributionItemFilter = value; OnPropertyChanged(); _ = LoadItemsAsync(); }
        }
        public string DistributionItemSearchText
        {
            get => _distributionItemSearchText;
            set { _distributionItemSearchText = value; OnPropertyChanged(); _ = LoadItemsAsync(); }
        }

        public ObservableCollection<DropdownItem> AvailableEvents
        {
            get => _availableEvents;
            set { _availableEvents = value; OnPropertyChanged(); }
        }
        public ObservableCollection<DropdownItem> AvailableLocations
        {
            get => _availableLocations;
            set { _availableLocations = value; OnPropertyChanged(); }
        }

        public DropdownItem SelectedEventDropdown
        {
            get => _selectedEventDropdown;
            set
            {
                _selectedEventDropdown = value; OnPropertyChanged();
                if (SelectedDistribution != null && value != null)
                    SelectedDistribution.Event = value.Display;
            }
        }
        public DropdownItem SelectedLocationDropdown
        {
            get => _selectedLocationDropdown;
            set
            {
                _selectedLocationDropdown = value; OnPropertyChanged();
                if (SelectedDistribution != null && value != null)
                    SelectedDistribution.DistributionLocation = value.Display;
            }
        }
        public Visibility DropdownPanelVisibility
        {
            get => _dropdownPanelVisibility;
            set { _dropdownPanelVisibility = value; OnPropertyChanged(); }
        }
        public Visibility ActionButtonsVisibility
        {
            get => _actionButtonsVisibility;
            set { _actionButtonsVisibility = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand UpdateCommand { get; } = new RelayCommand(() => { });
        public ICommand DeleteCommand { get; } = new RelayCommand(() => { });
        public ICommand AddNewLocationCommand { get; }
        public ICommand AddDistributionItemCommand { get; }

        // Inventory dropdown for adding distribution items
        private ObservableCollection<DropdownItem> _availableInventory;
        private DropdownItem _selectedInventoryDropdown;
        private string _pendingItemQuantity;

        public ObservableCollection<DropdownItem> AvailableInventory
        {
            get => _availableInventory;
            set { _availableInventory = value; OnPropertyChanged(); }
        }
        public DropdownItem SelectedInventoryDropdown
        {
            get => _selectedInventoryDropdown;
            set { _selectedInventoryDropdown = value; OnPropertyChanged(); }
        }
        public string PendingItemQuantity
        {
            get => _pendingItemQuantity;
            set { _pendingItemQuantity = value; OnPropertyChanged(); }
        }

        public admindashboard_donation_Distribution_viewModel()
        {
            Distributions = new List<AdminDistribution>();
            DistributionItems = new List<AdminDistributionItem>();
            AvailableEvents = new ObservableCollection<DropdownItem>();
            AvailableLocations = new ObservableCollection<DropdownItem>();
            AvailableInventory = new ObservableCollection<DropdownItem>();
            AddNewLocationCommand = new AsyncRelayCommand(AddNewLocationAsync);
            AddDistributionItemCommand = new AsyncRelayCommand(AddDistributionItemAsync);
            SaveCommand = new AsyncRelayCommand(SaveDistributionAsync);
            _ = LoadAsync();
            _ = LoadItemsAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                var loaded = await DatabaseManager.GetAdminDistributionAsync(
                    SelectedDistributionFilter, DistributionSearchText);
                Application.Current.Dispatcher.Invoke(() => Distributions = loaded);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load distributions.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadItemsAsync()
        {
            try
            {
                var loaded = await DatabaseManager.GetAdminDistributionItemsAsync(
                    SelectedDistributionItemFilter, DistributionItemSearchText);
                Application.Current.Dispatcher.Invoke(() => DistributionItems = loaded);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load distribution items.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDropdownsAsync()
        {
            var events = await DatabaseManager.GetAvailableDisasterEventsAsync();
            var locations = await DatabaseManager.GetAvailableEvacuationCentersAsync();

            // Load available inventory items
            var inventoryList = await DatabaseManager.GetAdminInventoryAsync("Available", null);

            Application.Current.Dispatcher.Invoke(() =>
            {
                AvailableEvents.Clear();
                foreach (var e in events)
                    AvailableEvents.Add(new DropdownItem { Id = e.Id, Display = e.Name });

                AvailableLocations.Clear();
                foreach (var l in locations)
                    AvailableLocations.Add(new DropdownItem { Id = l.Id, Display = l.Name });

                AvailableInventory.Clear();
                foreach (var inv in inventoryList)
                    AvailableInventory.Add(new DropdownItem
                    {
                        Id = inv.InventoryId,
                        Display = $"{inv.Item} (Available: {inv.QuantityAvailable})"
                    });
            });
        }

        private async Task SaveDistributionAsync()
        {
            if (string.IsNullOrWhiteSpace(BeneficiaryName))
            {
                MessageBox.Show("Please enter a Beneficiary name.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedEventDropdown == null)
            {
                MessageBox.Show("Please select a Disaster Event.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedLocationDropdown == null)
            {
                MessageBox.Show("Please select a Distribution Location.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Use usp_AddDistribution — it auto-generates DS#### ID
                await DatabaseManager.AddDistributionViaSpAsync(
                    BeneficiaryName,
                    SelectedEventDropdown.Id,
                    SelectedLocationDropdown.Id,
                    NewDistributionDate);

                await LoadAsync();

                // Auto-select the newest distribution so admin can immediately add items to it
                if (Distributions.Count > 0)
                    SelectedDistribution = Distributions[Distributions.Count - 1];

                BeneficiaryName = string.Empty;
                NewDistributionDate = null;
                SelectedEventDropdown = null;
                SelectedLocationDropdown = null;
                MessageBox.Show("Distribution added. You can now add items to it using the panel below.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not save distribution.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddDistributionItemAsync()
        {
            if (SelectedDistribution == null)
            {
                MessageBox.Show("Please select a Distribution first.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedInventoryDropdown == null)
            {
                MessageBox.Show("Please select an Inventory Item.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(PendingItemQuantity, out int qty) || qty <= 0)
            {
                MessageBox.Show("Please enter a valid quantity.", "Invalid Quantity", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int remaining = await DatabaseManager.AddDistributionItemAsync(
                    SelectedDistribution.DistributionId,
                    SelectedInventoryDropdown.Id,
                    qty);

                MessageBox.Show($"Item added. Remaining inventory: {remaining}", "Added", MessageBoxButton.OK, MessageBoxImage.Information);
                PendingItemQuantity = string.Empty;
                SelectedInventoryDropdown = null;
                await LoadItemsAsync();
                await LoadDropdownsAsync(); // refresh inventory quantities
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not add distribution item.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddNewLocationAsync()
        {
            // Re-use the same simple dialog pattern as Operations VM
            string name = ShowInputDialog("Enter Evacuation Center Name:", "New Location");
            if (string.IsNullOrWhiteSpace(name)) return;

            // Note: Evacuation Center links to Location table; for now just add to the dropdown as a display item
            // Full implementation requires adding to Evacuation Center table which needs a Location row first.
            // This is a placeholder that adds the location to the DB and then adds to the center dropdown.
            string barangay = ShowInputDialog("Enter Barangay:", "New Location");
            string city = ShowInputDialog("Enter City:", "New Location");
            string province = ShowInputDialog("Enter Province:", "New Location");

            try
            {
                string locationId = await DatabaseManager.AddLocationAsync(barangay, city, province);
                var newItem = new DropdownItem { Id = locationId, Display = name };
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AvailableLocations.Add(newItem);
                    SelectedLocationDropdown = newItem;
                });
                MessageBox.Show($"Location added.", "Location Added", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not add location.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            var buttons = new System.Windows.Controls.StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, HorizontalAlignment = System.Windows.HorizontalAlignment.Right };
            string result = string.Empty;
            var ok = new System.Windows.Controls.Button { Content = "OK", Width = 72, Margin = new System.Windows.Thickness(0, 0, 8, 0), IsDefault = true };
            ok.Click += (s, e) => { result = input.Text; dialog.Close(); };
            var cancel = new System.Windows.Controls.Button { Content = "Cancel", Width = 72, IsCancel = true };
            cancel.Click += (s, e) => { result = string.Empty; dialog.Close(); };
            buttons.Children.Add(ok); buttons.Children.Add(cancel);
            panel.Children.Add(buttons);
            dialog.Content = panel;
            dialog.ShowDialog();
            return result;
        }

        public void EnterAddMode()
        {
            ActionButtonsVisibility = Visibility.Visible;
            DropdownPanelVisibility = Visibility.Visible;
            _ = LoadDropdownsAsync();
        }
        public void EnterAddItemMode() { ActionButtonsVisibility = Visibility.Visible; }
        public void EnterManageMode()
        {
            ActionButtonsVisibility = Visibility.Visible;
            DropdownPanelVisibility = Visibility.Visible;
            _ = LoadDropdownsAsync();
        }
        public void EnterViewMode()
        {
            ActionButtonsVisibility = Visibility.Collapsed;
            DropdownPanelVisibility = Visibility.Collapsed;
        }
    }
}