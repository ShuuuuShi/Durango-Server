using System.Collections.Generic;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class CheatCommandPanel : MonoBehaviour
{
	public delegate void ButtonClickedDelegator(CheatCommandPanel panel, CheatCommandButton button, int count);

	[SerializeField]
	private KScrollView _scrollView;

	private bool _firstShow = true;

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
		base.gameObject.SetActive(show);
		if (_firstShow && show)
		{
			ResetScrollPosition();
			_firstShow = false;
		}
	}

	public List<CheatCommandButton> GetButtons(CheatCommandButton.ButtonType? type = null, string group = null)
	{
		ListObjectPool nodes = _scrollView.Nodes;
		List<CheatCommandButton> list = new List<CheatCommandButton>(nodes.Count);
		for (int i = 0; i < nodes.Count; i++)
		{
			CheatCommandButton cheatCommandButton = nodes.Get<CheatCommandButton>(i);
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

	public void AddMacroButton(string buttonText, string command, bool disabled)
	{
		CreateButton(disabled).InitToMacroButton(buttonText, command);
	}

	public void AddPageButton(string buttonText, string pageName, bool disabled)
	{
		CreateButton(disabled).InitToPageButton(buttonText, pageName);
	}

	public void AddParentMenuButton(string buttonText, string childMenuName, bool disabled)
	{
		CreateButton(disabled).InitToParentMenuButton(buttonText, childMenuName);
	}

	public void AddPushButton(string buttonText, string command, bool disabled)
	{
		CreateButton(disabled).InitToPushButton(buttonText, command);
	}

	public void AddSeperatorButton(string buttonText)
	{
		CreateButton(disabled: true).InitToSeperatorButton(buttonText);
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

	public void RefreshToggleButtonSelectStates(Dictionary<string, bool> toggleDictionary)
	{
		ListObjectPool nodes = _scrollView.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			CheatCommandButton cheatCommandButton = nodes.Get<CheatCommandButton>(i);
			if (!(cheatCommandButton == null) && cheatCommandButton.Type == CheatCommandButton.ButtonType.Toggle)
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
			CheatCommandButton cheatCommandButton = nodes.Get<CheatCommandButton>(i);
			if (!(cheatCommandButton == null) && cheatCommandButton.Type == CheatCommandButton.ButtonType.ParentMenu)
			{
				cheatCommandButton.IsChecked = cheatCommandButton == toggleButton;
			}
		}
	}

	public string GetSelectedChildPanelName()
	{
		ListObjectPool nodes = _scrollView.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			CheatCommandButton cheatCommandButton = nodes.Get<CheatCommandButton>(i);
			if (!(cheatCommandButton == null) && cheatCommandButton.IsChecked)
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

	private CheatCommandButton CreateButton(bool disabled)
	{
		ListObjectPool nodes = _scrollView.Nodes;
		CheatCommandButton cheatCommandButton = nodes.Add<CheatCommandButton>();
		int num = nodes.BaseObject.GetComponent<UIWidget>().height * nodes.Count;
		cheatCommandButton.transform.localPosition = nodes.BaseObject.transform.localPosition + Vector3.down * num;
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
