using System.Collections.Generic;
using Durango.Logic.Item;
using Messages;
using UnityEngine;

namespace Durango.UI.Popup;

public class ItemInfoTooltip : TooltipBase
{
	[SerializeField]
	private ItemInfoContainer _itemInfo;

	private KeyValuePair<string, int>? _prototype;

	private ItemData _item;

	private Pet? _pet;

	public static int Width { get; private set; }

	public static int Height { get; private set; }

	protected override void OnAwake()
	{
		SoundType = UISound.GroupType.NoSound;
		Width = base.Widget.width;
		Height = base.Widget.height;
	}

	public void Set(ItemData item)
	{
		_item = item;
		_pet = null;
		_prototype = null;
	}

	public void Set(Pet pet)
	{
		_item = null;
		_pet = pet;
		_prototype = null;
	}

	public void Set(string prototypeId, int level)
	{
		_item = null;
		_pet = null;
		_prototype = new KeyValuePair<string, int>(prototypeId, level);
	}

	protected override void FillData()
	{
		if (_item != null)
		{
			_itemInfo.Show(_item);
		}
		else if (_pet.HasValue)
		{
			_itemInfo.Show(_pet.Value);
		}
		else if (_prototype.HasValue)
		{
			_itemInfo.Show(_prototype.Value.Key, _prototype.Value.Value);
		}
	}

	protected override void UpdateLayout()
	{
	}

	protected override void OnHide()
	{
		base.OnHide();
		base.Widget.SetDimensions(Width, Height);
	}
}
