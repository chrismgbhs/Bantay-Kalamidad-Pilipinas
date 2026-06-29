using Bantay_Kalamidad_Pilipinas.Model;
using Bantay_Kalamidad_Pilipinas.View;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class donation_login_ViewModel : ObservableObject
    {
        public static UserModel CurrentUser { get; set; }

        private string _password;
        private bool _isPasswordVisible;

        public ICommand LoginCommand { get; set; }
        public ICommand OpenSignupCommand { get; set; }
        public ICommand BackCommand { get; set; }
        public ICommand TogglePasswordVisibilityCommand { get; set; }
        public ICommand GoogleLoginCommand { get; set; }
        public ICommand ForgotPasswordCommand { get; set; }

        public donation_login_ViewModel()
        {
            CurrentUser = new UserModel();

            LoginCommand = new RelayCommand(async () => await Login());
            OpenSignupCommand = new RelayCommand(Signup);
            BackCommand = new RelayCommand(Back);
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);
            GoogleLoginCommand = new RelayCommand(async () => await GoogleLogin());
            ForgotPasswordCommand = new RelayCommand(ForgotPassword);
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

        public string PasswordToggleText
        {
            get { return IsPasswordVisible ? "Hide" : "Show"; }
        }

        public Visibility MaskedPasswordVisibility
        {
            get { return IsPasswordVisible ? Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility VisiblePasswordVisibility
        {
            get { return IsPasswordVisible ? Visibility.Visible : Visibility.Collapsed; }
        }

        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private void Signup()
        {
            Application.Current.MainWindow.Content = new donation_signup_view();
        }

        private async Task Login()
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
                    "Please enter your email and password.",
                    "Login",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            await DatabaseManager.Login(CurrentUser, "donation", "Donor");
        }

        private async Task GoogleLogin()
        {
            try
            {
                await GoogleAuthHelper.SignOutAsync();

                var result = await GoogleAuthHelper.SignInAsync();

                await DatabaseManager.LoginWithGoogle(
                    result.Email,
                    result.DisplayName,
                    "donation",
                    "Donor");
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
        }

        private void ForgotPassword()
        {
            MessageBox.Show(
                "Password recovery is not implemented yet.",
                "Forgot Password",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Back()
        {
            Window currentWindow = Application.Current.MainWindow;

            var startWindow = new start_view();
            Application.Current.MainWindow = startWindow;
            startWindow.Show();

            currentWindow?.Close();
        }
    }
}