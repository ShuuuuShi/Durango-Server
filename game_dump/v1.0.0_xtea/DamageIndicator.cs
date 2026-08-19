using System.Collections.Generic;
using Shared.Battle;
using UnityEngine;

public class DamageIndicator : KSingleton<DamageIndicator>
{
	[SerializeField]
	private DamageLabelIndicator _damageLabel;

	[SerializeField]
	private float _popPower = 300f;

	[SerializeField]
	private float _gravity = 980f;

	[SerializeField]
	private float _throwRatio = 1f;

	[SerializeField]
	private float _fowDepth = 100f;

	[SerializeField]
	private float[] _stateTime;

	private Stack<DamageLabelIndicator> _damageLabelPool;

	public static float PopPower => KSingleton<DamageIndicator>.Instance()._popPower;

	public static float Gravity => KSingleton<DamageIndicator>.Instance()._gravity;

	public static float ThrowRatio => KSingleton<DamageIndicator>.Instance()._throwRatio;

	public static float FowDepth => KSingleton<DamageIndicator>.Instance()._fowDepth;

	public static float[] StateTime => KSingleton<DamageIndicator>.Instance()._stateTime;

	protected override void OnAwake()
	{
		Init();
	}

	public void Init()
	{
		((Component)_damageLabel).gameObject.SetActive(false);
		_damageLabelPool = new Stack<DamageLabelIndicator>();
	}

	public DamageLabelIndicator DamageLabelPop()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		DamageLabelIndicator damageLabelIndicator = null;
		if (_damageLabelPool.Count > 0)
		{
			damageLabelIndicator = _damageLabelPool.Pop();
		}
		else
		{
			damageLabelIndicator = ((Component)((Component)_damageLabel).transform.parent).gameObject.AddChild(((Component)_damageLabel).gameObject).GetComponent<DamageLabelIndicator>();
			((Component)damageLabelIndicator).transform.rotation = ((Component)_damageLabel).transform.rotation;
			damageLabelIndicator.OnFinished = DamageLabelPush;
		}
		((Component)damageLabelIndicator).gameObject.SetActive(true);
		damageLabelIndicator.Dir = Vector3.zero;
		return damageLabelIndicator;
	}

	private void DamageLabelPush(DamageLabelIndicator damageUI)
	{
		_damageLabelPool.Push(damageUI);
		((Component)damageUI).gameObject.SetActive(false);
	}

	public DamageLabelIndicator AddDamageIndicator(CharacterBehavior victim, CharacterBehavior attacker, BodyPart bodypart)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)victim == (Object)null || (Object)(object)attacker == (Object)null)
		{
			return null;
		}
		DamageLabelIndicator damageLabelIndicator = DamageLabelPop();
		Transform bodyPartTransform = victim.GetBodyPartTransform(bodypart, bAllowNull: true);
		Vector3 val = ((!((Object)(object)bodyPartTransform == (Object)null)) ? bodyPartTransform.position : victim.InteractionPosition);
		((Component)damageLabelIndicator).transform.position = val;
		Vector3 dir = val - attacker.CurrentPosition;
		dir.y = 0f;
		damageLabelIndicator.Dir = dir;
		return damageLabelIndicator;
	}
}
