using AudioRecorder.Service;
using AudioRecorder.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace AudioRecorder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Services = ConfigureServices();
        }

        public static App Current => (App)Application.Current;

        public IServiceProvider Services { get; }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<MainWindowViewModel>();
            services.AddSingleton<AudioService>();

            return services.BuildServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Settings.Default.Save();
            Helpers.PathHelper.RemoveFolder("Temp");
            Environment.Exit(0);
        }

        /// <summary>
        /// Включение даркмода на топ баре окна
        /// </summary>
        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

        public void SetWindowTobBarDarkMode(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;
            if (DwmSetWindowAttribute(handle, 19, new[] { 1 }, 4) != 0)
                DwmSetWindowAttribute(handle, 20, new[] { 1 }, 4);
        }
    }

}
