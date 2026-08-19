using System;
using System.Collections.Generic;
using FatigueData;
using Shared.StatusEffect;
using StatusEffectData;
using UnityEngine;
using Yaml;

public class FatigueMomentum : MonoBehaviour
{
	private struct FatigueReasonStruct
	{
		public string key;

		public string name;

		public string icon;

		public float velocity;
	}

	[SerializeField]
	private UISprite _fatigueIcon;

	[SerializeField]
	private UILabel _fatigueLabel;

	[SerializeField]
	private UISprite _fatigueLabelBg;

	[SerializeField]
	private UILabel _fatigueVelocity;

	[SerializeField]
	private UILabel _fatigueVelocityPeriod;

	[SerializeField]
	private ListObjectPool _fatigueReasons;

	private UIWidget _widget;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public void Set(FatigueVelocity fatigueVelocity, IList<StatusEffect> statusEffects)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		if (fatigueVelocity.CategoryData == null)
		{
			return;
		}
		FatigueCategory categoryData = fatigueVelocity.CategoryData;
		_fatigueIcon.spriteName = categoryData.icon;
		_fatigueLabel.text = $"{categoryData.name}";
		Color color = categoryData.GetColor();
		_fatigueIcon.color = color;
		_fatigueLabelBg.color = color;
		List<FatigueReasonStruct> list = new List<FatigueReasonStruct>();
		string key = fatigueVelocity.Category.ToString().ToLower();
		int i = 0;
		for (int num = statusEffects?.Count ?? 0; i < num; i++)
		{
			StatusEffect statusEffect = statusEffects[i];
			if (statusEffect.Template.type == EffectType.Fatigue && statusEffect.Effects.TryGetValue(key, out var value) && Math.Abs(value) > float.Epsilon)
			{
				list.Add(new FatigueReasonStruct
				{
					key = statusEffect.Id,
					name = statusEffect.Name,
					icon = statusEffect.Template.icon,
					velocity = value
				});
			}
		}
		float num2 = Mathf.Abs(fatigueVelocity.Value * 60f);
		_fatigueVelocityPeriod.text = LocalizeSystem.Get((!(fatigueVelocity.Value < 0f)) ? "#fatigue_gauge_increase_per_min" : "#fatigue_gauge_decrease_per_min");
		_fatigueVelocity.text = string.Format("{1}{0:0.#}", num2, (!(fatigueVelocity.Value < 0f)) ? "+" : "-");
		_fatigueReasons.Set(list.Count);
		UIWidget component = _fatigueReasons.BaseObject.GetComponent<UIWidget>();
		Vector3 localPosition = ((Component)component).transform.localPosition;
		Vector3 val = localPosition;
		int j = 0;
		for (int count = _fatigueReasons.Count; j < count; j++)
		{
			SimpleContainer component2 = _fatigueReasons[j].GetComponent<SimpleContainer>();
			UILabel uILabel = component2.Get<UILabel>("label");
			UISprite uISprite = component2.Get<UISprite>("icon");
			float num3 = Mathf.Abs(list[j].velocity * 60f);
			uILabel.text = string.Format("{0} {2}{1:0.#}", list[j].name, num3, (!(list[j].velocity < 0f)) ? "+" : "-");
			uILabel.color = ((!(list[j].velocity < 0f)) ? PresetColor.UILightRed : PresetColor.UISkyBlue);
			uISprite.spriteName = list[j].icon;
			((Component)component2).transform.localPosition = localPosition + Vector3.right * (float)(j % 2) * (float)(component.width + 10) + Vector3.down * (float)(j / 2) * (float)(component.height + 10);
			val = ((Component)component2).transform.localPosition;
		}
		int num4 = (int)(0f - val.y + (float)component.height * component.pivotOffset.y);
		if (_fatigueReasons.Count == 0)
		{
			num4 -= component.height;
		}
		Widget.height = num4;
	}
}
