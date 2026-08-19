using System;
using System.Collections.Generic;
using System.Text;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Pet;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class PetMilestoneHelpItemWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _indexLabel;

	[SerializeField]
	private UIWidget _tagWidget;

	[SerializeField]
	private UILabel _tagNameLabel;

	[SerializeField]
	private UIWidget _tagModifierWidget;

	[SerializeField]
	private ListObjectPool _tagModifierArrows;

	[SerializeField]
	private UIWidget _skillWidget;

	[SerializeField]
	private UILabel _skillLabel;

	[SerializeField]
	private UISprite _skillSprite;

	[SerializeField]
	private UIWidget _itemsWidget;

	[SerializeField]
	private ListObjectPool _items;

	[SerializeField]
	private UIWidget _activeSkillHelpWidget;

	private Yaml.Tag _tag;

	private Messages.PetActiveSkill? _activeSkill;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_items.Init(delegate(GameObject obj)
			{
				UIEventListener uIEventListener2 = UIEventListener.Get(obj);
				uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClickItemWidget));
			});
			UIEventListener uIEventListener = UIEventListener.Get(_activeSkillHelpWidget.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickActiveSkillHelp));
		}
	}

	public void SetIndex(int index)
	{
		_indexLabel.text = index.ToString();
	}

	public void SetMiletone(string tagId, float origin, float weight)
	{
		Init();
		_tagWidget.gameObject.SetActive(value: true);
		_skillWidget.gameObject.SetActive(value: false);
		_itemsWidget.gameObject.SetActive(value: true);
		_activeSkillHelpWidget.gameObject.SetActive(value: false);
		Yaml.Tag tag = (_tag = SingletonDict<string, Yaml.Tag>.Get(tagId));
		_activeSkill = null;
		_tagNameLabel.text = $"<tag>{tagId}</tag>";
		int petMilestoneDiffLevel = PetUtil.GetPetMilestoneDiffLevel(weight - origin);
		if (petMilestoneDiffLevel > 0)
		{
			_tagModifierWidget.gameObject.SetActive(value: true);
			_tagModifierArrows.Set(petMilestoneDiffLevel);
			UIUtility.WidgetsReposition(_tagModifierArrows, Vector3.down, Vector3.zero, -4f, 0.5f);
		}
		else
		{
			_tagModifierWidget.gameObject.SetActive(value: false);
		}
		_items.BeginLoad();
		if (tag != null && tag.PetFoodReference != null)
		{
			string[] petFoodReference = tag.PetFoodReference;
			foreach (string prototypeId in petFoodReference)
			{
				Prototype itemPrototype = PrototypeYaml.GetItemPrototype(prototypeId);
				GameObject next = _items.GetNext();
				if (itemPrototype == null)
				{
					next.transform.Find("Icon").GetComponent<ItemIconTex>().SetIcon("icon_question");
				}
				else
				{
					next.transform.Find("Icon").GetComponent<ItemIconTex>().SetIcon(itemPrototype.Icon, itemPrototype.ColorR, itemPrototype.ColorG, itemPrototype.ColorB);
				}
			}
		}
		_items.EndLoad();
		UIUtility.WidgetsReposition(_items, _itemsWidget, Vector3.left, 5f);
		UpdateLayout();
	}

	public void SetSkill(Messages.PetActiveSkill skill)
	{
		Init();
		_tag = null;
		_activeSkill = skill;
		_tagWidget.gameObject.SetActive(value: false);
		_skillWidget.gameObject.SetActive(value: true);
		_itemsWidget.gameObject.SetActive(value: false);
		PetActiveSkillCondition petActiveSkillCondition = PetActiveSkillConditions.Get(skill.SkillId, skill.Rank);
		if (petActiveSkillCondition != null && KUtility.GetSize(petActiveSkillCondition.TagCondition) > 0)
		{
			_activeSkillHelpWidget.gameObject.SetActive(value: true);
		}
		else
		{
			_activeSkillHelpWidget.gameObject.SetActive(value: false);
		}
		Yaml.PetActiveSkill petActiveSkill = PetActiveSkills.Get(skill.SkillId, skill.Rank);
		if (skill.Rank == SkillRank.S)
		{
			_skillLabel.text = $"<em>{petActiveSkill.Name}</em>";
		}
		else
		{
			_skillLabel.text = petActiveSkill.Name;
		}
		_skillSprite.spriteName = petActiveSkill.Icon;
		UpdateLayout();
	}

	private void UpdateLayout()
	{
		GetComponent<RectLayoutComponent>().UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	private void OnClickItemWidget(GameObject obj)
	{
		if (_tag == null)
		{
			return;
		}
		int num = _items.IndexOf(obj);
		if (num != -1)
		{
			string prototypeId = _tag.PetFoodReference[num];
			Prototype itemPrototype = PrototypeYaml.GetItemPrototype(prototypeId);
			if (itemPrototype != null)
			{
				WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
				widgetTooltipControl.Set(null, itemPrototype.Name);
				widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
				widgetTooltipControl.Show();
			}
		}
	}

	private void OnClickActiveSkillHelp(GameObject obj)
	{
		Messages.PetActiveSkill? activeSkill = _activeSkill;
		if (!activeSkill.HasValue)
		{
			return;
		}
		PetActiveSkillCondition petActiveSkillCondition = PetActiveSkillConditions.Get(_activeSkill.Value.SkillId, _activeSkill.Value.Rank);
		if (petActiveSkillCondition == null || KUtility.GetSize(petActiveSkillCondition.TagCondition) == 0)
		{
			return;
		}
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		foreach (KeyValuePair<string, int> item in petActiveSkillCondition.TagCondition)
		{
			if (value.Length > 0)
			{
				value.Append(", ");
			}
			value.AppendFormat("<tag>{0},{1}</tag>", item.Key, item.Value);
		}
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		widgetTooltipControl.Set(T._("달성 조건"), value.ToString());
		widgetTooltipControl.AutoPosition = false;
		widgetTooltipControl.Show(10f);
		UISprite componentInChildren = obj.GetComponentInChildren<UISprite>();
		widgetTooltipControl.SetPosition(componentInChildren, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), Vector2.up * 20f);
	}

	private void OnClick()
	{
		Messages.PetActiveSkill? activeSkill = _activeSkill;
		if (activeSkill.HasValue)
		{
			Yaml.PetActiveSkill petActiveSkill = PetActiveSkills.Get(_activeSkill.Value.SkillId, _activeSkill.Value.Rank);
			if (petActiveSkill != null)
			{
				WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
				widgetTooltipControl.Set(petActiveSkill.Name, petActiveSkill.Description, 500);
				widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
				widgetTooltipControl.Show(_skillSprite, Vector2.up * 20f, 10f);
			}
		}
	}
}
