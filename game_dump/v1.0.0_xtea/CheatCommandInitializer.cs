using System.Collections.Generic;

public class CheatCommandInitializer
{
	public class ButtonDefine
	{
		public string type;

		public string text;

		public string command;

		public string message;

		public bool disabled;
	}

	public const string ItemCategoryPanelName = "#ItemCategory#";

	public const string EmigratePanelName = "#Emigrate#";

	public const string BlueprintPanelName = "#Blueprint#";

	private const string buttonTypeParent = "Parent";

	private const string buttonTypePush = "Push";

	private const string buttonTypeToggle = "Toggle";

	private const string buttonTypeInput = "Input";

	private const string buttonTypeConfirm = "Confirm";

	public static void Load(CheatCommandPanelContainer[] containers, CheatCommandPanel.ButtonClickedDelegator buttonClickedDelegator)
	{
		List<Dictionary<string, List<ButtonDefine>>> list = KUtility.ParseJsonFile<List<Dictionary<string, List<ButtonDefine>>>>("cheat_menu_buttons");
		for (int i = 0; i < list.Count; i++)
		{
			Dictionary<string, List<ButtonDefine>> dictionary = list[i];
			if (i >= containers.Length)
			{
				break;
			}
			CheatCommandPanelContainer cheatCommandPanelContainer = containers[i];
			foreach (KeyValuePair<string, List<ButtonDefine>> item in dictionary)
			{
				CheatCommandPanel cheatCommandPanel = cheatCommandPanelContainer.CreatePanel(item.Key, string.Empty);
				cheatCommandPanel.ButtonClicked += buttonClickedDelegator;
				AddButtons(cheatCommandPanel, item.Value);
			}
		}
	}

	private static void AddButtons(CheatCommandPanel panel, List<ButtonDefine> buttonDefineList)
	{
		for (int i = 0; i < buttonDefineList.Count; i++)
		{
			ButtonDefine buttonDefine = buttonDefineList[i];
			switch (buttonDefine.type)
			{
			case "Parent":
				panel.AddParentMenuButton(buttonDefine.text, buttonDefine.command, buttonDefine.disabled);
				break;
			case "Push":
				panel.AddPushButton(buttonDefine.text, buttonDefine.command, buttonDefine.disabled);
				break;
			case "Toggle":
				panel.AddToggleButton(buttonDefine.text, buttonDefine.command, buttonDefine.disabled);
				break;
			case "Input":
				panel.AddInputNumberButton(buttonDefine.text, buttonDefine.message, buttonDefine.command, buttonDefine.disabled);
				break;
			case "Confirm":
				panel.AddConfirmButton(buttonDefine.text, buttonDefine.message, buttonDefine.command, buttonDefine.disabled);
				break;
			}
		}
	}
}
