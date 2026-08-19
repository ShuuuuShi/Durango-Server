using System;
using UnityEngine;

namespace Durango.UI;

[Serializable]
public class ItemTagControlList : ListObjectPool
{
	public void Show(bool show)
	{
		for (int i = 0; i < base.Count; i++)
		{
			base[i].SetActive(show);
		}
	}

	public Vector3 UpdateLayout(Vector3 origin, int rowCount)
	{
		UIWidget component = BaseObject.GetComponent<UIWidget>();
		int num = ((component != null) ? component.width : 0);
		int num2 = ((component != null) ? component.height : 0);
		for (int i = 0; i < base.Count; i++)
		{
			float num3 = origin.x + (float)(num * (i % rowCount));
			float num4 = origin.y - (float)(num2 * (i / rowCount));
			base[i].transform.localPosition = Vector3.up * num4 + Vector3.right * num3;
		}
		if (base.Count > 0)
		{
			int num5 = (base.Count - 1) / rowCount + 1;
			origin.y -= num2 * num5;
		}
		return origin;
	}
}
