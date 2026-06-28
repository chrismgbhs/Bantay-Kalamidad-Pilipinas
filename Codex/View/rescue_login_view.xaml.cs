using Bantay_Kalamidad_Pilipinas.ViewModel;
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
    /// Interaction logic for rescue_login_view.xaml
    /// </summary>
    public partial class rescue_login_view : UserControl
    {
        public rescue_login_view()
        {
            InitializeComponent();
            this.DataContext = new ViewModel.rescue_login_ViewModel();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is rescue_login_ViewModel vm)
            {
                vm.Password = ((PasswordBox)sender).Password;
            }
        }

    }
}
