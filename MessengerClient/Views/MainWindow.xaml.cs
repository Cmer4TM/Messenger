using MessengerClient.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MessengerClient
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel();
        }
        private void ListBox_SelectionChanged(object sender, EventArgs e)
            => sv.ScrollToEnd();
        private async void Window_Closed(object sender, EventArgs e)
            => await MainWindowViewModel.Exit();
    }
}