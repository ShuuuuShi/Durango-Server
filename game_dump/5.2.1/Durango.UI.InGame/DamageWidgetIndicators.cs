using System.Collections.Generic;
using Durango.Logic.Combat;
using L10N;
using Messages;
using Shared.Battle;
using UnityEngine;

namespace Durango.UI.InGame;

public class DamageWidgetIndicators : MonoBehaviour
{
	[SerializeField]
	private DamageWidgetIndicator _damageWidget;

	[SerializeField]
	private DamageLabelIndicator _damageLabel;

	private readonly Stack<DamageLabelIndicator> _damageLabelPool = new Stack<DamageLabelIndicator>();

	private readonly Stack<DamageWidgetIndicator> _damageWidgetPool = new Stack<DamageWidgetIndicator>();

	private void Awake()
	{
		_damageWidget.gameObject.SetActive(value: false);
		_damageLabel.gameObject.SetActive(value: false);
		GameSystem<CombatSystem>.Instance().DamagedProcesser.Damaged += OnDamaged;
	}

	private void OnDamaged(Damaged damaged)
	{
		DamageableEntities damageableEntities = GameSystem<CombatSystem>.Instance().DamageableEntities;
		bool isAlly;
		DamageableEntity damageableEntity = damageableEntities.Get(damaged.VictimId, out isAlly);
		if (damageableEntity == null)
		{
			return;
		}
		bool isAlly2;
		DamageableEntity damageableEntity2 = damageableEntities.Get(damaged.AttackerId, out isAlly2);
		Damage damage = damaged.Damage;
		if (damageableEntity2 != null && damageableEntity2.GetEntityId() == GameManager.PlayerId)
		{
			DamageWidgetIndicator damageWidgetIndicator = DamageWidgetPop();
			damageWidgetIndicator.Set(damage, damageableEntity);
			damageWidgetIndicator.Begin();
			return;
		}
		string text = null;
		Color color = Color.red;
		if (damageableEntity.GetEntityId() == GameManager.PlayerId)
		{
			if ((damage.Effects & DamageEffects.CrossCounter) > DamageEffects.None)
			{
				text = T._("크로스 카운터");
				color = PresetColor.UISkyBlue;
			}
			else
			{
				switch (damage.Result)
				{
				case DamageResult.Hit:
					text = damage.Value.ToString();
					break;
				case DamageResult.Guarded:
				case DamageResult.AutoGuarded:
					text = T._("막음");
					break;
				case DamageResult.Dodged:
				case DamageResult.Evaded:
				case DamageResult.AutoDodged:
					text = T._("피함");
					color = PresetColor.UISkyBlue;
					break;
				case DamageResult.Missed:
					text = damage.Value.ToString();
					break;
				default:
					text = damage.Value.ToString();
					color = PresetColor.UILightGray;
					break;
				}
			}
		}
		else if (isAlly2 || isAlly)
		{
			switch (damage.Result)
			{
			case DamageResult.Hit:
				text = damage.Value.ToString();
				break;
			case DamageResult.Guarded:
			case DamageResult.AutoGuarded:
				text = T._("막음");
				break;
			case DamageResult.Dodged:
			case DamageResult.Evaded:
			case DamageResult.AutoDodged:
				text = T._("피함");
				break;
			case DamageResult.Missed:
				text = damage.Value.ToString();
				break;
			default:
				text = damage.Value.ToString();
				break;
			}
			color = ((!isAlly2) ? Color.white : PresetColor.UIYellow);
		}
		if (!string.IsNullOrEmpty(text))
		{
			DamageLabelPop().Begin(text, color, damageableEntity, damageableEntity2, damage.Part);
		}
	}

	private DamageWidgetIndicator DamageWidgetPop()
	{
		DamageWidgetIndicator damageWidgetIndicator;
		if (_damageWidgetPool.Count > 0)
		{
			damageWidgetIndicator = _damageWidgetPool.Pop();
		}
		else
		{
			damageWidgetIndicator = Object.Instantiate(_damageWidget, base.transform);
			Transform obj = damageWidgetIndicator.transform;
			obj.localPosition = Vector3.zero;
			obj.rotation = _damageWidget.transform.rotation;
			damageWidgetIndicator.Finished = DamageWidgetPush;
		}
		damageWidgetIndicator.gameObject.SetActive(value: true);
		return damageWidgetIndicator;
	}

	private void DamageWidgetPush(DamageWidgetIndicator damageUI)
	{
		_damageWidgetPool.Push(damageUI);
		damageUI.gameObject.SetActive(value: false);
	}

	private DamageLabelIndicator DamageLabelPop()
	{
		DamageLabelIndicator damageLabelIndicator;
		if (_damageLabelPool.Count > 0)
		{
			damageLabelIndicator = _damageLabelPool.Pop();
		}
		else
		{
			damageLabelIndicator = Object.Instantiate(_damageLabel, _damageLabel.transform.parent);
			damageLabelIndicator.transform.rotation = _damageLabel.transform.rotation;
			damageLabelIndicator.Finished = DamageLabelPush;
		}
		damageLabelIndicator.gameObject.SetActive(value: true);
		return damageLabelIndicator;
	}

	private void DamageLabelPush(DamageLabelIndicator damageUI)
	{
		_damageLabelPool.Push(damageUI);
		damageUI.gameObject.SetActive(value: false);
	}
}
