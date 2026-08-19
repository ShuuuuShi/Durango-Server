using System;
using System.Collections.Generic;
using Shared.Battle;
using StatusEffectData;
using UnityEngine;

public class InjuryEffectsControl : MonoBehaviour
{
	private enum InjuryIconType
	{
		head_injury,
		body_injury,
		leg_injury,
		tail_injury
	}

	private class InjuryIconObject
	{
		private GameObject _object;

		private BodyPart _bodyPart;

		public InjuryIconObject(GameObject obj, BodyPart bodyPart)
		{
			_object = obj;
			_bodyPart = bodyPart;
		}

		public void Activate()
		{
			if (!_object.activeSelf)
			{
				_object.SetActive(true);
			}
		}

		public void Deactivate()
		{
			_object.SetActive(false);
		}

		public void Update(AnimalBehavior animal, Vector3 offsetforWorld, Vector3 offsetforUI)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			if (_object.activeSelf)
			{
				Transform bodyPartTransform = animal.GetBodyPartTransform(_bodyPart, bAllowNull: false, PlayerBehavior.LocalPlayer.CurrentPosition);
				_object.transform.localPosition = MainCamera.WorldToNGUIPos(bodyPartTransform.position + offsetforWorld) + offsetforUI;
			}
		}
	}

	[Serializable]
	[EnumType(typeof(InjuryIconType))]
	private class InjuryIconLink : EnumKeyList
	{
		[SerializeField]
		private List<GameObject> _values;

		public GameObject Get(InjuryIconType type)
		{
			return _values[(int)type];
		}
	}

	[SerializeField]
	private Vector3 _offsetForWorld;

	[SerializeField]
	private Vector3 _offsetForUI;

	[SerializeField]
	private InjuryIconLink _injuryIconLink;

	private AnimalBehavior _targetAnimal;

	private readonly Dictionary<string, InjuryIconObject> _injuryIcons = new Dictionary<string, InjuryIconObject>();

	private void Awake()
	{
		AddInjuryIconObject(InjuryIconType.head_injury, BodyPart.Head);
		AddInjuryIconObject(InjuryIconType.body_injury, BodyPart.Body);
		AddInjuryIconObject(InjuryIconType.leg_injury, BodyPart.Leg);
		AddInjuryIconObject(InjuryIconType.tail_injury, BodyPart.Tail);
	}

	private void Update()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_targetAnimal != (Object)null)
		{
			Dictionary<string, InjuryIconObject>.Enumerator enumerator = _injuryIcons.GetEnumerator();
			while (enumerator.MoveNext())
			{
				enumerator.Current.Value.Update(_targetAnimal, _offsetForWorld, _offsetForUI);
			}
		}
	}

	private void OnEnable()
	{
		GameSystem<TargetStatusEffectSystem>.Instance().StatusEffectsUpdated += OnUpdateStatusEffect;
	}

	private void OnDisable()
	{
		GameSystem<TargetStatusEffectSystem>.Instance().StatusEffectsUpdated -= OnUpdateStatusEffect;
	}

	public void SetTarget(GameObject target)
	{
		Dictionary<string, InjuryIconObject>.Enumerator enumerator = _injuryIcons.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Value.Deactivate();
		}
		_targetAnimal = ((!((Object)(object)target != (Object)null)) ? null : target.GetComponent<AnimalBehavior>());
	}

	private void AddInjuryIconObject(InjuryIconType type, BodyPart bodyPart)
	{
		GameObject val = ((Component)this).gameObject.AddChild(_injuryIconLink.Get(type));
		val.SetActive(false);
		_injuryIcons[type.ToString()] = new InjuryIconObject(val, bodyPart);
	}

	private void OnUpdateStatusEffect()
	{
		IList<StatusEffect> statusEffects = GameSystem<TargetStatusEffectSystem>.Instance().StatusEffects;
		int i = 0;
		for (int count = statusEffects.Count; i < count; i++)
		{
			if (_injuryIcons.TryGetValue(statusEffects[i].Id, out var value))
			{
				value.Activate();
			}
		}
	}
}
