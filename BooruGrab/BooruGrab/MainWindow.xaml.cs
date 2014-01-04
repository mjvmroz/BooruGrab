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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Scarlett.Danbooru.Boorugrab.Engine;
using System.IO;

namespace Scarlett.Danbooru.Boorugrab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Manager manager;

        private string Status
        {
            get
            {
                return StatusBlock.Text;
            }
            set
            {
                Dispatcher.Invoke(() => StatusBlock.Text = value);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            manager = new Manager(UsernameField.Text, KeyField.Text);
            manager.DownloadProgressUpdate += manager_DownloadProgressUpdate;
            manager.DownloadComplete += manager_DownloadComplete;

            try
            {
                ControlGrid.IsEnabled = false;
                manager.DownloadAsync(DestinationField.Text, TagField.Text);
            }
            catch(Exception ex)
            {
                Status = ex.Message;
                ControlGrid.IsEnabled = true;
            }
        }

        void manager_DownloadComplete()
        {
            Dispatcher.Invoke(() => ControlGrid.IsEnabled = true);
        }

        void manager_DownloadProgressUpdate(DownloadProgressEventArgs e)
        {
            Status = e;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
