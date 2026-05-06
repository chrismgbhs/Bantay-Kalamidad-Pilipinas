using Bantay_Kalamidad_Pilipinas.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class donation_login_ViewModel
    {
        public static UserModel CurrentUser { get; set; }
        public ICommand LoginCommand { get; set; }

        public donation_login_ViewModel()
        {
            CurrentUser = new UserModel();
            LoginCommand = new RelayCommand(Login);
        }

        private async void Login()
        {
            await DatabaseManager.Login(CurrentUser, "donation", "Donor");
        }
    }
}
