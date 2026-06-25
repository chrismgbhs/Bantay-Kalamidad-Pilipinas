using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bantay_Kalamidad_Pilipinas.ViewModel;

namespace Bantay_Kalamidad_Pilipinas.Model
{
    public class AdminDonations : ObservableObject
    {
        private string _donationId;
        private string _donor;
        private string _events;
        private DateTime? _dataReceived;
        private string _status;

        private string _donatedItemId;
        private string _itemName;
        private string _quantity;


        public string DonationId
        {
            get => _donationId;
            set
            {
                if (_donationId != value)
                {
                    _donationId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Donor
        {
            get => _donor;
            set
            {
                if (_donor != value)
                {
                    _donor = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Event
        {
            get => _events;
            set
            {
                if (_events != value)
                {
                    _events = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? DateReceived
        {
            get => _dataReceived;
            set
            {
                if (_dataReceived != value)
                {
                    _dataReceived = value;
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

        public string DonatedItemID
        {
            get => _donatedItemId;
            set
            {
                if (_donatedItemId != value)
                {
                    _donatedItemId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ItemName
        {
            get => _itemName;
            set
            {
                if (_itemName != value)
                {
                    _itemName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
            }
        }





    }
}
