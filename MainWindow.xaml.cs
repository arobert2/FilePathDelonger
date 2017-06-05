using System;
using System.Collections.Generic;
using System.IO;
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

namespace FilePathDelonger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
      PathTools PathTools = new PathTools();   //PathTools

        public MainWindow()
        {
            InitializeComponent();
        }

        #region PathTools events



        #endregion

        #region WPF Events
        // FolderScan gets focus
        private void FolderScan_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox s = (TextBox)sender;
            s.Text = "";
            s.Foreground = Brushes.Black;
            s.FontStyle = FontStyles.Normal;
        }
        // FolderScan loses focus
        private void FolderScan_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FolderScan.Text == "")
            {
                FolderScan.Text = "Select a folder to fix.";
                FolderScan.Foreground = Brushes.Gray;
                FolderScan.FontStyle = FontStyles.Italic;
            }
        }
        // Output loses focus
        private void Output_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FolderScan.Text == "")
            {
                Output.Text = "Select an output location.";
                Output.Foreground = Brushes.Gray;
                Output.FontStyle = FontStyles.Italic;
            }
        }
        // Browse button is clicked for FolderScan field.
        private void BrowseScan_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                FolderScan_GotFocus(FolderScan, new RoutedEventArgs());
                var result = fbd.ShowDialog();
                FolderScan.Text = fbd.SelectedPath;
            }
        }
        // Browse button is clicked for trhe Output field.
        private void BrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                FolderScan_GotFocus(Output, new RoutedEventArgs());
                var result = fbd.ShowDialog();
                Output.Text = fbd.SelectedPath;
            }
        }
        // Quit button is clicked.
        private void bttnQuit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        // Fix button is clicked.
        private async void FixButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(FolderScan.Text))
            {
                if (Directory.Exists(Output.Text))
                {
                    
                    FileTree tree = new FileTree();  //FileTree
                    await Task.Run(() => { tree = PathTools.BuildTree(FolderScan.Text); });   //Build the tree
                    await Task.Run(() => { tree = PathTools.MarkCuts(tree, Output.Text, ""); });  //Mark cuts
                    await Task.Run(() => { PathTools.MoveFiles(tree, Output.Text); });    //move files
                }
                else
                    MessageBox.Show("Path " + Output.Text + " does not exist.", "Folder not found!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
                MessageBox.Show("Path " + FolderScan.Text + " does not exist.", "Folder not found!", MessageBoxButton.OK, MessageBoxImage.Error);
                
        }
        // About button is clicked.
        private void bttnAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Author: Allen Roberts\n\n" +
                "WARNING! This program moves files from one path to another, it does not copy, It is possible for you" +
                "to misplace files when this is run on a path. If you cannot find a file check the log files in the destination folder to see where it was moved.\n\n" +
                "This program is designed to move files and folders that exceed the Windows Explorer character limit to a specified location." +
                "Paths that exceed this limit became very hard to back up and restore, and bypassing the path limit can cause OS instability.");
        }
        #endregion
    }
}
