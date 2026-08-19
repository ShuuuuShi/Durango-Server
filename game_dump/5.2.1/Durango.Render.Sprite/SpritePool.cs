using System.Collections.Generic;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Render.Sprite;

public class SpritePool : MonoBehaviour
{
	private readonly List<Sprite> _list = new List<Sprite>();

	private bool _selectable;

	[ExposedInEditor(null)]
	private int _count;

	public void Init(bool selectable)
	{
		_selectable = selectable;
	}

	public void ResetSprites()
	{
		int i = 0;
		for (int count = _list.Count; i < count; i++)
		{
			Sprite sprite = _list[i];
			if (Application.isEditor && sprite.GameObject == null)
			{
				sprite = CreateSprite();
				_list[i] = sprite;
			}
			sprite.GameObject.SetActive(value: false);
		}
		_count = 0;
	}

	[NotNull]
	public Sprite Alloc()
	{
		if (_count >= _list.Count)
		{
			_list.Add(CreateSprite());
		}
		return _list[_count++];
	}

	public void Release([NotNull] Sprite sprite)
	{
		int num = _list.IndexOf(sprite);
		if (num != -1)
		{
			int index = _count - 1;
			Sprite value = _list[num];
			_list[num] = _list[index];
			_list[index] = value;
			if ((bool)sprite.GameObject)
			{
				sprite.GameObject.SetActive(value: false);
			}
			_count--;
		}
	}

	private Sprite CreateSprite()
	{
		return Singleton<SpriteManager>.Instance().CreateSprite(base.transform, _selectable);
	}

	public void CheckLoaded()
	{
		for (int i = 0; i < _count; i++)
		{
			if (!(_list[i].GameObject == null))
			{
				_list[i].CheckLoaded();
			}
		}
	}
}
