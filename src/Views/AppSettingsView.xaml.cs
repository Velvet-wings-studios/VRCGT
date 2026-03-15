using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace VRCGroupTools.Views;

public partial class AppSettingsView : UserControl
{
    public AppSettingsView()
    {
        InitializeComponent();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = e.Uri.AbsoluteUri,
            UseShellExecute = true
        });

        e.Handled = true;
    }
}