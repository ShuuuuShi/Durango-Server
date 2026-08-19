using System.Collections.Generic;
using Durango.Utils;
using UnityEngine;

public class UIRootAnchor : Singleton<UIRootAnchor>
{
	private class Anchor
	{
		public readonly UIWidget Widget;

		private readonly int[] _values = new int[4];

		private readonly Dictionary<string, int>[] _padding = new Dictionary<string, int>[4];

		public Anchor(UIWidget widget)
		{
			Widget = widget;
		}

		public void Reset(int left, int bottom, int right, int top)
		{
			_values[0] = left;
			_values[1] = bottom;
			_values[2] = right;
			_values[3] = top;
			Widget.leftAnchor.absolute = GetAnchorValue(0);
			Widget.bottomAnchor.absolute = GetAnchorValue(1);
			Widget.rightAnchor.absolute = -GetAnchorValue(2);
			Widget.topAnchor.absolute = -GetAnchorValue(3);
		}

		public bool SetPadding(int index, string key, int? value)
		{
			if (!value.HasValue)
			{
				return ResetPadding(index, key);
			}
			Dictionary<string, int> dictionary = _padding[index];
			int anchorValue = GetAnchorValue(index);
			if (dictionary == null)
			{
				dictionary = new Dictionary<string, int>();
				_padding[index] = dictionary;
			}
			dictionary[key] = value.Value;
			int anchorValue2 = GetAnchorValue(index);
			if (anchorValue == anchorValue2)
			{
				return false;
			}
			UpdateAnchor(index, anchorValue2);
			return true;
		}

		private bool ResetPadding(int index, string key)
		{
			Dictionary<string, int> dictionary = _padding[index];
			if (dictionary == null)
			{
				return false;
			}
			int anchorValue = GetAnchorValue(index);
			dictionary.Remove(key);
			int anchorValue2 = GetAnchorValue(index);
			if (anchorValue == anchorValue2)
			{
				return false;
			}
			UpdateAnchor(index, anchorValue2);
			return true;
		}

		private int GetAnchorValue(int index)
		{
			Dictionary<string, int> dictionary = _padding[index];
			int num = _values[index];
			if (dictionary != null && dictionary.Count > 0)
			{
				int num2 = int.MinValue;
				foreach (KeyValuePair<string, int> item in dictionary)
				{
					num2 = Mathf.Max(item.Value, num2);
				}
				num += num2;
			}
			return num;
		}

		private void UpdateAnchor(int index, int value)
		{
			switch (index)
			{
			case 0:
				Widget.leftAnchor.absolute = value;
				break;
			case 1:
				Widget.bottomAnchor.absolute = value;
				break;
			case 2:
				Widget.rightAnchor.absolute = -value;
				break;
			case 3:
				Widget.topAnchor.absolute = -value;
				break;
			}
		}
	}

	private readonly Dictionary<int, Anchor> _anchors = new Dictionary<int, Anchor>();

	protected override void OnAwake()
	{
		base.hideFlags = HideFlags.HideAndDontSave;
	}

	private Anchor GetAnchor(UIBase.AnchorType type)
	{
		Dictionary<int, Anchor> anchors = _anchors;
		if (anchors.TryGetValue((int)type, out var value))
		{
			return value;
		}
		UIRoot uIRoot = Singleton<UIManager>.Instance().UIRoot;
		UIWidget uIWidget = uIRoot.gameObject.AddChild<UIWidget>();
		uIWidget.name = $"Anchor.{type}";
		value = new Anchor(uIWidget);
		uIWidget.SetAnchor(uIRoot.gameObject);
		anchors[(int)type] = value;
		return value;
	}

	public static void UpdateAndResetRootAnchors()
	{
		UIRootAnchor uIRootAnchor = Singleton<UIRootAnchor>.Instance();
		if (uIRootAnchor == null)
		{
			return;
		}
		foreach (KeyValuePair<int, Anchor> anchor in uIRootAnchor._anchors)
		{
			if ((bool)anchor.Value.Widget)
			{
				anchor.Value.Widget.ResetAndUpdateAnchors();
			}
		}
	}

	public static UIWidget GetRootAnchor(UIBase.AnchorType type)
	{
		UIRootAnchor uIRootAnchor = Singleton<UIRootAnchor>.Instance();
		if (uIRootAnchor == null)
		{
			return null;
		}
		return uIRootAnchor.GetAnchor(type)?.Widget;
	}

	public static void Reset(UIBase.AnchorType type, int left, int bottom, int right, int top)
	{
		UIRootAnchor uIRootAnchor = Singleton<UIRootAnchor>.Instance();
		if (!(uIRootAnchor == null))
		{
			uIRootAnchor.ResetAnchor(type, left, bottom, right, top);
		}
	}

	private void ResetAnchor(UIBase.AnchorType type, int left, int bottom, int right, int top)
	{
		GetAnchor(type)?.Reset(left, bottom, right, top);
	}

	public static void Set(string key, UIBase.AnchorType type, int? left, int? bottom, int? right, int? top)
	{
		UIRootAnchor uIRootAnchor = Singleton<UIRootAnchor>.Instance();
		if (!(uIRootAnchor == null))
		{
			uIRootAnchor.SetAnchor(key, type, left, bottom, right, top);
		}
	}

	private void SetAnchor(string key, UIBase.AnchorType type, int? left, int? bottom, int? right, int? top)
	{
		Anchor anchor = GetAnchor(type);
		if (anchor != null)
		{
			bool flag = false;
			flag |= anchor.SetPadding(0, key, left);
			flag |= anchor.SetPadding(1, key, bottom);
			flag |= anchor.SetPadding(2, key, right);
			if (flag | anchor.SetPadding(3, key, top))
			{
				UpdateAnchorType(type);
			}
		}
	}

	private void UpdateAnchorType(UIBase.AnchorType type)
	{
		Anchor anchor = GetAnchor(type);
		if (anchor == null)
		{
			return;
		}
		anchor.Widget.UpdateAnchors();
		Transform transform = Singleton<UIManager>.Instance().UIRoot.transform;
		int i = 0;
		for (int childCount = transform.childCount; i < childCount; i++)
		{
			UIBase component = transform.GetChild(i).GetComponent<UIBase>();
			if (!(component == null) && component.Anchor == type)
			{
				UIUtility.UpdateAnchors(component.transform);
			}
		}
	}
}
