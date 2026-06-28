using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Bantay_Kalamidad_Pilipinas.Model;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    public class admindashboard_donation_Waste_viewModel : ObservableObject, IAdminDonationModuleViewModel
    {
        private List<AdminWaste> _wasteRecords;
        private AdminWaste _selectedWaste;
        private string _selectedWasteFilter;
        private string _wasteSearchText;

        public List<AdminWaste> WasteRecords
        {
            get => _wasteRecords;
            set { _wasteRecords = value; OnPropertyChanged(); }
        }

        public AdminWaste SelectedWaste
        {
            get => _selectedWaste;
            set { _selectedWaste = value; OnPropertyChanged(); }
        }

        public string SelectedWasteFilter
        {
            get => _selectedWasteFilter;
            set { _selectedWasteFilter = value; OnPropertyChanged(); _ = LoadAsync(); }
        }

        public string WasteSearchText
        {
            get => _wasteSearchText;
            set { _wasteSearchText = value; OnPropertyChanged(); _ = LoadAsync(); }
        }

        // Waste is read-only — no adding, no editing, only transfer back to inventory
        public ICommand SaveCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand TransferToInventoryCommand { get; }

        // ActionButtonsVisibility kept for interface compliance but not used
        public Visibility ActionButtonsVisibility { get; } = Visibility.Collapsed;

        public admindashboard_donation_Waste_viewModel()
        {
            WasteRecords = new List<AdminWaste>();

            SaveCommand = new RelayCommand(() =>
                MessageBox.Show("Adding waste records directly is not allowed. Use the Inventory screen's 'Add to Waste' button.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information));
            UpdateCommand = new RelayCommand(() =>
                MessageBox.Show("Waste records cannot be edited.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information));
            DeleteCommand = new RelayCommand(() =>
                MessageBox.Show("Waste records cannot be deleted.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information));
            TransferToInventoryCommand = new AsyncRelayCommand(TransferToInventoryAsync);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                var loaded = await DatabaseManager.GetAdminWasteAsync(SelectedWasteFilter, WasteSearchText);
                Application.Current.Dispatcher.Invoke(() => WasteRecords = loaded);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load waste records.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task TransferToInventoryAsync()
        {
            if (SelectedWaste == null || string.IsNullOrWhiteSpace(SelectedWaste.WasteId))
            {
                MessageBox.Show("Please select a waste record to transfer back to inventory.",
                    "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                $"Transfer {SelectedWaste.Quantity} unit(s) of '{SelectedWaste.InventoryItem}' back to inventory?\n\n" +
                $"This will delete the waste record and restore the quantity to available inventory.",
                "Confirm Transfer",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                await DatabaseManager.TransferWasteToInventoryAsync(SelectedWaste.WasteId);
                MessageBox.Show("Item transferred back to inventory successfully.",
                    "Transferred", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not transfer item.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // All mode changes are blocked — waste is always read-only
        public void EnterAddMode() =>
            MessageBox.Show("Adding waste records is not allowed here. Use 'Add to Waste' in the Inventory screen.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information);
        public void EnterAddItemMode() => EnterAddMode();
        public void EnterManageMode() =>
            MessageBox.Show("Waste records cannot be edited directly.", "Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information);
        public void EnterViewMode() { }
    }
}