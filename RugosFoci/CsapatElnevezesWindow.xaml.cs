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
    /// Interaction logic for CsapatElnevezesWindow.xaml
    /// </summary>
    public partial class CsapatElnevezesWindow : Window
    {
        public CsapatElnevezesWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        }

        private void CsapatOKClick(object sender, RoutedEventArgs e)
        {
            Felulet.SajatCsapat = txtSajatCsapat.Text;
            Felulet.EllenfelCsapat = txtEllenfelCsapat.Text;
            this.DialogResult = true;
        }

        private void CsapatokWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }
    }
}
