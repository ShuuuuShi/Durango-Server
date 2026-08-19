using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Shared.Item;
using UnityEngine;

namespace Durango.UI;

public class EquipQuickSlotWidget : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CCoShow_003Ed__9 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public EquipQuickSlotWidget _003C_003E4__this;

		private ListObjectPoolBase<GameObject>.Enumerator _003C_003E7__wrap1;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoShow_003Ed__9(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = default(ListObjectPoolBase<GameObject>.Enumerator);
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				EquipQuickSlotWidget equipQuickSlotWidget = _003C_003E4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					foreach (GameObject slot in equipQuickSlotWidget._slots)
					{
						slot.SetActive(value: false);
					}
					equipQuickSlotWidget._config.gameObject.SetActive(value: false);
					_003C_003E7__wrap1 = equipQuickSlotWidget._slots.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					_003C_003E7__wrap1.Current.SetActive(value: true);
					_003C_003E2__current = equipQuickSlotWidget._delay;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = default(ListObjectPoolBase<GameObject>.Enumerator);
				equipQuickSlotWidget._config.gameObject.SetActive(value: true);
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			((IDisposable)_003C_003E7__wrap1).Dispose();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

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
			slot.gameObject.FindComponent<UILabel>("Label").text = ((int)(slotType - 1 + 1)).ToString();
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
			if (!GameSystem<EquipSystem>.Instance().GetEquipPreset(item).IsHidden)
			{
				_slots.Get<SelectableWidget>(num).Selected = item == GameSystem<EquipSystem>.Instance().CurrentEquipPreset;
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoShow_003Ed__9(0)
		{
			_003C_003E4__this = this
		};
	}
}
