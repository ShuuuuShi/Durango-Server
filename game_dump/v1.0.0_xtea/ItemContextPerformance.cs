using System;
using System.Collections.Generic;
using Building_;
using ItemSystem;
using L10N;
using Shared.Item;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class ItemContextPerformance : ItemContextBase
{
	private class SimpleValueAttribute
	{
		public string Name { get; private set; }

		public string Text { get; private set; }

		public string Value { get; private set; }

		public int Order { get; private set; }

		public SimpleValueAttribute(string name, string text, string value, int order)
		{
			Name = name;
			Text = text;
			Value = value;
			Order = ((order <= 0) ? int.MaxValue : order);
		}

		public static int Compare(SimpleValueAttribute x, SimpleValueAttribute y)
		{
			return x.Order - y.Order;
		}
	}

	[SerializeField]
	private ListObjectPool _simpleValueControls;

	[SerializeField]
	private ListObjectPool _actionTagControls;

	[SerializeField]
	private UIWidget _seperator;

	[SerializeField]
	private ListObjectPool _buffTagControls;

	protected override void OnInit()
	{
		_simpleValueControls.Init(null);
		_actionTagControls.Init(InitTagControl);
		_buffTagControls.Init(InitTagControl);
	}

	public void Set(PerformanceData data, Dictionary<string, PerformanceVisibleInfo> visibleInfoDict)
	{
		base.Id = data.id;
		base.HeaderText = ((!string.IsNullOrEmpty(data.name)) ? LocalizeSystem.Get(data.name) : data.id);
		FillData(data, visibleInfoDict);
		UpdateLayout();
	}

	public void Set(Reins reins)
	{
		base.Id = "item_reins";
		base.HeaderText = T._("탑승 동물");
		FillData(reins);
		UpdateLayout();
	}

	public void Set(ArtifactCapsule capsule)
	{
		base.Id = "artifact_capsule";
		base.HeaderText = GameSystem<RecipeSystem>.Instance().GetBlueprint(capsule.BlueprintId).LocalizedName;
		FillData(capsule);
		UpdateLayout();
	}

	private void InitTagControl(GameObject obj)
	{
		UIEventListener uIEventListener = UIEventListener.Get(obj);
		if (uIEventListener.onClick == null)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(obj);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClickTag));
		}
	}

	private void UpdateLayout()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = _simpleValueControls.BaseObject.GetComponent<UIWidget>().GetPosition(0f, 1f);
		int i = 0;
		for (int count = _simpleValueControls.Count; i < count; i++)
		{
			KeyValueLabel component = _simpleValueControls[i].GetComponent<KeyValueLabel>();
			component.UpdateLayout(_body.width);
			component.Widget.SetPosition(position, 0f, 1f);
			position.y -= (float)component.Widget.height;
		}
		float x = _actionTagControls.BaseObject.GetComponent<UIWidget>().GetPosition(0f, 0f).x;
		int height = _actionTagControls.BaseObject.GetComponent<UIWidget>().height;
		float num = _body.localCorners[2].x - (x - _body.localCorners[0].x);
		position.x = x;
		for (int j = 0; j < _actionTagControls.Count; j++)
		{
			UIWidget component2 = _actionTagControls[j].GetComponent<UIWidget>();
			if (position.x > num)
			{
				position.x = x;
				position.y -= (float)height + 10f;
			}
			component2.SetPosition(position, 0f, 1f);
			position.x += (float)component2.width + 10f;
		}
		if (_actionTagControls.Count > 0)
		{
			position.y -= (float)height + 10f;
		}
		if (_buffTagControls.Count > 0)
		{
			((Component)_seperator).gameObject.SetActive(true);
			Vector3 localPosition = ((Component)_seperator).transform.localPosition;
			localPosition.y = position.y;
			((Component)_seperator).transform.localPosition = localPosition;
			position.y -= 10f;
			position.x = x;
			height = _buffTagControls.BaseObject.GetComponent<UIWidget>().height;
			for (int k = 0; k < _buffTagControls.Count; k++)
			{
				UIWidget component3 = _buffTagControls[k].GetComponent<UIWidget>();
				if (position.x > num)
				{
					position.x = x;
					position.y -= (float)height + 10f;
				}
				component3.SetPosition(position, 0f, 1f);
				position.x += (float)component3.width + 10f;
			}
			position.y -= (float)height + 10f;
		}
		else
		{
			((Component)_seperator).gameObject.SetActive(false);
		}
		_body.height = (int)Mathf.Abs(position.y);
	}

	private void AddKeyValueInfo(string key, string value)
	{
		KeyValueLabel keyValueLabel = ((ListObjectPoolBase<GameObject>)_simpleValueControls).Add<KeyValueLabel>();
		keyValueLabel.Set(key, value);
	}

	private void FillData(PerformanceData data, IDictionary<string, PerformanceVisibleInfo> visibleInfoDictionary)
	{
		_simpleValueControls.Clear();
		_actionTagControls.Clear();
		_buffTagControls.Clear();
		List<SimpleValueAttribute> list = new List<SimpleValueAttribute>();
		foreach (KeyValuePair<string, float> num_attr in data.num_attrs)
		{
			if (!visibleInfoDictionary.TryGetValue(num_attr.Key, out var value))
			{
				continue;
			}
			switch (value.type)
			{
			case PerformanceVisibleType.Number:
			{
				SimpleValueAttribute simpleValueAttribute2 = CreateNumberAttribute(num_attr.Key, num_attr.Value, value);
				if (simpleValueAttribute2 != null)
				{
					list.Add(simpleValueAttribute2);
				}
				break;
			}
			case PerformanceVisibleType.BasicStat:
			{
				SimpleValueAttribute simpleValueAttribute = CreateNumberAttribute(num_attr.Key, num_attr.Value, value);
				if (simpleValueAttribute != null)
				{
					list.Add(simpleValueAttribute);
				}
				break;
			}
			case PerformanceVisibleType.DerivedStat:
				AddDerivedStatTag(num_attr.Key, num_attr.Value);
				break;
			}
		}
		foreach (KeyValuePair<string, string> str_attr in data.str_attrs)
		{
			if (!visibleInfoDictionary.TryGetValue(str_attr.Key, out var value2))
			{
				continue;
			}
			switch (value2.type)
			{
			case PerformanceVisibleType.String:
			{
				SimpleValueAttribute simpleValueAttribute3 = CreateStringAttribute(str_attr.Key, str_attr.Value, value2);
				if (simpleValueAttribute3 != null)
				{
					list.Add(simpleValueAttribute3);
				}
				break;
			}
			case PerformanceVisibleType.ActionSet:
				AddActionSetTag(str_attr.Value);
				break;
			case PerformanceVisibleType.StatusEffect:
			{
				float value3 = 0f;
				data.num_attrs.TryGetValue(str_attr.Key + "_level", out value3);
				AddStatusEffectTag(str_attr.Value, (int)value3, value2);
				break;
			}
			}
		}
		list.Sort(SimpleValueAttribute.Compare);
		for (int i = 0; i < list.Count; i++)
		{
			AddKeyValueInfo(list[i].Text, list[i].Value);
		}
		list.Clear();
	}

	private void AddActionSetTag(string id)
	{
		if (SingletonDict<string, ActionSet>.TryGetValue(id, out var value))
		{
			PerformanceTagControl performanceTagControl = ((ListObjectPoolBase<GameObject>)_actionTagControls).Add<PerformanceTagControl>();
			performanceTagControl.SetActionSetTag(value.icon, value.name, value.description);
		}
	}

	private void AddStatusEffectTag(string id, int level, PerformanceVisibleInfo visibleInfo)
	{
		StatusEffectTemplate statusEffectTemplate = StatusEffectTemplateYaml.GetStatusEffectTemplate(id, level);
		if (statusEffectTemplate != null)
		{
			PerformanceTagControl performanceTagControl = ((ListObjectPoolBase<GameObject>)_buffTagControls).Add<PerformanceTagControl>();
			performanceTagControl.SetStatusEffectTag(statusEffectTemplate, visibleInfo.negative);
		}
	}

	private void AddDerivedStatTag(string id, float value)
	{
		if (SingletonDict<string, SkillModifier>.TryGetValue(id, out var value2))
		{
			PerformanceTagControl performanceTagControl = ((ListObjectPoolBase<GameObject>)_buffTagControls).Add<PerformanceTagControl>();
			performanceTagControl.SetDerivedStatTag(GetLocalizedAttributeName(id), value, value2);
		}
	}

	private static SimpleValueAttribute CreateNumberAttribute(string name, float value, PerformanceVisibleInfo visibleInfo)
	{
		string localizedAttributeName = GetLocalizedAttributeName(name);
		if (localizedAttributeName == null)
		{
			return null;
		}
		if (visibleInfo.min_value != 0f && visibleInfo.min_value > value)
		{
			return null;
		}
		string text = $"N0{visibleInfo.digits}";
		return new SimpleValueAttribute(name, localizedAttributeName, value.ToString(text), visibleInfo.order);
	}

	private static SimpleValueAttribute CreateStringAttribute(string name, string value, PerformanceVisibleInfo visibleInfo)
	{
		string localizedAttributeName = GetLocalizedAttributeName(name);
		if (localizedAttributeName == null)
		{
			return null;
		}
		return new SimpleValueAttribute(name, localizedAttributeName, GetLocalizedAttributeValue(value), visibleInfo.order);
	}

	private static string GetLocalizedAttributeName(string attrName)
	{
		string key = $"#attribute_{attrName}";
		return (!LocalizeSystem.Has(key)) ? null : LocalizeSystem.Get(key);
	}

	private static string GetLocalizedAttributeValue(string attrValue)
	{
		string key = $"#attribute_value_{attrValue}";
		return LocalizeSystem.Get(key);
	}

	private void FillData(Reins reins)
	{
		_simpleValueControls.Clear();
		_actionTagControls.Clear();
		_buffTagControls.Clear();
		AddKeyValueInfo(T._("이름"), reins.PetName);
		AddKeyValueInfo(T._("덩치"), reins.Size.ToString());
		AddKeyValueInfo(T._("가방"), $"{reins.ItemSize}/{reins.Capacity}");
		KeyValueLabel keyValueLabel = ((ListObjectPoolBase<GameObject>)_simpleValueControls).Add<KeyValueLabel>();
		keyValueLabel.Set(T._("배고픔"), new SyncString(delegate(out string text, out float period)
		{
			text = T._("{0:p0}", reins.Hungry.Ratio());
			float num = reins.Hungry.Velocity();
			period = ((!(Mathf.Abs(num) < 0.0001f)) ? (1f / num) : 10000f);
		}));
	}

	private void FillData(ArtifactCapsule capsule)
	{
		_simpleValueControls.Clear();
		_actionTagControls.Clear();
		_buffTagControls.Clear();
		Building_.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(capsule.BlueprintId);
		AddKeyValueInfo(T._("레벨"), LocalizeUtil.FormatLevel(capsule.ArtifactLevel));
		AddKeyValueInfo(T._("크기"), $"{blueprint.Size.x}x{blueprint.Size.y}");
		int i = 0;
		for (int size = KUtility.GetSize(capsule.LookNames); i < size; i++)
		{
			int num = -1;
			int j = 0;
			for (int size2 = KUtility.GetSize(blueprint.Slots); j < size2; j++)
			{
				if (blueprint.Slots[j].Id == capsule.LookNames[i].Key)
				{
					num = j;
					break;
				}
			}
			AddKeyValueInfo((num != -1) ? blueprint.Slots[num].LocalizedName : T._("재질"), capsule.LookNames[i].Value);
		}
	}

	private void OnClickTag(GameObject go)
	{
		PerformanceTagControl component = go.GetComponent<PerformanceTagControl>();
		if ((Object)(object)component != (Object)null)
		{
			component.ShowTooltip();
		}
	}
}
