using System;
using System.Collections.Generic;
using System.Text;
using Durango.Logic.Skill;
using Durango.Prologue;
using Durango.Render.Camera;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Shared.Battle;
using Shared.Skill;
using UnityEngine;
using Yaml;

namespace Durango.UI.Prologue;

public class PrologueCharacterSelectGroup_PC : PrologueCharacterSelectGroupBase
{
	[SerializeField]
	private UILabel _categoryTitleLabel;

	[SerializeField]
	private UILabel _categoryTitleIcon;

	[SerializeField]
	private UILabel _backButton;

	[SerializeField]
	private SelectableWidget _leftArrow;

	[SerializeField]
	private SelectableWidget _rightArrow;

	[SerializeField]
	private SelectableWidget _interactionIcon;

	[SerializeField]
	private SelectableWidget _interactionButton;

	[SerializeField]
	private GameObject _bgCollider;

	[SerializeField]
	[Tooltip("상호작용 버튼 Y 오프셋")]
	private float _interactionButtonOffsetY;

	private InteractionObject[] _characters;

	protected override void Awake()
	{
		base.Awake();
		_backButton.text = string.Format("<shortcut_box>{0},{1}</shortcut_box>", InputCommand.Back, T._("뒤로"));
		UIEventListener.Get(_bgCollider).onClick = delegate
		{
			OnClickClose();
		};
		GameSystem<InputSystem>.Instance().On(InputCommand.PrevUIGroup, delegate
		{
			OnClickArrow(isNext: false);
		});
		GameSystem<InputSystem>.Instance().On(InputCommand.NextUIGroup, delegate
		{
			OnClickArrow(isNext: true);
		});
		GameSystem<InputSystem>.Instance().On(InputCommand.FullScreenUISpaceKey, delegate
		{
			if (base.IsOpened && Singleton<PrologueManager>.Instance().CurrentState == PrologueManager.State.CharacterSelect)
			{
				OnChangeCharacterCostume(null);
			}
		});
		SelectableWidget leftArrow = _leftArrow;
		leftArrow.Clicked = (Action)Delegate.Combine(leftArrow.Clicked, (Action)delegate
		{
			OnClickArrow(isNext: false);
		});
		SelectableWidget rightArrow = _rightArrow;
		rightArrow.Clicked = (Action)Delegate.Combine(rightArrow.Clicked, (Action)delegate
		{
			OnClickArrow(isNext: true);
		});
		SelectableWidget interactionIcon = _interactionIcon;
		interactionIcon.Clicked = (Action)Delegate.Combine(interactionIcon.Clicked, (Action)delegate
		{
			OnChangeCharacterCostume(null);
		});
		SelectableWidget interactionButton = _interactionButton;
		interactionButton.Clicked = (Action)Delegate.Combine(interactionButton.Clicked, (Action)delegate
		{
			OnChangeCharacterCostume(null);
		});
		_interactionButton.gameObject.SetActive(value: false);
	}

	private void OnClickClose()
	{
		if (base.IsOpened)
		{
			OnCancelSelectCharacter(null);
			GameSystem<InputSystem>.Instance().StopPropagation();
		}
	}

	private void OnClickArrow(bool isNext)
	{
		if (!base.IsOpened || Singleton<PrologueManager>.Instance().CurrentState != PrologueManager.State.CharacterSelect)
		{
			return;
		}
		InitCharacterList();
		if (KUtility.GetSize(_characters) == 0)
		{
			return;
		}
		int num = 0;
		InteractionObject target = GameSystem<InteractionSystem>.Instance().Target;
		if (target != null)
		{
			for (int i = 0; i < _characters.Length; i++)
			{
				if (_characters[i].Target == target.Target)
				{
					num = ((!isNext) ? (i - 1) : (i + 1));
					num = (int)Mathf.Repeat(num, _characters.Length);
					break;
				}
			}
		}
		GameSystem<InteractionSystem>.Instance().SetInteractionTarget(_characters[num]);
	}

	protected override void OnSetSelectCharacterInfo(Job data)
	{
		if (data == null)
		{
			_categoryTitleLabel.text = string.Empty;
		}
		else if (KUtility.GetSize(data.category_levels) == 0)
		{
			_categoryTitleLabel.text = T._("스킬 없음");
			_categoryTitleIcon.text = string.Empty;
		}
		else
		{
			StringBuilder[] array = new StringBuilder[2]
			{
				new StringBuilder(),
				new StringBuilder()
			};
			foreach (KeyValuePair<Shared.Skill.Category, int> category_level in data.category_levels)
			{
				StringBuilder[] array2 = array;
				foreach (StringBuilder stringBuilder in array2)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(", ");
					}
				}
				Shared.Skill.Category key = category_level.Key;
				array[0].Append(T._("{1:lv:} {0}", Util.CategoryLocalizeName(key), category_level.Value));
				array[1].AppendFormat("[icon={0}]", Util.CategoryIcon(key));
			}
			_categoryTitleLabel.text = array[0].ToString().Trim();
			_categoryTitleIcon.text = array[1].ToString().Trim();
		}
		UpdateInteractionButton();
	}

	protected override void OnCancelSelectCharacter(GameObject go)
	{
		base.OnCancelSelectCharacter(go);
		GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.Init);
	}

	private void UpdateInteractionButton()
	{
		InteractionObject target = GameSystem<InteractionSystem>.Instance().Target;
		if (target == null || target.ObjectType != InteractionObject.Type.PrologueSelectCharacter)
		{
			_interactionButton.gameObject.SetActive(value: false);
			return;
		}
		_interactionButton.gameObject.SetActive(value: true);
		Vector3 position = target.CharacterTarget.GetBodyPartTransform(BodyPart.Head).position;
		position.y += _interactionButtonOffsetY;
		position = MainCamera.WorldToNGUIPos(position);
		_interactionButton.transform.localPosition = position;
	}

	private void InitCharacterList()
	{
		if (_characters != null && _characters.Length != 0)
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		InteractionSystem.GetNearObjectsInternal(list, LayerHelper.DefaultMask, 800f, (GameObject o) => (!(o.GetComponent<TriggerPrologueSelectCharacter>() != null)) ? null : o);
		List<InteractionObject> list2 = new List<InteractionObject>();
		foreach (GameObject item2 in list)
		{
			InteractionObject item = new InteractionObject(item2);
			list2.Add(item);
		}
		list2.Sort(delegate(InteractionObject a, InteractionObject b)
		{
			Vector3 position = a.CharacterTarget.GetBodyPartTransform(BodyPart.Leg).position;
			Vector3 position2 = b.CharacterTarget.GetBodyPartTransform(BodyPart.Leg).position;
			int num = (int)position.x / 148;
			int num2 = (int)position2.x / 148;
			if (num < num2)
			{
				return -1;
			}
			if (num > num2)
			{
				return 1;
			}
			return (num % 2 == 0) ? position.z.CompareTo(position2.z) : position2.z.CompareTo(position.z);
		});
		_characters = list2.ToArray();
	}

	private void LateUpdate()
	{
		if (base.IsOpened)
		{
			UpdateInteractionButton();
		}
	}
}
