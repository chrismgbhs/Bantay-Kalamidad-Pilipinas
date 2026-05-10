using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class rescuer_signup_ViewModel
    {
        public static string FirstName { get; set; }
        public static string LastName { get; set; }
        public static string EmailAddress { get; set; }
        public static string Password { get; set; }
        public static string PhoneNumber { get; set; }

        public ICommand SignupCommand { get; set; }
        public ICommand OpenSigninCommand { get; set; }
        public ICommand BackCommand { get; set; }

        public rescuer_signup_ViewModel() 
        {
            SignupCommand = new RelayCommand(Signup);
            OpenSigninCommand = new RelayCommand(Signin);
            BackCommand = new RelayCommand(Back);   
        }

        public static void Back()
        {
            var window = new Window();
            window = new View.start_view();
            window.Show();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = window;
        }

        public static void Signup()
        {
            if (FirstName != null && LastName != null && EmailAddress != null && Password != null && PhoneNumber != null) 
            {
                DatabaseManager.AddVolunteer(EmailAddress, Password, FirstName + " " + LastName, PhoneNumber);
            }

            else
            {
                MessageBox.Show("Please fill out all of the fields.");
            }

        }
        public static void Signin()
        {
            var userControl = new View.rescue_login_view(); // this is a UserControl
            Application.Current.MainWindow.Content = userControl;
        }
    }
}
