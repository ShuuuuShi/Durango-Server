using System;
using System.Collections.Generic;
using Durango.Logic.PlayGuide;
using Durango.Render.Camera;
using Durango.Utils;
using InteractionData;
using L10N;
using UnityEngine;

public class ClientInteractionToDo : SelectableObject
{
	[Serializable]
	private class ActionElem
	{
		[LocalizableString]
		public string Name = string.Empty;

		public Interaction Action = Interaction.ClientSidePropAction;

		public string MotionName = string.Empty;

		public float Duration = 3f;

		public string Icon = string.Empty;

		public string ToDo;
	}

	[SerializeField]
	private List<ActionElem> _actionList = new List<ActionElem>();

	[LocalizableString]
	[SerializeField]
	private string _targetName = string.Empty;

	private void Update()
	{
		if (Selectable)
		{
			return;
		}
		for (int i = 0; i < _actionList.Count; i++)
		{
			if (IsToDoProgressing(_actionList[i].ToDo))
			{
				Selectable = true;
				break;
			}
		}
	}

	private static bool IsToDoProgressing(string todoName)
	{
		ToDoBase toDoBase = GameSystem<ToDoListSystem>.Instance().FindToDo(todoName);
		if (toDoBase != null)
		{
			return !toDoBase.IsCompleted;
		}
		return false;
	}

	public override void InteractionTouched()
	{
		KUtility.DelayedCall(this, MakeInteractionMenuList, 0.1f);
	}

	public override bool MenuClicked(GameObject target, InteractionMenuData menu)
	{
		foreach (ActionElem action in _actionList)
		{
			string todo = action.ToDo;
			if (!(menu.Id != todo))
			{
				SelectableObject.PlayMotion(action.MotionName, action.Duration);
				KUtility.DelayedCall(this, delegate
				{
					SelectableObject.OnPlayMotionFinished();
					Selectable = false;
					GameSystem<ToDoListSystem>.Instance().CallComplete(todo);
				}, action.Duration);
				return true;
			}
		}
		return false;
	}

	private void MakeInteractionMenuList()
	{
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		menuList.Reset();
		foreach (ActionElem action in _actionList)
		{
			if (IsToDoProgressing(action.ToDo))
			{
				InteractionMenuData data = new InteractionMenuData(action.Action);
				data.Name = T._(action.Name);
				data.Icon = action.Icon;
				data.Id = action.ToDo;
				menuList.Add(data);
			}
		}
		string text = GetName();
		menuList.Name = text;
		menuList.Apply();
		Singleton<CameraController>.Instance().Target(base.gameObject, 0.3f);
	}

	public override string GetName()
	{
		return T._(_targetName);
	}
}
