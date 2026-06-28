using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bantay_Kalamidad_Pilipinas.ViewModel;

namespace Bantay_Kalamidad_Pilipinas.Model
{
    public class AdminOperationAssignment : ObservableObject
    {
        private string _assignmentId;
        private string _operation;
        private string _rescuerName;
        private string _volunteerId;
        private string _role;
        private string _status;
        private bool _isNew;

        public string AssignmentId
        {
            get => _assignmentId;
            set
            {
                if (_assignmentId != value)
                {
                    _assignmentId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Operation
        {
            get => _operation;
            set
            {
                if (_operation != value)
                {
                    _operation = value;
                    OnPropertyChanged();
                }
            }
        }

        public string RescuerName
        {
            get => _rescuerName;
            set
            {
                if (_rescuerName != value)
                {
                    _rescuerName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string VolunteerId
        {
            get => _volunteerId;
            set
            {
                if (_volunteerId != value)
                {
                    _volunteerId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Role
        {
            get => _role;
            set
            {
                if (_role != value)
                {
                    _role = value;
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