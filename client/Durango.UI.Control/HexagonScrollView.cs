using UnityEngine;

namespace Durango.UI.Control;

public class HexagonScrollView : NodesScrollView
{
	protected override float OnUpdateLayout(bool instant)
	{
		Vector2 localSize = base.Nodes.BaseObject.GetComponent<UIWidget>().localSize;
		float num = (float)base.Margin * 2f / Mathf.Sqrt(3f);
		Direction dir = base.Dir;
		Vector2 viewSize = base.ViewSize;
		float num2 = ((dir != Direction.Horizontal) ? viewSize.x : viewSize.y);
		float num3 = ((dir != Direction.Horizontal) ? localSize.x : localSize.y);
		int num4 = Mathf.Max(1, Mathf.RoundToInt((num2 - num3 / 4f - num) / (num3 * 3f / 4f + num)));
		float num5 = (float)num4 * (num3 * 3f / 4f + num) + num3 / 4f - num;
		Vector3 basePosition = GetBasePosition();
		if (dir == Direction.Horizontal)
		{
			basePosition.y += viewSize.y * 0.5f;
			basePosition.y -= localSize.y * 0.5f;
			basePosition.y -= (num2 - num5) * 0.5f;
			basePosition.x += base.Vector.x * localSize.x * 0.5f;
		}
		else
		{
			basePosition.x -= viewSize.x * 0.5f;
			basePosition.x += localSize.x * 0.5f;
			basePosition.x += (num2 - num5) * 0.5f;
			basePosition.y += base.Vector.y * localSize.y * 0.5f;
		}
		int num6 = 0;
		for (int i = 0; i < GetNodeCount(); i++)
		{
			UIWidget node = GetNode(i);
			if (!UIUtility.IsVisibleWidget(node))
			{
				continue;
			}
			Vector3 pos = basePosition;
			int num7 = num6 % num4;
			int num8 = num6 / num4;
			if (dir == Direction.Horizontal)
			{
				pos.y += (float)num7 * (localSize.y * 3f / 4f + num);
				pos.x += (float)num8 * (localSize.x + (float)base.Margin);
				if (num7 % 2 != 0)
				{
					pos.x += base.Vector.x * localSize.y / 4f * Mathf.Sqrt(3f);
				}
			}
			else
			{
				pos.x += (float)num7 * (localSize.x * 3f / 4f + num);
				pos.y -= (float)num8 * (localSize.y + (float)base.Margin);
				if (num7 % 2 != 0)
				{
					pos.y += base.Vector.y * localSize.x / 4f * Mathf.Sqrt(3f);
				}
			}
			node.SetPosition(pos, 0.5f, 0.5f);
			num6++;
		}
		int num9 = Mathf.CeilToInt((float)num6 / (float)num4);
		return num3 / 4f * Mathf.Sqrt(3f) * (float)(num9 * 2 + 1) + (float)(base.Margin * (num9 - 1));
	}
}
