using Bantay_Kalamidad_Pilipinas.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class rescue_login_ViewModel : ObservableObject
    {
        public static UserModel CurrentUser { get; set; }
        public ICommand LoginCommand { get; set; }
        public ICommand GoogleLoginCommand { get; set; }

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
                CurrentUser.Password = value;
            }
        }

        public rescue_login_ViewModel()
        {
            CurrentUser = new UserModel();
            LoginCommand = new RelayCommand(Login);
            OpenSignupCommand = new RelayCommand(Signup);
            BackCommand = new RelayCommand(Back);
            TogglePasswordVisibilityCommand = new RelayCommand(() => IsPasswordVisible = !IsPasswordVisible);
            GoogleLoginCommand = new RelayCommand(async () => await GoogleLogin());
        }

        public ICommand OpenSignupCommand { get; set; }
        public ICommand BackCommand { get; set; }
        public ICommand TogglePasswordVisibilityCommand { get; }

        private async void Signup()
        {
            var userControl = new View.rescue_signup_view(); // this is a UserControl
            Application.Current.MainWindow.Content = userControl;
        }

        private async void Login()
        {
            await DatabaseManager.Login(CurrentUser, "rescue", "Volunteer");
        }

        private async Task GoogleLogin()
        {
            try
            {
                var result = await GoogleAuthHelper.SignInAsync();
                await DatabaseManager.LoginWithGoogle(result.Email, result.DisplayName, "rescue", "Volunteer");
            }
            catch (InvalidOperationException ex)
            {
                // Thrown by GoogleAuthHelper when GoogleAuthConfig.cs still has placeholder values.
                MessageBox.Show(ex.Message, "Google Sign-In Not Set Up", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception)
            {
                // Covers the user closing the browser tab, denying consent, or losing network
                // mid-flow — GoogleWebAuthorizationBroker surfaces these as generic exceptions
                // rather than a specific "user cancelled" type, so this is a catch-all on purpose.
                MessageBox.Show(
                    "Google sign-in was cancelled or could not be completed. Please try again.",
                    "Google Sign-In",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void Back()
        {
            var window = new Window();
            window = new View.start_view();
            window.Show();
            Application.Current.MainWindow.Close();
            Application.Current.MainWindow = window;
        }

    }
}