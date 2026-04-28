using Bantay_Kalamidad_Pilipinas.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class RescueLoginViewModel : ObservableObject
    {
        public static UserModel CurrentUser { get; set; }
        public ICommand ViewPortalCommand { get; set; }
        public ICommand LoginCommand { get; set; }

        public RescueLoginViewModel() 
        { 
            CurrentUser = new UserModel();
            ViewPortalCommand = new RelayCommand(ViewPortal);
            LoginCommand = new RelayCommand(Login);
        }

        private async void Login()
        {
            MessageBox.Show($"{CurrentUser.Username}");
            await DatabaseManager.RescueLogin();
        }

        private void ViewPortal()
        {
            var mainWindow = new MainWindow();
            System.Windows.Application.Current.MainWindow = mainWindow; // ✅ Set BEFORE closing
            mainWindow.Show();                           // ✅ Non-blocking
            System.Windows.Application.Current.Windows
                .OfType<View.RescueLogin>()
                .FirstOrDefault()?.Close();                 // ✅ Close login after
        }
    }
}
