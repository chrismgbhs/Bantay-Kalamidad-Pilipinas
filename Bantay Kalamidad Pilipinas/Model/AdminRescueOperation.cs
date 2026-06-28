using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bantay_Kalamidad_Pilipinas.ViewModel;

namespace Bantay_Kalamidad_Pilipinas.Model
{
    public class AdminRescueOperation : ObservableObject
    {
        private string _operationId;
        private string _eventId;       // raw FK stored in DB
        private string _event;         // display name shown in grid
        private string _locationId;    // raw FK stored in DB
        private string _location;      // display string shown in grid
        private DateTime? _dateStarted;
        private string _status;
        private bool _isNew;

        public string OperationId
        {
            get => _operationId;
            set { if (_operationId != value) { _operationId = value; OnPropertyChanged(); } }
        }

        /// <summary>Raw Event_ID FK — used in INSERT/UPDATE queries.</summary>
        public string EventId
        {
            get => _eventId;
            set { if (_eventId != value) { _eventId = value; OnPropertyChanged(); } }
        }

        /// <summary>Display name (Event_Name) — shown in the grid column.</summary>
        public string Event
        {
            get => _event;
            set { if (_event != value) { _event = value; OnPropertyChanged(); } }
        }

        /// <summary>Raw Location_ID FK — used in INSERT/UPDATE queries.</summary>
        public string LocationId
        {
            get => _locationId;
            set { if (_locationId != value) { _locationId = value; OnPropertyChanged(); } }
        }

        /// <summary>Display string (Barangay, City) — shown in the grid column.</summary>
        public string Location
        {
            get => _location;
            set { if (_location != value) { _location = value; OnPropertyChanged(); } }
        }

        public DateTime? DateStarted
        {
            get => _dateStarted;
            set
            {
                if (_dateStarted != value)
                {
                    _dateStarted = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsNew
        {
            get => _isNew;
            set
            {
                if (_isNew != value)
                {
                    _isNew = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}