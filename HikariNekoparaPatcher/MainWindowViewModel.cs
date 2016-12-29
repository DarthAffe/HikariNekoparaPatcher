using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using HikariNekoparaPatcher.Misc;
using HikariNekoparaPatcher.Services;

namespace HikariNekoparaPatcher
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Properties & Fields

        private readonly InstallPathService _installPathService;
        private readonly PatchService _patchService;

        private string _gamePath;
        public string GamePath
        {
            get { return _gamePath; }
            set
            {
                _gamePath = value;
                OnPropertyChanged();
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(nameof(IsGamePathSelected));
            }
        }

        public bool IsGamePathSelected => !string.IsNullOrWhiteSpace(GamePath) && Directory.Exists(GamePath);

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        private ActionCommand _openHomepageCommand;
        public ActionCommand OpenHomepageCommand => _openHomepageCommand ?? (_openHomepageCommand = new ActionCommand(OpenHomepage));

        private ActionCommand _exitCommand;
        public ActionCommand ExitCommand => _exitCommand ?? (_exitCommand = new ActionCommand(Exit));

        private ActionCommand _browseGamePathCommand;
        public ActionCommand BrowseGamePathCommand => _browseGamePathCommand ?? (_browseGamePathCommand = new ActionCommand(BrowseGamePath));

        private ActionCommand _startGameCommand;
        public ActionCommand StartGameCommand => _startGameCommand ?? (_startGameCommand = new ActionCommand(StartGame));

        private ActionCommand _applyPatchCommand;
        public ActionCommand ApplyPatchCommand => _applyPatchCommand ?? (_applyPatchCommand = new ActionCommand(ApplyPatch));

        #endregion

        #region Constructors

        public MainWindowViewModel(InstallPathService installPathService, PatchService patchService)
        {
            this._installPathService = installPathService;
            this._patchService = patchService;

            GamePath = _installPathService.GetPathIfDefault();
        }

        #endregion

        #region Methods

        private void OpenHomepage()
        {
            Process.Start("http://hikari-translations.de");
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }

        private void BrowseGamePath()
        {
            GamePath = _installPathService.BrowseGamePath();
        }

        private void StartGame()
        {
            string path = _installPathService.GetExePath(GamePath);
            if (path != null)
                Process.Start(path);
        }

        private async void ApplyPatch()
        {
            IsBusy = true;
            await Task.Run(() => _patchService.ApplyPatch(GamePath));
            IsBusy = false;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
