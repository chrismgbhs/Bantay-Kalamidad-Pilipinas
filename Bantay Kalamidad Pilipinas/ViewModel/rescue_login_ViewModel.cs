using Bantay_Kalamidad_Pilipinas.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Bantay_Kalamidad_Pilipinas.View;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    public class rescue_login_ViewModel : ObservableObject
    {
        public static UserModel CurrentUser { get; set; } = new UserModel();

        private string _password;
        private bool _isPasswordVisible;
        private bool _isBusy;

        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();

                    if (CurrentUser == null)
                    {
                        CurrentUser = new UserModel();
                    }

                    CurrentUser.Password = value;
                    OnPropertyChanged(nameof(CurrentUser));
                }
            }
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                if (_isPasswordVisible != value)
                {
                    _isPasswordVisible = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PasswordToggleText));
                    OnPropertyChanged(nameof(MaskedPasswordVisibility));
                    OnPropertyChanged(nameof(VisiblePasswordVisibility));
                }
            }
        }

        public string PasswordToggleText => IsPasswordVisible ? "Hide" : "Show";

        public Visibility MaskedPasswordVisibility =>
            IsPasswordVisible ? Visibility.Collapsed : Visibility.Visible;

        public Visibility VisiblePasswordVisibility =>
            IsPasswordVisible ? Visibility.Visible : Visibility.Collapsed;

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand GoogleLoginCommand { get; }
        public ICommand OpenSignupCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand ForgotPasswordCommand { get; }

        public rescue_login_ViewModel()
        {
            if (CurrentUser == null)
            {
                CurrentUser = new UserModel();
            }

            LoginCommand = new AsyncRelayCommand(LoginAsync, CanSubmit);
            GoogleLoginCommand = new AsyncRelayCommand(GoogleLoginAsync, CanSubmit);
            OpenSignupCommand = new RelayCommand(OpenSignup);
            BackCommand = new RelayCommand(Back);
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);
            ForgotPasswordCommand = new RelayCommand(ForgotPassword);
        }

        private bool CanSubmit()
        {
            return !IsBusy;
        }

        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private async Task LoginAsync()
        {
            if (CurrentUser == null)
            {
                CurrentUser = new UserModel();
            }

            CurrentUser.Password = Password;

            if (string.IsNullOrWhiteSpace(CurrentUser.Username) ||
                string.IsNullOrWhiteSpace(CurrentUser.Password))
            {
                MessageBox.Show(
                    "Please enter your username and password.",
                    "Login",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsBusy = true;
                await DatabaseManager.Login(CurrentUser, "rescue", "Volunteer");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GoogleLoginAsync()
        {
            try
            {
                IsBusy = true;

                await GoogleAuthHelper.SignOutAsync();

                GoogleSignInResult result = await GoogleAuthHelper.SignInAsync();

                await DatabaseManager.LoginWithGoogle(
                    result.Email,
                    result.DisplayName,
                    "rescue",
                    "Volunteer");
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Google Sign-In Not Set Up",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Google sign-in was cancelled or could not be completed.\n\n" + ex.Message,
                    "Google Sign-In",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OpenSignup()
        {
            Application.Current.MainWindow.Content = new rescue_signup_view();
        }

        private void Back()
        {
            Window currentWindow = Application.Current.MainWindow;

            var startWindow = new start_view();
            Application.Current.MainWindow = startWindow;
            startWindow.Show();

            currentWindow?.Close();
        }

        private void ForgotPassword()
        {
            MessageBox.Show(
                "Password recovery is not implemented yet.",
                "Forgot Password",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}