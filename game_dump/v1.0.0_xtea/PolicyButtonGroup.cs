using System.Collections.Generic;
using Shared.Battle;
using UnityEngine;

public class PolicyButtonGroup : UIBase
{
	[SerializeField]
	private PolicyButtonContainer _policyButtonContainer;

	[SerializeField]
	private GameObject _attackPointContainer;

	[SerializeField]
	private GameObject _attackPointFront;

	[SerializeField]
	private GameObject _attackPointBack;

	[SerializeField]
	private GameObject _attackPointLeft;

	[SerializeField]
	private GameObject _attackPointRight;

	private CombatPolicyUI _combatPolicyUI;

	private readonly Dictionary<DamageDirection, GameObject> _attackPoints = new Dictionary<DamageDirection, GameObject>();

	private void Awake()
	{
		_combatPolicyUI = KSingleton<CombatPolicyUI>.Instance();
		_policyButtonContainer.Init();
		((Component)_policyButtonContainer).gameObject.SetActive(false);
		_attackPointContainer.gameObject.SetActive(false);
		_attackPoints[DamageDirection.Front] = _attackPointFront;
		_attackPoints[DamageDirection.Back] = _attackPointBack;
		_attackPoints[DamageDirection.Left] = _attackPointLeft;
		_attackPoints[DamageDirection.Right] = _attackPointRight;
	}

	private void OnEnable()
	{
		GameSystem<CombatSystem>.Instance().PolicyChanged += OnChangePolicy;
		GameSystem<CombatSystem>.Instance().ChangedCombatMode += OnChangeCombatMode;
		GameSystem<CombatSystem>.Instance().CombatPoliciesUpdated += OnUpdateCombatPolicies;
		_policyButtonContainer.PolicyClicked += OnClickPolicyButton;
		GameSystem<CombatSystem>.Instance().DirectionSelected += OnDirectionSelected;
		_combatPolicyUI.CombatDirectionChanged += CombatPolicyUI_CircleClicked;
	}

	private void OnDisable()
	{
		GameSystem<CombatSystem>.Instance().PolicyChanged -= OnChangePolicy;
		GameSystem<CombatSystem>.Instance().ChangedCombatMode -= OnChangeCombatMode;
		GameSystem<CombatSystem>.Instance().CombatPoliciesUpdated -= OnUpdateCombatPolicies;
		_policyButtonContainer.PolicyClicked -= OnClickPolicyButton;
		GameSystem<CombatSystem>.Instance().DirectionSelected -= OnDirectionSelected;
		_combatPolicyUI.CombatDirectionChanged -= CombatPolicyUI_CircleClicked;
	}

	private void Update()
	{
		if (_combatPolicyUI.IsActivated)
		{
			UpdateAttackPoint(DamageDirection.Front);
			UpdateAttackPoint(DamageDirection.Back);
			UpdateAttackPoint(DamageDirection.Left);
			UpdateAttackPoint(DamageDirection.Right);
		}
	}

	public PolicyButton FindPolicyButton(string id)
	{
		return _policyButtonContainer.FindPolicyButton(id);
	}

	private GameObject GetAttackPoint(DamageDirection direction)
	{
		GameObject value;
		return (!_attackPoints.TryGetValue(direction, out value)) ? null : value;
	}

	private void UpdateAttackPoint(DamageDirection direction)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		GameObject attackPoint = GetAttackPoint(direction);
		if ((Object)(object)attackPoint != (Object)null)
		{
			attackPoint.transform.localPosition = _combatPolicyUI.GetArcNGuiPosition(direction);
		}
	}

	private void CombatPolicyUI_CircleClicked(DamageDirection direction)
	{
		GameObject attackPoint = GetAttackPoint(direction);
		if ((Object)(object)attackPoint != (Object)null)
		{
			CombatSystem.RequestCombatInputReply(direction.ToString().ToLower(), GameSystem<CombatSystem>.Instance().SelectDirectionPolicy(direction), attackPoint);
		}
	}

	private void OnClickPolicyButton(PolicyButton button)
	{
		CombatSystem.RequestCombatInputReply(button.PolicyId, GameSystem<CombatSystem>.Instance().ChangeCombatPolicy(button.PolicyId), ((Component)button).gameObject);
	}

	private void OnChangePolicy(string key, bool useDirection)
	{
		_policyButtonContainer.SelectPolicyById(key);
		_combatPolicyUI.SetDirectionEnable(useDirection);
	}

	private void OnDirectionSelected(List<DamageDirection> directions)
	{
		_combatPolicyUI.SetArcSelection(selected: false);
		for (int i = 0; i < directions.Count; i++)
		{
			_combatPolicyUI.SetArcSelection(selected: true, directions[i]);
		}
	}

	private void OnChangeCombatMode(bool isCombat)
	{
		if (isCombat)
		{
			OnUpdateCombatPolicies();
			_policyButtonContainer.Show();
			_attackPointContainer.gameObject.SetActive(true);
		}
		else
		{
			_policyButtonContainer.Hide();
			_attackPointContainer.gameObject.SetActive(false);
		}
	}

	private void OnUpdateCombatPolicies()
	{
		if (GameSystem<CombatSystem>.Instance().CombatMode)
		{
			_policyButtonContainer.RefreshButtons(GameSystem<CombatSystem>.Instance().CurrentCombatPolicies);
		}
	}
}
