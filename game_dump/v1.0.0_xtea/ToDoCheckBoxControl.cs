using ChatData;
using PlayGuide;
using UnityEngine;

public class ToDoCheckBoxControl : MonoBehaviour
{
	[SerializeField]
	private GameObject _unchecked;

	[SerializeField]
	private GameObject _checked;

	[SerializeField]
	private UISpriteLabel _text;

	[SerializeField]
	private UILabel _progressText;

	[SerializeField]
	private Color _checkTextColor = Color.white;

	[SerializeField]
	private UIWidget _collider;

	[SerializeField]
	private UITweener _tweener;

	private Color _uncheckTextColor = Color.white;

	public ToDoBase Todo { get; private set; }

	public int Height => _text.Label.height;

	private void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_uncheckTextColor = _text.Label.color;
		UIEventListener.Get(((Component)_collider).gameObject).onClick = Collider_OnClick;
	}

	private void Collider_OnClick(GameObject go)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (Todo.FromAutoGuide)
		{
			GameSystem<AutoGuideSystem>.Instance().SetLastSelected(Todo);
			UIManager.Open<AutoGuideGroup>();
		}
		else
		{
			if (Todo.OnClicked())
			{
				return;
			}
			if (!string.IsNullOrEmpty(Todo.Tooltip))
			{
				WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
				int maxWidth = UIManager.ScreenWidth - 350;
				widgetTooltipControl.Set(null, Todo.Tooltip, maxWidth);
				widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Horizontal;
				widgetTooltipControl.Show(((Component)this).gameObject, new Vector2(5f, -5f), 10f);
			}
			else
			{
				ChattingGroup chattingGroup = UIManager.FindScript<ChattingGroup>();
				if ((Object)(object)chattingGroup != (Object)null)
				{
					chattingGroup.Open(ChatFilterType.System, 1000uL);
				}
			}
		}
	}

	public void SetToDo(ToDoBase todo)
	{
		Todo = todo;
		UpdateContext();
	}

	public void UpdateContext()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		if (Todo != null)
		{
			_unchecked.SetActive(!Todo.IsCompleted);
			_checked.SetActive(Todo.IsCompleted);
			_text.Label.color = ((!Todo.IsCompleted) ? _uncheckTextColor : _checkTextColor);
			_text.text = ((!Todo.IsCompleted) ? Todo.LocalText : ("[s]" + Todo.LocalText));
			_progressText.text = ((!Todo.IsCompleted && Todo.TargetProgress != 0) ? $"{Todo.CurrentProgress} / {Todo.TargetProgress}" : string.Empty);
			_collider.height = _text.Label.height - (int)((Component)_text).transform.localPosition.y;
		}
	}

	public void ShowUpdatedFeedBack()
	{
		_tweener.ResetToBeginning();
		_tweener.PlayForward();
	}
}
