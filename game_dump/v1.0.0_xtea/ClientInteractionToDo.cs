using System;
using System.Collections.Generic;
using InteractionData;
using L10N;
using PlayGuide;
using UnityEngine;

public class ClientInteractionToDo : SelectableObject
{
	[Serializable]
	public class ActionElem
	{
		[LocalizableString]
		public string Name = string.Empty;

		public Interaction Action = Interaction.ClientSidePropAction;

		public string MotionName = string.Empty;

		public float Duration = 3f;

		public string Icon = string.Empty;
	}

	[SerializeField]
	private List<ActionElem> _actionList = new List<ActionElem>();

	[SerializeField]
	private string _targetName = string.Empty;

	[SerializeField]
	private string _todoName = string.Empty;

	private void Update()
	{
		if (!Selectable)
		{
			ToDoBase toDoBase = GameSystem<ToDoListSystem>.Instance().FindToDo(_todoName);
			if (toDoBase != null && !toDoBase.IsCompleted)
			{
				Selectable = true;
			}
		}
	}

	public override void InteractionTouched()
	{
		KUtility.DelayedCall((MonoBehaviour)(object)this, MakeInteractionMenuList, 0.1f);
	}

	public override bool MenuClicked(GameObject target, InteractionMenuData menu)
	{
		for (int i = 0; i < _actionList.Count; i++)
		{
			ActionElem actionElem = _actionList[i];
			if (!(menu.Name != T._(actionElem.Name)))
			{
				SelectableObject.PlayMotion(actionElem.MotionName, actionElem.Duration);
				KUtility.DelayedCall((MonoBehaviour)(object)this, delegate
				{
					Selectable = false;
					GameSystem<ToDoListSystem>.Instance().CallComplete(_todoName);
					SelectableObject.ShowInteractionButton(show: true);
					InteractionButtonGroup.RefreshInteractions(reset: true);
				}, actionElem.Duration);
				return true;
			}
		}
		return false;
	}

	private void MakeInteractionMenuList()
	{
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		menuList.Reset();
		int count = _actionList.Count;
		for (int i = 0; i < count; i++)
		{
			InteractionMenuData data = new InteractionMenuData(_actionList[i].Action);
			data.Name = T._(_actionList[i].Name);
			data.Icon = _actionList[i].Icon;
			menuList.Add(data);
		}
		string name = LocalizeSystem.Get(_targetName);
		menuList.Name = name;
		menuList.Apply();
		KSingleton<CameraController>.Instance().SetCameraTarget(((Component)this).gameObject);
	}
}
