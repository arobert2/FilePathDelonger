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
        TreeData TreeData = new TreeData();

        public MainWindow()
        {
            InitializeComponent();
            PathTools.FixStart = FixStartedEvent;
            PathTools.FixEnd = FixEndedEvent;
            PathTools.ScanStarted = ScanStartedEvent;
            PathTools.ScanEnded = ScanEndedEvent;
            PathTools.ContentMoved = ContentMovedEvent;
            PathTools.ContentCopied = ContentCopiedEvent;
        }

        #region PathTools events

        public void ScanStartedEvent(object o, EventArgs args)
        {
            StatusText.Text = "Scan Started";
            Progressbar.IsIndeterminate = true;
        }

        public void ScanEndedEvent(object o, EventArgs args)
        {
            Progressbar.IsIndeterminate = false;
            StatusText.Text = "Scan Ended";
        }

        public void FixStartedEvent(object o, EventArgs args)
        {
            Progressbar.Maximum = TreeData.Count;
            Progressbar.Value = 0;
            StatusText.Text = "Fixing Files";
            PercentText.Text = "0";
        }

        public void FixEndedEvent(object o, EventArgs args)
        {
            StatusText.Text = "Done!";
            Progressbar.Value = 0;
            PercentText.Text = "0";
        }

        public void ContentMovedEvent(object o, EventArgs args)
        {
            Progressbar.Value++;
            int diff = (int)(Progressbar.Value / TreeData.PathBreakPoints.Length);
            PercentText.Text = diff.ToString();
        }

        public void ContentCopiedEvent(object o, EventArgs args)
        {
            Progressbar.Value++;
            int diff = (int)(Progressbar.Value / TreeData.Count);
            PercentText.Text = diff.ToString();
        }

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
        // Move button is clicked.
        private async void FixButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult mbr = MessageBox.Show("Moving files could leave to data loss!", "Warning!", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            //Allow user to cancel.
            if (mbr == MessageBoxResult.Cancel)
                return;

            if (!CheckPath())
                return;
                    
            TreeData = await Task.Run(() => { 
                PathTools PathTools = new PathTools();
                return PathTools.ParsePath(FolderScan.Text, Output.Text); 
            });   //Build the tree
            await Task.Run(() => { PathTools.MoveFiles(TreeData, Output.Text); });    //move files
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
        // Copy button clicked
        private async void Copy_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult mbr = MessageBox.Show("You are about to copy damaged file trees to a new destination. Do you wish to continue?", "Fix File Tree?", MessageBoxButton.OKCancel);

            if (mbr == MessageBoxResult.Cancel)
                return;

            if (!CheckPath())
                return;

            TreeData = await Task.Run(() => {
                PathTools PathTools = new PathTools();
                return PathTools.ParsePath(FolderScan.Text, Output.Text);
            });
            await Task.Run(() => PathTools.CopyFiles(TreeData, Output.Text));

        }
        #endregion
        /// <summary>
        /// Checks the path.
        /// </summary>
        /// <returns></returns>
        private bool CheckPath()
        {
            bool r = true;
            if (!Directory.Exists(FolderScan.Text))
            {
                MessageBox.Show("Path " + FolderScan.Text + " does not exist.", "Folder not found!", MessageBoxButton.OK, MessageBoxImage.Error);
                r = false;
            }
            if (!Directory.Exists(Output.Text))
            {
                MessageBox.Show("Path " + Output.Text + " does not exist.", "Folder not found!", MessageBoxButton.OK, MessageBoxImage.Error);
                r = false;
            }
            return r;
        }
    }
}
