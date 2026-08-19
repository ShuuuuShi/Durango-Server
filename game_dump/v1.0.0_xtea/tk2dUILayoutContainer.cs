using System;
using UnityEngine;

public abstract class tk2dUILayoutContainer : tk2dUILayout
{
	protected Vector2 innerSize = Vector2.zero;

	public event Action OnChangeContent;

	public Vector2 GetInnerSize()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return innerSize;
	}

	protected abstract void DoChildLayout();

	public override void Reshape(Vector3 dMin, Vector3 dMax, bool updateChildren)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		bMin += dMin;
		bMax += dMax;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(bMin.x, bMax.y);
		Transform transform = ((Component)this).transform;
		transform.position += val;
		bMin -= val;
		bMax -= val;
		DoChildLayout();
		if (this.OnChangeContent != null)
		{
			this.OnChangeContent();
		}
	}

	public void AddLayout(tk2dUILayout layout, tk2dUILayoutItem item)
	{
		item.gameObj = ((Component)layout).gameObject;
		item.layout = layout;
		layoutItems.Add(item);
		((Component)layout).gameObject.transform.parent = ((Component)this).transform;
		Refresh();
	}

	public void AddLayoutAtIndex(tk2dUILayout layout, tk2dUILayoutItem item, int index)
	{
		item.gameObj = ((Component)layout).gameObject;
		item.layout = layout;
		layoutItems.Insert(index, item);
		((Component)layout).gameObject.transform.parent = ((Component)this).transform;
		Refresh();
	}

	public void RemoveLayout(tk2dUILayout layout)
	{
		foreach (tk2dUILayoutItem layoutItem in layoutItems)
		{
			if ((Object)(object)layoutItem.layout == (Object)(object)layout)
			{
				layoutItems.Remove(layoutItem);
				((Component)layout).gameObject.transform.parent = null;
				break;
			}
		}
		Refresh();
	}
}
