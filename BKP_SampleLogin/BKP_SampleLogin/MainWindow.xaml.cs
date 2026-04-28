using System.Windows;
using BKP_SampleLogin.View;

namespace BKP_SampleLogin
{
    public partial class MainWindow : Window
    {
        public MainWindow(string v)
        {
            InitializeComponent();
        }

        public MainWindow()
        {
        }

        private void BtnRescue_Click(object sender, RoutedEventArgs e)
        {
            var rescueLogin = new RescueLogin("Rescue");
            rescueLogin.Show();
            this.Close();
        }

        private void BtnDonation_Click(object sender, RoutedEventArgs e)
        {
            var donationLogin = new DonationLogin("Donation");
            donationLogin.Show();
            this.Close();
        }
    }
}
