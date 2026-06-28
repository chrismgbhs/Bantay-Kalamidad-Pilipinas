using Bantay_Kalamidad_Pilipinas.ViewModel;

namespace Bantay_Kalamidad_Pilipinas.Model
{
    public class UserModel : ObservableObject
    {

        //DECLARATION OF OBJECT ELEMENTS
        private int _userID;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _role = string.Empty;

        public int UserID
        {
            get => _userID;
            set
            {
                if (_userID != value)
                {
                    _userID = value;
                    OnPropertyChanged();
                }
            }
        }

        //REFERENCE USERNAME TO PROPERTY CHANGE
        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
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
    }
}