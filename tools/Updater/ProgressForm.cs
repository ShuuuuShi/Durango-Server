using System.Drawing;
using System.Windows.Forms;

namespace DurangoUpdater;

internal sealed class ProgressForm : Form
{
    private readonly Label _status;
    private readonly Label _percent;
    private readonly ProgressBar _progress;

    public ProgressForm()
    {
        Text = "Durango TH Updater";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = true;
        ClientSize = new Size(520, 142);
        Font = new Font("Segoe UI", 9F);

        Label title = new Label
        {
            AutoSize = true,
            Text = "Durango TH",
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Location = new Point(20, 16)
        };
        Controls.Add(title);

        _percent = new Label
        {
            AutoSize = true,
            Text = "กำลังตรวจสอบ...",
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(390, 20)
        };
        Controls.Add(_percent);

        _status = new Label
        {
            AutoSize = false,
            Text = "กำลังตรวจสอบแพท...",
            Location = new Point(20, 50),
            Size = new Size(480, 22)
        };
        Controls.Add(_status);

        _progress = new ProgressBar
        {
            Location = new Point(20, 86),
            Size = new Size(480, 24),
            Minimum = 0,
            Maximum = 100,
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 25
        };
        Controls.Add(_progress);
    }

    public void SetStatus(string message)
    {
        _status.Text = message;
    }

    public void SetBusy(string message)
    {
        _status.Text = message;
        _percent.Text = "กำลังทำงาน...";
        _progress.Style = ProgressBarStyle.Marquee;
    }

    public void SetProgress(long received, long total)
    {
        if (total <= 0)
        {
            SetBusy("กำลังดาวน์โหลดแพท...");
            return;
        }

        int value = (int)Math.Clamp(received * 100L / total, 0L, 100L);
        _progress.Style = ProgressBarStyle.Continuous;
        _progress.Value = value;
        _percent.Text = value + "%";
        _status.Text = $"กำลังดาวน์โหลดแพท... {FormatBytes(received)} / {FormatBytes(total)}";
    }

    public void SetComplete(string message)
    {
        _progress.Style = ProgressBarStyle.Continuous;
        _progress.Value = 100;
        _percent.Text = "100%";
        _status.Text = message;
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024 * 1024)
        {
            return $"{bytes / 1024d:0.0} KB";
        }
        return $"{bytes / (1024d * 1024d):0.0} MB";
    }
}
