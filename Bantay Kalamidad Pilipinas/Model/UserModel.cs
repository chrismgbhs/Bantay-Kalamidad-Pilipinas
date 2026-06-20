using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bantay_Kalamidad_Pilipinas.Model
{
    internal class UserModel : ViewModel.ObservableObject
    {
        //DECLARATION OF OBJECT ELEMENTS
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _role = string.Empty;

        //REFERENCE USERNAME TO PROPERTY CHANGE
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        //REFERENCE PIN TO PROPERTY CHANGE
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
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
                    OnPropertyChanged(nameof(Role));
                }
            }
        }
    }
}
