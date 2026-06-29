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
    public partial class rescuedashboard_mainlayout_view : Window
    {
        public rescuedashboard_mainlayout_view()
        {
            InitializeComponent();
            DataContext = new rescue_dashboard_mainlayout_ViewModel();
        }
    }
}