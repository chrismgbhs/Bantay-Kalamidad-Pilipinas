using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bantay_Kalamidad_Pilipinas.Model;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class admin_login_ViewModel
    {
        public static UserModel CurrentUser { get; set; }
        public ICommand LoginCommand { get; set; }
        public ICommand BackCommand { get; set; }

        public admin_login_ViewModel()
        {
            CurrentUser = new UserModel();
            LoginCommand = new RelayCommand(Login);
            BackCommand = new RelayCommand(Back);
        }

        private async void Login()
        {
            //MessageBox.Show($"username: {CurrentUser.Username}, password: {CurrentUser.Password}");
            await DatabaseManager.Login(CurrentUser, "admin", "Admin");
        }

        private void Back()
        {
            var window = new Window();
            window = new View.start_view();
            window.Show();
            System.Windows.Application.Current.MainWindow.Close();
            System.Windows.Application.Current.MainWindow = window;
        }
    }
}
