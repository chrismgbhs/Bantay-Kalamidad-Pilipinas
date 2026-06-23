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
using System.Windows.Shapes;
using Bantay_Kalamidad_Pilipinas.ViewModel;

namespace Bantay_Kalamidad_Pilipinas.View
{
    public partial class admindashboard_rescue_mainlayout_view : Window
    {
        public admindashboard_rescue_mainlayout_view()
        {
            InitializeComponent();
            DataContext = new admindashboard_rescue_mainlayout_viewModel();
        }
    }
}