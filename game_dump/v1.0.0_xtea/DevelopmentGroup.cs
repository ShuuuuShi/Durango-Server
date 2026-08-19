using UnityEngine;

public class DevelopmentGroup : UIBase
{
	[SerializeField]
	private GameObject _stats;

	private ConsoleGUI _consoleGui;

	private void Awake()
	{
		_consoleGui = Object.FindObjectOfType<ConsoleGUI>();
	}

	private void Start()
	{
	}

	public void OnCommands()
	{
		CommandButtonGroup commandButtonGroup = UIManager.FindScript<CommandButtonGroup>();
		if (!((Object)(object)commandButtonGroup == (Object)null))
		{
			if (commandButtonGroup.IsOpen)
			{
				commandButtonGroup.Close();
			}
			else
			{
				commandButtonGroup.Open();
			}
		}
	}

	public void OnConsole()
	{
		CommandButtonGroup commandButtonGroup = UIManager.FindScript<CommandButtonGroup>();
		if ((Object)(object)commandButtonGroup != (Object)null)
		{
			commandButtonGroup.Close();
		}
		_consoleGui.IsOpen = !_consoleGui.IsOpen;
	}

	public void OnStats()
	{
		_stats.SetActive(!_stats.activeSelf);
	}
}
