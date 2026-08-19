using System;
using UnityEngine;

public class MinimapGroup : UIBase
{
	[SerializeField]
	private GameObject _minimapTouchBox;

	[SerializeField]
	private Transform _attachDock;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_minimapTouchBox);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			OpenWorldMap();
		});
		AttachMapContext();
		ToDoListGroup toDoListGroup = UIManager.FindScript<ToDoListGroup>();
		toDoListGroup.WidthRatioChanged += OnChangeTodoWidthRatio;
	}

	private void OpenWorldMap()
	{
		WorldMapGroup worldMapGroup = UIManager.FindScript<WorldMapGroup>();
		if (!((Object)(object)worldMapGroup == (Object)null))
		{
			worldMapGroup.Open();
		}
	}

	public void AttachMapContext()
	{
		UIManager.MapContext.Attach(worldMapMode: false, _attachDock);
	}

	private void OnChangeTodoWidthRatio(float ratio)
	{
		base.Alpha = ratio;
	}
}
