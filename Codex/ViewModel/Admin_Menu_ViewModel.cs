using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bantay_Kalamidad_Pilipinas.View;
using System.Windows;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    public class Admin_Menu_ViewModel : ObservableObject
    {
        public ICommand OpenAdminRescueDashboardCommand { get; }
        public ICommand OpenAdminDonationDashboardCommand { get; }
        public ICommand BackToSigninCommand { get; }

        public Admin_Menu_ViewModel()
        {
            OpenAdminRescueDashboardCommand = new RelayCommand(OpenAdminRescueDashboard);
            OpenAdminDonationDashboardCommand = new RelayCommand(OpenAdminDonationDashboard);
            BackToSigninCommand = new RelayCommand(BackToSignin);
        }

        private void OpenAdminRescueDashboard()
        {
            Window currentWindow = Application.Current.MainWindow;

            var rescueDashboard = new admindashboard_rescue_mainlayout_view();
            rescueDashboard.Show();

            Application.Current.MainWindow = rescueDashboard;
            currentWindow?.Close();
        }

        private void OpenAdminDonationDashboard()
        {
            Window currentWindow = Application.Current.MainWindow;

            var donationDashboard = new admindashboard_donation_mainlayout_view();
            donationDashboard.Show();

            Application.Current.MainWindow = donationDashboard;
            currentWindow?.Close();
        }

        private void BackToSignin()
        {
            Application.Current.MainWindow.Content = new admin_login_view();
        }
    }
}