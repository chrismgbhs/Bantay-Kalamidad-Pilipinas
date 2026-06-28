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
    internal class rescuer_signup_ViewModel : ObservableObject
    {
        public static string FirstName { get; set; }
        public static string LastName { get; set; }
        public static string EmailAddress { get; set; }
        public static string PhoneNumber { get; set; }
        public static string password { get; set; }

        private bool _isPasswordVisible;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                _isPasswordVisible = value;
                OnPropertyChanged();
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                password = value;
            }
        }

        public ICommand SignupCommand { get; set; }
        public ICommand OpenSigninCommand { get; set; }
        public ICommand BackCommand { get; set; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand GoogleLoginCommand { get; set; }

        public rescuer_signup_ViewModel()
        {
            SignupCommand = new RelayCommand(async () => await Signup());
            OpenSigninCommand = new RelayCommand(Signin);
            BackCommand = new RelayCommand(Back);
            TogglePasswordVisibilityCommand = new RelayCommand(() => IsPasswordVisible = !IsPasswordVisible);
            GoogleLoginCommand = new RelayCommand(async () => await GoogleSignup());
        }

        public static void Back()
        {
            var window = new Window();
            window = new View.start_view();
            window.Show();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = window;
        }

        public static async Task Signup()
        {
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(EmailAddress) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(PhoneNumber))
            {
                MessageBox.Show("Please fill out all of the fields.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!EmailAddress.Contains("@") || !EmailAddress.Contains("."))
            {
                MessageBox.Show("Please enter a valid email address.", "Invalid Email", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Weak Password", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await DatabaseManager.AddVolunteer(EmailAddress, password, FirstName + " " + LastName, PhoneNumber);

            FirstName = null;
            LastName = null;
            EmailAddress = null;
            password = null;
            PhoneNumber = null;
        }
        public static void Signin()
        {
            var userControl = new View.rescue_login_view(); // this is a UserControl
            Application.Current.MainWindow.Content = userControl;
        }

        private async Task GoogleSignup()
        {
            try
            {
                var result = await GoogleAuthHelper.SignInAsync();
                await DatabaseManager.LoginWithGoogle(result.Email, result.DisplayName, "rescue", "Volunteer");
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Google Sign-In Not Set Up", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Google sign-in was cancelled or could not be completed. Please try again.",
                    "Google Sign-In",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}