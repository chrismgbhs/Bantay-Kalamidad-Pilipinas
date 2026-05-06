using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bantay_Kalamidad_Pilipinas.View
{
    /// <summary>
    /// Interaction logic for rescue_signup_view.xaml
    /// </summary>
    public partial class rescue_signup_view : UserControl
    {
        public rescue_signup_view()
        {
            InitializeComponent();
            this.DataContext = new ViewModel.rescuer_signup_ViewModel();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Assign to ViewModel or use directly
            ViewModel.rescuer_signup_ViewModel.Password = passwordBox.Password;
        }
    }
}
