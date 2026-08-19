using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Durango.Development;
using Durango.Logic.Item;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using Messages;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class CommandButtonGroup : UIBase
{
	public enum Role
	{
		Invalid = -1,
		Sandbox = 0,
		Tutorial = 1,
		Rural = 3,
		Risky = 4,
		Outpost = 5,
		Urban = 6,
		Safehouse = 7,
		Instance = 8,
		Personal = 9
	}

	[SerializeField]
	private KScrollView _mainScroll;

	[SerializeField]
	private GameObject _touchBox;

	private CheatCommandPanelContainer[] _panelContainers;

	private readonly Dictionary<string, MakeCheatGroup.Tab> _pagePanelNames = new Dictionary<string, MakeCheatGroup.Tab>();

	private bool _initializedDynamicMenuButtons;

	private string _commandForAfterConfirm = string.Empty;

	private string _commandFormatForInputNumber = string.Empty;

	private void Awake()
	{
		_mainScroll.Nodes.Set(3);
		_mainScroll.Nodes.Reposition(Vector3.right);
		_panelContainers = new CheatCommandPanelContainer[_mainScroll.Nodes.Count];
		for (int i = 0; i < _mainScroll.Nodes.Count; i++)
		{
			_panelContainers[i] = _mainScroll.Nodes[i].GetComponent<CheatCommandPanelContainer>();
		}
		UIEventListener uIEventListener = UIEventListener.Get(_touchBox);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, (UIEventListener.BoolDelegate)delegate(GameObject go, bool press)
		{
			if (!press)
			{
				ForceClose();
			}
		});
		Connections.Frontend.On<CheatFlags>(OnCheatFlags);
		base.OnOpenSucceed += OpenSucceed;
		for (int j = 0; j < _panelContainers.Length; j++)
		{
			_panelContainers[j].Init(j);
		}
		CheatCommandInitializer.Load(_panelContainers, panel_ButtonClicked);
		RefreshPanelContainers(0, string.Empty);
		SetActiveButtonPanels(activated: false);
		_pagePanelNames.Add("#Blueprint#", MakeCheatGroup.Tab.Build);
		_pagePanelNames.Add("#Item#", MakeCheatGroup.Tab.Item);
		_pagePanelNames.Add("#Collectible#", MakeCheatGroup.Tab.Gathering);
		_pagePanelNames.Add("#Animal#", MakeCheatGroup.Tab.Animal);
		_pagePanelNames.Add("#Market#", MakeCheatGroup.Tab.Market);
	}

	private CheatCommandPanelContainer GetPanelContainer(int containerIndex)
	{
		if (0 <= containerIndex && containerIndex < _panelContainers.Length)
		{
			return _panelContainers[containerIndex];
		}
		return null;
	}

	private CheatCommandPanel GetPanel(string panelName)
	{
		return _panelContainers.Select((CheatCommandPanelContainer t) => t.GetPanel(panelName)).FirstOrDefault((CheatCommandPanel panel) => panel != null);
	}

	private void RefreshPanelContainers(int containerIndex, string panelName)
	{
		CheatCommandPanelContainer panelContainer = GetPanelContainer(containerIndex);
		if (!(panelContainer == null))
		{
			RefreshPanelContainers(panelContainer, panelName);
		}
	}

	private void RefreshPanelContainers(CheatCommandPanelContainer container, string panelName)
	{
		CheatCommandPanel cheatCommandPanel = container.ShowPanel(panelName);
		RefreshPanelContainers(container.Index + 1, (!(cheatCommandPanel != null)) ? string.Empty : cheatCommandPanel.GetSelectedChildPanelName());
	}

	private void SetActiveButtonPanels(bool activated)
	{
		_mainScroll.transform.parent.gameObject.SetActive(activated);
		for (int i = 0; i < _panelContainers.Length; i++)
		{
			_panelContainers[i].gameObject.SetActive(activated);
		}
	}

	private void OpenInputNumberPanel(string commandFormat, string inputLabel)
	{
		_commandFormatForInputNumber = commandFormat;
		UIManager.Popup.Tooltip<TextInputPopup>().Show(OnInputNumber, inputLabel);
	}

	private void OnInputNumber(string value)
	{
		Durango.Utils.Singleton<Commands>.Instance().Cheat(string.Format(_commandFormatForInputNumber, value));
		Close();
	}

	private void OpenConfirmMessagePanel(string command, string confirmMessage)
	{
		_commandForAfterConfirm = command;
		UIManager.MessageBox.Show(confirmMessage, OnMessageBox);
	}

	private void OnMessageBox(bool ok)
	{
		if (ok)
		{
			Durango.Utils.Singleton<Commands>.Instance().Cheat(_commandForAfterConfirm);
			Close();
		}
	}

	private void DoPushButton(CheatCommandButton button)
	{
		Durango.Utils.Singleton<Commands>.Instance().Cheat(button.Command);
	}

	private void DoToggleButton(CheatCommandButton button)
	{
		Durango.Utils.Singleton<Commands>.Instance().Cheat(button.Command);
		Connections.Frontend.Send(default(GetCheatFlags));
	}

	private void DoConfirmButton(CheatCommandButton button)
	{
		OpenConfirmMessagePanel(button.Command, button.Message);
		SetActiveButtonPanels(activated: false);
	}

	private void DoInputNumberButton(CheatCommandButton button)
	{
		OpenInputNumberPanel(button.Command, button.Message);
		SetActiveButtonPanels(activated: false);
	}

	private void DoMacroButton(CheatCommandButton button)
	{
		string command = button.Command;
		if (command != null && command == "#ClearInventory2#")
		{
			List<ItemData> list = new List<ItemData>();
			Durango.Logic.Item.Inventory playerInventory = GameSystem<InventorySystem>.Instance().PlayerInventory;
			foreach (ItemData item in playerInventory.Items)
			{
				string category = PrototypeYaml.GetItemPrototype(item.PrototypeId, item.Level).Category;
				if (!string.IsNullOrEmpty(category) && !item.Locked)
				{
					switch (category)
					{
					case "weapon/tool":
					case "accessory":
					case "clothing":
						continue;
					}
					list.Add(item);
				}
			}
			InventorySystem.DropItems(InventorySystem.MakeDumpItemsPacket(playerInventory, Util.ItemsToIds(list)));
		}
		else
		{
			Debug.LogError("Wrong input");
		}
	}

	private void DoPageButton(CheatCommandPanel panel, CheatCommandButton button)
	{
		CheatCommandPanelContainer panelContainer = GetPanelContainer(panel.ContainerIndex + 1);
		if (!(panelContainer == null))
		{
			string command = button.Command;
			if (!_pagePanelNames.ContainsKey(command))
			{
				Debug.LogError("Invalid cheat panel type : " + command);
				return;
			}
			ClosePanelAndRefresh(panelContainer, panel);
			Close();
			UIManager.FindScript<MakeCheatGroup>().OpenTab(_pagePanelNames[command]);
		}
	}

	private CheatCommandPanelContainer DoParentMenuButton(CheatCommandPanel panel, CheatCommandButton button)
	{
		CheatCommandPanelContainer panelContainer = GetPanelContainer(panel.ContainerIndex + 1);
		if (panelContainer == null)
		{
			return null;
		}
		string obj = ((!(panelContainer.CurrentPanel != null)) ? string.Empty : panelContainer.CurrentPanel.Name);
		string childPanelName = button.GetChildPanelName();
		if (obj != childPanelName)
		{
			RefreshPanelContainers(panelContainer, childPanelName);
			panel.RefreshParentMenuButtonToggleStates(button);
		}
		else
		{
			ClosePanelAndRefresh(panelContainer, panel);
		}
		return panelContainer;
	}

	private void ClosePanelAndRefresh(CheatCommandPanelContainer container, CheatCommandPanel panel)
	{
		RefreshPanelContainers(container, string.Empty);
		panel.RefreshParentMenuButtonToggleStates(null);
	}

	private void panel_ButtonClicked(CheatCommandPanel panel, CheatCommandButton button, int count)
	{
		switch (button.Type)
		{
		case CheatCommandButton.ButtonType.Push:
			DoPushButton(button);
			break;
		case CheatCommandButton.ButtonType.Toggle:
			DoToggleButton(button);
			break;
		case CheatCommandButton.ButtonType.Confirm:
			DoConfirmButton(button);
			break;
		case CheatCommandButton.ButtonType.InputNumber:
			DoInputNumberButton(button);
			break;
		case CheatCommandButton.ButtonType.ParentMenu:
			DoParentMenuButton(panel, button);
			break;
		case CheatCommandButton.ButtonType.Page:
			DoPageButton(panel, button);
			break;
		case CheatCommandButton.ButtonType.Macro:
			DoMacroButton(button);
			break;
		case CheatCommandButton.ButtonType.Select:
			break;
		}
	}

	private string RegionToPanelName(KeyValuePair<string, RegionTemplate> pair)
	{
		if (string.IsNullOrEmpty(pair.Key))
		{
			return "#Misc#";
		}
		if (Regex.IsMatch(pair.Key, "^[a-z]{1}[0-9]{2}_{1}"))
		{
			return "#Seasonal#";
		}
		if (Regex.IsMatch(pair.Key, "^[a-z]{2}[0-9]{2}"))
		{
			if (pair.Key.StartsWith("ra"))
			{
				return "#Raid#";
			}
			if (pair.Key.StartsWith("ua"))
			{
				return "#Apopenia#";
			}
		}
		switch (pair.Value.Role)
		{
		case Shared.Region.Role.Tutorial:
		case Shared.Region.Role.Safehouse:
			return "#Tutorial#";
		case Shared.Region.Role.Rural:
			return "#Rural#";
		case Shared.Region.Role.Risky:
			return "#Risky#";
		case Shared.Region.Role.Outpost:
			return "#Outpost#";
		case Shared.Region.Role.Urban:
			return "#Urban#";
		case Shared.Region.Role.Personal:
			return "#Persoanl#";
		default:
			return "#Misc#";
		}
	}

	private void OpenSucceed()
	{
		if (_initializedDynamicMenuButtons)
		{
			return;
		}
		_initializedDynamicMenuButtons = true;
		foreach (KeyValuePair<string, RegionTemplate> item2 in SingletonDict<string, RegionTemplate>.Instance)
		{
			if (item2.Value.Active)
			{
				GetPanel(RegionToPanelName(item2)).AddPushButton(item2.Key, "em " + item2.Key, disabled: false);
			}
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (KeyValuePair<string, RegionTemplate> item3 in SingletonDict<string, RegionTemplate>.Instance)
		{
			if (!item3.Value.Active)
			{
				string item = RegionToPanelName(item3);
				CheatCommandPanel panel = GetPanel(RegionToPanelName(item3));
				if (!hashSet.Contains(item))
				{
					hashSet.Add(item);
					panel.AddSeperatorButton("이하 비활성화 된 템플릿들");
				}
				panel.AddPushButton(item3.Key, "em " + item3.Key, disabled: false);
			}
		}
		CheatCommandPanel panelSkill = GetPanel("#SkillLearning#");
		if ((bool)panelSkill)
		{
			Connections.Frontend.Send(new Cheat
			{
				_Cheat = "sc"
			}).On(delegate(Info msg, PacketHeader header)
			{
				if (!string.IsNullOrEmpty(msg.Text))
				{
					panelSkill.AddPushButton("모든 스킬 배우기", "scl all", disabled: false);
					string[] array = msg.Text.Split('\n');
					for (int i = 1; i < array.Length; i++)
					{
						string text = array[i].Substring(2);
						panelSkill.AddPushButton(text, "scl " + text, disabled: false);
					}
				}
			});
		}
		Connections.Frontend.Send(default(GetCheatFlags));
	}

	private void OnCheatFlags(CheatFlags msg, PacketHeader header)
	{
		Dictionary<string, bool> flags = msg.Flags;
		flags["ar"] = Durango.Utils.Singleton<Commands>.Instance().GetAttackRangeState();
		for (int i = 0; i < _panelContainers.Length; i++)
		{
			_panelContainers[i].RefreshToggleButtonSelectStates(flags);
		}
	}

	protected override bool TryOpen()
	{
		SetActiveButtonPanels(activated: true);
		_mainScroll.ResetPosition();
		return true;
	}

	protected override bool TryClose()
	{
		SetActiveButtonPanels(activated: false);
		return true;
	}
}
