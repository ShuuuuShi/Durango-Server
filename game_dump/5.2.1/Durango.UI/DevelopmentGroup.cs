using Durango.System.Config;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class DevelopmentGroup : UIBase
{
	[SerializeField]
	private UIWidget _container;

	[SerializeField]
	private GameObject _stats;

	[SerializeField]
	private SelectableButton _button;

	private ListObjectPool<SelectableButton> _buttons;

	private ConsoleGUI _consoleGui;

	private bool _isShow;

	private Rect _rect;

	private void Awake()
	{
		_consoleGui = Object.FindObjectOfType<ConsoleGUI>();
	}

	private void Start()
	{
		_buttons = new ListObjectPool<SelectableButton>();
		_buttons.BaseObject = _button;
		_buttons.UseBase = true;
		_buttons.Set(3);
		_buttons[0].Text = "Commands";
		_buttons[0].Clicked = OnCommands;
		_buttons[1].Text = "Console";
		_buttons[1].Clicked = OnConsole;
		_buttons[2].Text = "Stats";
		_buttons[2].Clicked = OnStats;
		float num = UIUtility.WidgetsReposition(_buttons, Vector3.right, _button.transform.localPosition, 5f);
		for (int i = 0; i < _buttons.Count; i++)
		{
			Vector3 localPosition = _buttons[i].transform.localPosition;
			localPosition.x -= num * 0.5f;
			localPosition.y = 0f;
			_buttons[i].transform.localPosition = localPosition;
		}
		_container.AddOnChange(UpdateWidgetRect);
		_stats.SetActive(Preferences.GetBool("Development:Stats"));
		if (ConfigInstance.GetValue("hide_debug_ui", defaultValue: false))
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void UpdateWidgetRect()
	{
		_rect = new Rect(_container.GetPosition(0f, 0f), _container.localSize);
	}

	private void Show()
	{
		if (!_isShow)
		{
			_isShow = true;
			_container.alpha = 1f;
		}
	}

	private void Hide()
	{
		if (_isShow)
		{
			_isShow = true;
			_container.alpha = 0f;
		}
	}

	private void OnCommands()
	{
		CommandButtonGroup commandButtonGroup = UIManager.FindScript<CommandButtonGroup>();
		if (!(commandButtonGroup == null))
		{
			if (commandButtonGroup.IsOpened)
			{
				commandButtonGroup.Close();
			}
			else
			{
				commandButtonGroup.Open();
			}
		}
	}

	private void OnConsole()
	{
		CommandButtonGroup commandButtonGroup = UIManager.FindScript<CommandButtonGroup>();
		if (commandButtonGroup != null)
		{
			commandButtonGroup.Close();
		}
		_consoleGui.IsOpen = !_consoleGui.IsOpen;
	}

	private void OnStats()
	{
		_stats.SetActive(!_stats.activeSelf);
		Preferences.SetBool("Development:Stats", _stats.activeSelf);
	}
}
