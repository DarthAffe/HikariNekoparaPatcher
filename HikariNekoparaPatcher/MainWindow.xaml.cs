using HikariNekoparaPatcher.Controls;

namespace HikariNekoparaPatcher
{
    public partial class MainWindow : BlurredDecorationWindow
    {
        public MainWindow(MainWindowViewModel vm)
        {
            InitializeComponent();

            this.DataContext = vm;
        }
    }
}
