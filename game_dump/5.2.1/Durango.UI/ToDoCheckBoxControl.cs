using Durango.Logic;
using Durango.Logic.PlayGuide;
using Durango.Logic.Social;
using Durango.UI.Popup;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class ToDoCheckBoxControl : UIWidget
{
	[SerializeField]
	private GameObject _unchecked;

	[SerializeField]
	private GameObject _checked;

	[SerializeField]
	private GameObject _bulletPoint;

	[SerializeField]
	private UILabel _text;

	[SerializeField]
	private UILabel _subtext;

	[SerializeField]
	private ToDoProgressGauge _progress;

	[CanBeNull]
	[SerializeField]
	private GameObject _tooltipParent;

	[SerializeField]
	private RectLayout _layout;

	public ToDoBase Todo { get; private set; }

	[UsedImplicitly]
	private void OnClick()
	{
		if (Todo == null || Todo.OnClicked())
		{
			return;
		}
		if (!string.IsNullOrEmpty(Todo.Tooltip))
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			int maxWidth = UIManager.SafeWidth - 350;
			widgetTooltipControl.Set(null, Todo.Tooltip, maxWidth);
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Horizontal;
			if (_tooltipParent == null)
			{
				widgetTooltipControl.Show(base.gameObject, new Vector2(5f, -5f), 10f);
			}
			else
			{
				widgetTooltipControl.Show(_tooltipParent, new Vector2(5f, 0f), 10f);
			}
		}
		else
		{
			ChattingGroupBase chattingGroupBase = UIManager.FindScript<ChattingGroupBase>();
			if (chattingGroupBase != null)
			{
				chattingGroupBase.Open(ChatFilterType.System, "1000");
			}
		}
	}

	private void SetContents(ArchipelagoToDo todo)
	{
		_unchecked.SetActive(value: false);
		_checked.SetActive(value: false);
		_bulletPoint.SetActive(value: true);
		_subtext.gameObject.SetActive(value: true);
		_text.overflowMethod = UILabel.Overflow.ClampContent;
		_text.overflowEllipsis = true;
		_text.height = 20;
		_text.text = todo.LocalText;
		_subtext.text = T._("{0:pt:}", todo.Point);
	}

	private void SetContents(ToDoBase todo)
	{
		_unchecked.SetActive(!todo.IsCompleted);
		_checked.SetActive(todo.IsCompleted);
		_bulletPoint.SetActive(value: false);
		_subtext.gameObject.SetActive(value: false);
		_text.overflowMethod = UILabel.Overflow.ResizeHeight;
		_text.text = ((!todo.IsCompleted) ? todo.LocalText : ("[s]" + todo.LocalText));
	}

	public void SetToDo(ToDoBase todo)
	{
		Todo = todo;
		if (todo is ArchipelagoToDo contents)
		{
			SetContents(contents);
		}
		else
		{
			SetContents(todo);
		}
		if (Todo.IsVisibleProgress)
		{
			_progress.gameObject.SetActive(value: true);
			_progress.Set(Todo.CurrentProgress, Todo.TargetProgress);
		}
		else
		{
			_progress.gameObject.SetActive(value: false);
		}
		_layout.UpdateLayout(base.width, 0f);
		_text.ProcessText();
		_layout.UpdateLayout(base.width, 0f);
	}

	public void ShowUpdatedFeedBack()
	{
		UITweener component = GetComponent<UITweener>();
		component.ResetToBeginning();
		component.PlayForward();
	}
}
