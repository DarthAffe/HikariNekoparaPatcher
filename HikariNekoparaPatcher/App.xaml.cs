using System.Windows;
using HikariNekoparaPatcher.Services;

namespace HikariNekoparaPatcher
{
    public partial class App : Application
    {
        #region Methods

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //TODO DarthAffe 19.11.2016: Headless start to be able to use this with the launcher
            InstallPathService installPathService = new InstallPathService();
            PatchService patchService = new PatchService();

            MainWindowViewModel vm = new MainWindowViewModel(installPathService, patchService);
            MainWindow = new MainWindow(vm);
            MainWindow.Show();
        }

        #endregion
    }
}
