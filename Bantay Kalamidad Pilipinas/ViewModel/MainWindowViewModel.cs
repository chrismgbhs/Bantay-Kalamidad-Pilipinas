using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Bantay_Kalamidad_Pilipinas.Model;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class MainWindowViewModel
    {
        public ICommand ViewRescueLoginCommand { get; set; }
        public ICommand ViewDonationLoginCommand { get; set; }

        public MainWindowViewModel()
        {
            ViewRescueLoginCommand = new RelayCommand(ViewRescueLogin);
            ViewDonationLoginCommand = new RelayCommand(ViewDonationLogin);
        }

        private void ViewRescueLogin()
        {
            var mainWindow = new View.RescueLogin();
            Application.Current.MainWindow = mainWindow; // ✅ Set BEFORE closing
            mainWindow.Show();                           // ✅ Non-blocking
            Application.Current.Windows
                .OfType<MainWindow>()
                .FirstOrDefault()?.Close();                 // ✅ Close login after
        }

        private void ViewDonationLogin()
        {
            var mainWindow = new View.DonationLogin();
            Application.Current.MainWindow = mainWindow; // ✅ Set BEFORE closing
            mainWindow.Show();                           // ✅ Non-blocking
            Application.Current.Windows
                .OfType<MainWindow>()
                .FirstOrDefault()?.Close();                 // ✅ Close login after
        }
    }
}
