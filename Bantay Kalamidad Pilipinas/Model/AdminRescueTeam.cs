using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bantay_Kalamidad_Pilipinas.ViewModel;

namespace Bantay_Kalamidad_Pilipinas.Model
{
    public class AdminRescueTeam : ObservableObject
    {
        private string _teamOperation;
        private string _event;
        private string _location;
        private string _members;
        private string _status;
        private bool _isNew;

        public string TeamOperation
        {
            get => _teamOperation;
            set
            {
                if (_teamOperation != value)
                {
                    _teamOperation = value;
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

        public string Members
        {
            get => _members;
            set
            {
                if (_members != value)
                {
                    _members = value;
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