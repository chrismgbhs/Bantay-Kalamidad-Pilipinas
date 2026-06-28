using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Bantay_Kalamidad_Pilipinas.Model;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    public class admindashboard_donation_Inventory_viewModel : ObservableObject, IAdminDonationModuleViewModel
    {
        private List<AdminInventory> _inventoryRecords;
        private AdminInventory _selectedInventoryRecord;
        private string _selectedInventoryFilter;
        private string _inventorySearchText;
        private Visibility _actionButtonsVisibility = Visibility.Collapsed;

        public List<AdminInventory> InventoryRecords
        {
            get => _inventoryRecords;
            set { _inventoryRecords = value; OnPropertyChanged(); }
        }

        public AdminInventory SelectedInventoryRecord
        {
            get => _selectedInventoryRecord;
            set { _selectedInventoryRecord = value; OnPropertyChanged(); }
        }

        public string SelectedInventoryFilter
        {
            get => _selectedInventoryFilter;
            set { _selectedInventoryFilter = value; OnPropertyChanged(); _ = LoadAsync(); }
        }

        public string InventorySearchText
        {
            get => _inventorySearchText;
            set { _inventorySearchText = value; OnPropertyChanged(); _ = LoadAsync(); }
        }

        public Visibility ActionButtonsVisibility
        {
            get => _actionButtonsVisibility;
            set { _actionButtonsVisibility = value; OnPropertyChanged(); }
        }

        public ICommand AddToWasteCommand { get; }
        public ICommand SaveCommand { get; } = new RelayCommand(() => { });
        public ICommand UpdateCommand { get; } = new RelayCommand(() => { });
        public ICommand DeleteCommand { get; } = new RelayCommand(() => { });

        public admindashboard_donation_Inventory_viewModel()
        {
            InventoryRecords = new List<AdminInventory>();
            AddToWasteCommand = new AsyncRelayCommand(AddToWasteAsync);
            _ = AutoMoveExpiredAndLoadAsync();
        }

        private async Task AutoMoveExpiredAndLoadAsync()
        {
            try
            {
                int moved = await DatabaseManager.MoveExpiredToWasteAsync();
                if (moved > 0)
                    MessageBox.Show(
                        $"{moved} expired item(s) have been automatically moved to Waste Tracking.",
                        "Expired Items Moved",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
            }
            catch { /* non-critical; just load the inventory anyway */ }
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                var loaded = await DatabaseManager.GetAdminInventoryAsync(
                    SelectedInventoryFilter, InventorySearchText);
                Application.Current.Dispatcher.Invoke(() => InventoryRecords = loaded);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load inventory.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddToWasteAsync()
        {
            if (SelectedInventoryRecord == null)
            {
                MessageBox.Show("Please select an inventory item to add to waste.",
                    "No Item Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(SelectedInventoryRecord.QuantityAvailable, out int available) || available <= 0)
            {
                MessageBox.Show("This item has no available quantity to add to waste.",
                    "Zero Quantity", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string qtyStr = ShowInputDialog("Enter quantity to add to waste:", "Add to Waste");
            if (!int.TryParse(qtyStr, out int qty) || qty <= 0)
            {
                MessageBox.Show("Please enter a valid positive quantity.", "Invalid Quantity",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (qty > available)
            {
                MessageBox.Show($"Quantity exceeds available stock ({available}).", "Too Much",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string reason = ShowInputDialog("Enter reason for waste (e.g. Expired, Damaged):", "Add to Waste");
            if (string.IsNullOrWhiteSpace(reason)) reason = "Manually disposed";

            try
            {
                await DatabaseManager.AddToWasteAsync(SelectedInventoryRecord.InventoryId, qty, reason);
                MessageBox.Show($"{qty} unit(s) added to waste successfully.",
                    "Waste Added", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not add to waste.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        public void EnterAddMode() => MessageBox.Show("Inventory is automatically managed from donations. Use 'Add to Waste' to remove expired items.", "Read-Only", MessageBoxButton.OK, MessageBoxImage.Information);
        public void EnterAddItemMode() { }
        public void EnterManageMode() => MessageBox.Show("Inventory is automatically managed from donations.", "Read-Only", MessageBoxButton.OK, MessageBoxImage.Information);
        public void EnterViewMode()
        {
            ActionButtonsVisibility = Visibility.Collapsed;
            _ = LoadAsync(); // always refresh when switching to Inventory view
        }
    }
}