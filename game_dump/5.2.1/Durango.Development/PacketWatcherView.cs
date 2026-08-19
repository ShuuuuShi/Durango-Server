using UnityEngine;

namespace Durango.Development;

public class PacketWatcherView : MonoBehaviour
{
	private static readonly string[] PostFix = new string[4] { "B", "KB", "MB", "GB" };

	[SerializeField]
	private UILabel _totalLabel;

	[SerializeField]
	private UILabel _totalLabelPerSec;

	[SerializeField]
	[Range(1f, 60f)]
	private float _recodeDuration;

	private int _prevPacketSize;

	private int _prevCheckSize;

	private float _prevCheckTime;

	private void Update()
	{
		PacketWatcher packetWatcher = PacketWatcher.Instance();
		if (packetWatcher != null)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			float num = realtimeSinceStartup - _prevCheckTime;
			int num2 = packetWatcher.TotalSendSize + packetWatcher.TotalReceiveSize;
			if (num >= 1f)
			{
				float size = (float)(num2 - _prevCheckSize) / num;
				_prevCheckSize = num2;
				_prevCheckTime = realtimeSinceStartup;
				int num3 = ToReadable(ref size);
				_totalLabelPerSec.text = $"{size:0.0} {PostFix[num3]}/sec";
			}
			if (_prevPacketSize != num2)
			{
				float size2 = num2;
				int num4 = ToReadable(ref size2);
				_totalLabel.text = $"{size2:0.0} {PostFix[num4]}";
				_prevPacketSize = num2;
			}
		}
	}

	private static int ToReadable(ref float size)
	{
		int num = 0;
		while (size > 1024f)
		{
			size /= 1024f;
			num++;
		}
		return num;
	}
}
