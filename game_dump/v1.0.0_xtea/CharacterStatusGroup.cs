using System;
using Shared.Ability;
using UnityEngine;

public class CharacterStatusGroup : UIBase
{
	[Serializable]
	private struct AbilityLayout
	{
		public Derived[] Abilities;
	}

	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private ListObjectPool _statusWidgets;

	[SerializeField]
	private AbilityLayout[] _abilityLayouts;

	private void Start()
	{
		_titleWidget.OnClose += base.ForceClose;
		base.OnClose();
	}

	private void OnEnable()
	{
		GameSystem<StatisticsSystem>.Instance().AbilitiesUpdated += OnUpdateAbilities;
	}

	private void OnDisable()
	{
		GameSystem<StatisticsSystem>.Instance().AbilitiesUpdated -= OnUpdateAbilities;
	}

	public override void Open()
	{
		base.Open();
		UpdateAbilities();
	}

	private void OnUpdateAbilities()
	{
		if (base.IsOpen)
		{
			UpdateAbilities();
		}
	}

	private void UpdateAbilities()
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		_statusWidgets.Set(_abilityLayouts.Length);
		int i = 0;
		for (int count = _statusWidgets.Count; i < count; i++)
		{
			CharacterStatusWidget component = _statusWidgets[i].GetComponent<CharacterStatusWidget>();
			int[] array = new int[_abilityLayouts[i].Abilities.Length];
			int j = 0;
			for (int num = _abilityLayouts[i].Abilities.Length; j < num; j++)
			{
				array[j] = GameSystem<StatisticsSystem>.Instance().DerivedAbilities.Get(_abilityLayouts[i].Abilities[j], 0);
			}
			component.SetData(_abilityLayouts[i].Abilities, array);
		}
		_statusWidgets.Reposition(Vector3.right);
	}
}
