using System;
using UnityEngine;

[Serializable]
public class WidgetLayout
{
	[Serializable]
	private struct Item
	{
		public bool Group;

		public UIWidget Widget;

		public Vector2 Size;

		public Vector2 Margin;

		public bool DontResize;
	}

	private enum Direction
	{
		Horizontal,
		Vertical
	}

	private enum Horizontal
	{
		Left,
		Right
	}

	private enum Vertical
	{
		Top,
		Bottom
	}

	[SerializeField]
	private Direction _direction;

	[SerializeField]
	private Horizontal _horizontal;

	[SerializeField]
	private Vertical _vertical;

	[SerializeField]
	private Item[] _items;

	public bool HasItems()
	{
		return _items.Length > 0;
	}

	public void UpdateLayout(UIWidget parent)
	{
		UpdateLayout(parent, new Point2(parent.width, parent.height));
	}

	public void UpdateLayout(UIWidget parent, Point2 size)
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0743: Unknown result type (might be due to invalid IL or missing references)
		//IL_0744: Unknown result type (might be due to invalid IL or missing references)
		//IL_0749: Unknown result type (might be due to invalid IL or missing references)
		//IL_077e: Unknown result type (might be due to invalid IL or missing references)
		//IL_077f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0784: Unknown result type (might be due to invalid IL or missing references)
		//IL_0786: Unknown result type (might be due to invalid IL or missing references)
		//IL_0788: Unknown result type (might be due to invalid IL or missing references)
		//IL_078a: Unknown result type (might be due to invalid IL or missing references)
		//IL_078f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0791: Unknown result type (might be due to invalid IL or missing references)
		//IL_0793: Unknown result type (might be due to invalid IL or missing references)
		//IL_06dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_071e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0723: Unknown result type (might be due to invalid IL or missing references)
		//IL_0728: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_052d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0550: Unknown result type (might be due to invalid IL or missing references)
		//IL_055c: Unknown result type (might be due to invalid IL or missing references)
		//IL_053a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0546: Unknown result type (might be due to invalid IL or missing references)
		//IL_0564: Unknown result type (might be due to invalid IL or missing references)
		//IL_0569: Unknown result type (might be due to invalid IL or missing references)
		//IL_056e: Unknown result type (might be due to invalid IL or missing references)
		Point2 zero = Point2.zero;
		if (size.x > 0)
		{
			parent.width = size.x;
		}
		if (size.y > 0)
		{
			parent.height = size.y;
		}
		int num = ((_horizontal != 0) ? ((_vertical != Vertical.Bottom) ? 2 : 3) : ((_vertical != Vertical.Bottom) ? 1 : 0));
		Vector2 val = default(Vector2);
		val.x = ((_horizontal == Horizontal.Left) ? 1 : (-1));
		val.y = ((_vertical == Vertical.Bottom) ? 1 : (-1));
		Vector2 pivot = default(Vector2);
		pivot.x = ((_horizontal != 0) ? 1 : 0);
		pivot.y = ((_vertical != Vertical.Bottom) ? 1 : 0);
		Vector3 val2 = parent.localCorners[num];
		int num2 = 0;
		int num3 = 0;
		Point2 point = default(Point2);
		while (num3 < _items.Length)
		{
			bool group = _items[num3].Group;
			int num4 = 0;
			int num5 = 0;
			int num6 = _items.Length;
			int num7 = 0;
			float num8 = 0f;
			for (int i = num3; i < _items.Length; i++)
			{
				Item item = _items[i];
				int num9 = ((_direction != 0) ? size.y : size.x);
				if (item.Group != group || (num9 > 0 && num4 > num9))
				{
					num6 = i;
					break;
				}
				if (!((Behaviour)item.Widget).enabled || !((Component)item.Widget).gameObject.activeSelf)
				{
					continue;
				}
				if (num2 > 0)
				{
					if (_direction == Direction.Horizontal)
					{
						zero.y += num2;
						val2 += Vector3.up * (float)num2 * val.y;
					}
					else
					{
						zero.x += num2;
						val2 += Vector3.right * (float)num2 * val.x;
					}
					num2 = 0;
				}
				if (num7 > 0)
				{
					num4 += num7;
					num7 = 0;
				}
				if (_direction == Direction.Horizontal)
				{
					point.x = parent.width - num4;
					point.y = parent.height - zero.y;
				}
				else
				{
					point.x = parent.width - zero.x;
					point.y = parent.height - num4;
				}
				Point2 zero2 = Point2.zero;
				zero2.x = ((item.Size.x != 0f) ? ((!(Mathf.Abs(item.Size.x) > 1f)) ? Mathf.RoundToInt(item.Size.x * (float)point.x) : ((int)item.Size.x)) : 0);
				zero2.y = ((item.Size.y != 0f) ? ((!(Mathf.Abs(item.Size.y) > 1f)) ? Mathf.RoundToInt(item.Size.y * (float)point.y) : ((int)item.Size.y)) : 0);
				if (zero2.x < 0)
				{
					zero2.x = point.x + zero2.x;
				}
				if (zero2.y < 0)
				{
					zero2.y = point.y + zero2.y;
				}
				if (!item.DontResize)
				{
					Point2 point2 = new Point2(item.Widget.width, item.Widget.height);
					if (zero2.x > 0)
					{
						item.Widget.width = zero2.x;
					}
					if (zero2.y > 0)
					{
						item.Widget.height = zero2.y;
					}
					if (item.Widget.width != point2.x || (item.Widget.height != point2.y && Application.isPlaying))
					{
						UIUtility.UpdateAnchors(((Component)item.Widget).transform);
					}
				}
				WidgetLayoutController component = ((Component)item.Widget).GetComponent<WidgetLayoutController>();
				if ((Object)(object)component != (Object)null)
				{
					component.UpdateLayout(zero2.x, zero2.y);
				}
				if (Application.isPlaying)
				{
					((Component)item.Widget).SendMessage("OnLayout", (object)zero2, (SendMessageOptions)1);
				}
				zero2.x = ((zero2.x <= 0) ? item.Widget.width : zero2.x);
				zero2.y = ((zero2.y <= 0) ? item.Widget.height : zero2.y);
				item.Widget.SetPosition(val2 + ((_direction != 0) ? (Vector3.up * val.y) : (Vector3.right * val.x)) * (float)num4, pivot);
				num4 += ((_direction != 0) ? zero2.y : zero2.x);
				int num10 = ((_direction != 0) ? size.y : size.x);
				if (num10 > 0 && i > num3 && num4 > num10)
				{
					num6 = i;
					break;
				}
				num5 = Mathf.Max(num5, (_direction != 0) ? zero2.x : zero2.y);
				num8 = Mathf.Max(num8, (_direction != 0) ? item.Margin.x : item.Margin.y);
				float num11 = ((_direction != 0) ? item.Margin.y : item.Margin.x);
				if (num11 > 0f)
				{
					num7 = (int)num11;
				}
			}
			if (_direction == Direction.Horizontal)
			{
				zero.x = Mathf.Max(zero.x, num4);
				zero.y += num5;
			}
			else
			{
				zero.x += num5;
				zero.y = Mathf.Max(zero.y, num4);
			}
			val2 += ((_direction != 0) ? Vector3.right : Vector3.up) * (float)num5 * ((_direction != 0) ? val.x : val.y);
			num2 = (int)num8;
			num3 = num6;
		}
		Vector3 position = parent.GetPosition(pivot);
		if (size.x == 0)
		{
			parent.width = zero.x;
		}
		if (size.y == 0)
		{
			parent.height = zero.y;
		}
		Vector3 position2 = parent.GetPosition(pivot);
		Vector3 val3 = position2 - position;
		if (val3 != Vector3.zero)
		{
			for (int j = 0; j < _items.Length; j++)
			{
				Transform transform = ((Component)_items[j].Widget).transform;
				transform.localPosition += val3;
			}
		}
	}
}
