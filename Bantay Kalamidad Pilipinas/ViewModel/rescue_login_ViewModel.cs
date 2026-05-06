using Bantay_Kalamidad_Pilipinas.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class rescue_login_ViewModel
    {
        public static UserModel CurrentUser { get; set; }
        public ICommand LoginCommand { get; set; }

        public rescue_login_ViewModel()
        {
            CurrentUser = new UserModel();
            LoginCommand = new RelayCommand(Login);
            OpenSignupCommand = new RelayCommand(Signup);
        }

        public ICommand OpenSignupCommand { get; set; }

        private async void Signup()
        {
            var userControl = new View.rescue_signup_view(); // this is a UserControl
            Application.Current.MainWindow.Content = userControl;
        }

        private async void Login()
        {
            await DatabaseManager.Login(CurrentUser, "rescue", "Volunteer");
        }
    }
}
