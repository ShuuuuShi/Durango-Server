using Durango.Prologue;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI.Prologue;

public class PrologueGuideMaskGroup_PC : PrologueGuideMaskGroup
{
	[SerializeField]
	private GameObject _targetShortcut;

	protected override void Awake()
	{
		base.Awake();
		_targetShortcut.SetActive(value: false);
		GameSystem<InputSystem>.Instance().On(InputCommand.Collect, OnClickBattleButton);
	}

	private void OnClickBattleButton(InputCommandMessage message)
	{
		if (_targetShortcut.activeInHierarchy)
		{
			CharacterBehavior characterBehavior = Singleton<ObjectManager>.Instance().FindCharacter(PrologueAIRaptor.FakeEntityId);
			if (!(characterBehavior == null))
			{
				GameSystem<InteractionSystem>.Instance().SetInteractionTarget(new InteractionObject(characterBehavior.gameObject));
				_targetShortcut.SetActive(value: false);
			}
		}
	}

	public override void SetType(string type)
	{
		base.SetType(type);
		EnableBattleButton(type == "Battle");
	}

	private void EnableBattleButton(bool show)
	{
		_targetShortcut.SetActive(show);
		if (show)
		{
			_targetShortcut.transform.localPosition = base.TargetPos;
		}
	}
}
