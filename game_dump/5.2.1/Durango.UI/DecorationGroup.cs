using System.Collections.Generic;
using Durango.Render.Camera;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class DecorationGroup : UIBase
{
	public struct Option
	{
		public Vector3 UIOffset;

		public Vector3 WorldOffset;
	}

	private struct Item
	{
		public Transform Parent;

		public Transform Decoration;

		public Option Option;
	}

	private List<Item> _items = new List<Item>();

	public GameObject Register([NotNull] Transform parent, [NotNull] GameObject decoPrefab, Option option)
	{
		Item item = default(Item);
		item.Parent = parent;
		item.Option = option;
		Item item2 = item;
		GameObject gameObject = base.gameObject.AddChild(decoPrefab);
		item2.Decoration = gameObject.transform;
		_items.Add(item2);
		base.enabled = true;
		return gameObject;
	}

	public void Stop(GameObject deco)
	{
		if (deco == null)
		{
			return;
		}
		Transform transform = deco.transform;
		for (int i = 0; i < _items.Count; i++)
		{
			if (_items[i].Decoration == transform)
			{
				Dispose(_items[i]);
				_items.RemoveAt(i);
				break;
			}
		}
	}

	private void Dispose(Item item)
	{
		Object.Destroy(item.Decoration.gameObject);
	}

	private void LateUpdate()
	{
		for (int i = 0; i < _items.Count; i++)
		{
			Item item = _items[i];
			Transform parent = item.Parent;
			if (parent == null || !parent.gameObject.activeSelf)
			{
				Dispose(_items[i]);
				_items.RemoveAt(i);
				i--;
			}
			else
			{
				Transform decoration = item.Decoration;
				Option option = item.Option;
				Vector3 localPosition = MainCamera.WorldToNGUIPos(parent.transform.position + option.WorldOffset);
				localPosition += option.UIOffset;
				decoration.transform.localPosition = localPosition;
			}
		}
		if (_items.Count == 0)
		{
			base.enabled = false;
		}
	}
}
