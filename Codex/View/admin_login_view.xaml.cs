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
using Bantay_Kalamidad_Pilipinas.ViewModel;

namespace Bantay_Kalamidad_Pilipinas.View
{
    public partial class admin_login_view : UserControl
    {
        public admin_login_view()
        {
            InitializeComponent();
            DataContext = new admin_login_ViewModel();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is admin_login_ViewModel vm && sender is PasswordBox pb)
            {
                vm.CurrentUser.Password = pb.Password;
            }
        }
    }
}