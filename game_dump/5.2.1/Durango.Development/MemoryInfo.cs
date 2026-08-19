using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace Durango.Development;

public class MemoryInfo : MonoBehaviour
{
	[SerializeField]
	private UILabel _memoryLabel;

	private float _prevUpdateTime;

	private void Update()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (!(realtimeSinceStartup - _prevUpdateTime < 1f))
		{
			_prevUpdateTime = realtimeSinceStartup;
			double num = (double)Profiler.GetMonoHeapSizeLong() / 1048576.0;
			double num2 = (double)Profiler.GetMonoUsedSizeLong() / 1048576.0;
			double num3 = (double)Profiler.GetTotalReservedMemoryLong() / 1048576.0;
			double num4 = (double)Profiler.GetTotalUnusedReservedMemoryLong() / 1048576.0;
			StringBuilder stringBuilder = new StringBuilder(256);
			stringBuilder.AppendFormat("Memory\n");
			stringBuilder.AppendFormat("MonoHeap: {0:0.000} MB\n", num);
			stringBuilder.AppendFormat("MonoUsed: {0:0.000} MB\n", num2);
			stringBuilder.AppendFormat("Reserved: {0:0.000} MB\n", num3);
			stringBuilder.AppendFormat("Unused reserved: {0:0.000} MB\n", num4);
			_memoryLabel.text = stringBuilder.ToString();
		}
	}
}
