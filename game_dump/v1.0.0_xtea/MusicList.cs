using System;
using System.IO;
using L10N;
using UnityEngine;

public class MusicList : MonoBehaviour
{
	public Action<string> MidiFileClicked;

	public Action<string> CreatedNewMusic;

	private KScrollView _scrollController;

	private void Awake()
	{
		_scrollController = ((Component)this).GetComponent<KScrollView>();
		_scrollController.Nodes.Init(NodeInit);
	}

	private void NodeInit(GameObject obj)
	{
		DefaultSelectableButton component = obj.GetComponent<DefaultSelectableButton>();
		component.Clicked = NodeClicked;
	}

	private void NodeClicked()
	{
		DefaultSelectableButton defaultSelectableButton = Selectable.Current as DefaultSelectableButton;
		if (!((Object)(object)defaultSelectableButton == (Object)null))
		{
			if (string.IsNullOrEmpty(defaultSelectableButton.Value))
			{
				UIManager.Popup.TextInput.Show(CreatedNewMusic, "New File Name");
			}
			else if (MidiFileClicked != null)
			{
				MidiFileClicked(defaultSelectableButton.Value);
			}
		}
	}

	private void OnEnable()
	{
		string[] directoryFiles = KFileUtil.GetDirectoryFiles("Players/Music", "*.mid", SearchOption.TopDirectoryOnly);
		_scrollController.Nodes.Set(directoryFiles.Length + 1);
		DefaultSelectableButton component = _scrollController.Nodes[directoryFiles.Length].GetComponent<DefaultSelectableButton>();
		component.Text = T._("[CCCC22][icon_plus][-] 새 악보 추가");
		component.TextLabel.alignment = NGUIText.Alignment.Center;
		component.Value = null;
		int i = 0;
		for (int num = directoryFiles.Length; i < num; i++)
		{
			DefaultSelectableButton component2 = _scrollController.Nodes[i].GetComponent<DefaultSelectableButton>();
			component2.Text = KFileUtil.GetFileName(directoryFiles[i]);
			component2.TextLabel.alignment = NGUIText.Alignment.Left;
			component2.Value = directoryFiles[i];
		}
		_scrollController.Reposition();
	}
}
