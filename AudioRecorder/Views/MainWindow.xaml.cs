using AudioRecorder.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace AudioRecorder.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<MainWindowViewModel>();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            App.Current.SetWindowTobBarDarkMode(this);
        }
    }
}