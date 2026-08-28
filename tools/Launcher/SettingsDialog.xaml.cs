using System.Windows;

namespace DinoWorld.Launcher;

public partial class SettingsDialog : Window
{
    public string ServerAddress { get; private set; }
    public bool AutoPatch { get; private set; }

    public SettingsDialog(string serverAddr, bool autoPatch)
    {
        InitializeComponent();
        AddrBox.Text = serverAddr;
        AutoPatchBox.IsChecked = autoPatch;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        string addr = AddrBox.Text.Trim();
        if (addr.Length == 0 || addr.Contains(' ') || addr.StartsWith("http"))
        {
            MessageBox.Show(this, "รูปแบบไม่ถูกต้อง — ใส่แบบ ip[:port] เช่น 127.0.0.1:8190",
                "ตั้งค่า", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        ServerAddress = addr;
        AutoPatch = AutoPatchBox.IsChecked == true;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
