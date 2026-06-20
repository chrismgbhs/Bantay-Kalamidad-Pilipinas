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
    /// Interaction logic for admin_login_view.xaml
    /// </summary>
    public partial class admin_login_view : UserControl
    {
        public admin_login_view()
        {
            InitializeComponent();
            this.DataContext = new ViewModel.admin_login_ViewModel();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Assign to ViewModel or use directly
            ViewModel.admin_login_ViewModel.CurrentUser.Password = passwordBox.Password;
        }

    }
}
