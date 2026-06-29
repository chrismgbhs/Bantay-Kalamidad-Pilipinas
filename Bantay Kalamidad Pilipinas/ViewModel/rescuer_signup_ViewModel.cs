using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Bantay_Kalamidad_Pilipinas.View;
using System.Text.RegularExpressions;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    public class rescuer_signup_ViewModel : ObservableObject
    {
        private string _firstName;
        private string _lastName;
        private string _emailAddress;
        private string _phoneNumber;
        private string _password;
        private bool _isPasswordVisible;
        private bool _isBusy;

        public string FirstName
        {
            get => _firstName;
            set
            {
                if (_firstName != value)
                {
                    _firstName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                if (_lastName != value)
                {
                    _lastName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string EmailAddress
        {
            get => _emailAddress;
            set
            {
                if (_emailAddress != value)
                {
                    _emailAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (_phoneNumber != value)
                {
                    _phoneNumber = value;
                    OnPropertyChanged();
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

        public ICommand SignupCommand { get; }
        public ICommand OpenSigninCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand GoogleLoginCommand { get; }

        public rescuer_signup_ViewModel()
        {
            SignupCommand = new AsyncRelayCommand(SignupAsync, CanSubmit);
            OpenSigninCommand = new RelayCommand(Signin);
            BackCommand = new RelayCommand(Back);
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);
            GoogleLoginCommand = new AsyncRelayCommand(GoogleSignupAsync, CanSubmit);
        }

        private bool CanSubmit()
        {
            return !IsBusy;
        }

        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private async Task SignupAsync()
        {
            string firstName = FirstName?.Trim();
            string lastName = LastName?.Trim();
            string email = EmailAddress?.Trim();
            string phone = PhoneNumber?.Trim();
            string password = Password;

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(
                    "Please fill out all of the fields.",
                    "Missing Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show(
                    "Please enter a valid email address.",
                    "Invalid Email",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show(
                    "Password must be at least 6 characters long.",
                    "Weak Password",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsBusy = true;

                string volunteerName = firstName + " " + lastName;

                bool created = await DatabaseManager.RegisterVolunteerAccountAsync(
                    email,
                    password,
                    volunteerName,
                    phone);

                if (!created)
                    return;

                ClearFields();
                Signin();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GoogleSignupAsync()
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
                    "Google sign-up was cancelled or could not be completed.\n\n" + ex.Message,
                    "Google Sign-Up",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Signin()
        {
            Application.Current.MainWindow.Content = new rescue_login_view();
        }

        private void Back()
        {
            Window currentWindow = Application.Current.MainWindow;

            var startWindow = new start_view();
            Application.Current.MainWindow = startWindow;
            startWindow.Show();

            currentWindow?.Close();
        }

        private void ClearFields()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            EmailAddress = string.Empty;
            PhoneNumber = string.Empty;
            Password = string.Empty;
            IsPasswordVisible = false;
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }
    }
}