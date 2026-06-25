using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bantay_Kalamidad_Pilipinas.ViewModel;

namespace Bantay_Kalamidad_Pilipinas.Model
{
    public class AdminPledges : ObservableObject
    {
        private string _pledgeId;
        private string _donorId;
        private DateTime? _datePledge;
        private string _pledgeStatus;

        private string _pledgeItemId;
        private string _itemName;
        private string _quantity;
        private DateTime? _expectedDelivery;

        public string PledgeId
        {
            get => _pledgeId;
            set
            {
                if (_pledgeId != value)
                {
                    _pledgeId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DonorId
        {
            get => _donorId;
            set
            {
                if (_donorId != value)
                {
                    _donorId = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? DatePledge
        {
            get => _datePledge;
            set
            {
                if (_datePledge != value)
                {
                    _datePledge = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PledgeStatus
        {
            get => _pledgeStatus;
            set
            {
                if (_pledgeStatus != value)
                {
                    _pledgeStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PledgeItemId
        {
            get => _pledgeItemId;
            set
            {
                if (_pledgeItemId != value)
                {
                    _pledgeItemId = value;
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

        public DateTime? ExpectedDelivery
        {
            get => _expectedDelivery;
            set
            {
                if (_expectedDelivery != value)
                {
                    _expectedDelivery = value;
                    OnPropertyChanged();
                }
            }
        }






    }
}
