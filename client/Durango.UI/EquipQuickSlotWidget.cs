using System;
using System.Collections;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Shared.Item;
using UnityEngine;

namespace Durango.UI;

public class EquipQuickSlotWidget : MonoBehaviour
{
	[SerializeField]
	private SelectableWidget _toggle;

	[SerializeField]
	private GameObject _slotAndConfig;

	[SerializeField]
	private ListObjectPool _slots;

	[SerializeField]
	private SelectableWidget _config;

	private ICoroutineBinder _showRoutine;

	private readonly WaitForSeconds _delay = new WaitForSeconds(0.05f);

	private void Awake()
	{
		GameSystem<EquipSystem>.Instance().EquipmentsUpdated += EquipSystem_EquipmentsUpdated;
		GameSystem<EquipSystem>.Instance().ChangePresetSucceeded += delegate
		{
			Show(_toggle.Selected);
		};
		SelectableWidget toggle = _toggle;
		toggle.Clicked = (Action)Delegate.Combine(toggle.Clicked, (Action)delegate
		{
			bool visible = !_toggle.Selected;
			Show(visible);
		});
		SelectableWidget config = _config;
		config.Clicked = (Action)Delegate.Combine(config.Clicked, (Action)delegate
		{
			UIManager.Open<CharacterInfoGroup>();
		});
		Show(visible: false);
	}

	private void EquipSystem_EquipmentsUpdated()
	{
		_slots.BeginLoad();
		foreach (EquipSlotType item in EquipSystem.EnumerateEquipPresetTypes())
		{
			EquipSlotType slotType = item;
			EquipSystem.EquipPreset equipPreset = GameSystem<EquipSystem>.Instance().GetEquipPreset(item);
			if (equipPreset.IsHidden)
			{
				continue;
			}
			SelectableWidget slot = _slots.GetNext().GetComponent<SelectableWidget>();
			slot.CanClickWhenDisabled = true;
			slot.Clicked = delegate
			{
				if (slot.Disabled)
				{
					UIManager.SystemMsg(T._("슬롯이 잠겨 있습니다."));
				}
				else
				{
					GameSystem<EquipSystem>.Instance().ChangePreset(slotType);
				}
			};
			slot.Disabled = equipPreset.IsLocked;
			UILabel uILabel = slot.gameObject.FindComponent<UILabel>("Label");
			uILabel.text = ((int)(slotType - 1 + 1)).ToString();
		}
		_slots.EndLoad();
		float num = UIUtility.WidgetsReposition(_slots, new Vector3(0f, 1f, 0f), 10f);
		_config.transform.localPosition = new Vector3(0f, num + 10f);
	}

	private void Show(bool visible)
	{
		bool flag = _toggle.Selected != visible;
		_toggle.Selected = visible;
		_slotAndConfig.SetActive(visible);
		if (!visible)
		{
			return;
		}
		int num = 0;
		foreach (EquipSlotType item in EquipSystem.EnumerateEquipPresetTypes())
		{
			if (_slots.Count <= num)
			{
				break;
			}
			EquipSystem.EquipPreset equipPreset = GameSystem<EquipSystem>.Instance().GetEquipPreset(item);
			if (!equipPreset.IsHidden)
			{
				SelectableWidget selectableWidget = _slots.Get<SelectableWidget>(num);
				selectableWidget.Selected = item == GameSystem<EquipSystem>.Instance().CurrentEquipPreset;
				num++;
			}
		}
		if (flag)
		{
			this.StartCoroutine(ref _showRoutine, CoShow());
		}
	}

	private IEnumerator CoShow()
	{
		foreach (GameObject slot in _slots)
		{
			slot.SetActive(value: false);
		}
		_config.gameObject.SetActive(value: false);
		foreach (GameObject obj in _slots)
		{
			obj.SetActive(value: true);
			yield return _delay;
		}
		_config.gameObject.SetActive(value: true);
	}
}
