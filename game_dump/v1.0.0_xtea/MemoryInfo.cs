using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class MemoryInfo : MonoBehaviour
{
	public struct meminf
	{
		public int memtotal;

		public int memfree;

		public int active;

		public int inactive;

		public int cached;

		public int swapcached;

		public int swaptotal;

		public int swapfree;
	}

	[SerializeField]
	private UILabel _memoryLabel;

	private float _prevUpdateTime;

	public static meminf minf;

	private static Regex re = new Regex("\\d+");

	private void Update()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (!(realtimeSinceStartup - _prevUpdateTime < 1f))
		{
			_prevUpdateTime = realtimeSinceStartup;
			float num = (float)Profiler.GetMonoHeapSize() / 1048576f;
			float num2 = (float)Profiler.GetMonoUsedSize() / 1048576f;
			float num3 = (float)Profiler.GetTotalReservedMemory() / 1048576f;
			float num4 = (float)Profiler.GetTotalUnusedReservedMemory() / 1048576f;
			StringBuilder stringBuilder = new StringBuilder(256);
			stringBuilder.AppendFormat("Memory\n");
			stringBuilder.AppendFormat("MonoHeap: {0:0.000} MB\n", num);
			stringBuilder.AppendFormat("MonoUsed: {0:0.000} MB\n", num2);
			stringBuilder.AppendFormat("Reserved: {0:0.000} MB\n", num3);
			stringBuilder.AppendFormat("Unused reserved: {0:0.000} MB\n", num4);
			GetMemInfo();
			stringBuilder.AppendFormat("Device\n");
			stringBuilder.AppendFormat("Total: {0}\nFree: {1}", minf.memtotal, minf.memfree);
			_memoryLabel.text = stringBuilder.ToString();
		}
	}

	public static bool GetMemInfo()
	{
		if (!File.Exists("/proc/meminfo"))
		{
			return false;
		}
		FileStream fileStream = new FileStream("/proc/meminfo", FileMode.Open, FileAccess.Read, FileShare.Read);
		StreamReader streamReader = new StreamReader(fileStream);
		string text;
		while ((text = streamReader.ReadLine()) != null)
		{
			text = text.ToLower().Replace(" ", string.Empty);
			if (text.Contains("memtotal"))
			{
				minf.memtotal = mVal(text);
			}
			if (text.Contains("memfree"))
			{
				minf.memfree = mVal(text);
			}
			if (text.Contains("active"))
			{
				minf.active = mVal(text);
			}
			if (text.Contains("inactive"))
			{
				minf.inactive = mVal(text);
			}
			if (text.Contains("cached") && !text.Contains("swapcached"))
			{
				minf.cached = mVal(text);
			}
			if (text.Contains("swapcached"))
			{
				minf.swapcached = mVal(text);
			}
			if (text.Contains("swaptotal"))
			{
				minf.swaptotal = mVal(text);
			}
			if (text.Contains("swapfree"))
			{
				minf.swapfree = mVal(text);
			}
		}
		streamReader.Close();
		fileStream.Close();
		fileStream.Dispose();
		return true;
	}

	private static int mVal(string s)
	{
		Match match = re.Match(s);
		return int.Parse(match.Value);
	}
}
