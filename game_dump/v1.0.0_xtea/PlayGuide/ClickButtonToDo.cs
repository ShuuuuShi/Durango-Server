using UnityEngine;

namespace PlayGuide;

internal class ClickButtonToDo : ToDoBase
{
	private readonly string _id;

	public ClickButtonToDo(string id)
	{
		_id = id;
	}

	public override void OnAddItem()
	{
		KSingleton<UIManager>.Instance().ToggleClickEventHandler(_id, UIManager_OnClick, add: true);
	}

	public override void OnRemoveItem()
	{
		KSingleton<UIManager>.Instance().ToggleClickEventHandler(_id, UIManager_OnClick, add: false);
	}

	private void UIManager_OnClick(GameObject go)
	{
		CallComplete();
	}
}
