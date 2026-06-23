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
        private string _event;
        private string _location;
        private DateTime? _dateStarted;
        private string _status;
        private bool _isNew;

        public string OperationId
        {
            get => _operationId;
            set
            {
                if (_operationId != value)
                {
                    _operationId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Event
        {
            get => _event;
            set
            {
                if (_event != value)
                {
                    _event = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Location
        {
            get => _location;
            set
            {
                if (_location != value)
                {
                    _location = value;
                    OnPropertyChanged();
                }
            }
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