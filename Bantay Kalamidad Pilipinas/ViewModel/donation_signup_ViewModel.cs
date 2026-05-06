using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class donation_signup_ViewModel
    {
        public static string FirstName { get; set; }
        public static string LastName { get; set; }
        public static string EmailAddress { get; set; }
        public static string Password { get; set; }
        public static string PhoneNumber { get; set; }

        public ICommand SignupCommand { get; set; }
        public ICommand OpenSigninCommand { get; set; }

        public donation_signup_ViewModel()
        {
            SignupCommand = new RelayCommand(Signup);
            OpenSigninCommand = new RelayCommand(Signin);
        }

        public static void Signup()
        {
            if (FirstName != null && LastName != null && EmailAddress != null && Password != null && PhoneNumber != null)
            {
                MessageBox.Show("You are now here.");
                DatabaseManager.AddDonor(EmailAddress, Password, FirstName + " " + LastName, PhoneNumber);
            }

            else
            {
                MessageBox.Show("Please fill out all of the fields.");
            }

        }

        public static void Signin()
        {
            var userControl = new View.donation_login_view(); // this is a UserControl
            Application.Current.MainWindow.Content = userControl;
        }
    }
}
