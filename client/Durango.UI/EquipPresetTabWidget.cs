using Durango.Logic.Item;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class EquipPresetTabWidget : IconTabWidget
{
	[SerializeField]
	private UISprite _iconLock;

	[SerializeField]
	private UISprite _iconDurability;

	[SerializeField]
	private UISprite _remainBar;

	public void SetLocked(bool locked)
	{
		_textLabel.gameObject.SetActive(!locked);
		_iconLock.gameObject.SetActive(locked);
	}

	public void SetDurability(DurabilityState state)
	{
		_iconDurability.gameObject.SetActive(state != DurabilityState.Good);
		_iconDurability.color = ((state != DurabilityState.Destroyed) ? ItemIconWidget.DurabilityWarningColor : ItemIconWidget.DurabilityDestroyedColor);
	}

	public void SetRemainRatio(float ratio)
	{
		_remainBar.gameObject.SetActive(ratio >= 0f);
		_remainBar.fillAmount = ratio;
	}
}
