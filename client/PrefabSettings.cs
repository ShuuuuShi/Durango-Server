using Durango.Utils;
using UnityEngine;

public class PrefabSettings : MonoBehaviour
{
	[SerializeField]
	public int RandomYaw;

	[SerializeField]
	public float MinHeight;

	[SerializeField]
	public float MaxHeight;

	[SerializeField]
	public float MinSizeRatio = 1f;

	[SerializeField]
	public float MaxSizeRatio = 1f;

	[SerializeField]
	private string[] _colorTables;

	public bool HasRandomColor()
	{
		return KUtility.GetSize(_colorTables) > 0;
	}

	public ThreeColor GetRandomColor(float ratio)
	{
		ThreeColor result = default(ThreeColor);
		int size = KUtility.GetSize(_colorTables);
		for (int i = 0; i < 3; i++)
		{
			int num = i % size;
			result[i] = ColorTableLoader.Load(_colorTables[num])?.GetColor(ratio) ?? Color.gray;
			ratio *= (float)i + 1.5f + ratio;
		}
		return result;
	}
}
