using System.Linq;
using System.Text;
using Building;
using Durango.Logic.Item;
using Durango.Logic.Social;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Newtonsoft.Json;
using Shared.Economy;
using UnityEngine;
using Yaml.Util;

namespace Yaml;

public class ContentDescription
{
	[JsonProperty(PropertyName = "source_key")]
	public string SourceKey;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;

	[JsonProperty(PropertyName = "only_popup")]
	public bool OnlyPopup;

	private bool _isLoaded = true;

	private bool _isLoading;

	private ObjectReferenceText _iconDescription;

	private ObjectReferenceText _name;

	private ObjectReferenceText _text;

	[CanBeNull]
	private ItemContent _item;

	[JsonProperty(PropertyName = "icon_description")]
	public string IconDescription
	{
		get
		{
			if (_iconDescription == null)
			{
				return null;
			}
			return _iconDescription.ToString();
		}
		set
		{
			_iconDescription = ((!string.IsNullOrEmpty(value)) ? new ObjectReferenceText(value) : null);
		}
	}

	[JsonProperty(PropertyName = "name")]
	public Gettext Name
	{
		get
		{
			return (_name != null) ? _name.ToString() : null;
		}
		set
		{
			_name = ((!string.IsNullOrEmpty(value)) ? new ObjectReferenceText(value) : null);
		}
	}

	[JsonProperty(PropertyName = "text")]
	public Gettext Text
	{
		get
		{
			return (_text != null) ? _text.ToString() : null;
		}
		set
		{
			_text = ((!string.IsNullOrEmpty(value)) ? new ObjectReferenceText(value) : null);
		}
	}

	[JsonProperty(PropertyName = "icon_colors")]
	public string[] IconColors
	{
		set
		{
			IconColor = ((KUtility.GetSize(value) <= 0) ? default(ItemColor) : new ItemColor(value));
		}
	}

	public ItemData Item { get; private set; }

	public string Motion { get; private set; }

	public ItemColor IconColor { get; private set; }

	public bool IsLoaded => _isLoaded;

	private void SetParent(object parent)
	{
		if (_text != null)
		{
			_text.SetParent(parent);
		}
		if (_iconDescription != null)
		{
			_iconDescription.SetParent(parent);
		}
		if (_name != null)
		{
			_name.SetParent(parent);
		}
	}

	public void FillDefaultData(CommodityContent content)
	{
		if (content is ItemContent item)
		{
			FillDefaultData(item);
		}
		else if (content is MoneyContent money)
		{
			FillDefaultData(money);
		}
		else if (content is StatusEffectsContent statusEffect)
		{
			FillDefaultData(statusEffect);
		}
		else if (content is ModularArtifactContent modular)
		{
			FillDefaultData(modular);
		}
		else if (content is VoucherContent voucher)
		{
			FillDefaultData(voucher);
		}
	}

	private void FillDefaultData(ItemContent item)
	{
		_isLoaded = false;
		_item = item;
		if (!IconColor.HasValue)
		{
			IconColors = item.colors;
		}
		Prototype itemPrototype = PrototypeYaml.GetItemPrototype(item.prototype_id, item.level);
		if (itemPrototype != null)
		{
			if (string.IsNullOrEmpty(Icon))
			{
				Icon = itemPrototype.Icon;
			}
			if (!IconColor.HasValue)
			{
				Color[] array = new Color[3];
				int num = 0;
				if (ItemIconTex.TryGetDefaultColor(itemPrototype.ColorR, out array[0]))
				{
					num++;
				}
				if (ItemIconTex.TryGetDefaultColor(itemPrototype.ColorG, out array[1]))
				{
					num++;
				}
				if (ItemIconTex.TryGetDefaultColor(itemPrototype.ColorB, out array[2]))
				{
					num++;
				}
				ItemColor iconColor = new ItemColor(num);
				for (int i = 0; i < num; i++)
				{
					iconColor.SetColor(i, array[i]);
				}
				IconColor = iconColor;
			}
			if (_iconDescription == null && item.count > 1)
			{
				_iconDescription = new ObjectReferenceText(item.count.ToString("N0", T.Culture));
			}
		}
		SetParent(item);
	}

	private void FillDefaultData(MoneyContent money)
	{
		if (string.IsNullOrEmpty(Icon))
		{
			Icon = Inventory.GetIcon(money.currency);
		}
		if (_iconDescription == null)
		{
			_iconDescription = new ObjectReferenceText(money.amount.ToString("N0", T.Culture));
		}
		if (_name == null)
		{
			string text = null;
			text = ((money.currency != Currency.RPiece) ? string.Format(T.Culture, "{0:N0} {1}", money.amount, money.currency.GetName()) : T._("{0} {1:N0}개", money.currency.GetName(), money.amount));
			if (!string.IsNullOrEmpty(text))
			{
				_name = new ObjectReferenceText(text);
			}
		}
		SetParent(money);
	}

	private void FillDefaultData(StatusEffectsContent statusEffect)
	{
		StatusEffectTemplate statusEffectTemplate = SingletonDict<string, StatusEffectTemplate[]>.Get(statusEffect.status_effects_id)?.FirstOrDefault();
		if (statusEffectTemplate != null)
		{
			if (string.IsNullOrEmpty(Icon))
			{
				Icon = statusEffectTemplate.Icon;
			}
			if (_name == null)
			{
				_name = new ObjectReferenceText(T._("{0} 일 동안 {1} 효과", statusEffect.duration_days, statusEffectTemplate.Name));
			}
		}
		SetParent(statusEffect);
	}

	private void FillDefaultData(ModularArtifactContent modular)
	{
		Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(modular.artifact_id);
		if (blueprint != null)
		{
			if (_name == null)
			{
				_name = new ObjectReferenceText($"{blueprint.Name}  {modular.size_x} × {modular.size_y}");
			}
			if (string.IsNullOrEmpty(Icon))
			{
				Icon = blueprint.Icon;
			}
		}
		SetParent(modular);
	}

	private void FillDefaultData(VoucherContent voucher)
	{
		if (SingletonDict<string, Voucher>.TryGetValue(voucher.voucher_id, out var value))
		{
			if (_name == null)
			{
				_name = new ObjectReferenceText(value.Name);
			}
			if (_text == null)
			{
				_text = new ObjectReferenceText(value.Description);
			}
			if (string.IsNullOrEmpty(Icon))
			{
				Icon = value.Icon;
			}
		}
		SetParent(voucher);
	}

	public void FillDefaultData(string motion)
	{
		Motion = motion;
		Durango.Logic.Social.Motion motion2 = GameSystem<SocialSystem>.Instance().Emotional.GetMotion(motion);
		if (motion2 != null)
		{
			if (string.IsNullOrEmpty(Icon))
			{
				Icon = "icon_emotionbook";
			}
			if (_name == null)
			{
				_name = new ObjectReferenceText(motion2.Name);
			}
		}
		SetParent(motion);
	}

	public void Load()
	{
		if (!_isLoaded && !_isLoading)
		{
			LoadingItem();
		}
	}

	private void LoadingItem()
	{
		if (_item == null)
		{
			return;
		}
		_isLoading = true;
		PrototypePreset.Request(_item.prototype_id, _item.level, delegate(PrototypePreset preset)
		{
			_isLoading = false;
			_isLoaded = true;
			if (preset == null)
			{
				Item = null;
			}
			else
			{
				Item = preset.ToItem();
				Item.Icon = new ItemIcon
				{
					Main = Icon,
					Colors = IconColor
				};
				OnLoadedItem();
			}
		});
	}

	private void OnLoadedItem()
	{
		if (Item == null || _item == null)
		{
			return;
		}
		if (_name == null)
		{
			_name = ((_item.count <= 1) ? new ObjectReferenceText(Item.Name) : new ObjectReferenceText(T._("{0} x{1}", Item.Name, _item.count)));
		}
		if (_text != null)
		{
			return;
		}
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		if (ShopCategories.IsShowTradeLock(Item))
		{
			value.Append(string.Format(" [preset=rect_box?text=[icon=img_notool] {0},color=724835]", T._("거래")));
		}
		if (ShopCategories.IsShowDumpLock(Item))
		{
			value.Append(string.Format(" [preset=rect_box?text=[icon=img_notool] {0},color=724835]", T._("버리기")));
		}
		if (ShopCategories.IsShowDyeLock(Item))
		{
			value.Append(string.Format(" [preset=rect_box?text=[icon=img_notool] {0},color=724835]", T._("염색")));
		}
		if (ShopCategories.IsShowRepairLock(Item))
		{
			value.Append(string.Format(" [preset=rect_box?text=[icon=img_notool] {0},color=724835]", T._("수리")));
		}
		if (ShopCategories.IsShowAvator(Item))
		{
			value.Append(string.Format(" [preset=rect_box?text={0},color=4E7737]", T._("외형")));
		}
		int petEntityType = Item.GetPetEntityType();
		if (petEntityType != 0)
		{
			Pet pet = SingletonDict<int, Pet>.Get(petEntityType);
			if (pet != null)
			{
				if (!pet.IsFightable)
				{
					value.Append(string.Format(" [preset=rect_box?text=[icon=img_notool] {0},color=724835]", T._("전투")));
				}
				if (!pet.IsRidable)
				{
					value.Append(string.Format(" [preset=rect_box?text=[icon=img_notool] {0},color=724835]", T._("탑승")));
				}
			}
		}
		_text = new ObjectReferenceText(value.ToString().Trim());
	}
}
