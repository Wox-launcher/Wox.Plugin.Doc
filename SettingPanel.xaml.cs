using System.Windows.Controls;

namespace Wox.Plugin.Doc
{
    /// <summary>
    /// Interaction logic for SettingPanel.xaml
    /// </summary>
    public partial class SettingPanel : UserControl
    {
        public SettingPanel()
        {
            InitializeComponent();

            Loaded += SettingPanel_Loaded;
        }

        void SettingPanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            lbInstalledDocs.ItemsSource = DocSet.InstalledDocs;
        }
    }
}
