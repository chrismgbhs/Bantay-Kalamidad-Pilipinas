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
    public partial class donation_signup_view : UserControl
    {
        public donation_signup_view()
        {
            InitializeComponent();
            DataContext = new donation_signup_ViewModel();
        }
    }
}