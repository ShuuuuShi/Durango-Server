using System;
using Durango.Render.Camera;
using UnityEngine;

namespace Durango.UI.Prologue;

public class PrologueInteractionButton_PC : MonoBehaviour
{
	[SerializeField]
	private GameObject _characterButton;

	[SerializeField]
	private UILabel _propButton;

	[SerializeField]
	private BoxCollider _propButtonCollider;

	[SerializeField]
	private GameObject _shortcut;

	[SerializeField]
	[Tooltip("단축키 아이콘이 표시되는 최소 거리")]
	private float _shortcutDisplayRange;

	private bool _init;

	public InteractionObject InteractionTarget { get; private set; }

	public bool IsPrologueCharacter { get; private set; }

	private void Init(Action<PrologueInteractionButton_PC> onClick)
	{
		if (!_init)
		{
			UIEventListener uIEventListener = UIEventListener.Get(_characterButton);
			uIEventListener.onClick = delegate
			{
				onClick(this);
			};
			uIEventListener = UIEventListener.Get(_propButtonCollider.gameObject);
			uIEventListener.onClick = delegate
			{
				onClick(this);
			};
			GameSystem<InputSystem>.Instance().On(InputCommand.Collect, OnClickCollectKey);
			_init = true;
		}
	}

	public void Set(InteractionObject obj, Action<PrologueInteractionButton_PC> onClick)
	{
		Init(onClick);
		InteractionTarget = obj;
		IsPrologueCharacter = InteractionTarget.ObjectType == InteractionObject.Type.PrologueSelectCharacter;
		_characterButton.SetActive(IsPrologueCharacter);
		_propButton.gameObject.SetActive(!IsPrologueCharacter);
		if (InteractionTarget.ObjectType == InteractionObject.Type.PropSelectableByClient)
		{
			SelectableObject component = InteractionTarget.Target.GetComponent<SelectableObject>();
			_propButton.text = component.GetName();
		}
	}

	public void SetWorldPosition(Vector3 worldPos)
	{
		base.transform.localPosition = MainCamera.WorldToNGUIPos(worldPos);
		if (!IsPrologueCharacter)
		{
			float num = Vector3.Distance(PlayerBehavior.LocalPlayer.CurrentPosition, worldPos);
			bool active = num < _shortcutDisplayRange;
			_shortcut.SetActive(active);
		}
	}

	private void OnClickCollectKey(InputCommandMessage message)
	{
		if (_propButton.gameObject.activeInHierarchy && _shortcut.activeSelf)
		{
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(InteractionTarget);
		}
	}
}
