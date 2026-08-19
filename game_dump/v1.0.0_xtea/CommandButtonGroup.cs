using System;
using System.Collections.Generic;
using System.Text;
using Building_;
using K1Network;
using Messages;
using Shared.Etc;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class CommandButtonGroup : UIBase
{
	private const int itemCreateMultiplyCount = 5;

	[SerializeField]
	private KScrollView _mainScroll;

	[SerializeField]
	private GameObject _touchBox;

	private CheatCommandPanelContainer[] _panelContainers;

	private bool _initializedDynamicMenuButtons;

	private string _commandForAfterConfirm = string.Empty;

	private string _commandFormatForInputNumber = string.Empty;

	private KeyValuePair<string, string>[] _levelCommands;

	private KeyValuePair<string, string>[] LevelCommands
	{
		get
		{
			if (_levelCommands == null)
			{
				_levelCommands = new KeyValuePair<string, string>[30];
				for (int i = 1; i <= 30; i++)
				{
					string text = i.ToString();
					ref KeyValuePair<string, string> reference = ref _levelCommands[i - 1];
					reference = new KeyValuePair<string, string>(text, "레벨 " + text);
				}
			}
			return _levelCommands;
		}
	}

	private void Awake()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
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
		Connections.Frontend.On<Tool_Collectibles>(OnTool_CollectiblesMsg);
		Connections.Frontend.On<CheatFlags>(OnCheatFlags);
		base.OnOpenSucceed += OpenSucceed;
		for (int j = 0; j < _panelContainers.Length; j++)
		{
			_panelContainers[j].Init(j);
		}
		CheatCommandInitializer.Load(_panelContainers, panel_ButtonClicked);
		RefreshPanelContainers(0, string.Empty);
		SetActiveButtonPanels(activated: false);
	}

	private void OnPortraitMode(bool isPortrait)
	{
		((Behaviour)_mainScroll.ScrollView).enabled = isPortrait;
	}

	private CheatCommandPanelContainer GetPanelContainer(int containerIndex)
	{
		return (0 > containerIndex || containerIndex >= _panelContainers.Length) ? null : _panelContainers[containerIndex];
	}

	private CheatCommandPanel GetPanel(string name)
	{
		for (int i = 0; i < _panelContainers.Length; i++)
		{
			CheatCommandPanel panel = _panelContainers[i].GetPanel(name);
			if ((Object)(object)panel != (Object)null)
			{
				return panel;
			}
		}
		return null;
	}

	private void RefreshPanelContainers(int containerIndex, string panelName)
	{
		CheatCommandPanelContainer panelContainer = GetPanelContainer(containerIndex);
		if (!((Object)(object)panelContainer == (Object)null))
		{
			RefreshPanelContainers(panelContainer, panelName);
		}
	}

	private void RefreshPanelContainers(CheatCommandPanelContainer container, string panelName)
	{
		CheatCommandPanel cheatCommandPanel = container.ShowPanel(panelName);
		RefreshPanelContainers(container.Index + 1, (!((Object)(object)cheatCommandPanel != (Object)null)) ? string.Empty : cheatCommandPanel.GetSelectedChildPanelName());
	}

	private void SetActiveButtonPanels(bool activated)
	{
		((Component)((Component)_mainScroll).transform.parent).gameObject.SetActive(activated);
		for (int i = 0; i < _panelContainers.Length; i++)
		{
			((Component)_panelContainers[i]).gameObject.SetActive(activated);
		}
	}

	private void OpenInputNumberPanel(string commandFormat, string inputLabel)
	{
		_commandFormatForInputNumber = commandFormat;
		TextInputWidget textInput = UIManager.Popup.TextInput;
		textInput.Show(OnInputNumber, inputLabel);
	}

	private void OnInputNumber(string value)
	{
		KSingleton<Commands>.Instance().Cheat(string.Format(_commandFormatForInputNumber, value));
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
			KSingleton<Commands>.Instance().Cheat(_commandForAfterConfirm);
			Close();
		}
	}

	private void DoPushButton(CheatCommandButton button)
	{
		KSingleton<Commands>.Instance().Cheat(button.Command);
	}

	private void DoToggleButton(CheatCommandButton button)
	{
		KSingleton<Commands>.Instance().Cheat(button.Command);
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

	private CheatCommandPanelContainer DoParentMenuButton(CheatCommandPanel panel, CheatCommandButton button)
	{
		CheatCommandPanelContainer panelContainer = GetPanelContainer(panel.ContainerIndex + 1);
		if ((Object)(object)panelContainer == (Object)null)
		{
			return null;
		}
		string text = ((!((Object)(object)panelContainer.CurrentPanel != (Object)null)) ? string.Empty : panelContainer.CurrentPanel.Name);
		if (text != button.GetChildPanelName())
		{
			RefreshPanelContainers(panelContainer, button.GetChildPanelName());
			panel.RefreshParentMenuButtonToggleStates(button);
		}
		else
		{
			RefreshPanelContainers(panelContainer, string.Empty);
			panel.RefreshParentMenuButtonToggleStates(null);
		}
		return panelContainer;
	}

	private void DoItemCategoryMenuButton(CheatCommandPanel panel, CheatCommandButton button)
	{
		CheatCommandPanelContainer cheatCommandPanelContainer = DoParentMenuButton(panel, button);
		if (!((Object)(object)cheatCommandPanelContainer == (Object)null))
		{
			CheatCommandPanel panel2 = cheatCommandPanelContainer.GetPanel(button.GetChildPanelName());
			if (!((Object)(object)panel2 == (Object)null) && panel2.ButtonCount == 0)
			{
				Connections.Frontend.Send(new Cheat
				{
					_Cheat = "collectible " + button.Command
				}).On<Collectible>(OnCollectible);
			}
		}
	}

	private void DoCreateItemButton(CheatCommandPanel panel, CheatCommandButton button, int count)
	{
		string text = panel.Command;
		List<CheatCommandButton> buttons = panel.GetButtons(CheatCommandButton.ButtonType.Select);
		if (buttons.Count > 0)
		{
			text = text + ":" + buttons[0].Command;
		}
		for (int num = Mathf.Max(1, count); num > 0; num--)
		{
			Connections.Frontend.Send(new Tool_Collect
			{
				CollectibleId = text,
				GeneratorId = button.Command
			}).On(delegate(Collected msg, PacketHeader _)
			{
				GameSystem<InventorySystem>.Instance().CollectedReceived(msg);
			});
		}
	}

	private void DoBlueprintButton(CheatCommandPanel panel, CheatCommandButton button)
	{
		CheatCommandPanelContainer cheatCommandPanelContainer = DoParentMenuButton(panel, button);
		if ((Object)(object)cheatCommandPanelContainer == (Object)null)
		{
			return;
		}
		Building_.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().BlueprintContainer.GetBlueprint(button.Command);
		if (blueprint == null)
		{
			return;
		}
		CheatCommandPanel panel2 = cheatCommandPanelContainer.GetPanel(button.GetChildPanelName());
		if ((Object)(object)panel2 == (Object)null || panel2.ButtonCount != 0)
		{
			return;
		}
		panel2.AddSelectButton(LevelCommands, "level");
		panel2.AddCreateArtifactButton("만들기", blueprint.Id);
		panel2.AddCreateArtifactButton("만들기\n(내구도 무한)", blueprint.Id, "immortal");
		if (blueprint.IsSizeVariable)
		{
			KeyValuePair<string, string>[] array = new KeyValuePair<string, string>[blueprint.Size.x];
			for (int i = 0; i < blueprint.Size.x; i++)
			{
				string text = (i + 1).ToString();
				ref KeyValuePair<string, string> reference = ref array[i];
				reference = new KeyValuePair<string, string>(text, "X 크기 : " + text);
			}
			panel2.AddSelectButton(array, "sizeX");
			KeyValuePair<string, string>[] array2 = new KeyValuePair<string, string>[blueprint.Size.y];
			for (int j = 0; j < blueprint.Size.y; j++)
			{
				string text2 = (j + 1).ToString();
				ref KeyValuePair<string, string> reference2 = ref array2[j];
				reference2 = new KeyValuePair<string, string>(text2, "Y 크기 : " + text2);
			}
			panel2.AddSelectButton(array2, "sizeY");
		}
		if (blueprint.IsLookChangeable())
		{
			for (int k = 0; k < blueprint.Slots.Length; k++)
			{
				Building_.BlueprintSlot blueprintSlot = blueprint.Slots[k];
				if (!blueprintSlot.HasLook || blueprintSlot.Looks.Count <= 1)
				{
					continue;
				}
				panel2.AddSeperatorButton(blueprintSlot.Name);
				foreach (KeyValuePair<string, ArtifactLook> look in blueprintSlot.Looks)
				{
					string key = look.Key;
					Gettext gettext = look.Value.name;
					if (key == "default")
					{
						gettext = "(기본) " + gettext;
					}
					panel2.AddArtifactLookButton(gettext, blueprintSlot.Id + ":" + key, blueprintSlot.Id, key == "default");
				}
			}
		}
		panel2.ResetScrollPosition();
	}

	private void DoCreateArtifactButton(CheatCommandPanel panel, CheatCommandButton button)
	{
		string option = null;
		if (button.args != null && button.args.Length > 0)
		{
			option = button.args[0];
		}
		List<string> looks = new List<string>();
		string level = null;
		Point2? size = null;
		if ((Object)(object)panel != (Object)null)
		{
			List<CheatCommandButton> buttons = panel.GetButtons(CheatCommandButton.ButtonType.Select, "level");
			if (buttons.Count > 0)
			{
				level = buttons[0].Command;
			}
			List<CheatCommandButton> buttons2 = panel.GetButtons(CheatCommandButton.ButtonType.Select, "sizeX");
			List<CheatCommandButton> buttons3 = panel.GetButtons(CheatCommandButton.ButtonType.Select, "sizeY");
			if (buttons2.Count > 0 && buttons3.Count > 0)
			{
				size = new Point2(int.Parse(buttons2[0].Command), int.Parse(buttons3[0].Command));
			}
			List<CheatCommandButton> buttons4 = panel.GetButtons(CheatCommandButton.ButtonType.ArtifactLook);
			for (int i = 0; i < buttons4.Count; i++)
			{
				if (buttons4[i].Type != CheatCommandButton.ButtonType.ArtifactLook || buttons4[i].IsChecked)
				{
					looks.Add(buttons4[i].Command);
				}
			}
		}
		Building_.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(button.Command);
		UIManager.FindScript<BuildGridGroup>().Open(blueprint, size, delegate
		{
			BuildManager buildManager = KSingleton<BuildManager>.Instance();
			CreateArtifact(blueprint, looks, size, buildManager.Rotated ? Rotation.Quarter : Rotation.None, buildManager.WorldTilePos, level, option);
		});
	}

	private void CreateArtifact(Building_.Blueprint blueprint, IList<string> looks, Point2? size, Rotation rotation, Point2 position, string level = null, string option = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (option != null)
		{
			stringBuilder.Append(option).Append(" ");
		}
		stringBuilder.Append("prop ").Append(blueprint.EntityType.ToString());
		for (int i = 0; i < looks.Count; i++)
		{
			stringBuilder.Append(" ").Append(looks[i]);
		}
		StringBuilder stringBuilder2 = stringBuilder.Append(" rotation:");
		int num = (int)rotation;
		stringBuilder2.Append(num.ToString());
		stringBuilder.Append(" position:").Append($"{position.x},{position.y}");
		if (size.HasValue)
		{
			stringBuilder.Append(" size:").Append($"{size.Value.x},{size.Value.y}");
		}
		if (!string.IsNullOrEmpty(level))
		{
			stringBuilder.Append(" level:").Append(level);
		}
		Connections.Frontend.Send(new Cheat
		{
			_Cheat = stringBuilder.ToString()
		});
	}

	private void DoArtifactLookButton(CheatCommandPanel panel, CheatCommandButton button)
	{
		button.IsChecked = true;
		List<CheatCommandButton> buttons = panel.GetButtons(CheatCommandButton.ButtonType.ArtifactLook);
		for (int i = 0; i < buttons.Count; i++)
		{
			CheatCommandButton cheatCommandButton = buttons[i];
			if ((Object)(object)cheatCommandButton != (Object)(object)button && cheatCommandButton.Group == button.Group && button.IsChecked)
			{
				cheatCommandButton.IsChecked = false;
			}
		}
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
		case CheatCommandButton.ButtonType.ItemCategory:
			DoItemCategoryMenuButton(panel, button);
			break;
		case CheatCommandButton.ButtonType.CreateItem:
			DoCreateItemButton(panel, button, count);
			break;
		case CheatCommandButton.ButtonType.Blueprint:
			DoBlueprintButton(panel, button);
			break;
		case CheatCommandButton.ButtonType.CreateArtifact:
			DoCreateArtifactButton(panel, button);
			break;
		case CheatCommandButton.ButtonType.ArtifactLook:
			DoArtifactLookButton(panel, button);
			break;
		case CheatCommandButton.ButtonType.Select:
			break;
		}
	}

	private void OpenSucceed()
	{
		if (_initializedDynamicMenuButtons)
		{
			return;
		}
		_initializedDynamicMenuButtons = true;
		CheatCommandPanel panel = GetPanel("#ItemCategory#");
		if ((Object)(object)panel != (Object)null)
		{
			Connections.Frontend.Send(new Tool_Collectibles
			{
				Collectibles = new KeyValuePair<string, string>[0]
			});
		}
		CheatCommandPanel panel2 = GetPanel("#Emigrate#");
		if ((Object)(object)panel2 != (Object)null)
		{
			foreach (KeyValuePair<string, RegionTemplate> item in SingletonDict<string, RegionTemplate>.Instance)
			{
				if (item.Value.active)
				{
					string key = item.Key;
					panel2.AddPushButton(key, $"em {key}", disabled: false);
				}
			}
			panel2.AddSeperatorButton("이하 비활성화 된 템플릿들");
			foreach (KeyValuePair<string, RegionTemplate> item2 in SingletonDict<string, RegionTemplate>.Instance)
			{
				if (!item2.Value.active)
				{
					string key2 = item2.Key;
					panel2.AddPushButton(key2, $"em {key2}", disabled: false);
				}
			}
		}
		CheatCommandPanel panel3 = GetPanel("#Blueprint#");
		if ((Object)(object)panel3 != (Object)null)
		{
			CheatCommandPanelContainer panelContainer = GetPanelContainer(panel3.ContainerIndex + 1);
			List<Building_.Blueprint> recipes = GameSystem<RecipeSystem>.Instance().BlueprintContainer.GetRecipes();
			for (int i = 0; i < recipes.Count; i++)
			{
				Building_.Blueprint blueprint = recipes[i];
				panel3.AddBlueprintButton(blueprint.Name, blueprint.Id, blueprint.Icon, showArrow: true);
				CheatCommandPanel cheatCommandPanel = panelContainer.CreatePanel(CheatCommandPanel.GetBlueprintPanelName(blueprint.Id), blueprint.Id);
				cheatCommandPanel.ButtonClicked += panel_ButtonClicked;
			}
		}
		Connections.Frontend.Send(default(GetCheatFlags));
	}

	private void OnTool_CollectiblesMsg(Tool_Collectibles msg, PacketHeader header)
	{
		CheatCommandPanel panel = GetPanel("#ItemCategory#");
		if ((Object)(object)panel == (Object)null)
		{
			return;
		}
		CheatCommandPanelContainer panelContainer = GetPanelContainer(panel.ContainerIndex + 1);
		if ((Object)(object)panelContainer == (Object)null || panel.ButtonCount != 0)
		{
			return;
		}
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>(msg.Collectibles);
		list.Sort(new CheatItemCategoryNameComparer());
		foreach (KeyValuePair<string, string> item in list)
		{
			string key = item.Key;
			string value = item.Value;
			string itemsPanelName = CheatCommandPanel.GetItemsPanelName(key);
			panel.AddItemCategoryMenuButton(value, key, disabled: false);
			CheatCommandPanel cheatCommandPanel = panelContainer.CreatePanel(itemsPanelName, key);
			cheatCommandPanel.ButtonClicked += panel_ButtonClicked;
		}
	}

	private void OnCheatFlags(CheatFlags msg, PacketHeader header)
	{
		Dictionary<string, bool> flags = msg.Flags;
		flags["camerazoom"] = KSingleton<Commands>.Instance().GetCameraZoomModeState();
		flags["dm"] = KSingleton<Commands>.Instance().GetDamageMeterState();
		flags["ar"] = KSingleton<Commands>.Instance().GetAttackRangeState();
		for (int i = 0; i < _panelContainers.Length; i++)
		{
			_panelContainers[i].RefreshToggleButtonSelectStates(flags);
		}
	}

	private void OnCollectible(Collectible msg, PacketHeader header)
	{
		string itemsPanelName = CheatCommandPanel.GetItemsPanelName(msg.CollectibleId);
		CheatCommandPanel panel = GetPanel(itemsPanelName);
		if (!((Object)(object)panel == (Object)null) && panel.ButtonCount == 0)
		{
			panel.AddSelectButton(LevelCommands);
			Generator[] generators = msg.Generators;
			for (int i = 0; i < generators.Length; i++)
			{
				Generator generator = generators[i];
				panel.AddItemCreateButton(generator.Name, generator.Id, generator.Icon, 5);
			}
			panel.ResetScrollPosition();
		}
	}

	protected override bool OnOpen()
	{
		SetActiveButtonPanels(activated: true);
		_mainScroll.ResetPosition();
		return true;
	}

	protected override bool OnClose()
	{
		SetActiveButtonPanels(activated: false);
		return true;
	}
}
