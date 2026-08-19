using System;
using System.Collections.Generic;
using Durango.Logic;
using L10N;
using Messages;
using Shared.Ability;
using Shared.StatusEffect;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class FatigueMomentum : MonoBehaviour
{
	private enum ReasonType
	{
		SignedPercentage,
		SignedValue,
		UnsignedValue
	}

	private struct FatigueReason
	{
		public string Name;

		public string Icon;

		public float Velocity;

		public ReasonType Type;
	}

	private const string ResistanceIconName = "icon_se_clothes";

	private const string BiomeFatigueIconName = "icon_se_skin_hardness";

	private const int FatigueLabelMargin = 15;

	[SerializeField]
	private UIWidget _infoContainer;

	[SerializeField]
	private UISprite _fatigueIcon;

	[SerializeField]
	private UILabel _fatigueLabel;

	[SerializeField]
	private UILabel _fatigueVelocity;

	[SerializeField]
	private UISprite _velocitySign;

	[SerializeField]
	private UIWidget _fatigueReasonContainer;

	[SerializeField]
	private FatigueMomentumReason _fatigueReasonItem;

	[SerializeField]
	private int _reasonMargin;

	[SerializeField]
	private RectLayout _layout;

	private ListObjectPool<FatigueMomentumReason> _fatigueReasons;

	private readonly List<FatigueReason> _reasonList = new List<FatigueReason>();

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_fatigueReasons = new ListObjectPool<FatigueMomentumReason>();
			_fatigueReasons.BaseObject = _fatigueReasonItem;
		}
	}

	private void Set(string title, string icon, float velocity, IList<FatigueReason> reasons)
	{
		Init();
		_fatigueIcon.spriteName = icon;
		_fatigueLabel.text = title;
		_infoContainer.height = _fatigueLabel.height + 30;
		float num = Mathf.Abs((float)Mathf.RoundToInt(velocity * 60f * 10f) / 10f);
		if (num > 0f)
		{
			_fatigueVelocity.text = $"{num:F1}";
			_velocitySign.flip = ((velocity > 0f) ? UIBasicSprite.Flip.Vertically : UIBasicSprite.Flip.Nothing);
			_velocitySign.color = ((!(velocity > 0f)) ? PresetColor.UISkyBlue : PresetColor.UILightRed);
		}
		else
		{
			_fatigueVelocity.text = "0";
			_velocitySign.alpha = 0f;
		}
		_fatigueReasons.Set(KUtility.GetSize(reasons));
		if (_fatigueReasons.Count > 0)
		{
			int i = 0;
			for (int count = _fatigueReasons.Count; i < count; i++)
			{
				FatigueMomentumReason fatigueMomentumReason = _fatigueReasons[i];
				FatigueReason fatigueReason = reasons[i];
				string text = string.Empty;
				switch (fatigueReason.Type)
				{
				case ReasonType.SignedPercentage:
					text = $"{fatigueReason.Name} {fatigueReason.Velocity:+#.%;-#.%}";
					break;
				case ReasonType.SignedValue:
					text = $"{fatigueReason.Name} {fatigueReason.Velocity:+#;-#}";
					break;
				case ReasonType.UnsignedValue:
					text = $"{fatigueReason.Name} {fatigueReason.Velocity}";
					break;
				}
				fatigueMomentumReason.Set(text, fatigueReason.Icon);
			}
			_fatigueReasonContainer.gameObject.SetActive(value: true);
			_fatigueReasonContainer.height = (int)UIUtility.WidgetsReposition(_fatigueReasons, _fatigueReasonContainer, Vector3.down, _reasonMargin);
		}
		else
		{
			_fatigueReasonContainer.gameObject.SetActive(value: false);
		}
		_layout.UpdateLayout(null, 0f);
	}

	public void Set(FatigueVelocity fatigueVelocity)
	{
		if (fatigueVelocity.CategoryData == null)
		{
			return;
		}
		FatigueCategory categoryData = fatigueVelocity.CategoryData;
		_reasonList.Clear();
		foreach (Durango.Logic.StatusEffect item in GameSystem<StatusEffectSystem>.Instance().GetStatusEffects().List)
		{
			float num = item.GetDetail(EffectType.Fatigue, categoryData.GetSnakeCase());
			if (Mathf.Abs(num) > 0f)
			{
				if (categoryData.DefaultRatio > 0f)
				{
					num /= categoryData.DefaultRatio;
				}
				_reasonList.Add(new FatigueReason
				{
					Name = item.Name,
					Icon = item.Icon,
					Velocity = num,
					Type = ReasonType.SignedPercentage
				});
			}
		}
		if (Math.Abs(fatigueVelocity.Resistance) > 0f)
		{
			string text = T._("{0} 저항", fatigueVelocity.CategoryData.Name);
			_reasonList.Add(new FatigueReason
			{
				Name = text,
				Icon = "icon_se_clothes",
				Velocity = Math.Max(-1f, Math.Min(1f, fatigueVelocity.Resistance)),
				Type = ReasonType.SignedPercentage
			});
		}
		Set(categoryData.Name, categoryData.Icon, fatigueVelocity.Value, _reasonList);
	}

	public void Set(Durango.Logic.StatusEffect statusEffect)
	{
		string title = statusEffect.Name;
		string icon = statusEffect.Icon;
		float detail = statusEffect.GetDetail(EffectType.Survival, "fatigue");
		Set(title, icon, detail, null);
	}

	public void Set(BiomeFatigue biomeFatigue, Derived resistanceType)
	{
		string title = T._("불안정 환경 효과");
		string text = resistanceType.GetName();
		_reasonList.Clear();
		if (biomeFatigue.EquipsResistance != 0)
		{
			_reasonList.Add(new FatigueReason
			{
				Name = text,
				Icon = "icon_se_clothes",
				Velocity = biomeFatigue.EquipsResistance,
				Type = ReasonType.SignedValue
			});
		}
		if (biomeFatigue.ResistanceLevel > 0)
		{
			_reasonList.Add(new FatigueReason
			{
				Name = T._("신체 {0} 레벨", text),
				Icon = "icon_se_skin_hardness",
				Velocity = biomeFatigue.ResistanceLevel,
				Type = ReasonType.UnsignedValue
			});
		}
		Set(title, "icon_se_skin_hardness", biomeFatigue.Velocity, _reasonList);
	}
}
