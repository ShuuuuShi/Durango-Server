using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DinoWorld.Launcher;

/// <summary>
/// ผีเสื้อ 8-bit สีชมพูเข้ม — วาดจากพิกเซลแมป 12x10 เหี่ยวๆ แบบเกมยุค 8 บิต
/// (A=ชมพูหลัก B=ชมพูเข้ม H=ไฮไลต์ K=ตัวดำ)
/// </summary>
public class PixelButterfly : Canvas
{
    private static readonly string[] Map =
    {
        "...K....K...",
        ".AAA....AAA.",
        "AAHAA..AAHAA",
        "AHAB....BAHA",
        ".ABKKKKKKBA.",
        "..BKKKKKKB..",
        ".ABBKKKKBBA.",
        "AABBAKKABBAA",
        "AHB.AAA.BHA.",
        ".A..AAA..A..",
    };

    private static readonly Dictionary<char, Color> Colors = new()
    {
        ['A'] = Color.FromRgb(0xFF, 0x2E, 0x7E),
        ['B'] = Color.FromRgb(0xA8, 0x13, 0x53),
        ['H'] = Color.FromRgb(0xFF, 0x9E, 0xC7),
        ['K'] = Color.FromRgb(0x1C, 0x09, 0x13),
    };

    public PixelButterfly()
    {
        const double cell = 2.0;   // ออกแบบที่ 24x20 px แล้วค่อยย่อ/ขยายจาก ContentControl
        for (int y = 0; y < Map.Length; y++)
        {
            for (int x = 0; x < Map[y].Length; x++)
            {
                char c = Map[y][x];
                if (c == '.' || !Colors.TryGetValue(c, out Color color))
                {
                    continue;
                }
                Rectangle r = new()
                {
                    Width = cell + 0.02,
                    Height = cell + 0.02,
                    Fill = new SolidColorBrush(color),
                };
                SetLeft(r, x * cell);
                SetTop(r, y * cell);
                Children.Add(r);
            }
        }
        Width = 12 * cell;
        Height = Map.Length * cell;
    }
}
