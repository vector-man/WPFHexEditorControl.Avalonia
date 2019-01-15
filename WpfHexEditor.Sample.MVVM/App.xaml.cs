using System;
using System.Windows;

namespace WpfHexEditor.Sample.MVVM {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class SampleApp : Application {
        public SampleApp() {
            DispatcherUnhandledException += (sender, e) => {

            };
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => {

            };
        }
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            new BootStrapper().Run();

        }
    }
}
