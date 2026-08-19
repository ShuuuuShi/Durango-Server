using Durango.Logic;
using Durango.Logic.PlayGuide;
using Durango.Logic.WarpRush;
using Durango.UI.Control;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class ToDoIconNode : SelectableWidget
{
	[SerializeField]
	private UISprite _portrait;

	[SerializeField]
	private UISprite _border;

	[SerializeField]
	private GameObject _messageOnly;

	[SerializeField]
	private UISprite _seasonSprite;

	[SerializeField]
	private GameObject _labelBg;

	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private GameObject _notification;

	private int _initialSize;

	public Durango.Logic.PlayGuide.ToDoCollection Collection { get; private set; }

	public float Alpha
	{
		get
		{
			return base.Widget.alpha;
		}
		set
		{
			base.Widget.alpha = value;
		}
	}

	protected override void OnInit()
	{
		_initialSize = _portrait.width;
	}

	protected override void OnRefresh(State state)
	{
		if (base.Selected)
		{
			SetWidgetState(State.Selected);
		}
		else
		{
			base.OnRefresh(state);
		}
	}

	private void OnEnable()
	{
		GameSystem<ToDoListSystem>.Instance().ContextUpdated += RefreshLabel;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		GameSystem<ToDoListSystem>.Instance().ContextUpdated -= RefreshLabel;
	}

	public void Set(Durango.Logic.PlayGuide.ToDoCollection collection)
	{
		Init();
		Collection = collection;
		_portrait.spriteName = Collection.Icon;
		int iconSize = Collection.IconSize;
		iconSize = ((iconSize != 0) ? iconSize : _initialSize);
		_portrait.SetDimensions(iconSize, iconSize);
		bool flag = Collection.IsMessageOnly();
		_messageOnly.SetActive(flag);
		_portrait.alpha = ((!flag) ? 1f : 0.6f);
		_border.alpha = 1f;
		RereshSeasonInfo();
		RefreshLabel(collection);
	}

	private void RefreshLabel(Durango.Logic.PlayGuide.ToDoCollection collection, ToDoBase todo = null, bool textOnly = false)
	{
		if (Collection != collection)
		{
			return;
		}
		bool flag = collection is CustomerServiceToDoCollection;
		_notification.SetActive(flag);
		_labelBg.SetActive(flag);
		if (flag)
		{
			_label.gameObject.SetActive(value: true);
			_label.text = "CS";
			return;
		}
		bool active = false;
		ArchipelagoToDoCollection archipelagoTodo = collection as ArchipelagoToDoCollection;
		if (archipelagoTodo != null)
		{
			active = true;
			switch (archipelagoTodo.CurrentState)
			{
			case ArchipelagoToDoCollection.State.Doing:
				archipelagoTodo.CurrentPoint.Changed = delegate
				{
					_label.text = ((archipelagoTodo.ClearPoint <= 0) ? string.Empty : $"{(float)(int)archipelagoTodo.CurrentPoint / (float)archipelagoTodo.ClearPoint:P0}");
				};
				archipelagoTodo.CurrentPoint.Changed(-1);
				break;
			case ArchipelagoToDoCollection.State.Reportable:
			case ArchipelagoToDoCollection.State.Done:
				_label.text = $"{1f:P0}";
				break;
			case ArchipelagoToDoCollection.State.CanDo:
				_label.text = $"{0f:P0}";
				break;
			default:
				active = false;
				break;
			}
		}
		if (collection is EntryTodoCollection)
		{
			S02EntreeInfo entreeInfo = GameSystem<WarpRushSystem>.Instance().EntreeInfo;
			active = true;
			_label.text = $"{entreeInfo.QueueCount}/{OptionSystem.GetWarpRushEntryCount()}";
		}
		_label.gameObject.SetActive(active);
	}

	public void RereshSeasonInfo()
	{
		string subIcon = Collection.GetSubIcon();
		if (string.IsNullOrEmpty(subIcon))
		{
			_seasonSprite.gameObject.SetActive(value: false);
			return;
		}
		_seasonSprite.gameObject.SetActive(value: true);
		_seasonSprite.spriteName = subIcon;
		TweenRotation component = _seasonSprite.GetComponent<TweenRotation>();
		if (component != null)
		{
			component.ResetToBeginning();
			component.enabled = Collection.IsSubIconRotational;
		}
	}
}
