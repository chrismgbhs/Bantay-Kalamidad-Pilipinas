using Bantay_Kalamidad_Pilipinas.View;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    internal class donation_signup_ViewModel : ObservableObject
    {
        private string _firstName;
        private string _lastName;
        private string _emailAddress;
        private string _phoneNumber;
        private string _password;
        private bool _isPasswordVisible;

        public ICommand SignupCommand { get; set; }
        public ICommand OpenSigninCommand { get; set; }
        public ICommand BackCommand { get; set; }
        public ICommand TogglePasswordVisibilityCommand { get; set; }
        public ICommand GoogleLoginCommand { get; set; }

        public donation_signup_ViewModel()
        {
            SignupCommand = new RelayCommand(async () => await Signup());
            OpenSigninCommand = new RelayCommand(Signin);
            BackCommand = new RelayCommand(Back);
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);
            GoogleLoginCommand = new RelayCommand(async () => await GoogleSignup());
        }

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

        private async Task Signup()
        {
            string firstName = FirstName == null ? string.Empty : FirstName.Trim();
            string lastName = LastName == null ? string.Empty : LastName.Trim();
            string email = EmailAddress == null ? string.Empty : EmailAddress.Trim();
            string phone = PhoneNumber == null ? string.Empty : PhoneNumber.Trim();
            string password = Password == null ? string.Empty : Password;

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(phone))
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

            await DatabaseManager.AddDonor(
                email,
                password,
                firstName + " " + lastName,
                phone);

            ClearFields();
        }

        private void Signin()
        {
            Application.Current.MainWindow.Content = new donation_login_view();
        }

        private async Task GoogleSignup()
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
                    "Google sign-up was cancelled or could not be completed.\n\n" + ex.Message,
                    "Google Sign-Up",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
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