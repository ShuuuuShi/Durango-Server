using UnityEngine;

public class CombatGroup : UIBase
{
	public const string SelectEffect = "Particle/FX_Targeting_Common_01.prefab";

	[SerializeField]
	private HyperGaugeViewer _targetHp;

	[SerializeField]
	private UILabel _targetName;

	[SerializeField]
	private CombatTargetPortraits _targetPortraits;

	[SerializeField]
	private CombatTargetSelectContainer _targetSelectContainer;

	[SerializeField]
	private StatusEffectsControl _statusEffectsControl;

	[SerializeField]
	private InjuryEffectsControl _injuryEffectsControl;

	[SerializeField]
	private UILabel _targetDebugLabel;

	private Transform _targetDebugLabelTransform;

	private GameObject _selectParticle;

	private DamageableEntity _targetable;

	public CombatTargetSelectContainer TargetSelectContainer => _targetSelectContainer;

	private void Start()
	{
		((Component)_targetHp).gameObject.SetActive(false);
		_targetDebugLabelTransform = ((Component)_targetDebugLabel).transform;
		((Component)_targetDebugLabel).gameObject.SetActive(false);
		ParticleManager.Cache("Particle/FX_Targeting_Common_01.prefab");
		KSingleton<GameManager>.Instance().AddOnReady(delegate
		{
			((Component)_targetSelectContainer).gameObject.SetActive(true);
		});
	}

	private void Update()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (!(_targetable == null))
		{
			Vector3 val = MainCamera.WorldToNGUIPos(_targetable.GetCurrentPosition());
			_targetDebugLabelTransform.localPosition = val + Vector3.up * 50f;
			if (_targetable.GaugeChanged)
			{
				_targetHp.Set(gaugeScale: _targetable.GetGaugeScale(), gauge: _targetable.GetLife());
				_targetable.GaugeChanged = false;
			}
		}
	}

	private void OnEnable()
	{
		GameSystem<CombatSystem>.Instance().TargetChanged += OnChangeTarget;
		GameSystem<CombatSystem>.Instance().ChangedCombatMode += OnChangeCombatMode;
		GameSystem<CombatSystem>.Instance().DamageRecorder.RecordUpdated += OnUpdateDamageRecord;
		GameSystem<CombatSystem>.Instance().DamageRecorder.RecordEnded += OnEndDamageRecord;
	}

	private void OnDisable()
	{
		GameSystem<CombatSystem>.Instance().TargetChanged -= OnChangeTarget;
		GameSystem<CombatSystem>.Instance().ChangedCombatMode -= OnChangeCombatMode;
		GameSystem<CombatSystem>.Instance().DamageRecorder.RecordUpdated -= OnUpdateDamageRecord;
		GameSystem<CombatSystem>.Instance().DamageRecorder.RecordEnded -= OnEndDamageRecord;
	}

	private void SelectTarget(GameObject newTargetObj)
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		if ((bool)_targetable && (Object)(object)newTargetObj == (Object)(object)_targetable.GameObject)
		{
			return;
		}
		if ((bool)_targetable)
		{
			_targetable.RemoveLifeGaugeUpdateDelegate();
		}
		if ((Object)(object)newTargetObj == (Object)null)
		{
			((Component)_targetHp).gameObject.SetActive(false);
			((Component)_targetDebugLabel).gameObject.SetActive(false);
			ParticleManager.Stop(_selectParticle);
			_targetable = null;
		}
		else
		{
			DamageableEntity damageableEntity = DamageableEntity.Create(newTargetObj);
			((Component)_targetHp).gameObject.SetActive(!GameManager.IsPrologueMode);
			if (_targetable != damageableEntity)
			{
				ParticleManager.Stop(_selectParticle);
				_selectParticle = ParticleManager.EmitSync("Particle/FX_Targeting_Common_01.prefab", Vector3.zero, Quaternion.identity, newTargetObj.transform);
				float num = 1f;
				if ((Object)(object)_selectParticle != (Object)null)
				{
					_selectParticle.transform.localScale = Vector3.one * num;
				}
				_targetName.text = LocalizeSystem.Format("#combat_target_name_with_level", damageableEntity.GetName(), damageableEntity.GetLevel().ToString());
				_targetPortraits.SetPortrait(damageableEntity);
				damageableEntity.AddLifeGaugeUpdateDelegate();
				_targetHp.Set(gaugeScale: damageableEntity.GetGaugeScale(), gauge: damageableEntity.GetLife());
			}
			_targetable = damageableEntity;
		}
		if ((Object)(object)newTargetObj == (Object)null)
		{
			KSingleton<CameraController>.Instance().ResetCamera();
		}
	}

	public void SetTargetDebugLabel(string text)
	{
		((Component)_targetDebugLabel).gameObject.SetActive(true);
		_targetDebugLabel.text = text;
	}

	public void SetFocusTarget(GameObject targetObject)
	{
		_targetSelectContainer.SetFocusTarget(targetObject);
	}

	private void OnChangeTarget()
	{
		GameObject target = GameSystem<CombatSystem>.Instance().Target;
		SelectTarget(target);
		_targetSelectContainer.SetCurrentTarget(target);
		_injuryEffectsControl.SetTarget(target);
	}

	private void OnChangeCombatMode(bool isCombat)
	{
		if (isCombat)
		{
			InteractionGroupHelper.ShowInteractionButtons("Battle", show: false);
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
			if (!GameManager.IsPrologueMode)
			{
				_targetSelectContainer.SetCombatMode(combatMode: true);
			}
		}
		else
		{
			InteractionGroupHelper.ShowInteractionButtons("Battle", show: true);
			if (!GameManager.IsPrologueMode)
			{
				_targetSelectContainer.SetCombatMode(combatMode: false);
			}
		}
		UIBase.HideUI(UIFlag.HideToCombat, isCombat, "Battle");
	}

	private void OnUpdateDamageRecord()
	{
		if (CombatSystem.EnableDamageLog)
		{
			SetTargetDebugLabel(GameSystem<CombatSystem>.Instance().DamageRecorder.GetResult());
		}
	}

	private void OnEndDamageRecord()
	{
		if (CombatSystem.EnableDamageLog)
		{
			UIManager.Popup.Alarm.ShowAlarm($"Combat Result\n{GameSystem<CombatSystem>.Instance().DamageRecorder.GetResult()}", "alarm_private", 20f);
		}
	}

	private void OnPortraitMode(bool isPortrait)
	{
		((Component)_targetSelectContainer).GetComponent<UIWidget>().ResetAnchors();
	}
}
