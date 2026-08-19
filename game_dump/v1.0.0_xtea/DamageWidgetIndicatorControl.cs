using System.Collections.Generic;
using Shared.Battle;
using UnityEngine;

public class DamageWidgetIndicatorControl : MonoBehaviour
{
	[SerializeField]
	private DamageWidgetIndicator _damageWidget;

	private Stack<DamageWidgetIndicator> _damageWidgetPool;

	public void Init()
	{
		((Component)_damageWidget).gameObject.SetActive(false);
		_damageWidgetPool = new Stack<DamageWidgetIndicator>();
	}

	public DamageWidgetIndicator DamageWidgetPop()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		DamageWidgetIndicator damageWidgetIndicator = null;
		if (_damageWidgetPool.Count > 0)
		{
			damageWidgetIndicator = _damageWidgetPool.Pop();
		}
		else
		{
			damageWidgetIndicator = ((Component)((Component)_damageWidget).transform.parent).gameObject.AddChild(((Component)_damageWidget).gameObject).GetComponent<DamageWidgetIndicator>();
			damageWidgetIndicator.Init();
			((Component)damageWidgetIndicator).transform.rotation = ((Component)_damageWidget).transform.rotation;
			damageWidgetIndicator.OnFinished = DamageWidgetPush;
		}
		((Component)damageWidgetIndicator).gameObject.SetActive(true);
		return damageWidgetIndicator;
	}

	private void DamageWidgetPush(DamageWidgetIndicator damageUI)
	{
		_damageWidgetPool.Push(damageUI);
		((Component)damageUI).gameObject.SetActive(false);
	}

	public DamageWidgetIndicator AddDamageIndicator(CharacterBehavior victim, CharacterBehavior attacker, BodyPart bodypart)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)victim == (Object)null || (Object)(object)attacker == (Object)null)
		{
			return null;
		}
		ulong entityId = PlayerBehavior.LocalPlayer.EntityId;
		if (victim.EntityId != entityId && attacker.EntityId != entityId)
		{
			return null;
		}
		DamageWidgetIndicator damageWidgetIndicator = DamageWidgetPop();
		Transform bodyPartTransform = victim.GetBodyPartTransform(bodypart, bAllowNull: true);
		damageWidgetIndicator.SetTarget((!((Object)(object)bodyPartTransform != (Object)null)) ? ((Component)victim).transform : bodyPartTransform);
		return damageWidgetIndicator;
	}
}
