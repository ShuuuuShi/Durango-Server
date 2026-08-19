using System.Collections.Generic;
using UnityEngine;

public class CheatCommandPanel : MonoBehaviour
{
	public delegate void ButtonClickedDelegator(CheatCommandPanel panel, CheatCommandButton button, int count);

	[SerializeField]
	private KScrollView _scrollView;

	private bool firstShow = true;

	public int ContainerIndex { get; private set; }

	public string Name { get; private set; }

	public string Command { get; private set; }

	public int ButtonCount => _scrollView.Nodes.Count;

	public event ButtonClickedDelegator ButtonClicked;

	public void Init(int index, string name, string command)
	{
		ContainerIndex = index;
		Name = name;
		Command = command;
	}

	public void Show(bool show)
	{
		((Component)this).gameObject.SetActive(show);
		if (firstShow && show)
		{
			ResetScrollPosition();
			firstShow = false;
		}
	}

	public List<CheatCommandButton> GetButtons(CheatCommandButton.ButtonType? type = null, string group = null)
	{
		ListObjectPool nodes = _scrollView.Nodes;
		List<CheatCommandButton> list = new List<CheatCommandButton>(nodes.Count);
		for (int i = 0; i < nodes.Count; i++)
		{
			CheatCommandButton cheatCommandButton = ((ListObjectPoolBase<GameObject>)nodes).Get<CheatCommandButton>(i);
			if ((!type.HasValue || type.Value == cheatCommandButton.Type) && (group == null || !(group != cheatCommandButton.Group)))
			{
				list.Add(cheatCommandButton);
			}
		}
		return list;
	}

	public void ResetScrollPosition()
	{
		_scrollView.Reposition(resetPosition: true, tween: false);
	}

	public void AddParentMenuButton(string buttonText, string childMenuName, bool disabled)
	{
		CreateButton(disabled).InitToParentMenuButton(buttonText, childMenuName);
	}

	public void AddPushButton(string buttonText, string command, bool disabled)
	{
		CreateButton(disabled).InitToPushButton(buttonText, command);
	}

	public void AddBlueprintButton(string buttonText, string blueprintId, string iconName, bool showArrow)
	{
		CreateButton(disabled: false).InitToBluprintButton(buttonText, blueprintId, iconName, showArrow);
	}

	public void AddSeperatorButton(string buttonText)
	{
		CreateButton(disabled: true).InitToSeperatorButton(buttonText);
	}

	public void AddArtifactLookButton(string buttonText, string command, string group, bool selected)
	{
		CreateButton(disabled: false).InitToArtifactLookButton(buttonText, command, group, selected);
	}

	public void AddCreateArtifactButton(string buttonText, string command, string parameter = null)
	{
		CreateButton(disabled: false).InitToCreateArtifactButton(buttonText, command, parameter);
	}

	public void AddSelectButton(KeyValuePair<string, string>[] commands, string group = null)
	{
		CreateButton(disabled: false).InitToSelectButton(commands, group);
	}

	public void AddToggleButton(string buttonText, string command, bool disabled)
	{
		CreateButton(disabled).InitToToggleButton(buttonText, command);
	}

	public void AddConfirmButton(string buttonText, string confirmMessage, string command, bool disabled)
	{
		CreateButton(disabled).InitToConfirmButton(buttonText, confirmMessage, command);
	}

	public void AddInputNumberButton(string buttonText, string inputMessage, string commandFormat, bool disabled)
	{
		CreateButton(disabled).InitToInputNumberButton(buttonText, inputMessage, commandFormat);
	}

	public void AddItemCategoryMenuButton(string buttonText, string categoryId, bool disabled)
	{
		CreateButton(disabled).InitToItemCategoryMenuButton(buttonText, categoryId);
	}

	public void AddItemCreateButton(string buttonText, string itemId, string iconName, int multiplyCount)
	{
		CreateButton(disabled: false).InitToItemCreateButton(buttonText, itemId, iconName, multiplyCount);
	}

	public void RefreshToggleButtonSelectStates(Dictionary<string, bool> toggleDictionary)
	{
		ListObjectPool nodes = _scrollView.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			CheatCommandButton cheatCommandButton = ((ListObjectPoolBase<GameObject>)nodes).Get<CheatCommandButton>(i);
			if (!((Object)(object)cheatCommandButton == (Object)null) && cheatCommandButton.Type == CheatCommandButton.ButtonType.Toggle)
			{
				string key = cheatCommandButton.Command.Replace(' ', '_');
				cheatCommandButton.IsChecked = toggleDictionary.TryGetValue(key, out var value) && value;
			}
		}
	}

	public void RefreshParentMenuButtonToggleStates(CheatCommandButton toggleButton)
	{
		ListObjectPool nodes = _scrollView.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			CheatCommandButton cheatCommandButton = ((ListObjectPoolBase<GameObject>)nodes).Get<CheatCommandButton>(i);
			if (!((Object)(object)cheatCommandButton == (Object)null) && (cheatCommandButton.Type == CheatCommandButton.ButtonType.ParentMenu || cheatCommandButton.Type == CheatCommandButton.ButtonType.ItemCategory))
			{
				cheatCommandButton.IsChecked = (Object)(object)cheatCommandButton == (Object)(object)toggleButton;
			}
		}
	}

	public string GetSelectedChildPanelName()
	{
		ListObjectPool nodes = _scrollView.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			CheatCommandButton cheatCommandButton = ((ListObjectPoolBase<GameObject>)nodes).Get<CheatCommandButton>(i);
			if (!((Object)(object)cheatCommandButton == (Object)null) && cheatCommandButton.IsChecked)
			{
				string childPanelName = cheatCommandButton.GetChildPanelName();
				if (childPanelName != string.Empty)
				{
					return childPanelName;
				}
			}
		}
		return string.Empty;
	}

	public static string GetItemsPanelName(string categoryId)
	{
		return "ItemCategory_" + categoryId;
	}

	public static string GetBlueprintPanelName(string blueprintId)
	{
		return "Blueprint_" + blueprintId;
	}

	private CheatCommandButton CreateButton(bool disabled)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		ListObjectPool nodes = _scrollView.Nodes;
		CheatCommandButton cheatCommandButton = ((ListObjectPoolBase<GameObject>)nodes).Add<CheatCommandButton>();
		int num = nodes.BaseObject.GetComponent<UIWidget>().height * nodes.Count;
		((Component)cheatCommandButton).transform.localPosition = nodes.BaseObject.transform.localPosition + Vector3.down * (float)num;
		cheatCommandButton.IsDisabled = disabled;
		cheatCommandButton.Clicked += ButtonOnClicked;
		return cheatCommandButton;
	}

	private void ButtonOnClicked(CheatCommandButton button, int count)
	{
		if (this.ButtonClicked != null)
		{
			this.ButtonClicked(this, button, count);
		}
	}
}
