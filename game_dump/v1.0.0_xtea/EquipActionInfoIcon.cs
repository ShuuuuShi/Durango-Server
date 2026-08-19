using System;
using CombatData;
using UnityEngine;

public class EquipActionInfoIcon : Selectable
{
	[Serializable]
	private struct StateColor
	{
		public Color Seal;

		public Color SealSelect;

		public Color Normal;

		public Color NormalSelect;
	}

	[SerializeField]
	private UISprite _border;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private GameObject _sealIcon;

	[SerializeField]
	private StateColor _iconColor;

	[SerializeField]
	private StateColor _borderColor;

	public CombatData.Action Action { get; private set; }

	public void Set(CombatData.Action action)
	{
		Action = action;
		_iconSprite.spriteName = action.Icon;
		base.Disable = !action.IsLearned;
		Refresh();
	}

	protected override void OnInit()
	{
	}

	protected override void Refresh(bool select)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		_sealIcon.SetActive(base.Disable);
		_iconSprite.color = GetColor(_iconColor, base.Disable, select);
		_border.color = GetColor(_borderColor, base.Disable, select);
	}

	private static Color GetColor(StateColor color, bool disable, bool select)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (select)
		{
			return (!disable) ? color.NormalSelect : color.SealSelect;
		}
		return (!disable) ? color.Normal : color.Seal;
	}

	private void OnPress(bool isPress)
	{
		Refresh(isPress || base.Select);
	}
}
