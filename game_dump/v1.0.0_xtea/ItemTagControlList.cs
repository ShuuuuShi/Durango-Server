using System;
using UnityEngine;

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
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		UIWidget component = BaseObject.GetComponent<UIWidget>();
		int num = (((Object)(object)component != (Object)null) ? component.width : 0);
		int num2 = (((Object)(object)component != (Object)null) ? component.height : 0);
		for (int i = 0; i < base.Count; i++)
		{
			float num3 = origin.x + (float)(num * (i % rowCount));
			float num4 = origin.y - (float)(num2 * (i / rowCount));
			base[i].transform.localPosition = Vector3.up * num4 + Vector3.right * num3;
		}
		if (base.Count > 0)
		{
			int num5 = (base.Count - 1) / rowCount + 1;
			origin.y -= (float)(num2 * num5);
		}
		return origin;
	}
}
