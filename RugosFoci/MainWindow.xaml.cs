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

namespace RugosFoci
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UjJatekClick(object sender, RoutedEventArgs e)
        {
            CsapatElnevezesWindow csew = new CsapatElnevezesWindow();
            if (csew.ShowDialog() == true)
            {
                JatekWindow jw = new JatekWindow();
                jw.Show();
            }
        }

        private void KilepesClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SzabalyokClick(object sender, RoutedEventArgs e)
        {
            SzabalyokWindow szw = new SzabalyokWindow();
            szw.Show();
        }

        private void MainKeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Escape: Close(); break;
            }
        }

        
        
    }
}
