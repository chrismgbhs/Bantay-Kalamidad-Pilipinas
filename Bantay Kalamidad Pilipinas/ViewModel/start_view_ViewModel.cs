using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class start_view_ViewModel
    {
        public ICommand OpenRescueCommand { get; set; }
        public ICommand OpenDonationCommand { get; set; }
        public ICommand OpenAdminCommand { get;set; }

        public start_view_ViewModel()
        {
            OpenRescueCommand = new RelayCommand(OpenRescue);
            OpenDonationCommand = new RelayCommand(OpenDonation);
            OpenAdminCommand = new RelayCommand(OpenAdmin);
        }

        public static void OpenRescue()
        {
            // Inside your login logic
            var window = new View.rescue_login_view(); // this is a UserControl
            Application.Current.MainWindow.Content = window;
        }

        public static void OpenDonation()
        {
            // Inside your login logic
            var window = new View.donation_login_view(); // this is a UserControl
            Application.Current.MainWindow.Content = window;
        }

        public static void OpenAdmin()
        {
            var window = new View.admin_login_view(); // this is a UserControl
            Application.Current.MainWindow.Content = window;
        }   
    }
}
