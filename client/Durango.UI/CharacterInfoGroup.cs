using System;
using System.Collections.Generic;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Shared.Ability;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

[Uri("Character")]
public class CharacterInfoGroup : UIBase
{
	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private CharacterInfoWidget _info;

	[SerializeField]
	private EquipWidgetBase _equip;

	public EquipWidgetBase Equip => _equip;

	private void Awake()
	{
		_openCloseSound = UISound.GroupType.PopUp;
		SetChildrenActive(activated: false);
	}

	private void Start()
	{
		_titleWidget.Object.SetTitle(T._("캐릭터"));
	}

	[Uri("RepresentType")]
	private void RepresentTypePopupUri(string value)
	{
		if (value.TryEnum<RepresentType>(out var value2))
		{
			ShowRepresentTypePopup(value2);
		}
	}

	[Uri("RepresentType/Derived")]
	private void RepresentTypePopupByDerivedUri(string value)
	{
		if (value.TryEnum<Derived>(out var value2))
		{
			ShowRepresentTypePopup(value2);
		}
	}

	public static void ShowRepresentTypePopup(RepresentType type)
	{
		UIManager.Open<CharacterInfoGroup>();
		RepresentTypePopup representTypePopup = UIManager.Popup.Tooltip<RepresentTypePopup>();
		representTypePopup.Set(type);
		representTypePopup.Show();
	}

	public static void ShowRepresentTypePopup(Derived derived)
	{
		RepresentTypePopup representTypePopup = UIManager.Popup.Tooltip<RepresentTypePopup>();
		if (representTypePopup.Derived(derived))
		{
			UIManager.Open<CharacterInfoGroup>();
			representTypePopup.Show();
		}
	}

	public static void ShowResistanceInfoPopup()
	{
		UIManager.Open<CharacterInfoGroup>();
		UIManager.Popup.Tooltip<ResistanceInfoPopup>().Show();
	}

	public static void ShowTitleSelector([NotNull] Action<string> onConfirmed)
	{
		CharacterTitleSelector characterTitleSelector = UIManager.Popup.Tooltip<CharacterTitleSelector>();
		characterTitleSelector.Set(PlayerBehavior.LocalPlayer.Title.TitleId, onConfirmed);
		characterTitleSelector.Show();
	}

	public static void SetHonorFlagSelector([NotNull] Action<string> onSelected)
	{
		GenericSelector genericSelector = UIManager.Popup.Tooltip<GenericSelector>();
		genericSelector.ResetArguments();
		int num = -1;
		List<string> ids = new List<string>();
		genericSelector.AddItem(string.Format("{0}\n[size=4] [/size]\n[size=20][777163]{1}", T._("깃발 없음"), T._("거점을 점령해 깃발을 쟁취해 보세요!")));
		ids.Add(null);
		IEnumerable<string> attachableAccessories = GameSystem<EquipSystem>.Instance().AttachableAccessories;
		int num2 = 0;
		string accessory = PlayerBehavior.LocalPlayer.Display.Accessory;
		foreach (string item in attachableAccessories)
		{
			Accessory accessory2 = ((!string.IsNullOrEmpty(item)) ? SingletonDict<string, Accessory>.Get(item) : null);
			if (num == -1 && item == accessory)
			{
				num = num2 + 1;
			}
			ids.Add(item);
			string text = ((!string.IsNullOrEmpty(accessory2.Description)) ? $"{accessory2.Name}\n[size=4] [/size]\n[size=20][777163][icon=icon_map_chat] {accessory2.Description}[-][/size]" : accessory2.Name.ToString());
			genericSelector.AddItem(text);
			num2++;
		}
		if (num == -1)
		{
			num = 0;
		}
		genericSelector.DefaultSelectedIndex(num);
		genericSelector.SetSelected(delegate(int idx)
		{
			if (idx < 0 || idx >= ids.Count)
			{
				onSelected(null);
			}
			else
			{
				onSelected(ids[idx]);
			}
		});
		genericSelector.BlurOn();
		genericSelector.Show();
	}
}
