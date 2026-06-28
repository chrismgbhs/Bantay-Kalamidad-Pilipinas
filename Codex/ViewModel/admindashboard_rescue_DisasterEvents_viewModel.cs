using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bantay_Kalamidad_Pilipinas.Model;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    public class admindashboard_rescue_DisasterEvents_viewModel : ObservableObject, IAdminRescueModuleViewModel
    {
        private ObservableCollection<AdminDisasterEvent> _disasterEvents;
        private AdminDisasterEvent _selectedDisasterEvent;
        private string _selectedDisasterEventFilter;
        private string _disasterEventSearchText;
        private bool _isTableReadOnly = true;
        private Visibility _actionButtonsVisibility = Visibility.Collapsed;
        private int _pendingIdSequence = -1;

        public ObservableCollection<AdminDisasterEvent> DisasterEvents
        {
            get => _disasterEvents;
            set
            {
                if (_disasterEvents != value)
                {
                    _disasterEvents = value;
                    OnPropertyChanged();
                }
            }
        }

        public AdminDisasterEvent SelectedDisasterEvent
        {
            get => _selectedDisasterEvent;
            set
            {
                if (_selectedDisasterEvent != value)
                {
                    _selectedDisasterEvent = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedDisasterEventFilter
        {
            get => _selectedDisasterEventFilter;
            set
            {
                if (_selectedDisasterEventFilter != value)
                {
                    _selectedDisasterEventFilter = value;
                    OnPropertyChanged();
                    _ = LoadDisasterEventsAsync();
                }
            }
        }

        public string DisasterEventSearchText
        {
            get => _disasterEventSearchText;
            set
            {
                if (_disasterEventSearchText != value)
                {
                    _disasterEventSearchText = value;
                    OnPropertyChanged();
                    _ = LoadDisasterEventsAsync();
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

        public admindashboard_rescue_DisasterEvents_viewModel()
        {
            DisasterEvents = new ObservableCollection<AdminDisasterEvent>();
            SelectedDisasterEventFilter = "All Events";

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            UpdateCommand = new AsyncRelayCommand(UpdateAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync);

            _ = LoadDisasterEventsAsync();
        }

        public void EnterAddMode()
        {
            IsTableReadOnly = false;
            ActionButtonsVisibility = Visibility.Visible;

            _ = AddNewEventRowAsync();
        }

        private async Task AddNewEventRowAsync()
        {
            try
            {
                if (_pendingIdSequence < 0)
                {
                    string firstId = await DatabaseManager.GenerateEventIdAsync();
                    _pendingIdSequence = int.Parse(firstId.Substring(1));
                }
                else
                {
                    _pendingIdSequence++;
                }

                string newEventId = "E" + _pendingIdSequence.ToString("D4");

                AdminDisasterEvent newEvent = new AdminDisasterEvent
                {
                    EventId = newEventId,
                    EventName = string.Empty,
                    StartDate = null,
                    EndDate = null,
                    Status = "Upcoming",
                    IsNew = true
                };

                Application.Current.Dispatcher.Invoke(() =>
                {
                    DisasterEvents.Add(newEvent);
                    SelectedDisasterEvent = newEvent;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not generate new event ID.\n\n" + ex.Message,
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

        private async Task LoadDisasterEventsAsync()
        {
            try
            {
                ObservableCollection<AdminDisasterEvent> loadedEvents =
                    await DatabaseManager.GetAdminDisasterEventsAsync(
                        SelectedDisasterEventFilter,
                        DisasterEventSearchText);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    DisasterEvents = loadedEvents;
                    _pendingIdSequence = -1;
                });
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Could not load disaster events from the database.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An unexpected error occurred while loading disaster events.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedDisasterEvent == null)
            {
                MessageBox.Show("Please select the new event row to save.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!SelectedDisasterEvent.IsNew)
            {
                MessageBox.Show("This row already exists. Use UPDATE instead.",
                    "Save Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateDisasterEvent(SelectedDisasterEvent))
                return;

            try
            {
                await DatabaseManager.AddAdminDisasterEventAsync(SelectedDisasterEvent);

                MessageBox.Show("Disaster event saved successfully.",
                    "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadDisasterEventsAsync();
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                MessageBox.Show("A disaster event with this ID already exists.",
                    "Duplicate Event ID", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Could not save disaster event.\n\n" + ex.Message,
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
            if (SelectedDisasterEvent == null)
            {
                MessageBox.Show("Please select a disaster event to update.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedDisasterEvent.IsNew)
            {
                MessageBox.Show("This is a new row. Use SAVE instead.",
                    "Update Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateDisasterEvent(SelectedDisasterEvent))
                return;

            try
            {
                await DatabaseManager.UpdateAdminDisasterEventAsync(SelectedDisasterEvent);

                MessageBox.Show("Disaster event updated successfully.",
                    "Updated", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadDisasterEventsAsync();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Could not update disaster event.\n\n" + ex.Message,
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
            if (SelectedDisasterEvent == null)
            {
                MessageBox.Show("Please select a disaster event to delete.",
                    "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedDisasterEvent.IsNew)
            {
                DisasterEvents.Remove(SelectedDisasterEvent);
                SelectedDisasterEvent = null;
                return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                "Are you sure you want to delete this disaster event?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                await DatabaseManager.DeleteAdminDisasterEventAsync(SelectedDisasterEvent.EventId);

                MessageBox.Show("Disaster event deleted successfully.",
                    "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadDisasterEventsAsync();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    "Could not delete disaster event.\n\nThis event may already be used in operations, donations, pledges, distributions, or other records.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred while deleting.\n\n" + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateDisasterEvent(AdminDisasterEvent disasterEvent)
        {
            if (string.IsNullOrWhiteSpace(disasterEvent.EventId))
            {
                MessageBox.Show("Event ID is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (disasterEvent.EventId.Length > 10)
            {
                MessageBox.Show("Event ID must be 10 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(disasterEvent.EventName))
            {
                MessageBox.Show("Event name is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (disasterEvent.EventName.Length > 255)
            {
                MessageBox.Show("Event name must be 255 characters or less.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (disasterEvent.StartDate == null)
            {
                MessageBox.Show("Start date is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (disasterEvent.EndDate != null && disasterEvent.EndDate < disasterEvent.StartDate)
            {
                MessageBox.Show("End date cannot be earlier than start date.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
}