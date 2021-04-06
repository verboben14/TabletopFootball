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

namespace RugosFoci
{
    /// <summary>
    /// Interaction logic for SzabalyokWindow.xaml
    /// </summary>
    public partial class SzabalyokWindow : Window
    {
        public SzabalyokWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        private void SzabalyokWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        private void SzabalyokBezaras_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
