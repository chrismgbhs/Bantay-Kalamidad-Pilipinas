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

namespace Bantay_Kalamidad_Pilipinas.View
{
    /// <summary>
    /// Interaction logic for start_view.xaml
    /// </summary>
    public partial class start_view : Window
    {
        public start_view()
        {
            InitializeComponent();
            this.DataContext = new ViewModel.start_view_ViewModel();
        }
    }
}
