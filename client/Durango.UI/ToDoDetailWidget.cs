using System.Collections.Generic;
using Durango.Logic.PlayGuide;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class ToDoDetailWidget : UIWidget, IScreenResizeReceiver
{
	[SerializeField]
	private GameObject _headerWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UIWidget _commonWidget;

	[SerializeField]
	private UILabel _commonLabel;

	[SerializeField]
	private UIWidget _todoListWidget;

	[SerializeField]
	private KScrollView _todoList;

	[SerializeField]
	private GameObject _helpButton;

	[SerializeField]
	private ToDoProgressGauge _progress;

	[SerializeField]
	private SelectableButton _button;

	[SerializeField]
	private RectLayout _layout;

	[CanBeNull]
	[SerializeField]
	private TweenerPlayer _appearTweenerPlayer;

	private bool _active;

	private bool _visible;

	private int _contentsMaxHeight;

	private readonly Dictionary<string, float> _contentsScrollPosition = new Dictionary<string, float>();

	public ToDoCollection Collection { get; private set; }

	public void OnChangeScreenSize()
	{
		Vector3 position = base.transform.position;
		Transform transform = Singleton<UIManager>.Instance().UIRoot.transform;
		_contentsMaxHeight = (int)(transform.InverseTransformPoint(position).y + ((float)UIManager.ScreenHeight * 0.5f - 280f));
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (Application.isPlaying)
		{
			UIEventListener.Get(_helpButton).onClick = OnClickHelp;
		}
	}

	private void RefreshTodoList()
	{
		int size = KUtility.GetSize(Collection.ToDoList);
		ListObjectPool nodes = _todoList.Nodes;
		nodes.BeginLoad();
		for (int i = 0; i < size; i++)
		{
			ToDoBase toDo = Collection.ToDoList[i];
			ToDoCheckBoxControl component = nodes.GetNext().GetComponent<ToDoCheckBoxControl>();
			component.SetToDo(toDo);
		}
		nodes.EndLoad();
		float offset = _contentsScrollPosition.Get(Collection.Key, 0f);
		_todoList.UpdateLayout();
		_todoList.MoveTo(offset, instant: true);
		int num = (int)_todoList.ContentsLength;
		if (num < _contentsMaxHeight)
		{
			_todoListWidget.height = num;
			_todoList.ScrollView.enabled = false;
		}
		else
		{
			_todoListWidget.height = _contentsMaxHeight;
			_todoList.ScrollView.enabled = true;
		}
	}

	private void SaveContentsScrollPosition()
	{
		if (Collection == null)
		{
			return;
		}
		Reusable<HashSet<string>> reusable = null;
		foreach (KeyValuePair<string, float> item in _contentsScrollPosition)
		{
			if (GameSystem<ToDoListSystem>.Instance().FindCollection(item.Key) == null)
			{
				if (reusable == null)
				{
					reusable = ReusableHashSet<string>.Pop();
				}
				reusable.Value.Add(item.Key);
			}
		}
		if (reusable != null)
		{
			foreach (string item2 in reusable.Value)
			{
				_contentsScrollPosition.Remove(item2);
			}
			reusable.Dispose();
		}
		if (_todoListWidget.gameObject.activeInHierarchy)
		{
			string key = Collection.Key;
			float currentOffset = _todoList.CurrentOffset;
			if (currentOffset > 0f)
			{
				_contentsScrollPosition[key] = currentOffset;
			}
			else
			{
				_contentsScrollPosition.Remove(key);
			}
		}
	}

	public void Set([CanBeNull] ToDoCollection collection)
	{
		SaveContentsScrollPosition();
		ToDoCollection.Detail? detail = collection?.GetDetail();
		if (!detail.HasValue)
		{
			Collection = null;
			Activate(active: false);
			return;
		}
		Collection = collection;
		_titleLabel.text = Collection.Title;
		_helpButton.SetActive(Collection.HasHelp);
		bool flag = detail.Value.CommonText.HasText();
		bool flag2 = !string.IsNullOrEmpty(detail.Value.ButtonText);
		_headerWidget.SetActive(detail.Value.IsHeaderVisible);
		_commonWidget.gameObject.SetActive(flag);
		_todoListWidget.gameObject.SetActive(detail.Value.IsTodoListVisible);
		_progress.gameObject.SetActive(detail.Value.Progress.HasValue);
		_button.gameObject.SetActive(flag2);
		if (flag)
		{
			_commonLabel.alignment = detail.Value.CommonTextAlignment;
			_commonLabel.SetText(detail.Value.CommonText);
			_commonWidget.height = (int)_commonLabel.printedSize.y;
		}
		if (flag2)
		{
			_button.Text = detail.Value.ButtonText;
			_button.Clicked = detail.Value.ButtonClicked;
			_button.SetStyle(detail.Value.ButtonStyle);
			_button.SetEffect(detail.Value.ButtonEffect);
		}
		if (detail.Value.Progress.HasValue)
		{
			int item = detail.Value.Progress.Value.Item1;
			int item2 = detail.Value.Progress.Value.Item2;
			_progress.Set(item, item2);
		}
		if (detail.Value.IsTodoListVisible)
		{
			RefreshTodoList();
		}
		_layout.UpdateLayout(base.width, 0f);
		UIUtility.UpdateAnchors(base.transform);
		Activate(active: true);
	}

	private void Activate(bool active)
	{
		if (_appearTweenerPlayer == null)
		{
			base.gameObject.SetActive(active);
			return;
		}
		_active = active;
		Show(active);
	}

	public void Show(bool show)
	{
		if ((_active || !show) && _visible != show)
		{
			_visible = show;
			if (!(_appearTweenerPlayer == null))
			{
				_appearTweenerPlayer.Play(show, null);
			}
		}
	}

	public void ShowUpdatedFeedBack(ToDoBase todo)
	{
		for (int i = 0; i < _todoList.Nodes.Count; i++)
		{
			ToDoCheckBoxControl component = _todoList.Nodes[i].GetComponent<ToDoCheckBoxControl>();
			if (component.Todo == todo)
			{
				component.ShowUpdatedFeedBack();
				break;
			}
		}
	}

	private void OnClickHelp(GameObject obj)
	{
		if (Collection != null)
		{
			Collection.NotifyHelpClicked();
		}
	}
}
