using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bantay_Kalamidad_Pilipinas.ViewModel;

namespace Bantay_Kalamidad_Pilipinas.Model
{
    public class AdminLogistics : ObservableObject
    {
        private string _deliveryId;
        private string _distribution;
        private DateTime? _deliveryDate;
        private string _status;

        private string _pickupId;
        private string _donation;
        private DateTime? _pickupDate;

        public string DeliveryId
        {
            get => _deliveryId;
            set
            {
                if (_deliveryId != value)
                {
                    _deliveryId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Distribution
        {
            get => _distribution;
            set
            {
                if (_distribution != value)
                {
                    _distribution = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? DeliveryDate
        {
            get => _deliveryDate;
            set
            {
                if (_deliveryDate != value)
                {
                    _deliveryDate = value;
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

        public string PickupId
        {
            get => _pickupId;
            set
            {
                if (_pickupId != value)
                {
                    _pickupId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Donation
        {
            get => _donation;
            set
            {
                if (_donation != value)
                {
                    _donation = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? PickupDate
        {
            get => _pickupDate;
            set
            {
                if (_pickupDate != value)
                {
                    _pickupDate = value;
                    OnPropertyChanged();
                }
            }
        }




    }
}
