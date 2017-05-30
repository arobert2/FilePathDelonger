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

namespace FilenameDelonger
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

        private void FolderScan_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox s = (TextBox)sender;
            s.Text = "";
            s.Foreground = Brushes.Black;
            s.FontStyle = FontStyles.Normal;
        }

        private void FolderScan_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FolderScan.Text == "")
            {
                FolderScan.Text = "Select a folder to fix.";
                FolderScan.Foreground = Brushes.Gray;
                FolderScan.FontStyle = FontStyles.Italic;
            }
        }

        private void Output_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FolderScan.Text == "")
            {
                Output.Text = "Select an output location.";
                Output.Foreground = Brushes.Gray;
                Output.FontStyle = FontStyles.Italic;
            }
        }

        private void BrowseScan_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                FolderScan_GotFocus(FolderScan, new RoutedEventArgs());
                var result = fbd.ShowDialog();
                FolderScan.Text = fbd.SelectedPath;
            }
        }

        private void BrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                FolderScan_GotFocus(Output, new RoutedEventArgs());
                var result = fbd.ShowDialog();
                Output.Text = fbd.SelectedPath;
            }
        }

        private void bttnQuit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
