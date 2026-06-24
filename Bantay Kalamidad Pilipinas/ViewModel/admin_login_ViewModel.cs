using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bantay_Kalamidad_Pilipinas.Model;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows;
using Bantay_Kalamidad_Pilipinas.View;
using System.Data.SqlClient;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    public class admin_login_ViewModel : ObservableObject
    {
        private UserModel _currentUser;
        private bool _isBusy;
        private string _statusMessage;

        public UserModel CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    OnPropertyChanged();
                }
            }
        }

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

        // testing
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ForgotPasswordCommand { get; }
        public ICommand GoogleSignInCommand { get; }

        public admin_login_ViewModel()
        {
            CurrentUser = new UserModel();

            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
            BackCommand = new RelayCommand(Back);
            ForgotPasswordCommand = new RelayCommand(ForgotPassword);
            GoogleSignInCommand = new RelayCommand(GoogleSignInNotAvailable);
        }

        private bool CanLogin()
        {
            return !IsBusy;
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentUser.Username))
            {
                MessageBox.Show("Please enter your username.", "Admin Login",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentUser.Password))
            {
                MessageBox.Show("Please enter your password.", "Admin Login",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "Signing in...";

                UserModel authenticatedUser = await DatabaseManager.AuthenticateUserAsync(
                    CurrentUser.Username,
                    CurrentUser.Password,
                    "Admin");

                if (authenticatedUser == null)
                {
                    MessageBox.Show(
                        "Incorrect admin username or password, or this account is not registered as an Admin.",
                        "Login Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    StatusMessage = "Login failed.";
                    return;
                }

                CurrentUser.UserID = authenticatedUser.UserID;
                CurrentUser.Username = authenticatedUser.Username;
                CurrentUser.Role = authenticatedUser.Role;

                NavigateToAdminMenu();
            }
            catch (SqlException)
            {
                MessageBox.Show(
                    "Could not connect to the database. Please check your SQL Server connection and database name.",
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An unexpected error occurred while signing in.\n\n" + ex.Message,
                    "Admin Login Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
                StatusMessage = string.Empty;
            }
        }

        private void Back()
        {
            Application.Current.MainWindow.Content = new start_view();
        }

        private void ForgotPassword()
        {
            MessageBox.Show(
                "Password recovery is not implemented for admin accounts yet.",
                "Forgot Password",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void GoogleSignInNotAvailable()
        {
            MessageBox.Show(
                "Google Sign-In for admin accounts is not implemented yet.",
                "Google Sign-In",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void NavigateToAdminMenu()
        {
            Application.Current.MainWindow.Content = new admin_menu_view();
        }
    }
}