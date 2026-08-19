using System.Collections.Generic;
using Durango.Utils;

namespace Durango.UI;

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

	public const string BlueprintPanelName = "#Blueprint#";

	public const string CollectiblePanelName = "#Collectible#";

	public const string ItemPanelName = "#Item#";

	public const string AnimalPanelName = "#Animal#";

	public const string MarketPanelName = "#Market#";

	public const string EmigratePanelName = "#Emigrate#";

	public const string SkillLearningPanelName = "#SkillLearning#";

	public const string ClearInventoryWithoutToolPanelName = "#ClearInventory2#";

	private const string ButtonTypeParent = "Parent";

	private const string ButtonTypePush = "Push";

	private const string ButtonTypeToggle = "Toggle";

	private const string ButtonTypeInput = "Input";

	private const string ButtonTypeConfirm = "Confirm";

	private const string ButtonTypePage = "Page";

	private const string ButtonTypeMacro = "Macro";

	public const string RiskIslandPanelName = "#Risky#";

	public const string UrbanIslandPanelName = "#Urban#";

	public const string RuralIslandPanelName = "#Rural#";

	public const string ApopeniaIslandPanelName = "#Apopenia#";

	public const string PersoanlIslandPanelName = "#Persoanl#";

	public const string TotorialIslandPanelName = "#Tutorial#";

	public const string SeasonalIslandPanelName = "#Seasonal#";

	public const string OutpostIslandPanelName = "#Outpost#";

	public const string RaidIslandPanelName = "#Raid#";

	public const string MiscIslandPanelName = "#Misc#";

	public static void Load(CheatCommandPanelContainer[] containers, CheatCommandPanel.ButtonClickedDelegator buttonClickedDelegator)
	{
		List<Dictionary<string, List<ButtonDefine>>> list = Json.ReadFromFile<List<Dictionary<string, List<ButtonDefine>>>>("cheat_menu_buttons");
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
			case "Page":
				panel.AddPageButton(buttonDefine.text, buttonDefine.command, buttonDefine.disabled);
				break;
			case "Macro":
				panel.AddMacroButton(buttonDefine.text, buttonDefine.command, buttonDefine.disabled);
				break;
			}
		}
	}
}
