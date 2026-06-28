using System;
using System.Windows;
using System.Windows.Input;

namespace Bantay_Kalamidad_Pilipinas.ViewModel
{
    /// <summary>
    /// Backs donationdashboard_ContactUs_view.xaml.
    /// The form collects FullName, Email, Subject, and Message.
    /// There is no Contacts table in the current schema, so
    /// SendMessageCommand shows a thank-you confirmation for now.
    /// When a mail/storage backend is added, wire it here.
    /// </summary>
    public class donationdashboard_ContactUs_ViewModel : ObservableObject
    {
        private string _fullName;
        private string _email;
        private string _subject;
        private string _message;

        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Subject
        {
            get => _subject;
            set { _subject = value; OnPropertyChanged(); }
        }

        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        public ICommand SendMessageCommand { get; }

        public donationdashboard_ContactUs_ViewModel()
        {
            SendMessageCommand = new RelayCommand(SendMessage);
        }

        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Subject) || string.IsNullOrWhiteSpace(Message))
            {
                MessageBox.Show(
                    "Please fill in all fields before sending.",
                    "Contact Us",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (!Email.Contains("@") || !Email.Contains("."))
            {
                MessageBox.Show(
                    "Please enter a valid email address.",
                    "Contact Us",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show(
                $"Thank you, {FullName}! Your message has been received.\n\nWe will get back to you at {Email} as soon as possible.",
                "Message Sent",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Clear the form after submission
            FullName = string.Empty;
            Email = string.Empty;
            Subject = string.Empty;
            Message = string.Empty;
        }
    }
}