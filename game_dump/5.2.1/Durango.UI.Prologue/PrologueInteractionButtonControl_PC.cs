using System;
using System.Collections.Generic;
using Shared.Battle;
using UnityEngine;

namespace Durango.UI.Prologue;

public class PrologueInteractionButtonControl_PC : MonoBehaviour
{
	[SerializeField]
	private PrologueInteractionButton_PC _baseObject;

	private readonly ListObjectPool<PrologueInteractionButton_PC> _buttonPool = new ListObjectPool<PrologueInteractionButton_PC>();

	private readonly List<PrologueInteractionButton_PC> _activeList = new List<PrologueInteractionButton_PC>();

	private bool _isInit;

	public event Action<InteractionObject> InteractionClicked;

	private void Init()
	{
		if (!_isInit)
		{
			_baseObject.gameObject.SetActive(value: false);
			_buttonPool.BaseObject = _baseObject;
			_buttonPool.UseBase = false;
			_isInit = true;
		}
	}

	public void SetInteractionButtons(IList<InteractionObject> list)
	{
		Init();
		Clear();
		AddButtons(list);
	}

	private void Clear()
	{
		EnableButtons(enable: false);
		_activeList.Clear();
	}

	private void EnableButtons(bool enable)
	{
		foreach (PrologueInteractionButton_PC active in _activeList)
		{
			active.gameObject.SetActive(enable);
		}
	}

	private void AddButtons(IList<InteractionObject> list)
	{
		int i = 0;
		for (int size = KUtility.GetSize(list); i < size; i++)
		{
			InteractionObject obj = list[i];
			AddButton(obj);
		}
	}

	private void AddButton(InteractionObject obj)
	{
		PrologueInteractionButton_PC button = GetButton(obj);
		button.gameObject.SetActive(value: true);
		_activeList.Add(button);
	}

	private PrologueInteractionButton_PC GetButton(InteractionObject obj)
	{
		int i = 0;
		for (int count = _buttonPool.Count; i < count; i++)
		{
			if (_buttonPool[i].InteractionTarget.Target == obj.Target)
			{
				return _buttonPool[i];
			}
		}
		PrologueInteractionButton_PC prologueInteractionButton_PC = _buttonPool.Add();
		prologueInteractionButton_PC.Set(obj, OnClickButton);
		return prologueInteractionButton_PC;
	}

	private void OnClickButton(PrologueInteractionButton_PC btn)
	{
		if (btn.gameObject.activeInHierarchy && this.InteractionClicked != null)
		{
			this.InteractionClicked(btn.InteractionTarget);
		}
	}

	private void UpdateButtons()
	{
		if (GameSystem<InteractionSystem>.Instance().Target != null)
		{
			EnableButtons(enable: false);
			return;
		}
		EnableButtons(enable: true);
		foreach (PrologueInteractionButton_PC active in _activeList)
		{
			Vector3 position;
			if (active.IsPrologueCharacter)
			{
				position = active.InteractionTarget.CharacterTarget.GetBodyPartTransform(BodyPart.Head).position;
				position.y = active.InteractionTarget.Position.y;
			}
			else
			{
				position = active.InteractionTarget.Position;
			}
			active.SetWorldPosition(position);
		}
	}

	private void LateUpdate()
	{
		UpdateButtons();
	}
}
