using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bantay_Kalamidad_Pilipinas.Model;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class admin_login_ViewModel
    {
        public static UserModel CurrentUser { get; set; }
        public ICommand LoginCommand { get; set; }

        public admin_login_ViewModel()
        {
            CurrentUser = new UserModel();
            LoginCommand = new RelayCommand(Login);
        }

        private async void Login()
        {
            //MessageBox.Show($"username: {CurrentUser.Username}, password: {CurrentUser.Password}");
            await DatabaseManager.Login(CurrentUser, "admin", "Admin");
        }
    }
}
